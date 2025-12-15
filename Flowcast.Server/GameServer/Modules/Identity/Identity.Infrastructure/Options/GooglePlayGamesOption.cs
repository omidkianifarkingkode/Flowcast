using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Options
{
    public sealed class GooglePlayGamesOptions
    {
        public required string ClientId { get; init; }
        public required string ClientSecret { get; init; }
    }
}
