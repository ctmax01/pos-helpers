using System;
using System.Web;

namespace Pos.Helpers
{
    public static class AuthHelper
    {

        public static string ValidateDevice(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("REQUEST NULL");

            string deviceId = request.Headers["deviceId"];
            if (string.IsNullOrEmpty(deviceId))
                throw new ClientException("CLIENT ID NOT PROVIDED");

            return deviceId;
        }
    }
}
