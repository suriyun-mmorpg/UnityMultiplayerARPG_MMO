using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class DefaultIAPReceiptValidator : MonoBehaviour, IIAPReceiptValidator
    {
        public async UniTask<IAPReceiptValidateResult> ValidateIAPReceipt(string userId, string characterId, string unityIAPReceipt)
        {
            await UniTask.Yield();
            // No validating, you have to implement validating by yourself by create a component which implements `IIAPReceiptValidator`
            return new IAPReceiptValidateResult()
            {
                IsSuccess = true,
            };
        }
    }
}
