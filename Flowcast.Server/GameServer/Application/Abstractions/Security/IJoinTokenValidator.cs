using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.Security;

public interface IJoinTokenValidator
{
    /// <summary>Return true if token is valid for this session and player, not expired, not revoked.</summary>
    bool Validate(Guid sessionId, Guid playerId, string token);
}
