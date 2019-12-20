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
        public void SearchDestinyPlayer_PlayerExists_ShouldReturnResponseWithProperUserInfo()
        {
            int membershipType = BungieMembershipType.All;  // -1
            string displayName = "pimpdaddy";
            long expectedMembershipId = 4611686018467260757;
            Uri expectedUri = new Uri($"https://www.bungie.net/Platform/Destiny2/SearchDestinyPlayer/{membershipType}/{displayName}");
            string responseString = TestUtils.ReadFile("search-destiny-player-personal.json");

            Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", 
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.Method == HttpMethod.Get 
                        && req.RequestUri == expectedUri), 
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage() {
                    StatusCode = HttpStatusCode.OK, 
                    Content = new StringContent(responseString)})
                .Verifiable();

            HttpClient httpClient = new HttpClient(handlerMock.Object);
            httpClient.BaseAddress = new Uri("https://www.bungie.net/Platform/Destiny2/");
            httpClient.DefaultRequestHeaders.Add("X-API-KEY", "fake-bungie-api-key");

            BungieApiService bungieApiService = new BungieApiService(httpClient);

            SearchDestinyPlayerResponse response = bungieApiService.SearchDestinyPlayer(membershipType, displayName);

            Assert.AreEqual("Ok", response.Message);
            Assert.AreEqual(3, response.Response.Length);
            Assert.IsTrue(response.Response.Any(userInfoCard => userInfoCard.DisplayName == displayName && userInfoCard.MembershipId == expectedMembershipId));
        }

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