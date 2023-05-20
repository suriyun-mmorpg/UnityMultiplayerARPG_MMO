using System;
using System.Linq;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    /// <summary>
    /// Security exception for when the store is not supported by the <c>CrossPlatformValidator</c>.
    /// </summary>
    public class StoreNotSupportedException : IAPSecurityException
    {
        /// <summary>
        /// Constructs an instance with a message.
        /// </summary>
        /// <param name="message"> The message that describes the error. </param>
        public StoreNotSupportedException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Security exception for an invalid App bundle ID.
    /// </summary>
    public class InvalidBundleIdException : IAPSecurityException { }


    /// <summary>
    /// Security exception for invalid receipt Data.
    /// </summary>
    public class InvalidReceiptDataException : IAPSecurityException { }

    /// <summary>
    /// Security exception for a missing store secret.
    /// </summary>
    public class MissingStoreSecretException : IAPSecurityException
    {
        /// <summary>
        /// Constructs an instance with a message.
        /// </summary>
        /// <param name="message"> The message that describes the error. </param>
        public MissingStoreSecretException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Security exception for an invalid public key.
    /// </summary>
    public class InvalidPublicKeyException : IAPSecurityException
    {
        /// <summary>
        /// Constructs an instance with a message.
        /// </summary>
        /// <param name="message"> The message that describes the error. </param>
        public InvalidPublicKeyException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// A generic exception for <c>CrossPlatformValidator</c> issues.
    /// </summary>
    public class GenericValidationException : IAPSecurityException
    {
        /// <summary>
        /// Constructs an instance with a message.
        /// </summary>
        /// <param name="message"> The message that describes the error. </param>
        public GenericValidationException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Class that validates receipts on multiple platforms that support the Security module.
    /// Note that this currently only supports GooglePlay and Apple platforms.
    /// </summary>
    public class CrossPlatformValidator
    {
        private GooglePlayValidator google;
        private AppleValidator apple;
        private string googleBundleId, appleBundleId;

        /// <summary>
        /// Constructs an instance and checks the validity of the certification keys
        /// which only takes input parameters for the supported platforms and uses a common bundle ID for Apple and GooglePlay.
        /// </summary>
        /// <param name="googlePublicKey"> The GooglePlay public key. </param>
        /// <param name="appleRootCert"> The Apple certification key. </param>
        /// <param name="appBundleId"> The bundle ID for all platforms. </param>
        public CrossPlatformValidator(byte[] googlePublicKey, byte[] appleRootCert,
            string appBundleId) : this(googlePublicKey, appleRootCert, null, appBundleId, appBundleId, null)
        {
        }

        /// <summary>
        /// Constructs an instance and checks the validity of the certification keys
        /// which uses a common bundle ID for Apple and GooglePlay.
        /// </summary>
        /// <param name="googlePublicKey"> The GooglePlay public key. </param>
        /// <param name="appleRootCert"> The Apple certification key. </param>
        /// <param name="unityChannelPublicKey_not_used"> The Unity Channel public key. Not used because Unity Channel is no longer supported. </param>
        /// <param name="appBundleId"> The bundle ID for all platforms. </param>
        public CrossPlatformValidator(byte[] googlePublicKey, byte[] appleRootCert, byte[] unityChannelPublicKey_not_used,
            string appBundleId)
            : this(googlePublicKey, appleRootCert, null, appBundleId, appBundleId, appBundleId)
        {
        }

        /// <summary>
        /// Constructs an instance and checks the validity of the certification keys
        /// which only takes input parameters for the supported platforms.
        /// </summary>
        /// <param name="googlePublicKey"> The GooglePlay public key. </param>
        /// <param name="appleRootCert"> The Apple certification key. </param>
        /// <param name="googleBundleId"> The GooglePlay bundle ID. </param>
        /// <param name="appleBundleId"> The Apple bundle ID. </param>
        public CrossPlatformValidator(byte[] googlePublicKey, byte[] appleRootCert,
            string googleBundleId, string appleBundleId)
            : this(googlePublicKey, appleRootCert, null, googleBundleId, appleBundleId, null)
        {
        }

        /// <summary>
        /// Constructs an instance and checks the validity of the certification keys.
        /// </summary>
        /// <param name="googlePublicKey"> The GooglePlay public key. </param>
        /// <param name="appleRootCert"> The Apple certification key. </param>
        /// <param name="unityChannelPublicKey_not_used"> The Unity Channel public key. Not used because Unity Channel is no longer supported. </param>
        /// <param name="googleBundleId"> The GooglePlay bundle ID. </param>
        /// <param name="appleBundleId"> The Apple bundle ID. </param>
        /// <param name="xiaomiBundleId_not_used"> The Xiaomi bundle ID. Not used because Xiaomi is no longer supported. </param>
        public CrossPlatformValidator(byte[] googlePublicKey, byte[] appleRootCert, byte[] unityChannelPublicKey_not_used,
            string googleBundleId, string appleBundleId, string xiaomiBundleId_not_used)
        {
            try
            {
                if (null != googlePublicKey)
                {
                    google = new GooglePlayValidator(googlePublicKey);
                }

                if (null != appleRootCert)
                {
                    apple = new AppleValidator(appleRootCert);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPublicKeyException("Cannot instantiate self with an invalid public key. (" +
                    ex.ToString() + ")");
            }

            this.googleBundleId = googleBundleId;
            this.appleBundleId = appleBundleId;
        }

        /// <summary>
        /// Validates a receipt.
        /// </summary>
        /// <param name="unityIAPReceipt"> The receipt to be validated. </param>
        /// <returns> An array of receipts parsed from the validation process </returns>
        public IPurchaseReceipt[] Validate(string unityIAPReceipt)
        {
            try
            {
                var wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(unityIAPReceipt);
                if (null == wrapper)
                {
                    throw new InvalidReceiptDataException();
                }

                var store = (string)wrapper["Store"];
                var payload = (string)wrapper["Payload"];

                switch (store)
                {
                    case "GooglePlay":
                    {
                        if (null == google)
                        {
                            throw new MissingStoreSecretException(
                                "Cannot validate a Google Play receipt without a Google Play public key.");
                        }
                        var details = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
                        var json = (string)details["json"];
                        var sig = (string)details["signature"];
                        var result = google.Validate(json, sig);

                        // [IAP-1696] Check googleBundleId if packageName is present inside the signed receipt.
                        // packageName can be missing when the GPB v1 getPurchaseHistory API is used to fetch.
                        if (!string.IsNullOrEmpty(result.packageName) &&
                            !googleBundleId.Equals(result.packageName))
                        {
                            throw new InvalidBundleIdException();
                        }

                        return new IPurchaseReceipt[] { result };
                    }
                    case "AppleAppStore":
                    case "MacAppStore":
                    {
                        if (null == apple)
                        {
                            throw new MissingStoreSecretException(
                                "Cannot validate an Apple receipt without supplying an Apple root certificate");
                        }
                        var r = apple.Validate(Convert.FromBase64String(payload));
                        if (!appleBundleId.Equals(r.bundleID))
                        {
                            throw new InvalidBundleIdException();
                        }
                        return r.inAppPurchaseReceipts.ToArray();
                    }
                    default:
                    {
                        throw new StoreNotSupportedException("Store not supported: " + store);
                    }
                }
            }
            catch (IAPSecurityException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new GenericValidationException("Cannot validate due to unhandled exception. (" + ex + ")");
            }
        }
    }
}
