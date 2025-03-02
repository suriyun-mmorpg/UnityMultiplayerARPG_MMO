using Insthync.UnityRestClient;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public class RESTIAPReceiptValidator : RestClient, IIAPReceiptValidator
    {
        public string apiUrl = "http://localhost:9802";
        public string secretKey = "secret";

        public async UniTask<IAPReceiptValidateResult> ValidateIAPReceipt(CashPackage cashPackage, string userId, string characterId, string receipt)
        {
            Dictionary<string, object> form = new Dictionary<string, object>
            {
                { "userId", userId },
                { "characterId", characterId },
                { "receipt", receipt },
                { "packageId", cashPackage.Id },
            };
            Result result = await Post(GetUrl(apiUrl, "/internal/iap-validate"), form, secretKey, ApiKeyAuthHeaderSettings);
            return new IAPReceiptValidateResult()
            {
                IsSuccess = !result.IsError(),
                ChangeCash = cashPackage.CashAmount,
            };
        }
    }
}