using Shouldly;
using T3mmyvsa.Features.Auth.Login;

namespace T3mmyvsa.UnitTests;

public sealed class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void InvalidCredentialsShape_ShouldFailValidation()
    {
        var result = _validator.Validate(new LoginCommand("not-an-email", string.Empty));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(x => x.PropertyName).ShouldContain(nameof(LoginCommand.Email));
        result.Errors.Select(x => x.PropertyName).ShouldContain(nameof(LoginCommand.Password));
    }

    [Fact]
    public void ValidCredentialsShape_ShouldPassTransportValidation()
    {
        var result = _validator.Validate(new LoginCommand("dev@example.com", "correct-shape"));

        result.IsValid.ShouldBeTrue();
    }
}
