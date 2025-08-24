using System;
using System.Text;

namespace Pos.Helpers
{
    public class ClientException : Exception
    {
        public int Code { get; set; }
        public object Error { get; set; }

        public ClientException(string message, object error = null, int code = 400)
            : base(Helpers.FixEncoding(message))
        {
            Error = error;
            Code = code;
        }
    }

    public class ServerException : Exception
    {
        public int Code { get; set; }
        public object Error { get; set; }

        public ServerException(string message, object error = null, int code = 500)
            : base(Helpers.FixEncoding(message))
        {
            Error = error;
            Code = code;
        }
    }

}
