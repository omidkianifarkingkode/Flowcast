using FluentValidation;

namespace Shop.Application.Features.Commands;

public class CreatePurchaseCommandValidator : AbstractValidator<CreatePurchaseCommand>
{
    public CreatePurchaseCommandValidator()
    {
        RuleFor(x => x.Receipt)
           .NotEmpty()
           .WithErrorCode("INVALID_ARGUMENT");

        RuleFor(x => x.Receipt)
            .MaximumLength(10_000)
            .WithErrorCode("PAYLOAD_TOO_LARGE");
    }
}
