using BackEnd.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Ultilities
{
    public static class Extensions
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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
                if (!(String.IsNullOrEmpty(value) || value == "null"))
                {
                    formParameters.Add(key, value);
                }
            }

            for (int i = 0; i< collection.Files.Count;i++)
            {
                if (collection.Files[i].Name == null) continue;
                var value = collection.Files[i];

                // value = CrossSiteAttackUtil.CleanHtml(value);
                if (value != null)
                {
                    formParameters.Add(collection.Files[i].Name, value);
                }
            }

            return formParameters;
        }
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public static string ToPascalCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return Char.ToUpperInvariant(str[0]) + str.Substring(1);
            }
            return str;
        }
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public static IDictionary<string, object> DictionaryToPascal(IDictionary<string, object> source)
        {
            var dest = new Dictionary<string, object>();
            foreach (var couple in source)
            {
                dest.Add(couple.Key.ToPascalCase(), couple.Value);
            }
            return dest;
        }
        public static IFormFile GetDefaultAvatar(IWebHostEnvironment _env)
        {
            var ms = new MemoryStream();
            var rootPath = _env.ContentRootPath;
            string path = Path.Combine(rootPath, $"Files/Users/Images");

            var filePath = Path.Combine(path, "default.png");
            var file = File.OpenRead(filePath);
            try
            {
                file.CopyTo(ms);
                return new FormFile(ms, 0, ms.Length, "default", "default.png");
            }
            catch (Exception e)
            {
                ms.Dispose();
                throw;
            }
            finally
            {
                ms.Dispose();
            }
        }
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public static IFormFile GetRoomDefaultAvatar(IWebHostEnvironment _env)
        {
            var ms = new MemoryStream();
            var rootPath = _env.ContentRootPath;
            string path = Path.Combine(rootPath, $"Files/Images");

            var filePath = Path.Combine(path, "default.png");
            var file = File.OpenRead(filePath);
            try
            {
                file.CopyTo(ms);
                return new FormFile(ms, 0, ms.Length, "default", "default.png");
            }
            catch (Exception e)
            {
                ms.Dispose();
                throw;
            }
            finally
            {
                ms.Dispose();
            }
        }
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public static DateTime ToDateTime(this string s,
                string format = "ddMMyyyy", string cultureString = "en-US")
        {
            try
            {
                var r = DateTime.ParseExact(
                    s: s,
                    format: format,
                    provider: CultureInfo.GetCultureInfo(cultureString));
                return r;
            }
            catch (FormatException)
            {
                throw;
            }
            catch (CultureNotFoundException)
            {
                throw; // Given Culture is not supported culture
            }
        }
    }



}
