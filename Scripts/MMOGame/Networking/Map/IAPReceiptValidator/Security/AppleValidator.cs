using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace MultiplayerARPG.MMO
{
    using LipingShare.LCLib.Asn1Processor;

    /// <summary>
    /// This class will validate the Apple receipt is signed with the correct certificate.
    /// </summary>
    public class AppleValidator
    {
        private X509Cert cert;
        private AppleReceiptParser parser = new AppleReceiptParser();

        /// <summary>
        /// Constructs an instance with Apple Certificate.
        /// </summary>
        /// <param name="appleRootCertificate">The apple certificate.</param>
        public AppleValidator(byte[] appleRootCertificate)
        {
            cert = new X509Cert(appleRootCertificate);
        }

        /// <summary>
        /// Validate that the Apple receipt is signed correctly.
        /// </summary>
        /// <param name="receiptData">The Apple receipt to validate.</param>
        /// <returns>The parsed AppleReceipt</returns>
        /// <exception cref="InvalidSignatureException">The exception thrown if the receipt is incorrectly signed.</exception>
        public AppleReceipt Validate(byte[] receiptData)
        {
            PKCS7 receipt;
            var result = parser.Parse(receiptData, out receipt);
            if (!receipt.Verify(cert, result.receiptCreationDate))
            {
                throw new InvalidSignatureException();
            }
            return result;
        }
    }

    /// <summary>
    /// This class with parse the Apple receipt data received in byte[] into a AppleReceipt object
    /// </summary>
    public class AppleReceiptParser
    {
        // Cache the AppleReceipt object, PKCS7, and raw data for the most recently parsed data.
        private static Dictionary<string, object> _mostRecentReceiptData = new Dictionary<string, object>();
        private const string k_AppleReceiptKey = "k_AppleReceiptKey";
        private const string k_PKCS7Key = "k_PKCS7Key";
        private const string k_ReceiptBytesKey = "k_ReceiptBytesKey";

        /// <summary>
        /// Parse the Apple receipt data into a AppleReceipt object
        /// </summary>
        /// <param name="receiptData">Apple receipt data</param>
        /// <returns>The converted AppleReceipt object from the Apple receipt data</returns>
        public AppleReceipt Parse(byte[] receiptData)
        {
            return Parse(receiptData, out _);
        }

        internal AppleReceipt Parse(byte[] receiptData, out PKCS7 receipt)
        {
            // Avoid Culture-sensitive parsing for the duration of this method
            CultureInfo originalCulture = PushInvariantCultureOnThread();

            try
            {
                // Check to see if this receipt has been parsed before.
                // If so, return the most recent AppleReceipt and PKCS7; do not parse it again.
                if (_mostRecentReceiptData.ContainsKey(k_AppleReceiptKey) &&
                    _mostRecentReceiptData.ContainsKey(k_PKCS7Key) &&
                    _mostRecentReceiptData.ContainsKey(k_ReceiptBytesKey) &&
                    ArrayEquals<byte>(receiptData, (byte[])_mostRecentReceiptData[k_ReceiptBytesKey]))
                {
                    receipt = (PKCS7)_mostRecentReceiptData[k_PKCS7Key];
                    return (AppleReceipt)_mostRecentReceiptData[k_AppleReceiptKey];
                }

                using (var stm = new System.IO.MemoryStream(receiptData))
                {
                    Asn1Parser parser = new Asn1Parser();
                    parser.LoadData(stm);
                    receipt = new PKCS7(parser.RootNode);
                    var result = ParseReceipt(receipt.data);

                    // Cache the receipt info
                    _mostRecentReceiptData[k_AppleReceiptKey] = result;
                    _mostRecentReceiptData[k_PKCS7Key] = receipt;
                    _mostRecentReceiptData[k_ReceiptBytesKey] = receiptData;
                    return result;
                }
            }
            finally
            {
                PopCultureOffThread(originalCulture);
            }
        }

        /// <summary>
        /// Use InvariantCulture on this thread to avoid provoking Culture-sensitive reactions.
        /// E.g. when using DateTime.Parse we might load the host's current Culture, and that may
        /// have been stripped, and so this non-default culture would cause a crash.
        /// (*) NOTE Culture stripping for IL2CPP will be reduced in future Unitys in 2021
        /// (unity/il2cpp@5d3712f).
        /// </summary>
        /// <returns></returns>
        private static CultureInfo PushInvariantCultureOnThread()
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            return originalCulture;
        }

        /// <summary>
        /// Restores the original culture to this thread.
        /// </summary>
        /// <param name="originalCulture"></param>
        private static void PopCultureOffThread(CultureInfo originalCulture)
        {
            // Undo our parser Culture-preparations, safely
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }

        private AppleReceipt ParseReceipt(Asn1Node data)
        {
            if (data == null || data.ChildNodeCount != 1)
            {
                throw new InvalidPKCS7Data();
            }

            Asn1Node set = GetSetNode(data);

            var result = new AppleReceipt();
            var inApps = new List<AppleInAppPurchaseReceipt>();

            for (int t = 0; t < set.ChildNodeCount; t++)
            {
                var node = set.GetChildNode(t);
                // Each node should contain three children.

                if (node.ChildNodeCount == 3)
                {
                    var type = Asn1Util.BytesToLong(node.GetChildNode(0).Data);
                    var value = node.GetChildNode(2);
                    // See https://developer.apple.com/library/ios/releasenotes/General/ValidateAppStoreReceipt/Chapters/ReceiptFields.html#//apple_ref/doc/uid/TP40010573-CH106-SW1
                    switch (type)
                    {
                        case 2:
                            result.bundleID = Encoding.UTF8.GetString(value.GetChildNode(0).Data);
                            break;
                        case 3:
                            result.appVersion = Encoding.UTF8.GetString(value.GetChildNode(0).Data);
                            break;
                        case 4:
                            result.opaque = value.Data;
                            break;
                        case 5:
                            result.hash = value.Data;
                            break;
                        case 12:
                            var dateString = Encoding.UTF8.GetString(value.GetChildNode(0).Data);
                            result.receiptCreationDate = DateTime.Parse(dateString).ToUniversalTime();
                            break;
                        case 17:
                            inApps.Add(ParseInAppReceipt(value.GetChildNode(0)));
                            break;
                        case 19:
                            result.originalApplicationVersion = Encoding.UTF8.GetString(value.GetChildNode(0).Data);
                            break;
                    }
                }
            }

            result.inAppPurchaseReceipts = inApps.ToArray();
            return result;
        }

        private Asn1Node GetSetNode(Asn1Node data)
        {
            if (data.IsIndefiniteLength && data.ChildNodeCount == 1)
            {
                // Explanation: Receipts received from the iOS StoreKit Testing encodes the receipt data one layer deeper than expected.
                // It also has nodes with "Indeterminate" or "Undefined" length, including the node in question.
                // Failing to go one node deeper will result in an unparsed receipt.
                var intermediateNode = data.GetChildNode(0);
                return intermediateNode.GetChildNode(0);
            }
            else
            {
                return data.GetChildNode(0);
            }
        }

        private AppleInAppPurchaseReceipt ParseInAppReceipt(Asn1Node inApp)
        {
            var result = new AppleInAppPurchaseReceipt();
            for (int t = 0; t < inApp.ChildNodeCount; t++)
            {
                var node = inApp.GetChildNode(t);
                if (node.ChildNodeCount == 3)
                {
                    var type = Asn1Util.BytesToLong(node.GetChildNode(0).Data);
                    var value = node.GetChildNode(2);
                    switch (type)
                    {
                        case 1701:
                            result.quantity = (int)Asn1Util.BytesToLong(value.GetChildNode(0).Data);
                            break;
                        case 1702:
                            result.productID = Encoding.UTF8.GetString(value.GetChildNode(0).Data);
                            break;
                        case 1703:
                            result.transactionID = Encoding.UTF8.GetString(value.GetChildNode(0).Data);
                            break;
                        case 1705:
                            result.originalTransactionIdentifier = Encoding.UTF8.GetString(value.GetChildNode(0).Data);
                            break;
                        case 1704:
                            result.purchaseDate = TryParseDateTimeNode(value);
                            break;
                        case 1706:
                            result.originalPurchaseDate = TryParseDateTimeNode(value);
                            break;
                        case 1708:
                            result.subscriptionExpirationDate = TryParseDateTimeNode(value);
                            break;
                        case 1712:
                            result.cancellationDate = TryParseDateTimeNode(value);
                            break;

                        case 1707:
                            // looks like possibly a type?
                            result.productType = (int)Asn1Util.BytesToLong(value.GetChildNode(0).Data);
                            break;

                        case 1713:
                            // looks like possibly is_trial?
                            result.isFreeTrial = (int)Asn1Util.BytesToLong(value.GetChildNode(0).Data);
                            break;

                        case 1719:
                            result.isIntroductoryPricePeriod = (int)Asn1Util.BytesToLong(value.GetChildNode(0).Data);
                            break;

                        default:
                            break;


                    }

                }
            }
            return result;
        }

        /// <summary>
        /// Try and parse a DateTime, returning the minimum DateTime on failure.
        /// </summary>
        private static DateTime TryParseDateTimeNode(Asn1Node node)
        {
            var dateString = Encoding.UTF8.GetString(node.GetChildNode(0).Data);
            if (!string.IsNullOrEmpty(dateString))
            {
                return DateTime.Parse(dateString).ToUniversalTime();
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Indicates whether both arrays are the same or contains the same information.
        ///
        /// This method is used to validate if the receipts are different.
        /// </summary>
        /// <param name="a">First object to validate against second object.</param>
        /// <param name="b">Second object to validate against first object.</param>
        /// <typeparam name="T">Type of object to check.</typeparam>
        /// <returns>Returns true if they are the same length and contain the same information or else returns false.</returns>
        public static bool ArrayEquals<T>(T[] a, T[] b) where T : IEquatable<T>
        {
            if (a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; i++)
            {
                if (!a[i].Equals(b[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
