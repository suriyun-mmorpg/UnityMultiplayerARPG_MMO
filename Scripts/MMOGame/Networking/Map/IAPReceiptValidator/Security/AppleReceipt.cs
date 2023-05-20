using System;

namespace MultiplayerARPG.MMO
{
    /// <summary>
    /// An Apple receipt as defined here:
    /// https://developer.apple.com/library/ios/releasenotes/General/ValidateAppStoreReceipt/Chapters/ReceiptFields.html#//apple_ref/doc/uid/TP40010573-CH106-SW1
    /// </summary>
    public class AppleReceipt
    {
        /// <summary>
        /// The app bundle ID
        /// </summary>
        public string bundleID { get; internal set; }

        /// <summary>
        /// The app version number
        /// </summary>
        public string appVersion { get; internal set; }

        /// <summary>
        /// The expiration date of the receipt
        /// </summary>
        public DateTime expirationDate { get; internal set; }

        /// <summary>
        /// An opaque value used, with other data, to compute the SHA-1 hash during validation.
        /// </summary>
        public byte[] opaque { get; internal set; }

        /// <summary>
        /// A SHA-1 hash, used to validate the receipt.
        /// </summary>
        public byte[] hash { get; internal set; }

        /// <summary>
        /// The version of the app that was originally purchased.
        /// </summary>
        public string originalApplicationVersion { get; internal set; }

        /// <summary>
        /// The date the receipt was created
        /// </summary>
        public DateTime receiptCreationDate { get; internal set; }

        /// <summary>
        /// The receipts of the In-App purchases.
        /// </summary>
        public AppleInAppPurchaseReceipt[] inAppPurchaseReceipts;
    }

    /// <summary>
    /// The details of an individual purchase.
    /// </summary>
    public class AppleInAppPurchaseReceipt : IPurchaseReceipt
    {
        /// <summary>
        /// The number of items purchased.
        /// </summary>
        public int quantity { get; internal set; }

        /// <summary>
        /// The product ID
        /// </summary>
        public string productID { get; internal set; }

        /// <summary>
        /// The ID of the transaction.
        /// </summary>
        public string transactionID { get; internal set; }

        /// <summary>
        /// For a transaction that restores a previous transaction, the transaction ID of the original transaction. Otherwise, identical to the transactionID.
        /// </summary>
        public string originalTransactionIdentifier { get; internal set; }

        /// <summary>
        /// The date of purchase.
        /// </summary>
        public DateTime purchaseDate { get; internal set; }

        /// <summary>
        /// For a transaction that restores a previous transaction, the date of the original transaction.
        /// </summary>
        public DateTime originalPurchaseDate { get; internal set; }

        /// <summary>
        /// The expiration date for the subscription, expressed as the number of milliseconds since January 1, 1970, 00:00:00 GMT.
        /// </summary>
        public DateTime subscriptionExpirationDate { get; internal set; }

        /// <summary>
        /// For a transaction that was canceled by Apple customer support, the time and date of the cancellation.
        /// For an auto-renewable subscription plan that was upgraded, the time and date of the upgrade transaction.
        /// </summary>
        public DateTime cancellationDate { get; internal set; }

        /// <summary>
        /// For a subscription, whether or not it is in the free trial period.
        /// </summary>
        public int isFreeTrial { get; internal set; }

        /// <summary>
        /// The type of product.
        /// </summary>
        public int productType { get; internal set; }

        /// <summary>
        /// For an auto-renewable subscription, whether or not it is in the introductory price period.
        /// </summary>
        public int isIntroductoryPricePeriod { get; internal set; }
    }
}
