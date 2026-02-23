using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shop.Infrastructure.Options
{
    public sealed class ShopOptionsValidator : IValidateOptions<ShopOptions>
    {
        public ValidateOptionsResult Validate(string? name, ShopOptions options)
        {
            if (options is null) return ValidateOptionsResult.Fail("Shop options not found");
            return ValidateOptionsResult.Success;
        }
    }
}
