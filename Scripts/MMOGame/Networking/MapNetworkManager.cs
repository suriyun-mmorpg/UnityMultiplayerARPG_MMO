using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Insthync.MMOG
{
    public class MapNetworkManager : BaseAppServerNetworkManager
    {
        public override CentralServerPeerType PeerType { get { return CentralServerPeerType.MapServer; } }

        // This server will connect to central server to receive following data:
        // Chat server address, Database server configuration
        protected override void RegisterServerMessages()
        {
            base.RegisterServerMessages();
        }

        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();
        }

        public override string GetExtra()
        {
            return SceneManager.GetActiveScene().name;
        }
    }
}
