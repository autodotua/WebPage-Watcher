﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using WebPageWatcher.Data;

namespace WebPageWatcher.UI
{
    public class IsNotNullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ResponseTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)(ResponseType)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (ResponseType)(int)value;
        }
    }

    public class TimeSpan2Ms : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int interval = (int)value;
            return DateTime.Today + TimeSpan.FromMilliseconds(interval);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime? interval = value as DateTime?;
            if (interval.HasValue)
            {
                int ms = (int)(interval.Value - DateTime.Today).TotalMilliseconds;
                if (ms == 0)
                {
                    ms = 1000 * 60;
                }
                return ms;
            }
            return 1000 * 600;
        }
    }

    public class AttributesConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            HtmlAttributeCollection attributes = value as HtmlAttributeCollection;
            return string.Join(" ", attributes.Select(p => p.Name + ":" + p.Value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HtmlNodeConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            HtmlNode node = value as HtmlNode;
            if (node.InnerHtml.Length == 0)
            {
                return node.OuterHtml.Trim();
            }
            return node.OuterHtml.Replace(node.InnerHtml, "").Trim();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HtmlChildNodesFilterConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            HtmlNodeCollection nodes = value as HtmlNodeCollection;
            if (nodes == null || nodes.Count == 0)
            {
                return nodes;
            }
            foreach (var node in nodes.ToArray())
            {
                if (node.NodeType == HtmlNodeType.Text)
                {
                    nodes.Remove(node);
                }
            }
            return nodes;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class MethodToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(parameter is string methodName))
            {
                return null;
            }
            var methodInfo = value.GetType().GetMethod(methodName, new Type[0]);
            if (methodInfo == null)
            {
                return null;
            }
            return methodInfo.Invoke(value, new object[0]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public sealed class LogTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string type = value as string;
            return Strings.Get(type);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public sealed class TriggerEnumToStringConverter : IValueConverter
    {
        private static Dictionary<TriggerEvent, string> triggerEventToResouceKey
            = new Dictionary<TriggerEvent, string>()
            {
                [TriggerEvent.ExcuteScriptFailed] = "trigger_excuteScriptFailed",
                [TriggerEvent.ExcuteScriptSucceed] = "trigger_excuteScriptSucceed",
                [TriggerEvent.ExcuteWebPageFailed] = "trigger_excuteWebPageFailed",
                [TriggerEvent.ExcuteWebPageChanged] = "trigger_excuteWebPageChanged",
                [TriggerEvent.ExcuteWebPageNotChanged] = "trigger_excuteWebPageNotChanged",
                [TriggerEvent.None] = "trigger_none",
            };

        private static Dictionary<TriggerOperation, string> triggerOperationToResouceKey
            = new Dictionary<TriggerOperation, string>()
            {
                [TriggerOperation.ExcuteScript] = "trigger_excuteScript",
                [TriggerOperation.ExcuteWebPage] = "trigger_excuteWebPage",
                [TriggerOperation.ExcuteCommand] = "trigger_excuteCommand",
                [TriggerOperation.None] = "trigger_none",
            };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TriggerEvent @event)
            {
                return App.Current.FindResource(triggerEventToResouceKey[@event]) as string;
            }
            else if (value is TriggerOperation operation)
            {
                return App.Current.FindResource(triggerOperationToResouceKey[operation]);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}