using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fundo.Services.Tests.Integration
{
    public class LoanManagementControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public LoanManagementControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;

            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Test", "token");
        }

        [Fact]
        public async Task GetAllLoans_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/loans");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateLoan_ShouldReturnOk_AndTrue()
        {
            var json = "{ \"amount\": 100, \"applicationName\": \"Mateus\", \"currentBalance\": 100 }";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/loans", content);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"Expected 200, got {(int)response.StatusCode} {response.StatusCode}\nBody:\n{body}");
            }

            var bodyText = await response.Content.ReadAsStringAsync();
            Assert.Equal("true", bodyText.Trim().ToLowerInvariant());
        }

        [Fact]
        public async Task GetLoanById_WhenDoesNotExist_ShouldReturnNotFound()
        {

            var response = await _client.GetAsync("/loans/999999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task LoanPayment_WhenDoesNotExist_ShouldReturnNotFound()
        {
            var json = "{ \"amount\": 50 }";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/loans/999999/payment", content);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAllLoans_WithoutAuthorization_ShouldReturn401()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var response = await client.GetAsync("/loans");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
