using Xunit;

using Template.Web;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

using Template.Web.Controllers;

namespace Template.Test;

/**
 * As the Web application Program.cs file uses top level statements by default
 * we need to convert it to using a traditional class with Main method, so that
 * WebApplicationFactory can locate the application entry point 'Program'
 */

[Collection("Sequential")]
public class HomeControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

        public HomeControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task HomeContoller_IndexAction_ReturnsHtmlWithExpectedElement()
        {
            // Act
            var response = await _client.GetAsync("/");
           
            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
 
            // verify navbar contains the following string
            Assert.Contains("Login", content);
        }
}