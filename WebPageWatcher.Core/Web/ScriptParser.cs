using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using WebPageWatcher.Data;
using static WebPageWatcher.Web.ScriptException;

namespace WebPageWatcher.Web
{
    public class ScriptParser
    {
        public ScriptParser(Script script)
        {
            Script = script;
        }

        private List<ScriptVariable> variables = new List<ScriptVariable>();
        public ReadOnlyCollection<ScriptVariable> Variables => variables.AsReadOnly();
        public event EventHandler<string> Output;
        private int currentLine;
        private string currentCommand;
        public Script Script { get; private set; }
        public async Task ParseAsync()
        {
            string content = Script.Code;
            int index = 0;
            foreach (var line in content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
            {
                currentLine = ++index;
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                currentCommand = line;
                await ParseLineAsync(line);
            }
        }

        private async Task ParseLineAsync(string line)
        {
            Debug.Assert(!line.Contains(Environment.NewLine));

            string[] parts = line.Split(' ');
            if (parts.Length <= 1)
            {
                ThrowException("ex_syntaxError");
            }
            switch (parts[0])
            {
                case "cp": await ParseCompareAsync(parts); break;
                case "let": await ParseLetAsync(parts); break;
                case "set": ParseSet(parts); break;
                case "log": ParseLog(parts); break;
                default: throw new ScriptException("ex_unknownCommand", currentCommand, currentLine,Script);

            }

        }

        private async Task ParseCompareAsync(string[] parts)
        {
            if (parts.Length != 2)
            {
                ThrowException("ex_syntaxError");
            }
            string webPageName = parts[1];

            WebPage webPage = GetVariable(webPageName, "webpage") as WebPage;

            bool result = await BackgroundTask.CheckAndExcuteWebPageAsync(webPage, true);
            Output?.Invoke(this, $"compare {webPage}, change result is {result}");

        }

        private void ParseLog(string[] parts)
        {
            Output?.Invoke(this, string.Join(" ", parts.Skip(1)));
        }
        private void ParseSet(string[] parts)
        {
            if (parts.Length != 4)
            {
                ThrowException("ex_syntaxError");
            }
            WebPage webPage = GetVariable(parts[1], "webpage") as WebPage;
            string propertyName = parts[2];
            switch (propertyName)
            {
                case "Cookies":
                    {
                        if (!(GetVariable(parts[3], "response") is HttpWebResponse response))
                        {
                            ThrowException("ex_cannotFindProperty");
                            throw new Exception();
                        }
                        webPage.Request_Cookies.Clear();
                        foreach (System.Net.Cookie cookie in response.Cookies)
                        {
                            webPage.Request_Cookies.Add(new Data.Cookie(cookie.Name, cookie.Value));
                            Output?.Invoke(this, $"set cookie {cookie.Name} = {cookie.Value}");
                        }
                    }
                    break;
                default:
                    {
                        string newValue = GetVariable<string>(parts[3]);
                        PropertyInfo property = webPage.GetType().GetProperty(propertyName);
                        if (property == null)
                        {
                            ThrowException("ex_cannotFindProperty");
                        }
                        bool errorType = false;
                        try
                        {
                            switch (property.PropertyType.Name)
                            {
                                case "String":
                                    property.SetValue(webPage, newValue, null);
                                    return;
                                case "Int32":
                                    property.SetValue(webPage, int.Parse(newValue), null);
                                    break;
                                case "Double":
                                    property.SetValue(webPage, double.Parse(newValue), null);
                                    break;
                                case "Boolean":
                                    property.SetValue(webPage, bool.Parse(newValue), null);
                                    break;
                                default:
                                    errorType = true;
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            ThrowException("ex_wrongPropertyValue");
                        }
                        if (errorType)
                        {
                            ThrowException("ex_invalidPropertyType");
                        }
                    }
                    break;
            }

        }

        private void ThrowException(string messageKey) => throw new ScriptException(messageKey, currentCommand, currentLine, Script);
        private async Task ParseLetAsync(string[] parts)
        {
            if (parts.Length != 4 && parts.Length != 5)
            {
                ThrowException("ex_syntaxError");
            }
            string key = parts[1];
            if (variables.Any(p => p.Key == key))
            {
                ThrowException("ex_cannotFindVariable");
            }
            //待加入：重复key
            string type = parts[2];
            string value = parts[3];
            object realValue = type switch
            {
                "string" => GetString(value),
                "response" => await GetResponseAsync(value),
                "responseText" => await GetResponseTextAsync(value),
                "responseJsonValue" => GetResponseData(value),
                //"responseCookie" => GetResponseCookie(value),
                "webpage" => GetWebPage(value, parts.Length == 5 && parts[4].Contains("clone")),
                _ => throw new ScriptException("ex_wrongType", currentCommand, currentLine, Script),

            };
            Output?.Invoke(this, $"add new variable: {key} = {realValue.ToString()}");

        ScriptVariable variable = new ScriptVariable(key, realValue, type);
            variables.Add(variable);
        }


        //private CookieCollection GetResponseCookie(string command)
        //{
        //    string[] parts = command.Split('.');
        //    string variableKey = parts[0];

        //    HttpWebResponse response = GetVariable<HttpWebResponse>(variableKey);
        //    return response.Cookies;
        //}
        private string GetResponseData(string command)
        {
            string[] parts = command.Split('.');
            string variableKey = parts[0];

            string response = GetVariable(variableKey, "responseText") as string;

            JToken jToken = JToken.Parse(response);
            string path = command.Substring(command.IndexOf('.') + 1);
            var target = jToken.SelectToken(path);
            if (target == null)
            {
                ThrowException("ex_jsonPathError");
            }
            string value = target.Value<string>();
            Output?.Invoke(this, $"get response json data of {variableKey}: {value}");
            return value;
        }

        private object GetVariable(string variableKey, string type)
        {
            ScriptVariable variable = variables.FirstOrDefault(p => p.Key == variableKey && p.Type == type);
            if (variable == null)
            {
                ThrowException("ex_cannotFindVariable");
            }

            return variable.Value;
        }
        private T GetVariable<T>(string variableKey)
        {
            ScriptVariable variable = variables.FirstOrDefault(p => p.Key == variableKey && p.Value is T);
            if (variable == null)
            {
                ThrowException("ex_cannotFindVariable");
            }

            return (T)variable.Value;
        }

        private string GetString(string value)
        {
            string oldValue = value;
            foreach (var variable in variables.Where(p => p.Value is string))
            {
                value = value.Replace("{" + variable.Key + "}", variable.Value as string);
            }
            Output?.Invoke(this, $"get string \"{oldValue}\" => \"{value}\"");
            return value;
        }
        private async Task<HttpWebResponse> GetResponseAsync(string name)
        {

            WebPage webPage = GetVariable(name, "webpage") as WebPage;
            Output?.Invoke(this, $"get response of web page {webPage}");

            return await HtmlGetter.GetResponseAsync(webPage);
        }
        private async Task<string> GetResponseTextAsync(string name)
        {

            WebPage webPage = GetVariable(name, "webpage") as WebPage;

            string value = await HtmlGetter.GetResponseTextAsync(webPage);
            Output?.Invoke(this, $"get response text of web page {webPage}");
            return value;
        }
        private WebPage GetWebPage(string webPageName, bool clone = false)
        {
            var webPages = BackgroundTask.WebPages.Where(p => p.Name == webPageName);
            if (webPages.Count() == 0)
            {
                ThrowException("ex_cannotFindWebPageByName");
            }
            if (webPages.Count() > 1)
            {
                ThrowException("ex_webPageNameNotUnique");
            }

            Output?.Invoke(this, $"get web page by name \"{webPageName}\"");
            if (clone)
            {
                return webPages.First().Clone();
            }
            else
            {
                return webPages.First();
            }
        }

    }

    public class ScriptVariable
    {
        public ScriptVariable()
        {
        }

        public ScriptVariable(string key, object value, string type)
        {
            Key = key;
            Value = value;
            Type = type;
        }

        public string Key { get; }
        public object Value { get; }
        public string Type { get; }

        public override string ToString()
        {
            return $"{Type} {Key}: {Value}";
        }
    }

    
}
