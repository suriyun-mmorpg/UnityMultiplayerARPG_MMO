
namespace MultiplayerARPG.MMO
{
    public interface IAppServer
    {
        string CentralNetworkAddress { get; }
        int CentralNetworkPort { get; }
        string AppAddress { get; }
        int AppPort { get; }
        string AppExtra { get; }
        CentralServerPeerType PeerType { get; }
    }
}
