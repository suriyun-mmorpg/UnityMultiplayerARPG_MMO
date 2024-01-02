using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    [System.Serializable]
    public class IAPReceiptValidateResult
    {
        public bool IsSuccess { get; set; }
        public int ChangeCash { get; set; }
    }
}