using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public interface IIAPReceiptValidator
    {
        UniTask<IAPReceiptValidateResult> ValidateIAPReceipt(RequestCashPackageBuyValidationMessage request, string userId, string characterId);
    }
}
