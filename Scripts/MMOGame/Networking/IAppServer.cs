
namespace Insthync.MMOG
{
    public interface IAppServer
    {
        string CentralNetworkAddress { get; }
        int CentralNetworkPort { get; }
        string CentralConnectKey { get; }
        string AppAddress { get; }
        int AppPort { get; }
        string AppConnectKey { get; }
        string AppExtra { get; }
        CentralServerPeerType PeerType { get; }
    }
}
