using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityRestClient;

namespace MultiplayerARPG.MMO
{
    public class RESTIAPReceiptValidator : MonoBehaviour, IIAPReceiptValidator
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
            RestClient.Result result = await RestClient.Post(RestClient.GetUrl(apiUrl, "/internal/iap-validate"), form, secretKey);
            return new IAPReceiptValidateResult()
            {
                IsSuccess = !result.IsError(),
                ChangeCash = cashPackage.CashAmount,
            };
        }
    }
}