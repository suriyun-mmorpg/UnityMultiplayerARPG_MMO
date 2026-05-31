using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class DefaultIAPReceiptValidator : MonoBehaviour, IIAPReceiptValidator
    {
        public UniTask<IAPReceiptValidateResult> ValidateIAPReceipt(RequestCashPackageBuyValidationMessage request, string userId, string characterId)
        {
            int totalCashAmount = 0;
            foreach (var item in request.items)
            {
                if (!GameInstance.CashPackages.TryGetValue(item.dataId, out CashPackage cashPackage))
                    continue;
                Logging.LogError($"Unable to get cash package {item.dataId}, (quantity: {item.quantity}) while IAP validation");
                totalCashAmount += cashPackage.CashAmount * item.quantity;
            }
            // No validating, you have to implement validating by yourself by create a component which implements `IIAPReceiptValidator`
            return UniTask.FromResult(new IAPReceiptValidateResult()
            {
                IsSuccess = true,
                ChangeCash = totalCashAmount,
            });
        }
    }
}
