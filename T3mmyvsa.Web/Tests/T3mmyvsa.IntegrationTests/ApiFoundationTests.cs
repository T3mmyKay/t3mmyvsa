using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using T3mmyvsa.IntegrationTests.Infrastructure;

namespace T3mmyvsa.IntegrationTests;

public sealed class ApiFoundationTests(T3mmyvsaWebApplicationFactory factory)
    : IClassFixture<T3mmyvsaWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false
    });

    [Fact]
    public async Task Liveness_ShouldBeHealthy()
    {
        var response = await _client.GetAsync("/health/live", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Readiness_ShouldReachRealPostgreSql()
    {
        var response = await _client.GetAsync("/health/ready", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProtectedEndpoint_ShouldRejectAnonymousCaller()
    {
        var response = await _client.GetAsync("/api/v1/users", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task InvalidLoginPayload_ShouldReturnValidationProblem()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { Email = "not-an-email", Password = "" },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
    }
}
