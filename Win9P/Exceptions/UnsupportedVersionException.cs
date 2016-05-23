using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Win9P.Exceptions
{
    public class UnsupportedVersionException : Exception
    {
        public UnsupportedVersionException()
        {
        }

        public UnsupportedVersionException(string message) : base(SetMessage(message))
        {
        }

        public UnsupportedVersionException(string message, Exception innerException) : base(SetMessage(message), innerException)
        {
        }

        protected UnsupportedVersionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        private static string SetMessage(string version)
        {
            return $"Version {version} is not supported";
        }
    }
}
