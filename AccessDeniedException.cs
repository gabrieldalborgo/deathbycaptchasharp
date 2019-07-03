
using System;

namespace DeathByCaptchaSharp
{
    /// <summary>
    /// Exception indicating that access to the API was denied due to invalid credentials, insufficied balance, or because the DBC account was banned.
    /// </summary>
    public class AccessDeniedException : Exception
    {
        internal AccessDeniedException() : base()
        { }

        internal AccessDeniedException(string message) : base(message)
        { }
    }
}
