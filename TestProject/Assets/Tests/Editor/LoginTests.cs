using System;
using System.Collections;
using NUnit.Framework;
using AssetStoreTools.Uploader;
using AssetStoreTools.Utility.Json;
using UnityEngine.TestTools;

namespace Tests.Editor
{
    public class LoginTests
    {
        private readonly string Username = Environment.GetEnvironmentVariable("AST_USERNAME");
        private readonly string Password = Environment.GetEnvironmentVariable("AST_PASSWORD");
        
        private readonly string Username_NoPublisher = Environment.GetEnvironmentVariable("AST_USERNAMENOPUBLISHER");
        private readonly string Password_NoPublisher = Environment.GetEnvironmentVariable("AST_PASSWORDNOPUBLISHER");
        
        private readonly string CloudTokenSecret = Environment.GetEnvironmentVariable("AST_CLOUDTOKENSECRET");

        [UnityTest]
        public IEnumerator Login_WithCredentials_ValidCredentials()
        {           
            var loginTask = AssetStoreAPI.LoginWithCredentialsAsync(Username, Password);

            while (!loginTask.IsCompleted)
                yield return null;

            var result = loginTask.Result;
            if (!result.Success)
                Assert.Fail($"Message: {result.Error.Message}\nException: {result.Error.Exception}");

            var json = result.Response;

            // Is publisher valid
            if (!AssetStoreAPI.IsPublisherValid(json, out var err))
                Assert.Fail($"Message: {err.Message}\nException: {err.Exception}");
        }

        [UnityTest]
        public IEnumerator Login_WithCredentials_IncorrectCredentials()
        {
            string email = "honey@pot.com";
            string password = "!asdsad#asda";

            var loginTask = AssetStoreAPI.LoginWithCredentialsAsync(email, password);

            while (!loginTask.IsCompleted)
                yield return null;

            var result = loginTask.Result;

            if(!result.Success)
                Assert.Pass($"Message: {result.Error.Message}\nException: {result.Error.Exception}");
            else
            {
                var json = result.Response;
                Assert.Fail($"Logged in with invalid session.\n{json}");
            }
        }
        
        [UnityTest]
        public IEnumerator Login_WithCredentials_PublisherInvalid()
        {            
            var loginTask = AssetStoreAPI.LoginWithCredentialsAsync(Username_NoPublisher, Password_NoPublisher);

            while (!loginTask.IsCompleted)
                yield return null;

            var result = loginTask.Result;
            Assert.IsTrue(result.Success);
            var json = result.Response;

            if(!result.Success)
                Assert.Fail($"Failed on login with credentials.\n" +
                            $"Message: {result.Error.Message}\nException: {result.Error.Exception}");

            // Is publisher valid
            if (!AssetStoreAPI.IsPublisherValid(json, out var err))
                Assert.Pass($"Message: {err.Message}\nException: {err.Exception}");
            else
                Assert.Fail($"Logged in with invalid credentials: {json}");
        }

        [UnityTest]
        public IEnumerator Login_WithSession_InvalidSessions()
        {
            var session = "26c4202eb475d02864b40827dfff11a14657aa41";
            AssetStoreAPI.SavedSessionId = session;

            var loginTask = AssetStoreAPI.LoginWithSessionAsync();

            while (!loginTask.IsCompleted)
                yield return null;

            var result = loginTask.Result;
            if (!result.Success)
                Assert.Fail($"Message: {result.Error.Message}\nException: {result.Error.Exception}");

            var json = result.Response;

            // Is publisher valid
            if (AssetStoreAPI.IsPublisherValid(json, out var err))
                Assert.Fail($"Logged in with an invalid session");
            Assert.Pass(json.ToString());
        }

        [UnityTest]
        public IEnumerator Login_WithSession_ValidSession()
        {
            // Log in with credentials first to get a session key
            var loginTask = AssetStoreAPI.LoginWithCredentialsAsync(Username, Password);

            while (!loginTask.IsCompleted)
                yield return null;

            var result = loginTask.Result;
            if (!result.Success)
                Assert.Fail($"Message: {result.Error.Message}\nException: {result.Error.Exception}");

            var json = result.Response;
            var sessionSecret = json["xunitysession"].AsString();
            AssetStoreAPI.SavedSessionId = sessionSecret;

            loginTask = AssetStoreAPI.LoginWithSessionAsync();

            while (!loginTask.IsCompleted)
                yield return null;

            result = loginTask.Result;
            if (!result.Success)
                Assert.Fail($"Message: {result.Error.Message}\nException: {result.Error.Exception}");

            json = result.Response;

            // Is publisher valid
            if (!AssetStoreAPI.IsPublisherValid(json, out var err))
                Assert.Fail($"Message: {err.Message}\nException: {err.Exception}");

            Assert.Pass($"SessionId: {json["xunitysession"].AsString()}\n" +
                        $"User: {json["username"].AsString()}");
        }

        [UnityTest]
        public IEnumerator Login_WithCloudToken_InvalidToken()
        {
            var token = "13123123123";

            var loginTask = AssetStoreAPI.LoginWithTokenAsync(token);

            while (!loginTask.IsCompleted)
                yield return null;

            var result = loginTask.Result;
            if (!result.Success)
                Assert.Pass($"Message: {result.Error.Message}\nException: {result.Error.Exception}");
            else
            {
                var json = result.Response;
                Assert.Fail($"Logged in with invalid token.\n" +
                            $"SessionId: {json["xunitysession"].AsString()}\nUser: {json["username"].AsString()}");
            }
        }

        [UnityTest]
        public IEnumerator Login_WithCloudToken_ValidToken()
        {
            TestHelper.AssertIgnoreIfYamato(); // Ignore on Yamato because CLI login does not fetch token properly
            
            var loginTask = AssetStoreAPI.LoginWithTokenAsync(CloudTokenSecret);

            while (!loginTask.IsCompleted)
                yield return null;

            var result = loginTask.Result;
            Assert.IsTrue(result.Success);

            if (!result.Success)
                Assert.Fail($"Message: {result.Error.Message}\nException: {result.Error.Exception}");

            var json = result.Response;

            if(!AssetStoreAPI.IsPublisherValid(json, out ASError err))
                Assert.Fail($"Message: {err.Message}\nException: {err.Exception}");

            Assert.Pass($"SessionId: {json["xunitysession"].AsString()}\n" +
                        $"User: {json["username"].AsString()}");

        }
    }
}
