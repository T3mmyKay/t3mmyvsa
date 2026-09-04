using Carter;
using FluentValidation;
using NetArchTest.Rules;
using Shouldly;

namespace T3mmyvsa.ArchitectureTests;

public sealed class FeatureArchitectureTests
{
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(Program).Assembly;

    [Fact]
    public void CarterModules_ShouldUseEndpointSuffix()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICarterModule))
            .Should()
            .HaveNameEndingWith("Endpoint")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Carter modules must use the Endpoint suffix. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Endpoints_ShouldNotDependDirectlyOnEntityFrameworkCore()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICarterModule))
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Endpoints must delegate through mediator/handlers instead of accessing EF Core directly. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void FluentValidators_ShouldBeSealed()
    {
        var validators = ApplicationAssembly.GetTypes()
            .Where(type => !type.IsAbstract && InheritsFromAbstractValidator(type))
            .ToArray();

        validators.ShouldNotBeEmpty();
        validators.Where(type => !type.IsSealed)
            .Select(type => type.FullName ?? type.Name)
            .ShouldBeEmpty("FluentValidation validators should be sealed to keep slice contracts explicit.");
    }

    private static bool InheritsFromAbstractValidator(Type type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
            {
                return true;
            }
        }

        return false;
    }
}
