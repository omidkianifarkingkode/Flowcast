﻿using Flowcast.Lockstep;
using Flowcast.Synchronization;
using System;

namespace Flowcast.Data
{
    public class MatchInfo
    {
        public string MatchId { get; set; }
        public BasePlayerInfo[] Players { get; set; } = Array.Empty<BasePlayerInfo>();
        public long LocalPlayerId { get; set; }
        public string Mode { get; set; }

        public DateTime ServerStartTimeUtc { get; set; }

        public GameStateSyncOptions GameSettings { get; set; }
    }
}
