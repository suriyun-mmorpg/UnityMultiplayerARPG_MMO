using System;

namespace MultiplayerARPG.MMO
{
    /// <summary>
    /// A base exception for IAP Security issues.
    /// </summary>
    public class IAPSecurityException : Exception
    {
        /// <summary>
        /// Constructs an instance with no message.
        /// </summary>
        public IAPSecurityException() { }

        /// <summary>
        /// Constructs an instance with a message.
        /// </summary>
        /// <param name="message"> The message that describes the error. </param>
        public IAPSecurityException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// An exception for an invalid IAP Security signature.
    /// </summary>
    public class InvalidSignatureException : IAPSecurityException { }
}
