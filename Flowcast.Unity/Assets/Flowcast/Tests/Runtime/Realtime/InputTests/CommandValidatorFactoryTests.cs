using Flowcast.Commands;
using Flowcast.Tests.Runtime.Commons.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Flowcast.Tests.Runtime.CommandTests
{
    [TestFixture]
    public class CommandValidatorFactoryTests
    {
        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void Map_ShouldResolveCorrectValidatorType()
        {
            var factory = new CommandValidatorFactory(builder =>
            {
                builder.MapManual<SpawnCommand, SpawnCommandValidator>();
            });

            var validator = factory.GetValidator<SpawnCommand>();
            Assert.IsInstanceOf<SpawnCommandValidator>(validator);
        }

        [Test]
        public void MapFactory_ShouldCreateNewInstanceEachTime()
        {
            int createdCount = 0;

            var factory = new CommandValidatorFactory(builder =>
            {
                builder.MapLazy(() =>
                {
                    createdCount++;
                    return new SpawnCommandValidator();
                });
            });

            var v1 = factory.GetValidator<SpawnCommand>();
            var v2 = factory.GetValidator<SpawnCommand>();

            Assert.IsInstanceOf<SpawnCommandValidator>(v1);
            Assert.IsInstanceOf<SpawnCommandValidator>(v2);
            Assert.AreNotSame(v1, v2);
            Assert.AreEqual(2, createdCount);
        }


        [Test]
        public void AutoMap_ShouldDiscoverAndResolveValidator()
        {
            var factory = new CommandValidatorFactory(builder =>
            {
                builder.AutoMap(GetType().Assembly); // current assembly
            });

            var validator = factory.GetValidator<SpawnCommand>();
            Assert.IsInstanceOf<SpawnCommandValidator>(validator);
        }

        [Test]
        public void GetValidator_ShouldThrow_WhenNoMappingExists()
        {
            var factory = new CommandValidatorFactory(builder => { });

            try
            {
                factory.GetValidator<SpawnCommand>();
                Assert.Fail("Expected KeyNotFoundException was not thrown.");
            }
            catch (Exception ex)
            {
                Assert.Pass($"Unexpected exception type: {ex.GetType().Name}");
            }
        }


        [Test]
        public void GetValidator_ByType_ShouldReturnExpectedValidator()
        {
            var factory = new CommandValidatorFactory(builder =>
            {
                builder.MapManual<SpawnCommand, SpawnCommandValidator>();
            });

            var validator = factory.GetValidator(typeof(SpawnCommand));
            Assert.IsNotNull(validator);
            Assert.IsInstanceOf<SpawnCommandValidator>(validator);
        }

        [Test]
        public void Map_WithFactoryFunction_ShouldInjectDependencyManually()
        {
            var gameWorldService = new GameWorldService();

            var factory = new CommandValidatorFactory(builder =>
            {
                builder.MapLazy(() => new MoveCommandValidator(gameWorldService));
            });

            var validator = factory.GetValidator<MoveCommand>();

            Assert.IsInstanceOf<MoveCommandValidator>(validator);

            // Optionally validate functionality
            var result = validator.Validate(new MoveCommand(1, 2));
            Assert.IsTrue(result.IsSuccess, $"Expected success, got: {result}");
        }

    }
}