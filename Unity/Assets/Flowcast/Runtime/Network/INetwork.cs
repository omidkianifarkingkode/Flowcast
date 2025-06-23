using Flowcast.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flowcast.Network
{
    internal interface INetwork
    {
        event Action<object> OnRecievedAction;

        void SendInput(IInput input);
    }
}
