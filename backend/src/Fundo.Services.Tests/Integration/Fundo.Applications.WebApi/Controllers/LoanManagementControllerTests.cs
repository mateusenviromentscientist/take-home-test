using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Fundo.Services.Tests.Integration
{
    public class LoanManagementControllerTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public LoanManagementControllerTests(CustomWebApplicationFactory factory)
        {
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
            // Act
            var response = await _client.GetAsync("/loans");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetLoanById_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/loans/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateLoan_ShouldReturnCreated()
        {
            // Arrange
            var json = "{ \"amount\": 100, \"applicationName\": \"Mateus\", \"currentBalance\": 100 }";

            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/loans", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task LoanPayment_ShouldReturnCreated()
        {
            // Arrange
            var json = "{ \"id\": 1, \"amount\": 50 }";


            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/loans/1/payment", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task GetAllLoans_WithoutAuthorization_ShouldReturn401()
        {
            // Arrange
            var client = new WebApplicationFactory<Fundo.Applications.WebApi.Startup>()
                .CreateClient();

            // Act
            var response = await client.GetAsync("/loans");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
