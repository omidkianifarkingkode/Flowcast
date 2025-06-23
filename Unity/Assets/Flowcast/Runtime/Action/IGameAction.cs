using Flowcast.Inputs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flowcast.Action
{
    public interface IGameAction
    {
        long Id { get; }
        int PlayerId { get; }
        ulong Frame { get; } // Or Tick, Turn, Timestamp depending on game
    }

    // must register via DI container for every input
    public interface IGameActionProcessor<TGameAction> where TGameAction : IGameAction
    {
        bool Process(IGameAction action);
    }

    // two implementaions: 1. work with di continaner. 2. map (gameAction, processor) via reflection caching
    public interface IGameActionFactory
    {
        public IGameActionProcessor<TGameAction> GetProcessor<TGameAction>() where TGameAction : IGameAction;
    }

    // queue gameactions from INetwork:OnRecievedAction
    // check orders
    // process actions
    public interface IGameActionManager 
    {

    }
}