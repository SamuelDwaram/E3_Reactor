using Newtonsoft.Json;
using System;
using System.IO;

namespace E3.ReactorManager.Interfaces.Helpers
{
    public class JsonHelper
    {
        public static T ImportJson<T>(string jsonPath)
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
            {
                return Activator.CreateInstance<T>();
            }
            else
            {
                StreamReader sr = new StreamReader(jsonPath);
                string json = sr.ReadToEnd();
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                return JsonConvert.DeserializeObject<T>(json, settings);
            }
        }

        public static string SerializeObject<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
