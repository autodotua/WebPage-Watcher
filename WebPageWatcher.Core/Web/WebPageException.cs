using System;
using WebPageWatcher.Data;

namespace WebPageWatcher.Web
{
    public abstract class WebPageWatcherException : Exception
    {
        public string MessageKey { get; protected set; }
        public IDbModel Item { get; protected set; }
        protected WebPageWatcherException(string messageKey, string message, IDbModel item, Exception innerExcveption = null) : this(message, innerExcveption)
        {
            MessageKey = messageKey;
            Item = item;
        }
        protected WebPageWatcherException(string messageKey, IDbModel item, Exception innerExcveption = null) : this(Strings.Get(messageKey), innerExcveption)
        {
            MessageKey = messageKey;
            Item = item;
        }

        private WebPageWatcherException(string message, Exception innerException) : base(message, innerException)
        {
        }


        public virtual Log ToLog()
        {
            return new Log(MessageKey, Item?.ToString(), Item==null?-1:Item.ID);
        }
    }
    public class WebPageException : WebPageWatcherException
    {
        public WebPageException(string messageKey, IDbModel item, Exception innerExcveption = null) : base(messageKey, item, innerExcveption)
        {
            DbHelper.AddLogAsync(ToLog()).Wait();
        }

        public WebPageException(string messageKey, string message, IDbModel item, Exception innerExcveption = null) : base(messageKey, message, item, innerExcveption)
        {
            DbHelper.AddLogAsync(ToLog()).Wait();
        }
    }
    public class ScriptException : WebPageWatcherException
    {
        public string Command { get; }
        public int Line { get; }

        public ScriptException(string messageKey, string command, int line, Script script, Exception innerException = null) : base(messageKey, script, innerException)
        {
            Command = command;
            Line = line;
            Item = script;
            DbHelper.AddLogAsync(ToLog()).Wait();
        }


        public override string ToString()
        {
            return string.Format(Strings.Get("ex_CommandAndLine"), Line, Command) + Environment.NewLine + base.ToString();
        }
        public override Log ToLog()
        {
            return new Log(MessageKey, $"{ Item?.ToString()}  {Line}:{Command}", Item.ID);
        }
    }
}
