using Flowcast.Commons;

namespace Flowcast.Inputs
{
    public interface IInputValidator 
    {
        Result Validate(IInput input);
    }

    public interface IInputValidator<GenericInput> : IInputValidator where GenericInput : IInput
    {
        Result Validate(GenericInput input);
    }
}

