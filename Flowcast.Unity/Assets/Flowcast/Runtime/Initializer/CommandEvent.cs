using Flowcast.Commands;
using System;
using UnityEngine.Events;

namespace Flowcast
{
    [Serializable]
    public class CommandEvent : UnityEvent<CommandWrapper> { }

    [Serializable]
    public class CommandWrapper
    {
        public ICommand Command;

        public CommandWrapper(ICommand command)
        {
            Command = command;
        }
    }
}
