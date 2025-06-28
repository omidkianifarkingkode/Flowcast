using Flowcast.Commons;

namespace Flowcast.Inputs
{
    public abstract class InputValidatorBase<GenericInput> : IInputValidator<GenericInput> where GenericInput : IInput
    {
        public abstract Result Validate(GenericInput input);

        Result IInputValidator.Validate(IInput input)
        {
            if (input is GenericInput typed)
            {
                return Validate(typed);
            }

            return Result.Failure("Invalid input type.");
        }
    }
}

