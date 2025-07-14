using System;

namespace Smartitecture.Core.Exceptions
{
    /// <summary>
    /// Represents errors that occur during Python API service operations.
    /// </summary>
    public class PythonApiException : Exception
    {
        public PythonApiException()
        {
        }

        public PythonApiException(string message) 
            : base(message)
        {
        }

        public PythonApiException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Represents errors that occur when the Python API service is unavailable.
    /// </summary>
    public class ServiceUnavailableException : PythonApiException
    {
        public ServiceUnavailableException()
        {
        }

        public ServiceUnavailableException(string message) 
            : base(message)
        {
        }

        public ServiceUnavailableException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
