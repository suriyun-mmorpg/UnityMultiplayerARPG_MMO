using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
#if ENABLE_PURCHASING && UNITY_PURCHASING
using UnityEngine.Purchasing.Security;
#endif
using Validator = MultiplayerARPG.MMO.CrossPlatformValidator;

namespace MultiplayerARPG.MMO
{
    public class DefaultIAPReceiptValidator : MonoBehaviour, IIAPReceiptValidator
    {
        public async UniTask<IAPReceiptValidateResult> ValidateIAPReceipt(string unityIAPReceipt)
        {
            await UniTask.Yield();
#if ENABLE_PURCHASING && UNITY_PURCHASING
            List<CashPackage> cashPackages = new List<CashPackage>();
            // NOTE: If error occuring and it lead you here, you must learn and use IAP obfuscating dialog (https://docs.unity3d.com/Manual/UnityIAPValidatingReceipts.html)
            Validator validator = new Validator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
            try
            {
                // On Google Play, result has a single product ID.
                // On Apple stores, receipts contain multiple products.
                var validateResults = validator.Validate(unityIAPReceipt);
                // TODO: May store receipt to database
                for (int i = 0; i < validateResults.Length; ++i)
                {
                    if (GameInstance.CashPackages.TryGetValue(BaseGameData.MakeDataId(validateResults[i].productID), out CashPackage cashPackage))
                    {
                        cashPackages.Add(cashPackage);
                    }
                }
                return new IAPReceiptValidateResult()
                {
                    IsSuccess = true,
                    CashPackages = cashPackages,
                };
            }
            catch (System.Exception ex)
            {
                Logging.LogError(nameof(DefaultIAPReceiptValidator), ex.Message);
                return new IAPReceiptValidateResult()
                {
                    IsSuccess = false,
                };
            }
#else
            return new IAPReceiptValidateResult()
            {
                IsSuccess = false,
            };
#endif
        }
    }
}
