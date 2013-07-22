﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace MrCMS.Web.Apps.Ecommerce.Services.Templating
{
    public class NotificationTemplateProcesor : INotificationTemplateProcessor
    {
        public string ReplaceTokensAndMethods<T>(T tokenProvider, string template)
        {
            var processedTemplate = ReplaceTokens(tokenProvider, template);
            processedTemplate = ReplaceMethods(tokenProvider, processedTemplate);
            processedTemplate = ReplaceExtensionMethods(tokenProvider, processedTemplate);
            return processedTemplate;
        }

        public string ReplaceExtensionMethods<T>(T tokenProvider, string template)
        {
            var query = from type in tokenProvider.GetType().Assembly.GetTypes()
                        where type.IsSealed && !type.IsGenericType && !type.IsNested
                        from method in type.GetMethods(BindingFlags.Static
                            | BindingFlags.Public | BindingFlags.NonPublic)
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        where method.GetParameters()[0].ParameterType == tokenProvider.GetType()
                        select method;

            Dictionary<string, string> replacements = new Dictionary<string, string>();

            foreach (Match item in GetRegexMatches(template))
            {
                if (item.Value.Contains("()"))
                {
                    string cleanMethodName = item.Value.Replace("{", "").Replace("}", "").Replace("(", "").Replace(")", ""); ;
                    MethodInfo method = query.SingleOrDefault(x => x.Name.Contains(cleanMethodName));
                    if (method != null)
                        replacements.Add(method.Name + "()", method.Invoke(tokenProvider,new object[]{tokenProvider}).ToString());
                }
            }

            return ReplaceTokensForString(template, replacements);
        }

        public string ReplaceMethods<T>(T tokenProvider,string template)
        {
            MethodInfo[] methods = tokenProvider.GetType().GetMethods();
            Dictionary<string, string> replacements = new Dictionary<string, string>();

            foreach (Match item in GetRegexMatches(template))
            {
                if (item.Value.Contains("()"))
                {
                    string cleanMethodName = item.Value.Replace("{", "").Replace("}", "").Replace("(", "").Replace(")", ""); ;
                    MethodInfo method = methods.SingleOrDefault(x => x.Name.Contains(cleanMethodName));
                    if (method != null)
                        replacements.Add(method.Name + "()", method.Invoke(tokenProvider, null).ToString());
                }
            }

            return ReplaceTokensForString(template, replacements);
        }

        public string ReplaceTokens<T>(T tokenProvider, string template)
        {
            Type[] acceptedTypes = { typeof(String), typeof(Int32), typeof(Decimal), typeof(DateTime), typeof(Boolean), typeof(bool), typeof(float) };
            Dictionary<string, string> replacements = new Dictionary<string, string>();
            foreach (PropertyInfo item in tokenProvider.GetType().GetProperties())
            {
                if (acceptedTypes.Any(x => x == item.PropertyType))
                {
                    object value = item.GetValue(tokenProvider, null);
                    if (value != null)
                    {
                        replacements.Add(item.Name, value.ToString());
                    }
                }
            }

            return ReplaceTokensForString(template, replacements);
        }

        public string ReplaceTokensForString(string template, Dictionary<string, string> replacements)
        {
            var regex = new Regex(@"\{([^}]+)}");
            return (regex.Replace(template, delegate(Match match)
            {
                string key = match.Groups[1].Value;
                string replacement = replacements.ContainsKey(key) ? replacements[key] : match.Value;
                return (replacement);
            }));
        }

        public MatchCollection GetRegexMatches(string template)
        {
            var regex = new Regex(@"\{([^}]+)}");
            return regex.Matches(template);
        }
    }
}