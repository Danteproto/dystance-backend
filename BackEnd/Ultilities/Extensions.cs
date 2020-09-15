using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Ultilities
{
    public static class Extensions
    {
        public static IDictionary<string, string> ToDictionary(this NameValueCollection col)
        {
            IDictionary<string, string> myDictionary = new Dictionary<string, string>();
            if (col != null)
            {
                myDictionary =
                    col.Cast<string>()
                        .Select(s => new { Key = s, Value = col[s] })
                        .ToDictionary(p => p.Key, p => p.Value);
            }
            return myDictionary;
        }

        public static IDictionary<string, object> GetFormParameters(this IFormCollection collection)
        {
            IDictionary<string, object> formParameters = new Dictionary<string, object>();
            foreach (string key in collection.Keys)
            {
                if (key == null) continue;
                var value = collection[key];

                // value = CrossSiteAttackUtil.CleanHtml(value);
                if (!String.IsNullOrEmpty(value))
                {
                    formParameters.Add(key, value);
                }
            }

            return formParameters;


        }
    }
}
