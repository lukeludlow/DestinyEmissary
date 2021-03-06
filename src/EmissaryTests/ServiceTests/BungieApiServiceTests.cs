using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmissaryCore;
using Moq;
using System.Net.Http;
using Moq.Protected;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace EmissaryTests.Service
{
    [TestClass]
    public class BungieApiServiceTests
    {

        [TestMethod]
        public void GetOAuthAccessToken_ValidCode_ShouldReturnAccessTokenAndNullErrorMessage()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m =>
                    m["Bungie:ApiKey"] == "dummy-api-key" &&
                    m["Bungie:ClientId"] == "dummy-client-id" &&
                    m["Bungie:ClientSecret"] == "dummy-client-secret");
            // arrange
            Uri uri = new Uri("https://www.bungie.net/Platform/App/OAuth/Token/");
            string authCode = "15bf7985a28208b83997b090302b36a7";  // just an old example auth code
            string content = $"grant_type=authorization_code&code={authCode}&client_id={"dummy-client-id"}&client_secret={"dummy-client-secret"}";
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
            BungieApiService bungieApiService = new BungieApiService(config, httpClient);
            OAuthRequest request = new OAuthRequest(authCode);
            // act
            OAuthResponse actual = bungieApiService.GetOAuthAccessToken(request);
            // assert
            Assert.AreEqual(expectedAccessCode, actual.AccessToken);
            Assert.IsTrue(string.IsNullOrWhiteSpace(actual.ErrorType) && string.IsNullOrWhiteSpace(actual.ErrorDescription));
        }

        [TestMethod]
        public void GetOAuthAccessToken_InvalidCode_ShouldReturnNullAccessTokenAndErrorMessage()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m =>
                    m["Bungie:ApiKey"] == "dummy-api-key" &&
                    m["Bungie:ClientId"] == "dummy-client-id" &&
                    m["Bungie:ClientSecret"] == "dummy-client-secret");
            // arrange
            Uri uri = new Uri("https://www.bungie.net/Platform/App/OAuth/Token/");
            // just an old example auth code. let's pretend this code expired or was already used
            string authCode = "15bf7985a28208b83997b090302b36a7";
            string content = $"grant_type=authorization_code&code={authCode}&client_id={"dummy-client-id"}&client_secret={"dummy-client-secret"}";
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
            BungieApiService bungieApiService = new BungieApiService(config, httpClient);
            OAuthRequest request = new OAuthRequest(authCode);
            // act
            OAuthResponse actual = bungieApiService.GetOAuthAccessToken(request);
            // assert
            Assert.IsTrue(string.IsNullOrWhiteSpace(actual.AccessToken));
            Assert.AreEqual("AuthorizationCodeInvalid", actual.ErrorDescription);
        }

        [TestMethod]
        public void GetMembershipsForUser_PersonalAccount_ShouldReturnAllDestinyMemberships()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Bungie:ApiKey"] == "dummy-api-key");
            // arrange
            Uri uri = new Uri("https://www.bungie.net/Platform/User/GetMembershipsForCurrentUser/");
            // just an old example access token
            string accessToken = "CLDjARKGAgAgVJDu+K+f85W6S6eJJi+s7U9tXkxDzInlc8I78HfgcabgAAAATOBrgq37w0FGjQ6XoVCLI4Mntf9IjfT91ByO4T59755lmaJvWMdnNpm4YcKglZiJN9IT0lLuZNifSUZRtWl1Xi+m83Eoh6VBMxRaec9Feeu4Coa53XzEAVr/BadPeaugfqB8A5jgEcRdQrnSH092D1h1ntzLpm0cOUttRGqMFpw/nR9Sm0vF1i4kdrq8F9gx+PQ6fJvbBxOKYZRSnQUgr3WjSSgWGmOvAu778Ikf/0tN7dmgpX6JFHcb2U1fcvSprnbb0qcqsGB71KsSvRqgJ5T9/LswkBT9TIHbrtS/cPg=";
            string responseString = TestUtils.ReadFile("GetMembershipsForCurrentUser-valid-personal-account.json");
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Get
                        && req.RequestUri == uri
                        && req.Headers.Any(h => h.Key == "X-API-KEY" && !string.IsNullOrEmpty(h.Value.FirstOrDefault()))
                        && req.Headers.Any(h => h.Key == "Authorization" && h.Value.FirstOrDefault() == $"Bearer {accessToken}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);
            BungieApiService bungieApiService = new BungieApiService(config, httpClient);
            UserMembershipsRequest request = new UserMembershipsRequest(accessToken);
            // act
            UserMembershipsResponse actual = bungieApiService.GetMembershipsForUser(request);
            // assert
            Assert.AreEqual(2, actual.DestinyMemberships.Count);
            Assert.AreEqual("anime8094", actual.DestinyMemberships[0].DisplayName);
            Assert.AreEqual(4611686018497175745, actual.DestinyMemberships[0].DestinyProfileId);
            Assert.AreEqual(1, actual.DestinyMemberships[0].MembershipType);
            Assert.AreEqual(3, actual.DestinyMemberships[0].CrossSaveOverride);
            Assert.AreEqual("pimpdaddy", actual.DestinyMemberships[1].DisplayName);
            Assert.AreEqual(4611686018467260757, actual.DestinyMemberships[1].DestinyProfileId);
            Assert.AreEqual(3, actual.DestinyMemberships[1].MembershipType);
            Assert.AreEqual(3, actual.DestinyMemberships[1].CrossSaveOverride);
        }

        [TestMethod]
        public void GetMembershipsForUser_InvalidAccessToken_ShouldThrowApiExceptionUnauthorized()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Bungie:ApiKey"] == "dummy-api-key");
            // arrange
            Uri uri = new Uri("https://www.bungie.net/Platform/User/GetMembershipsForCurrentUser/");
            // just an old example access token
            string accessToken = "invalid.access.token";
            string responseString = TestUtils.ReadFile("GetMembershipsForCurrentUser-invalid-access-token.html");
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Get
                        && req.RequestUri == uri
                        && req.Headers.Any(h => h.Key == "X-API-KEY" && !string.IsNullOrEmpty(h.Value.FirstOrDefault()))
                        && req.Headers.Any(h => h.Key == "Authorization" && h.Value.FirstOrDefault() == $"Bearer {accessToken}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.Unauthorized,  // 401 Unauthorized
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);
            BungieApiService bungieApiService = new BungieApiService(config, httpClient);
            UserMembershipsRequest request = new UserMembershipsRequest(accessToken);
            // act (and assert)
            var exception = Assert.ThrowsException<BungieApiException>(() => bungieApiService.GetMembershipsForUser(request));
            Assert.IsTrue(exception.Message.Contains("Unauthorized"));
        }

        [TestMethod]
        public void GetProfileCharacters_PersonalAccount_ShouldReturnAllCharacters()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Bungie:ApiKey"] == "dummy-api-key");
            int membershipType = 3;
            long destinyMembershipId = 4611686018467260757;
            Uri uri = new Uri($"https://www.bungie.net/Platform/Destiny2/{membershipType}/Profile/{destinyMembershipId}/?components=200");
            string responseString = TestUtils.ReadFile("GetProfile-valid-personal.json");
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Get
                        && req.RequestUri == uri
                        && req.Headers.Any(h => h.Key == "X-API-KEY" && !string.IsNullOrEmpty(h.Value.FirstOrDefault()))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);
            BungieApiService bungieApiService = new BungieApiService(config, httpClient);

            DestinyCharacter expectedCharacter = new DestinyCharacter();
            expectedCharacter.CharacterId = 2305843009504575107;
            expectedCharacter.DateLastPlayed = DateTimeOffset.Parse("2019-12-24T22:40:31Z");
            expectedCharacter.DestinyProfileId = 4611686018467260757;
            expectedCharacter.DestinyMembershipType = 3;

            ProfileCharactersRequest request = new ProfileCharactersRequest(membershipType, destinyMembershipId);
            // act
            ProfileCharactersResponse actual = bungieApiService.GetProfileCharacters(request);
            // assert
            Assert.AreEqual(3, actual.Characters.Values.Count);
            Assert.IsTrue(actual.Characters.ContainsKey(expectedCharacter.CharacterId));
            DestinyCharacter actualCharacter = actual.Characters[expectedCharacter.CharacterId];
            Assert.AreEqual(expectedCharacter.CharacterId, actualCharacter.CharacterId);
            Assert.AreEqual(expectedCharacter.DateLastPlayed, actualCharacter.DateLastPlayed);
            Assert.AreEqual(expectedCharacter.DestinyProfileId, actualCharacter.DestinyProfileId);
            Assert.AreEqual(expectedCharacter.DestinyMembershipType, actualCharacter.DestinyMembershipType);
        }

        // on my account, i have cross save enabled. so when i search for my memberships,
        // it returns data for the xbox account and steam account. each one has its own 
        // membership type and membership id. however, if you try to search for my xbox account,
        // it will return an error and say it's not found. only searching my steam account will
        // give results, because it's the primary cross save account.
        [TestMethod]
        public void GetProfileCharacters_CrossSaveOverrideSearchForXboxAccount_ShouldThrowApiExceptionDestinyAccountNotFound()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Bungie:ApiKey"] == "dummy-api-key");
            int membershipType = 1;
            long destinyMembershipId = 4611686018497175745;
            Uri uri = new Uri($"https://www.bungie.net/Platform/Destiny2/{membershipType}/Profile/{destinyMembershipId}/?components=200");
            string responseString = TestUtils.ReadFile("GetProfile-xbox-exists-but-not-found-because-cross-save-override.json");
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Get
                        && req.RequestUri == uri
                        && req.Headers.Any(h => h.Key == "X-API-KEY" && !string.IsNullOrEmpty(h.Value.FirstOrDefault()))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);
            BungieApiService bungieApiService = new BungieApiService(config, httpClient);
            ProfileCharactersRequest request = new ProfileCharactersRequest(membershipType, destinyMembershipId);
            // act (and assert)
            var exception = Assert.ThrowsException<BungieApiException>(() => bungieApiService.GetProfileCharacters(request));
            Assert.IsTrue(exception.Message.Contains("DestinyAccountNotFound"));
        }

        [TestMethod]
        public void GetCharacterEquipment_Personal_ShouldReturnEquippedGear()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Bungie:ApiKey"] == "dummy-api-key");
            // assemble
            int membershipType = 3;
            long membershipId = 4611686018467260757;
            long characterId = 2305843009504575107;
            Uri uri = new Uri($"https://www.bungie.net/Platform/Destiny2/{membershipType}/Profile/{membershipId}/Character/{characterId}/?components=205");
            string responseString = TestUtils.ReadFile("GetCharacterEquipment-valid-personal.json");
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Get
                        && req.RequestUri == uri
                        && req.Headers.Any(h => h.Key == "X-API-KEY" && !string.IsNullOrEmpty(h.Value.FirstOrDefault()))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);
            BungieApiService bungieApiService = new BungieApiService(config, httpClient);
            CharacterEquipmentRequest request = new CharacterEquipmentRequest(membershipType, membershipId, characterId);
            // act
            CharacterEquipmentResponse actual = bungieApiService.GetCharacterEquipment(request);
            // assert
            Assert.AreEqual(17, actual.Items.Count);
            Assert.IsTrue(actual.Items.All(item => item.ItemHash != 0 && item.ItemInstanceId != 0));
        }

        [TestMethod]
        public void GetCharacterEquipment_InvalidMembershipType_ShouldThrowApiExceptionInvalidParameters()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Bungie:ApiKey"] == "dummy-api-key");
            // assemble
            int membershipType = 1;
            long membershipId = 4611686018467260757;
            long characterId = 2305843009504575107;
            Uri uri = new Uri($"https://www.bungie.net/Platform/Destiny2/{membershipType}/Profile/{membershipId}/Character/{characterId}/?components=205");
            string responseString = TestUtils.ReadFile("GetCharacterEquipment-invalid-membership-type.json");
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Get
                        && req.RequestUri == uri
                        && req.Headers.Any(h => h.Key == "X-API-KEY" && !string.IsNullOrEmpty(h.Value.FirstOrDefault()))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);
            BungieApiService bungieApiService = new BungieApiService(config, httpClient);
            CharacterEquipmentRequest request = new CharacterEquipmentRequest(membershipType, membershipId, characterId);
            // act (and assert)
            var exception = Assert.ThrowsException<BungieApiException>(() => bungieApiService.GetCharacterEquipment(request));
            Assert.IsTrue(exception.Message.Contains("InvalidParameters"));
        }

        [TestMethod]
        public void GetCharacterEquipment_InvalidCharacterId_ShouldThrowApiExceptionBecauseResponseDataIsEmpty()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Bungie:ApiKey"] == "dummy-api-key");
            // assemble
            int membershipType = 3;
            long membershipId = 4611686018467260757;
            long characterId = 69;
            Uri uri = new Uri($"https://www.bungie.net/Platform/Destiny2/{membershipType}/Profile/{membershipId}/Character/{characterId}/?components=205");
            string responseString = TestUtils.ReadFile("GetCharacterEquipment-invalid-character-id.json");
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Get
                        && req.RequestUri == uri
                        && req.Headers.Any(h => h.Key == "X-API-KEY" && !string.IsNullOrEmpty(h.Value.FirstOrDefault()))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);
            BungieApiService bungieApiService = new BungieApiService(config, httpClient);
            CharacterEquipmentRequest request = new CharacterEquipmentRequest(membershipType, membershipId, characterId);
            // act (and assert)
            var exception = Assert.ThrowsException<BungieApiException>(() => bungieApiService.GetCharacterEquipment(request));
            Assert.IsTrue(exception.Message.Contains("character not found"));
        }

        [TestMethod]
        public void GetCharacterEquipment_InvalidMembershipId_ShouldThrowApiExceptionAccountNotFound()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Bungie:ApiKey"] == "dummy-api-key");
            // assemble
            int membershipType = 3;
            long membershipId = 69;
            long characterId = 2305843009504575107;
            Uri uri = new Uri($"https://www.bungie.net/Platform/Destiny2/{membershipType}/Profile/{membershipId}/Character/{characterId}/?components=205");
            string responseString = TestUtils.ReadFile("GetCharacterEquipment-invalid-membership-id.json");
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Get
                        && req.RequestUri == uri
                        && req.Headers.Any(h => h.Key == "X-API-KEY" && !string.IsNullOrEmpty(h.Value.FirstOrDefault()))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);
            BungieApiService bungieApiService = new BungieApiService(config, httpClient);
            CharacterEquipmentRequest request = new CharacterEquipmentRequest(membershipType, membershipId, characterId);
            // act (and assert)
            var exception = Assert.ThrowsException<BungieApiException>(() => bungieApiService.GetCharacterEquipment(request));
            Assert.IsTrue(exception.Message.Contains("We couldn't find the account you're looking for"));
        }


        [TestMethod]
        public void EquipItems_JustOneItem_ShouldChangeCorrespondingItem()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Bungie:ApiKey"] == "dummy-api-key");
            // for example, if i tell it to equip the recluse, it should change whatever was 
            // in my energy weapon slot before this. since this is all just item hashes and ids, 
            // this is just something that i figured out manually for the test.

            // uint recluseItemHash = 3354242550;
            long recluseItemInstanceId = 6917529123204409619;
            // uint suddenDeathItemHash = 1879212552;
            // long suddenDeathItemInstanceId = 6917529043814140192;

            int membershipType = 3;
            long characterId = 2305843009504575107;
            Uri uri = new Uri($"https://www.bungie.net/Platform/Destiny2/Actions/Items/EquipItems/");
            string responseString = TestUtils.ReadFile("EquipItems-equip-recluse-success.json");
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Post
                        && req.RequestUri == uri
                        && req.Headers.Any(h => h.Key == "X-API-KEY" && !string.IsNullOrEmpty(h.Value.FirstOrDefault())
                        && req.Headers.Any(h => h.Key == "Authorization" && !string.IsNullOrWhiteSpace(h.Value.FirstOrDefault()))
                        && req.Content.Headers.Any(h => h.Key == "Content-Type" && h.Value.FirstOrDefault().Contains("application/json")))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);
            BungieApiService bungieApiService = new BungieApiService(config, httpClient);
            EquipItemsRequest request = new EquipItemsRequest("access.token", membershipType, characterId, new long[] { recluseItemInstanceId });
            EquipItemsResponse actual = bungieApiService.EquipItems(request);
            Assert.AreEqual(1, actual.EquipResults.Count);
            Assert.AreEqual(BungiePlatformErrorCodes.Success, actual.EquipResults[0].EquipStatus);
        }

        [TestMethod]
        public void EquipItems_EquipEveryInventoryItem_AllShouldSucceed()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Bungie:ApiKey"] == "dummy-api-key");
            // item instance IDs
            long perfectParadox = 6917529138356180356;
            long suddenDeath = 6917529043814140192;
            long apexPredator = 6917529137866710642;
            long maskOfRull = 6917529110566559001;
            long reverieDawnGauntlets = 6917529138010460936;
            long plateOfTranscendence = 6917529109687230597;
            long peacekeepers = 6917529122999918127;
            long markOfTheGreatHunt = 6917529128966008940;
            long starMapShell = 6917529134911753611;
            long soloStandSparrow = 6917529096117947574;
            long safePassageShip = 6917529128292261186;
            long sentinelSubclass = 6917529102011422104;
            long clanBanner = 6917529137830892799;
            long prismaticInfernoEmblem = 6917529105645094388;
            long finisher = 6917529137848551327;
            long emote = 6917529101999517414;
            long lanternOfOsiris = 6917529137968184629;

            int membershipType = 3;
            long characterId = 2305843009504575107;
            Uri uri = new Uri($"https://www.bungie.net/Platform/Destiny2/Actions/Items/EquipItems/");
            string responseString = TestUtils.ReadFile("EquipItems-equip-all.json");
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Post
                        && req.RequestUri == uri
                        && req.Headers.Any(h => h.Key == "X-API-KEY" && !string.IsNullOrEmpty(h.Value.FirstOrDefault())
                        && req.Headers.Any(h => h.Key == "Authorization" && !string.IsNullOrWhiteSpace(h.Value.FirstOrDefault()))
                        && req.Content.Headers.Any(h => h.Key == "Content-Type" && h.Value.FirstOrDefault().Contains("application/json")))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);
            BungieApiService bungieApiService = new BungieApiService(config, httpClient);
            long[] itemsToEquip = new long[] {
                perfectParadox,
                suddenDeath,
                apexPredator,
                maskOfRull,
                reverieDawnGauntlets,
                plateOfTranscendence,
                peacekeepers,
                markOfTheGreatHunt,
                starMapShell,
                soloStandSparrow,
                safePassageShip,
                sentinelSubclass,
                clanBanner,
                prismaticInfernoEmblem,
                finisher,
                emote,
                lanternOfOsiris
            };
            EquipItemsRequest request = new EquipItemsRequest("access.token", membershipType, characterId, itemsToEquip);
            EquipItemsResponse actual = bungieApiService.EquipItems(request);
            Assert.AreEqual(17, actual.EquipResults.Count);
            Assert.IsTrue(actual.EquipResults.All(r => r.EquipStatus == BungiePlatformErrorCodes.Success));
        }

        [TestMethod]
        public void EquipItems_EquipZeroItems_ShouldSucceedButdoNothing()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Bungie:ApiKey"] == "dummy-api-key");
            int membershipType = 3;
            long characterId = 2305843009504575107;
            Uri uri = new Uri($"https://www.bungie.net/Platform/Destiny2/Actions/Items/EquipItems/");
            string responseString = TestUtils.ReadFile("EquipItems-zero-items.json");
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Post
                        && req.RequestUri == uri
                        && req.Headers.Any(h => h.Key == "X-API-KEY" && !string.IsNullOrEmpty(h.Value.FirstOrDefault())
                        && req.Headers.Any(h => h.Key == "Authorization" && !string.IsNullOrWhiteSpace(h.Value.FirstOrDefault()))
                        && req.Content.Headers.Any(h => h.Key == "Content-Type" && h.Value.FirstOrDefault().Contains("application/json")))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);
            BungieApiService bungieApiService = new BungieApiService(config, httpClient);
            EquipItemsRequest request = new EquipItemsRequest("access.token", membershipType, characterId, new long[] { });
            EquipItemsResponse actual = bungieApiService.EquipItems(request);
            Assert.AreEqual(0, actual.EquipResults.Count);
        }

        [TestMethod]
        public void EquipItems_EquipTheSameItemMultipleTimesInSameRequest_ShouldSucceed()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Bungie:ApiKey"] == "dummy-api-key");
            long recluseItemInstanceId = 6917529123204409619;

            int membershipType = 3;
            long characterId = 2305843009504575107;
            Uri uri = new Uri($"https://www.bungie.net/Platform/Destiny2/Actions/Items/EquipItems/");
            string responseString = TestUtils.ReadFile("EquipItems-recluse-multiple-times.json");
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Post
                        && req.RequestUri == uri
                        && req.Headers.Any(h => h.Key == "X-API-KEY" && !string.IsNullOrEmpty(h.Value.FirstOrDefault())
                        && req.Headers.Any(h => h.Key == "Authorization" && !string.IsNullOrWhiteSpace(h.Value.FirstOrDefault()))
                        && req.Content.Headers.Any(h => h.Key == "Content-Type" && h.Value.FirstOrDefault().Contains("application/json")))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);
            BungieApiService bungieApiService = new BungieApiService(config, httpClient);
            long[] itemsToEquip = new long[] { recluseItemInstanceId, recluseItemInstanceId, recluseItemInstanceId };
            EquipItemsRequest request = new EquipItemsRequest("access.token", membershipType, characterId, itemsToEquip);
            EquipItemsResponse actual = bungieApiService.EquipItems(request);
            Assert.AreEqual(3, actual.EquipResults.Count);
        }

        [TestMethod]
        public void EquipItems_SomeInvalidItemInstanceIDs_ShouldSuccessfullyEquipItemsAndReturnErrorStatusForInvalidItem()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Bungie:ApiKey"] == "dummy-api-key");
            long invalidItemInstanceId = 69;
            long recluseItemInstanceId = 6917529123204409619;

            int membershipType = 3;
            long characterId = 2305843009504575107;
            Uri uri = new Uri($"https://www.bungie.net/Platform/Destiny2/Actions/Items/EquipItems/");
            string responseString = TestUtils.ReadFile("EquipItems-some-invalid-items.json");
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Post
                        && req.RequestUri == uri
                        && req.Headers.Any(h => h.Key == "X-API-KEY" && !string.IsNullOrEmpty(h.Value.FirstOrDefault())
                        && req.Headers.Any(h => h.Key == "Authorization" && !string.IsNullOrWhiteSpace(h.Value.FirstOrDefault()))
                        && req.Content.Headers.Any(h => h.Key == "Content-Type" && h.Value.FirstOrDefault().Contains("application/json")))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);
            BungieApiService bungieApiService = new BungieApiService(config, httpClient);
            long[] itemsToEquip = new long[] { invalidItemInstanceId, recluseItemInstanceId };
            EquipItemsRequest request = new EquipItemsRequest("access.token", membershipType, characterId, itemsToEquip);
            EquipItemsResponse actual = bungieApiService.EquipItems(request);
            Assert.AreEqual(2, actual.EquipResults.Count);
            Assert.AreEqual(69, actual.EquipResults[0].ItemInstanceId);
            Assert.AreEqual(BungiePlatformErrorCodes.DestinyItemNotFound, actual.EquipResults[0].EquipStatus);
            Assert.AreEqual(recluseItemInstanceId, actual.EquipResults[1].ItemInstanceId);
            Assert.AreEqual(BungiePlatformErrorCodes.Success, actual.EquipResults[1].EquipStatus);
        }

        // InvalidPlatform (xbox but my cross save override is steam) will return error status 1601 DestinyAccountNotFound
        [TestMethod]
        public void EquipItems_InvalidPlatform_ShouldReturnErrorCodeDestinyAccountNotFound()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Bungie:ApiKey"] == "dummy-api-key");
            long recluseItemInstanceId = 6917529123204409619;

            int invalidMembershipType = 1;
            long characterId = 2305843009504575107;
            Uri uri = new Uri($"https://www.bungie.net/Platform/Destiny2/Actions/Items/EquipItems/");
            string responseString = TestUtils.ReadFile("EquipItems-destiny-account-not-found.json");
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Post
                        && req.RequestUri == uri
                        && req.Headers.Any(h => h.Key == "X-API-KEY" && !string.IsNullOrEmpty(h.Value.FirstOrDefault())
                        && req.Headers.Any(h => h.Key == "Authorization" && !string.IsNullOrWhiteSpace(h.Value.FirstOrDefault()))
                        && req.Content.Headers.Any(h => h.Key == "Content-Type" && h.Value.FirstOrDefault().Contains("application/json")))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);
            BungieApiService bungieApiService = new BungieApiService(config, httpClient);
            long[] itemsToEquip = new long[] { recluseItemInstanceId };
            EquipItemsRequest request = new EquipItemsRequest("access.token", invalidMembershipType, characterId, itemsToEquip);
            EquipItemsResponse actual = bungieApiService.EquipItems(request);
            Assert.AreEqual(1, actual.EquipResults.Count);
            Assert.AreEqual(BungiePlatformErrorCodes.DestinyAccountNotFound, actual.EquipResults[0].EquipStatus);
        }

        [TestMethod]
        public void EquipItems_AccessTokenInvalidExpired_ShouldThrowApiExceptionUnauthorized()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Bungie:ApiKey"] == "dummy-api-key");
            long recluseItemInstanceId = 6917529123204409619;
            int membershipType = 3;
            long characterId = 2305843009504575107;
            Uri uri = new Uri($"https://www.bungie.net/Platform/Destiny2/Actions/Items/EquipItems/");
            string responseString = TestUtils.ReadFile("EquipItems-invalid-unauthorized.html");
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Post
                        && req.RequestUri == uri
                        && req.Headers.Any(h => h.Key == "X-API-KEY" && !string.IsNullOrEmpty(h.Value.FirstOrDefault())
                        && req.Headers.Any(h => h.Key == "Authorization" && !string.IsNullOrWhiteSpace(h.Value.FirstOrDefault()))
                        && req.Content.Headers.Any(h => h.Key == "Content-Type" && h.Value.FirstOrDefault().Contains("application/json")))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);
            BungieApiService bungieApiService = new BungieApiService(config, httpClient);
            long[] itemsToEquip = new long[] { recluseItemInstanceId };
            EquipItemsRequest request = new EquipItemsRequest("access.token", membershipType, characterId, itemsToEquip);
            BungieApiException exception = Assert.ThrowsException<BungieApiException>(() => bungieApiService.EquipItems(request));
            Assert.IsTrue(exception.Message.Contains("Unauthorized"));
        }


        [TestMethod]
        public void RefreshAccessToken_ValidRefreshToken_ShouldReturnNewAccessTokenAndSameRefreshTokenWithUpdatedExpireTime()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m =>
                    m["Bungie:ApiKey"] == "dummy-api-key" &&
                    m["Bungie:ClientId"] == "dummy-client-id" &&
                    m["Bungie:ClientSecret"] == "dummy-client-secret");
            Uri uri = new Uri("https://www.bungie.net/Platform/App/OAuth/Token/");
            string refreshToken = "refresh-token";
            string content = $"grant_type=refresh_token&refresh_token={refreshToken}&client_id={"dummy-client-id"}&client_secret={"dummy-client-secret"}";
            string responseString = TestUtils.ReadFile("OAuth.Refresh-valid-refresh-token-success.json");
            string expectedAccessToken = "CMfnARKGAgAgMgc7bAgu5A0xcQOHQONRWu9Mjq0O1/1zXz7R/kRg5hXgAAAADJt21jY58Cp3Mw/e6aSKOpqQ/jwpM5Yw8CrelL+zLyepdoi8CoZdEqmAoIVjPMezEzuNpL5tkSl13RoD6XvUP+zPRAl85OETHiTayetjNDqlNgu8UYAAt97lnh/db+TAlUuwZqpOmgUs/cAJmk6JS4piZvALo1KFstbWJIz1WmqQFMAT41dVi49P9PD5w+AJlqNR3lIfJs66UCwkgXH4EYWewBnzlplFLwxU56w3Q5Q8fQYc0Db9gUL+plPN4DNeAwPPtshgxNDhrfTsrH3R8TXq4ZcXLvJqYSucx2tjlmE=";
            string expectedRefreshToken = "CMfnARKGAgAgM29n1MFKoiXR6F7Bo2TRKkuhhUD144lyxl4j697iupPgAAAAbQrA9ez/tfFeAIGNddkmZHv21ZqIn11UxQAHvzmqQmERczk8YLezxlJr76mWUYklbBjsmWZlqsSG9VfnaEy/sdc7E9oJg0f1muLmmA3vZvmetrfss12G7v6TczeT3W01upv4j6odQReOUDmJ+dxQb99hDgAoR1L4j6eL8FfdfDC3LFmQwrGJw/FJr/KvpjPQ9jncRsFt5v6UIl0usuCEy8rl/lETjXxVLYHX6Sr+ERwGOJYJVfFMcults6uWumr3S2ZVnHfG2gEHTlxyyQkkXOK1P4mQjdgRPWDgIR9fMy8=";
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

            BungieApiService bungieApiService = new BungieApiService(config, httpClient);
            OAuthResponse actual = bungieApiService.RefreshAccessToken("refresh-token");

            Assert.AreEqual(expectedAccessToken, actual.AccessToken);
            Assert.AreEqual(expectedRefreshToken, actual.RefreshToken);
            Assert.AreEqual(3600, actual.AccessTokenExpiresInSeconds);
            Assert.AreEqual(7776000, actual.RefreshTokenExpiresInSeconds);
            Assert.IsTrue(string.IsNullOrWhiteSpace(actual.ErrorType));
        }

        [TestMethod]
        public void RefreshAccessToken_ExpiredRefreshTokenOrMaybeBadRequest_ShouldReturnError()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m =>
                                m["Bungie:ApiKey"] == "dummy-api-key" &&
                                m["Bungie:ClientId"] == "dummy-client-id" &&
                                m["Bungie:ClientSecret"] == "dummy-client-secret");
            Uri uri = new Uri("https://www.bungie.net/Platform/App/OAuth/Token/");
            string refreshToken = "bad-refresh-token";
            string content = $"grant_type=refresh_token&refresh_token={refreshToken}&client_id={"dummy-client-id"}&client_secret={"dummy-client-secret"}";
            string responseString = TestUtils.ReadFile("OAuth.Refresh-error.json");
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest  ,
                    Content = new StringContent(responseString)
                })
                .Verifiable();
            HttpClient httpClient = new HttpClient(mock.Object);

            OAuthResponse errorResponse = new OAuthResponse();
            errorResponse.ErrorType = "server_error";
            errorResponse.ErrorDescription = "Invalid length for a Base-64 char array or string.";

            IBungieApiService bungieApiService = new BungieApiService(config, httpClient);
            OAuthResponse actual = bungieApiService.RefreshAccessToken("bad-refresh-token");
            Assert.IsTrue(string.IsNullOrWhiteSpace(actual.AccessToken));
            Assert.AreEqual("server_error", actual.ErrorType);
        }


    }
}