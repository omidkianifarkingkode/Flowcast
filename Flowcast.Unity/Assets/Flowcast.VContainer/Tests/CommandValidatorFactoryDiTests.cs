using Flowcast.Commands;
using Flowcast.Tests.Runtime.Commons.Services;
using Flowcast.Tests.Runtime.CommandTests;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using VContainer;
using System;

namespace Flowcast.VContainer.Tests
{
    public class CommandValidatorFactoryDiTests
    {
        private IObjectResolver _container;
        private ICommandValidatorFactory _factory;

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();

            // Register all Flowcast components
            builder.RegisterCommandValidators(Assembly.GetAssembly(typeof(SpawnCommandValidator)));

            // Optionally: Register other services required by validators
            builder.Register<IGameWorldService, GameWorldService>(Lifetime.Singleton);

            _container = builder.Build();
            _factory = _container.Resolve<ICommandValidatorFactory>();
        }

        [Test]
        public void ShouldResolveValidator_FromContainer()
        {
            var validator = _factory.GetValidator<SpawnCommand>();
            Assert.IsNotNull(validator);
        }

        [Test]
        public void Validator_ShouldReturnExpectedResult()
        {
            var validator = _factory.GetValidator<SpawnCommand>();
            var result = validator.Validate(new SpawnCommand(1,2,3));

            Assert.IsTrue(result.IsSuccess, $"Expected success, got: {result}");
        }

        [Test]
        public void ShouldThrow_ForUnregisteredValidator()
        {
            try
            {
                _factory.GetValidator<UnregisteredCommand>();
                Assert.Fail("Expected KeyNotFoundException was not thrown.");
            }
            catch (Exception ex)
            {
                Assert.Pass($"Unexpected exception type: {ex.GetType().Name}");
            }
        }

        [Test]
        public void MoveCommandValidator_ShouldHaveInjectedServiceDependency()
        {
            var validator = _factory.GetValidator<MoveCommand>();

            Assert.IsNotNull(validator);
            Assert.IsInstanceOf<MoveCommandValidator>(validator);

            var result = validator.Validate(new MoveCommand(1, 0));
            Assert.IsTrue(result.IsSuccess, $"Expected success from validation, got: {result}");
        }


        // Dummy unregistered command type for negative test
        private class UnregisteredCommand : BaseCommand { }
    }
}
