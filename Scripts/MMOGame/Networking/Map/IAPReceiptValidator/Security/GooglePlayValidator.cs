using System;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    internal class GooglePlayValidator
    {
        private RSAKey key;
        public GooglePlayValidator(byte[] rsaKey)
        {
            key = new RSAKey(rsaKey);
        }

        public GooglePlayReceipt Validate(string receipt, string signature)
        {
            var rawReceipt = System.Text.Encoding.UTF8.GetBytes(receipt); // "{\"orderId\":\"G...
            var rawSignature = System.Convert.FromBase64String(signature);

            if (!key.Verify(rawReceipt, rawSignature))
            {
                throw new InvalidSignatureException();
            }

            var dic = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
            object orderID, packageName, productId, purchaseToken, purchaseTime, purchaseState;

            dic.TryGetValue("orderId", out orderID);
            dic.TryGetValue("packageName", out packageName);
            dic.TryGetValue("productId", out productId);
            dic.TryGetValue("purchaseToken", out purchaseToken);
            dic.TryGetValue("purchaseTime", out purchaseTime);
            dic.TryGetValue("purchaseState", out purchaseState);

            // Google specifies times in milliseconds since 1970.
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            // NOTE: to safely handle null values for these fields, using Convert.ToDouble & ToInt32 in place of casts
            var time = epoch.AddMilliseconds(Convert.ToDouble(purchaseTime));
            var state = (GooglePurchaseState)Convert.ToInt32(purchaseState);

            return new GooglePlayReceipt((string)productId, (string)orderID, (string)packageName,
                (string)purchaseToken, time, state);
        }
    }
}
