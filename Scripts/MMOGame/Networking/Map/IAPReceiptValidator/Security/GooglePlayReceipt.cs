using System;

namespace MultiplayerARPG.MMO
{
    // See Google's reference docs.
    // http://developer.android.com/google/play/billing/billing_reference.html

    /// <summary>
    /// The state of the GooglePlay purchase.
    /// </summary>
    public enum GooglePurchaseState
    {
        /// <summary>
        /// The purchase was completed.
        /// </summary>
        Purchased = 0,

        /// <summary>
        /// The purchase was cancelled.
        /// </summary>
        Cancelled = 1,

        /// <summary>
        /// The purchase was refunded.
        /// </summary>
        Refunded = 2,

        /// <summary>
        /// The purchase was deferred.
        /// </summary>
        Deferred = 4
    }

    /// <summary>
    /// A GooglePlay purchase receipt
    /// </summary>
    public class GooglePlayReceipt : IPurchaseReceipt
    {
        /// <summary>
        /// The item's product identifier.
        /// </summary>
        public string productID { get; private set; }

        /// <summary>
        /// A unique order identifier for the transaction. This identifier corresponds to the Google payments order ID.
        /// </summary>
        public string orderID { get; private set; }

        /// <summary>
        /// The ID  of the transaction.
        /// </summary>
        public string transactionID => orderID;

        /// <summary>
        /// The package name of the app.
        /// </summary>
        public string packageName { get; private set; }

        /// <summary>
        /// A token that uniquely identifies a purchase for a given item and user pair.
        /// </summary>
        public string purchaseToken { get; private set; }

        /// <summary>
        /// The time the product was purchased, in milliseconds since the epoch (Jan 1, 1970).
        /// </summary>
        public DateTime purchaseDate { get; private set; }

        /// <summary>
        /// The purchase state of the order.
        /// </summary>
        public GooglePurchaseState purchaseState { get; private set; }

        /// <summary>
        /// Constructor that initializes the members from the input parameters.
        /// </summary>
        /// <param name="productID"> The item's product identifier. </param>
        /// <param name="orderID"> The unique order identifier for the transaction. </param>
        /// <param name="packageName"> The package name of the app. </param>
        /// <param name="purchaseToken"> The token that uniquely identifies a purchase for a given item and user pair. </param>
        /// <param name="purchaseTime"> The time the product was purchased, in milliseconds since the epoch (Jan 1, 1970). </param>
        /// <param name="purchaseState"> The purchase state of the order. </param>
        public GooglePlayReceipt(string productID, string orderID, string packageName,
            string purchaseToken, DateTime purchaseTime, GooglePurchaseState purchaseState)
        {
            this.productID = productID;
            this.orderID = orderID;
            this.packageName = packageName;
            this.purchaseToken = purchaseToken;
            this.purchaseDate = purchaseTime;
            this.purchaseState = purchaseState;
        }
    }
}
