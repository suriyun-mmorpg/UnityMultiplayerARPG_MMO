namespace MultiplayerARPG.MMO
{
    public interface IAppServer
    {
        string ClusterServerAddress { get; }
        int ClusterServerPort { get; }
        string AppAddress { get; }
        int AppPort { get; }
        string AppExtra { get; }
        CentralServerPeerType PeerType { get; }
    }
}
