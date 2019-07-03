
using System;

namespace DeathByCaptchaSharp
{
    /// <summary>
    /// Exception indicating the CAPTCHA image was rejected due to being empty, or too big, or not a valid image at all.
    /// </summary>
    public class InvalidCaptchaException : Exception
    {
        internal InvalidCaptchaException() : base()
        { }

        internal InvalidCaptchaException(string message) : base(message)
        { }
    }
}
