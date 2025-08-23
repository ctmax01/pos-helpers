using System;
using System.Web;
using System.Web.Script.Serialization;
using System.IO;

namespace Pos.Helpers
{
    public static class WebHelper
    {
        private static JavaScriptSerializer serializer = new JavaScriptSerializer();

        // --- Работа с JSON ---
        public static void WriteJson(HttpResponse response, int statusCode, object data)
        {
            response.StatusCode = statusCode;
            response.ContentType = "application/json; charset=utf-8";
            response.Write(serializer.Serialize(data));
        }

        public static T ReadJson<T>(HttpRequest request)
        {
            using (var reader = new StreamReader(request.InputStream))
            {
                string json = reader.ReadToEnd();
                return serializer.Deserialize<T>(json);
            }
        }

        // --- Получение branchName из URL ---
        public static string GetBranchName(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            string path = request.Path;
            var segments = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length > 0)
                return segments[0];

            throw new Exception("Не удалось определить branchName");
        }
    }
}
