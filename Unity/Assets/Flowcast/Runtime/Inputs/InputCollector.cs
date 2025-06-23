using Flowcast.Commons;
using Flowcast.Player;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flowcast.Inputs
{
    public class InputCollector : IInputCollector
    {
        private readonly IInputValidatorFactory _validatorFactory;
        private readonly Queue<IInput> _inputQueue = new();

        private readonly IPlayerProvider _playerProvider;
        private readonly IFrameProvider _frameProvider;
        private readonly IIdGenerator _idGenerator;

        public IReadOnlyCollection<IInput> BufferedInputs => _inputQueue.ToList().AsReadOnly();

        public InputCollector(IInputValidatorFactory validatorFactory, IPlayerProvider playerProvider, IFrameProvider frameProvider, IIdGenerator idGenerator)
        {
            _validatorFactory = validatorFactory;
            _playerProvider = playerProvider;
            _frameProvider = frameProvider;
            _idGenerator = idGenerator;
        }

        public Result Collect(IInput input)
        {
            if (input == null)
                return Result.Failure("Input is null.");

            if (input is InputBase inputBase)
            {
                inputBase.Id = _idGenerator.Generate();
                inputBase.PlayerId = _playerProvider.GetLocalPlayerId();
                inputBase.Frame = _frameProvider.GetCurrentFrame();
            }

            var validator = _validatorFactory.GetValidator(input.GetType());

            if (validator != null && (validator.Validate(input) is var result && result.IsFailure))
                return result;

            _inputQueue.Enqueue(input);

            return Result.Success();
        }


        public void Clear() => _inputQueue.Clear();
    }
}

