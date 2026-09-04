using Shouldly;
using T3mmyvsa.Configuration;

namespace T3mmyvsa.UnitTests;

public sealed class DatabaseProvidersTests
{
    [Theory]
    [InlineData("sqlserver", DatabaseProviders.SqlServer)]
    [InlineData("mssql", DatabaseProviders.SqlServer)]
    [InlineData("pgsql", DatabaseProviders.PostgreSql)]
    [InlineData("npgsql", DatabaseProviders.PostgreSql)]
    [InlineData("mysql", DatabaseProviders.MySql)]
    [InlineData("sqlite3", DatabaseProviders.Sqlite)]
    public void Normalize_ShouldResolveSupportedAliases(string configured, string expected)
    {
        DatabaseProviders.Normalize(configured).ShouldBe(expected);
    }

    [Fact]
    public void Normalize_ShouldRejectUnknownProvider()
    {
        Should.Throw<InvalidOperationException>(() => DatabaseProviders.Normalize("oracle"));
    }
}
