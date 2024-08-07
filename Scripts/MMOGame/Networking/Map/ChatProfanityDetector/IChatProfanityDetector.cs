using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public interface IChatProfanityDetector
    {
        UniTask<ProfanityDetectResult> Proceed(string message);
    }
}
