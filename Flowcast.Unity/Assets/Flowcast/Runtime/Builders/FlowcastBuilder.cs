﻿using Flowcast.Builders;

namespace Flowcast
{
    public static class FlowcastBuilder 
    {
        public static IRequireMatchInfo CreateLockstep()
        {
            return new LockstepBuilder();
        }

        //public static IRequirePlayers CreateTurnBased()
        //{
        //    return new TurnBasedBuilder();
        //}
    }
}
