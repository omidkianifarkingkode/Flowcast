using Flowcast.Inputs;
using Flowcast.Tests.Runtime.Commons.Services;
using Flowcast.Tests.Runtime.InputTests;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using VContainer;

namespace Flowcast.VContainer.Tests
{
    public class InputValidatorFactoryDiTests
    {
        private IObjectResolver _container;
        private IInputValidatorFactory _factory;

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();

            // Register all Flowcast components
            builder.RegisterInputValidators(Assembly.GetAssembly(typeof(SpawnInputValidator)));

            // Optionally: Register other services required by validators
            builder.Register<IGameWorldService, GameWorldService>(Lifetime.Singleton);

            _container = builder.Build();
            _factory = _container.Resolve<IInputValidatorFactory>();
        }

        [Test]
        public void ShouldResolveValidator_FromContainer()
        {
            var validator = _factory.GetValidator<SpawnInput>();
            Assert.IsNotNull(validator);
        }

        [Test]
        public void Validator_ShouldReturnExpectedResult()
        {
            var validator = _factory.GetValidator<SpawnInput>();
            var result = validator.Validate(new SpawnInput(1,2,3));

            Assert.IsTrue(result.IsSuccess, $"Expected success, got: {result}");
        }

        [Test]
        public void ShouldThrow_ForUnregisteredValidator()
        {
            Assert.Throws<KeyNotFoundException>(() =>
            {
                _factory.GetValidator<UnregisteredInput>();
            });
        }

        [Test]
        public void MoveInputValidator_ShouldHaveInjectedServiceDependency()
        {
            var validator = _factory.GetValidator<MoveInput>();

            Assert.IsNotNull(validator);
            Assert.IsInstanceOf<MoveInputValidator>(validator);

            var result = validator.Validate(new MoveInput(1, 0));
            Assert.IsTrue(result.IsSuccess, $"Expected success from validation, got: {result}");
        }


        // Dummy unregistered input type for negative test
        private class UnregisteredInput : InputBase { }
    }
}
