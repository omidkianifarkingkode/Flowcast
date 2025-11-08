using UnityEngine;

namespace Flowcast.Network
{
    public class DummerServerRunner : MonoBehaviour 
    {
        public DummyNetworkServer Server;

        public void Create(DummyNetworkServerOptions options) 
        {
            Server = new DummyNetworkServer
            {
                Options = options
            };
        }
    }
}
