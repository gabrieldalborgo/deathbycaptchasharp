
using System;

namespace DeathByCaptchaSharp
{
    /// <summary>
    /// Exception indicating that CAPTCHA was rejected due to service being overloaded.
    /// </summary>
    public class ServiceOverloadException : Exception
    {
        internal ServiceOverloadException() : base()
        { }

        internal ServiceOverloadException(string message) : base(message)
        { }
    }
}
