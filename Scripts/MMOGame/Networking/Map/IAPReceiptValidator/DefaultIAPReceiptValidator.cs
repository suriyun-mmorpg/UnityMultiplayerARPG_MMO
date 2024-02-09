using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class DefaultIAPReceiptValidator : MonoBehaviour, IIAPReceiptValidator
    {
        public UniTask<IAPReceiptValidateResult> ValidateIAPReceipt(CashPackage cashPackage, string userId, string characterId, string unityIAPReceipt)
        {
            // No validating, you have to implement validating by yourself by create a component which implements `IIAPReceiptValidator`
            return UniTask.FromResult(new IAPReceiptValidateResult()
            {
                IsSuccess = true,
                ChangeCash = cashPackage.CashAmount,
            });
        }
    }
}
