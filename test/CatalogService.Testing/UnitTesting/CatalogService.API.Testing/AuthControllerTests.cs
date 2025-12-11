using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CatalogService.API.Controllers.v1;
using CatalogService.Transversal.Classes.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CatalogService.Testing.UnitTesting.CatalogService.API.Testing
{
    public class AuthControllerTests
    {
        private static IHttpClientFactory CreateHttpClientFactory(HttpMessageHandler handler)
        {
            var client = new HttpClient(handler);
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
            return factory.Object;
        }

        private class CapturingHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
            public HttpRequestMessage? LastRequest { get; private set; }

            public CapturingHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
            {
                _responder = responder;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                return Task.FromResult(_responder(request));
            }
         }

         /*[Fact]
         public async Task GetToken_ReturnsJsonContent_OnSuccess()
         {
             // Arrange
             var expectedJson = "{\"access_token\":\"abc\",\"token_type\":\"Bearer\"}";
             var handler = new CapturingHandler(_ =>
             new HttpResponseMessage(HttpStatusCode.OK)
             {
             Content = new StringContent(expectedJson)
             });

             var factory = CreateHttpClientFactory(handler);
             var controller = new AuthController(factory);

             var login = new LoginRequest { Username = "user", Password = "pass" };

             // Act
             var result = await controller.GetToken(login);

             // Assert
             var contentResult = Assert.IsType<ContentResult>(result);
             Assert.Equal("application/json", contentResult.ContentType);
             Assert.Equal(expectedJson, contentResult.Content);

             // Verify request
             Assert.NotNull(handler.LastRequest);
             Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
             Assert.Equal("https://localhost:5001/connect/token", handler.LastRequest!.RequestUri!.ToString());

             // Verify form content
             var formContent = Assert.IsType<FormUrlEncodedContent>(handler.LastRequest!.Content);
             var pairs = await formContent.ReadAsStringAsync();
             Assert.Contains("grant_type=password", pairs);
             Assert.Contains("client_id=catalog-api-client", pairs);
             Assert.Contains("client_secret=catalog-secret", pairs);
             Assert.Contains("username=user", pairs);
             Assert.Contains("password=pass", pairs);
             Assert.Contains("scope=openid%20profile%20role%20manager.read%20customer.read%20manager.create%20manager.update%20manager.delete%20offline_access", pairs);
         }*/

         [Fact]
         public async Task GetToken_PropagatesStatusCode_OnFailure()
         {
             // Arrange
             var errorJson = "{\"error\":\"invalid_grant\"}";
             var handler = new CapturingHandler(_ =>
             new HttpResponseMessage(HttpStatusCode.BadRequest)
             {
                Content = new StringContent(errorJson)
             });

             var factory = CreateHttpClientFactory(handler);
             var controller = new AuthController(factory);

             var login = new LoginRequest { Username = "bad", Password = "creds" };

             // Act
             var result = await controller.GetToken(login);

             // Assert
             var objectResult = Assert.IsType<ObjectResult>(result);
             Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
             Assert.Equal(errorJson, Assert.IsType<string>(objectResult.Value));
        }
     }
}
