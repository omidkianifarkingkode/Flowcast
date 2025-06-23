using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flowcast.Player
{
    public interface IFrameProvider
    {
        ulong GetCurrentFrame();
    }

    public interface IPlayerProvider
    {
        int GetLocalPlayerId();
    }
}
