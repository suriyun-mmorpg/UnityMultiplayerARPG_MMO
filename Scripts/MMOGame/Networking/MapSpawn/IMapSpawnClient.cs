using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial interface IMapSpawnClient
    {
        /// <summary>
        /// Connect to map-spawn server
        /// </summary>
        /// <returns></returns>
        UniTask<bool> Connect();

        /// <summary>
        /// Marks this Game Server as ready to receive connections.
        /// </summary>
        /// <returns>
        UniTask<bool> Ready();

        /// <summary>
        /// Marks this Game Server as ready to shutdown.
        /// </summary>
        /// <returns>
        UniTask<bool> Shutdown();

        /// <summary>
        /// Marks this Game Server as Allocated.
        /// </summary>
        /// <returns></returns>
        UniTask<bool> Allocate();

    }
}
