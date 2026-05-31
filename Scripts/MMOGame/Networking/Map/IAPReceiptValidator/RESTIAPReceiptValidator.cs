using Insthync.UnityRestClient;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public class RESTIAPReceiptValidator : RestClient, IIAPReceiptValidator
    {
        public string apiUrl = "http://localhost:9802";
        public string secretKey = "secret";

        public async UniTask<IAPReceiptValidateResult> ValidateIAPReceipt(RequestCashPackageBuyValidationMessage request, string userId, string characterId)
        {
            Dictionary<string, object> form = new Dictionary<string, object>
            {
                { "userId", userId },
                { "characterId", characterId },
                { "platform", request.platform.ToString() },
                { "transactionID", request.transactionID },
                { "receipt", request.receipt },
                { "appleJwsRepresentation", request.appleJwsRepresentation },
            };
            Result result = await Post(GetUrl(apiUrl, "/internal/iap-validate"), form, secretKey, ApiKeyAuthHeaderSettings);
            int totalCashAmount = 0;
            foreach (var item in request.items)
            {
                if (!GameInstance.CashPackages.TryGetValue(item.dataId, out CashPackage cashPackage))
                    continue;
                Logging.LogError($"Unable to get cash package {item.dataId}, (quantity: {item.quantity}) while IAP validation");
                totalCashAmount += cashPackage.CashAmount * item.quantity;
            }
            return new IAPReceiptValidateResult()
            {
                IsSuccess = !result.IsError(),
                ChangeCash = totalCashAmount,
            };
        }
    }
}