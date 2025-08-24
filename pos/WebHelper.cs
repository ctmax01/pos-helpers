using System;
using System.Web;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Pos.Helpers
{
    public static class WebHelper
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            StringEscapeHandling = StringEscapeHandling.Default,
            ContractResolver = new DefaultContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public static void WriteJson(HttpResponse response, int statusCode, object data)
        {
            response.Clear();
            response.TrySkipIisCustomErrors = true;
            response.StatusCode = statusCode;
            response.ContentType = "application/json; charset=utf-8";
            response.Charset = "utf-8";
            response.ContentEncoding = Encoding.UTF8;
            string json = JsonConvert.SerializeObject(data, JsonSettings);
            response.Write(json);
            response.Flush();
        }

        public static T ReadJson<T>(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request null");
            using (var reader = new StreamReader(request.InputStream, Encoding.UTF8))
            {
                string json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        public static void Json(HttpResponse response, int statusCode, bool success, string message, object data = null, object errors = null, object extra = null)
        {
            if (response == null)
                throw new ArgumentNullException("response null");

            var payload = new Dictionary<string, object>();
            payload.Add("success", success);
            payload.Add("message", message);
            payload.Add("errors", errors);
            payload.Add("data", data);

            if (extra != null)
            {
                var extraDict = extra.GetType()
                                     .GetProperties();
                foreach (var prop in extraDict)
                {
                    var key = prop.Name;
                    var value = prop.GetValue(extra, null);
                    payload.Add(key, value);
                }
            }

            WriteJson(response, statusCode, payload);
        }


        public static void Success(HttpResponse response, object data = null, string message = "OK", object extra = null)
        {
            Json(response, 200, true, message, data, null, extra);
        }

        public static void ClientError(HttpResponse response, string message, object error = null, int statusCode = 400, object extra = null)
        {
            Json(response, statusCode, false, message, null, error, extra);
        }

        public static void ServerError(HttpResponse response, string message = "Ошибка сервера", string error = null, object extra = null)
        {
            Json(response, 500, false, message, null, error, extra);
        }
    }
}
