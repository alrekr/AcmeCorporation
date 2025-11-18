
using System.Net;
using System.Net.Http.Json;
using AcmeCorporation.Configuration;
using AcmeCorporation.Library; 
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using NUnit.Framework; // NUnit
using Shouldly;

namespace AcmeCorporation.Tests;

[TestFixture]
public class SerialNumberApiTests
{
    private WebApplicationFactory<Program> _factory;
    private ISerialNumberGenerator _mockGenerator;

    [SetUp]
    public void SetUp()
    {
        _mockGenerator = Substitute.For<ISerialNumberGenerator>();
        _mockGenerator.CreateSerialNumber().Returns("MOCK-SN-123");

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.Configure<SerialNumberApiOptions>(opts =>
                    {
                        opts.MaxBatchSize = 10;
                    });

                    services.RemoveAll<ISerialNumberGenerator>();
                    services.AddSingleton(_mockGenerator);
                });

                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<ISerialNumberGenerator>();
                    services.AddSingleton(_mockGenerator);
                });
            });
    }

    [TearDown]
    public void TearDown()
    {
        _factory.Dispose();
    }

    [Test]
    public async Task GetSerialNumbers_ShouldReturn200_WhenRequestIsValid()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        const int requestedCount = 5;

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/serialnumbers?n={requestedCount}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        List<string>? result = await response.Content.ReadFromJsonAsync<List<string>>();
        result.ShouldNotBeNull();
        result.Count.ShouldBe(requestedCount);

        _mockGenerator.Received(requestedCount).CreateSerialNumber();
    }

    [Test]
    public async Task GetSerialNumbers_ShouldReturn400_WhenLimitExceeded()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        const int requestedCount = 11;

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/serialnumbers?n={requestedCount}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        string errorMsg = await response.Content.ReadAsStringAsync();
        errorMsg.ShouldContain("You cannot generate more than 10 serial numbers");

        _mockGenerator.DidNotReceive().CreateSerialNumber();
    }

    [Test]
    public async Task GetSerialNumbers_ShouldReturn400_WhenCountIsNegative()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/v1/serialnumbers?n=-5");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        _mockGenerator.DidNotReceive().CreateSerialNumber();
    }

    [Test]
    public async Task GetSerialNumbers_ShouldDefaultToOne_WhenParamIsMissing()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/v1/serialnumbers");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        List<string>? result = await response.Content.ReadFromJsonAsync<List<string>>();

        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        _mockGenerator.Received(1).CreateSerialNumber();
    }
}