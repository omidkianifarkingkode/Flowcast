using Flowcast.Inputs;
using Flowcast.Tests.Runtime.Commons.Services;
using NUnit.Framework;
using System.Collections.Generic;

namespace Flowcast.Tests.Runtime.InputTests
{
    [TestFixture]
    public class InputValidatorFactoryTests
    {
        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void Map_ShouldResolveCorrectValidatorType()
        {
            var factory = new InputValidatorFactory(builder =>
            {
                builder.Map<SpawnInput, SpawnInputValidator>();
            });

            var validator = factory.GetValidator<SpawnInput>();
            Assert.IsInstanceOf<SpawnInputValidator>(validator);
        }

        [Test]
        public void MapFactory_ShouldCreateNewInstanceEachTime()
        {
            int createdCount = 0;

            var factory = new InputValidatorFactory(builder =>
            {
                builder.Map(() =>
                {
                    createdCount++;
                    return new SpawnInputValidator();
                });
            });

            var v1 = factory.GetValidator<SpawnInput>();
            var v2 = factory.GetValidator<SpawnInput>();

            Assert.IsInstanceOf<SpawnInputValidator>(v1);
            Assert.IsInstanceOf<SpawnInputValidator>(v2);
            Assert.AreNotSame(v1, v2);
            Assert.AreEqual(2, createdCount);
        }


        [Test]
        public void AutoMap_ShouldDiscoverAndResolveValidator()
        {
            var factory = new InputValidatorFactory(builder =>
            {
                builder.AutoMap(GetType().Assembly); // current assembly
            });

            var validator = factory.GetValidator<SpawnInput>();
            Assert.IsInstanceOf<SpawnInputValidator>(validator);
        }

        [Test]
        public void GetValidator_ShouldThrow_WhenNoMappingExists()
        {
            var factory = new InputValidatorFactory(builder => { });

            Assert.Throws<KeyNotFoundException>(() =>
            {
                factory.GetValidator<SpawnInput>();
            });
        }

        [Test]
        public void GetValidator_ByType_ShouldReturnExpectedValidator()
        {
            var factory = new InputValidatorFactory(builder =>
            {
                builder.Map<SpawnInput, SpawnInputValidator>();
            });

            var validator = factory.GetValidator(typeof(SpawnInput));
            Assert.IsNotNull(validator);
            Assert.IsInstanceOf<SpawnInputValidator>(validator);
        }

        [Test]
        public void Map_WithFactoryFunction_ShouldInjectDependencyManually()
        {
            var gameWorldService = new GameWorldService();

            var factory = new InputValidatorFactory(builder =>
            {
                builder.Map(() => new MoveInputValidator(gameWorldService));
            });

            var validator = factory.GetValidator<MoveInput>();

            Assert.IsInstanceOf<MoveInputValidator>(validator);

            // Optionally validate functionality
            var result = validator.Validate(new MoveInput(1, 2));
            Assert.IsTrue(result.IsSuccess, $"Expected success, got: {result}");
        }

    }
}