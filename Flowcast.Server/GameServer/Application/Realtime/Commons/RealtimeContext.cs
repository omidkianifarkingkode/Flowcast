using Application.Realtime.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Realtime.Commons;

public sealed class RealtimeContext
{
    public Guid UserId { get; init; }
    public MessageHeader Header { get; init; }
}
