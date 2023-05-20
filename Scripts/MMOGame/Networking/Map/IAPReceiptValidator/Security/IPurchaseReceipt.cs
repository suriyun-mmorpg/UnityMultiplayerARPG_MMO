using System;

namespace MultiplayerARPG.MMO
{
    /// <summary>
    /// Represents a parsed purchase receipt from a store.
    /// </summary>
    public interface IPurchaseReceipt
    {
        /// <summary>
        /// The ID of the transaction.
        /// </summary>
        string transactionID { get; }

        /// <summary>
        /// The ID of the product purchased.
        /// </summary>
        string productID { get; }

        /// <summary>
        /// The date of the purchase.
        /// </summary>
        DateTime purchaseDate { get; }
    }
}
