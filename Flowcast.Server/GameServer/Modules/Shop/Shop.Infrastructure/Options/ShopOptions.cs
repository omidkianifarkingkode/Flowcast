using System;
using System.Collections.Generic;
using System.Text;

namespace Shop.Infrastructure.Options
{
    public sealed class ShopOptions 
    {
        public const string SectionName = "Shop";

        public bool UseInMemoryDatabase { get; init; } = false;
        public string? ConnectionStrings { get; init; }
    }
}
