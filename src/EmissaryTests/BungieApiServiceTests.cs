using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Emissary;
using Moq;
using System.Net.Http;
using Moq.Protected;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Linq;

namespace EmissaryTests
{
    [TestClass]
    public class BungieApiServiceTests
    {

        [TestMethod]
        public void GetOAuthAccessToken_ValidCode_ShouldReturnAccessTokenAndNullErrorMessage()
        {
            // arrange
            Uri uri = new Uri("https://www.bungie.net/Platform/App/OAuth/Token/");
            string authCode = "15bf7985a28208b83997b090302b36a7";  // just an old example auth code
            string clientId = "69420";
            string clientSecret = "X.69-x";
            string content = $"grant_type=authorization_code&code={authCode}&client_id={clientId}&client_secret={clientSecret}";
            string responseString = TestUtils.ReadFile("OAuth-valid.json");
            string expectedAccessCode = "CLDjARKGAgAgVJDu+K+f85W6S6eJJi+s7U9tXkxDzInlc8I78HfgcabgAAAATOBrgq37w0FGjQ6XoVCLI4Mntf9IjfT91ByO4T59755lmaJvWMdnNpm4YcKglZiJN9IT0lLuZNifSUZRtWl1Xi+m83Eoh6VBMxRaec9Feeu4Coa53XzEAVr/BadPeaugfqB8A5jgEcRdQrnSH092D1h1ntzLpm0cOUttRGqMFpw/nR9Sm0vF1i4kdrq8F9gx+PQ6fJvbBxOKYZRSnQUgr3WjSSgWGmOvAu778Ikf/0tN7dmgpX6JFHcb2U1fcvSprnbb0qcqsGB71KsSvRqgJ5T9/LswkBT9TIHbrtS/cPg=";
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Post
                        && req.RequestUri == uri
                        && req.Content.Headers.Any(h => h.Key == "X-API-KEY" && !string.IsNullOrEmpty(h.Value.FirstOrDefault()))
                        && req.Content.Headers.Any(h => h.Key == "Content-Type" && h.Value.FirstOrDefault() == "application/x-www-form-urlencoded")
                        && req.Content.ReadAsStringAsync().Result == content),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);
            BungieApiService bungieApiService = new BungieApiService(httpClient);
            OAuthRequest request = new OAuthRequest(authCode, clientId, clientSecret);
            // act
            OAuthResponse actual = bungieApiService.GetOAuthAccessToken(request);
            // assert
            Assert.AreEqual(expectedAccessCode, actual.AccessToken);
            Assert.IsTrue(string.IsNullOrWhiteSpace(actual.ErrorMessage));
        }

        [TestMethod]
        public void GetOAuthAccessToken_InvalidCode_ShouldReturnNullAccessTokenAndErrorMessage()
        {
            // arrange
            Uri uri = new Uri("https://www.bungie.net/Platform/App/OAuth/Token/");
            // just an old example auth code. let's pretend this code expired or was already used
            string authCode = "15bf7985a28208b83997b090302b36a7";  
            string clientId = "69420";
            string clientSecret = "X.69-x";
            string content = $"grant_type=authorization_code&code={authCode}&client_id={clientId}&client_secret={clientSecret}";
            string responseString = TestUtils.ReadFile("OAuth-invalid-auth-code.json");
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Post
                        && req.RequestUri == uri
                        && req.Content.Headers.Any(h => h.Key == "X-API-KEY" && !string.IsNullOrEmpty(h.Value.FirstOrDefault()))
                        && req.Content.Headers.Any(h => h.Key == "Content-Type" && h.Value.FirstOrDefault() == "application/x-www-form-urlencoded")
                        && req.Content.ReadAsStringAsync().Result == content),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);
            BungieApiService bungieApiService = new BungieApiService(httpClient);
            OAuthRequest request = new OAuthRequest(authCode, clientId, clientSecret);
            // act
            OAuthResponse actual = bungieApiService.GetOAuthAccessToken(request);
            // assert
            Assert.IsTrue(string.IsNullOrWhiteSpace(actual.AccessToken));
            Assert.AreEqual("AuthorizationCodeInvalid", actual.ErrorMessage);
        }






        // [TestMethod]
        // public void SearchDestinyPlayer_PlayerExists_ShouldReturnResponseWithProperUserInfo()
        // {
        //     int membershipType = BungieMembershipType.All;  // -1
        //     string displayName = "pimpdaddy";
        //     long expectedMembershipId = 4611686018467260757;
        //     Uri expectedUri = new Uri($"https://www.bungie.net/Platform/Destiny2/SearchDestinyPlayer/{membershipType}/{displayName}");
        //     string responseString = TestUtils.ReadFile("search-destiny-player-personal.json");

        //     Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        //     handlerMock
        //         .Protected()
        //         .Setup<Task<HttpResponseMessage>>("SendAsync", 
        //             ItExpr.Is<HttpRequestMessage>(req => 
        //                 req.Method == HttpMethod.Get 
        //                 && req.RequestUri == expectedUri), 
        //             ItExpr.IsAny<CancellationToken>())
        //         .ReturnsAsync(new HttpResponseMessage() {
        //             StatusCode = HttpStatusCode.OK, 
        //             Content = new StringContent(responseString)})
        //         .Verifiable();

        //     HttpClient httpClient = new HttpClient(handlerMock.Object);
        //     httpClient.BaseAddress = new Uri("https://www.bungie.net/Platform/Destiny2/");
        //     httpClient.DefaultRequestHeaders.Add("X-API-KEY", "fake-bungie-api-key");

        //     BungieApiService bungieApiService = new BungieApiService(httpClient);

        //     SearchDestinyPlayerResponse response = bungieApiService.SearchDestinyPlayer(membershipType, displayName);

        //     Assert.AreEqual("Ok", response.Message);
        //     Assert.AreEqual(3, response.Response.Length);
        //     Assert.IsTrue(response.Response.Any(userInfoCard => userInfoCard.DisplayName == displayName && userInfoCard.MembershipId == expectedMembershipId));
        // }

        // [TestMethod]
        // public void SearchDestinyPlayer_

        // [TestMethod]
        // public void TrySearchDestinyPlayer_PersonalAccount_ShouldReturnTrueAndProperMembershipId()
        // {
        //     Mock<IBungieApiService> mock = new Mock<IBungieApiService>();
        //     string requestString = "https://www.bungie.net/platform/Destiny2/SearchDestinyPlayer/-1/pimpdaddy/";
        //     string responseString = ReadFile("search-destiny-player-personal.json");
        //     mock.Setup(m => m.Get(requestString)).Returns(responseString);
        //     IEmissary emissary = new Emissary(mock.Object, new ManifestAccessor());
        //     long expectedMembershipId = 4611686018467260757;
        //     bool success = emissary.TrySearchDestinyPlayer("pimpdaddy", out long membershipId);
        //     Assert.IsTrue(success);
        //     Assert.AreEqual(expectedMembershipId, membershipId);
        // }

        // what about the situation SameUsernameExistsOnMultiplePlatforms ?

    }
}