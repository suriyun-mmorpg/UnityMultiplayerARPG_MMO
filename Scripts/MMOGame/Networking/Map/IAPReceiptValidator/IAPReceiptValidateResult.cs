using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    [System.Serializable]
    public class IAPReceiptValidateResult
    {
        public bool IsSuccess { get; set; }
        public List<CashPackage> CashPackages { get; set; } = new List<CashPackage>();
    }
}