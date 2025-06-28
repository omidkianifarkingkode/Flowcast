using System;

namespace Flowcast.Inputs
{
    public interface IInputValidatorFactory 
    {
        IInputValidator GetValidator(Type inputType);
        IInputValidator<TInput> GetValidator<TInput>() where TInput : IInput;
    }
}

