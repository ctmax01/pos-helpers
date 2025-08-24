using System;

namespace Pos.Helpers
{
    public class ClientException : Exception
    {
        public int Code { get; set; }
        public object Errors { get; set; }

        public ClientException(string message, object errors = null, int code = 400)
            : base(message)
        {
            Errors = errors;
            Code = code;
        }
    }

    public class ServerException : Exception
    {
        public int Code { get; set; }

        public ServerException(string message, int code = 500)
            : base(message)
        {
            Code = code;
        }
    }
}
