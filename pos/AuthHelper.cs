using System;
using System.Web;

namespace Pos.Helpers
{
    public static class AuthHelper
    {

        public static void ValidateDevice(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            string deviceId = request.Headers["deviceId"];
            if (string.IsNullOrEmpty(deviceId))
                throw new Exception("deviceId обязателен");
        }
    }
}
