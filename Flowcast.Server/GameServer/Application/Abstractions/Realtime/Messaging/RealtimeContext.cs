using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.Realtime.Messaging;

public sealed class RealtimeContext
{
    public Guid UserId { get; init; }
    public MessageHeader Header { get; init; }
}
