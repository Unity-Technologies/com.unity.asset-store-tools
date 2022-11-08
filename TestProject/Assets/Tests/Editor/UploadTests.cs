using NUnit.Framework;
using System.Collections;
using AssetStoreTools.Uploader;
using System;
using UnityEngine.TestTools;
using System.Threading.Tasks;
using AssetStoreTools.Utility.Json;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading;

namespace Tests.Editor
{
    public class UploadTests
    {
        private readonly string Username = Environment.GetEnvironmentVariable("AST_USERNAME");
        private readonly string Password = Environment.GetEnvironmentVariable("AST_PASSWORD");

        private readonly string PackageId = "227403";

        private readonly string VersionId = "719827";
        private readonly string VersionIdAlt = "719999";

        private const string SmallPackageReference = "Assets/Tests/Resources/SmallPackage.unitypackage";
        private const string LargePackageReference = "Assets/Tests/Resources/LargePackage.unitypackage";
        private const string MultiPackageReference = "Assets/Tests/Resources/MultiPackage.unitypackage";

        private class UploadInfo
        {
            public bool HasThisVersion;
            public DateTime Timestamp;
            public string Size;
        }

        [UnitySetUp]
        public IEnumerator Setup()
        {
            if (AssetStoreAPI.SavedSessionId == string.Empty)
            {
                var loginTask = AssetStoreAPI.LoginWithCredentialsAsync(Username, Password);

                while (!loginTask.IsCompleted)
                    yield return null;

                Assert.IsTrue(loginTask.Result.Success);
                var info = loginTask.Result.Response;
                Assert.IsTrue(AssetStoreAPI.IsPublisherValid(info, out var error), error.Message);
                AssetStoreAPI.SavedSessionId = info["xunitysession"].AsString();
            }
        }

        [TearDown]
        public void Teardown()
        {
            if (AssetStoreAPI.IsUploading)
            {
                AssetStoreAPI.AbortUploadTasks();
                while(AssetStoreAPI.IsUploading)
                    Thread.Sleep(10);
            }
        }

        private bool UploadTaskComplete(Task task) => (task.IsCompleted || task.IsCanceled || task.IsFaulted);

        private string SelectPackageToUpload(string existingPackageSize)
        {
            var option1 = new FileInfo(SmallPackageReference);
            var option2 = new FileInfo(MultiPackageReference);

            if (string.IsNullOrEmpty(existingPackageSize))
                return option1.FullName;

            // The size returned from Publisher Portal may not match the size on disk, so find the one with the largest difference
            var delta1 = Math.Abs(option1.Length - (long)Convert.ToDouble(existingPackageSize));
            var delta2 = Math.Abs(option2.Length - (long)Convert.ToDouble(existingPackageSize));

            if (delta1 > delta2)
                return option1.FullName;
            return option2.FullName;
        }

        private string GenerateRandomString()
        {
            var source = "ABCDEFGHIJKLMOPQRSTUVWXYZabdefghijklmnopqrstuvwxyz";
            var random = new System.Random();

            var generated = string.Empty;
            for (int i = 0; i < 30; i++)
            {
                generated += source[random.Next(0, source.Length)];
            }
            return generated;
        }

        private async Task<UploadInfo> GetLastUploadTimestamp(string versionId)
        {
            var uri = APIUri("management", $"package_version/{versionId}", AssetStoreAPI.SavedSessionId);

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Unity-Session", AssetStoreAPI.SavedSessionId);

            var response = await httpClient.GetAsync(uri)
                .ContinueWith((x) => x.Result.Content.ReadAsStringAsync().Result);

            if (!JSONParser.AssetStoreResponseParse(response, out var error, out var jsonMainData))
                throw error.Exception;

            var versions = jsonMainData["package"].AsDict()["version"].AsDict()["unitypackages"].AsList();

            foreach (var version in versions)
                if (version["unity_version"].AsString() == Application.unityVersion)
                    return new UploadInfo() { HasThisVersion = true, Timestamp = DateTime.Parse(version["timestamp"].AsString()), Size = version["size"].AsString() };

            return new UploadInfo() { HasThisVersion = false };

            // Helper methods

            Uri APIUri(string apiPath, string endPointPath, string sessionId)
            {
                return APIUriFull(apiPath, endPointPath, sessionId, null);
            }

            Uri APIUriFull(string apiPath, string endPointPath, string sessionId, IDictionary<string, string> extraQuery)
            {
                Dictionary<string, string> extraQueryMerged;

                if (extraQuery == null)
                    extraQueryMerged = new Dictionary<string, string>();
                else
                    extraQueryMerged = new Dictionary<string, string>(extraQuery);

                extraQueryMerged.Add("unityversion", Application.unityVersion);
                extraQueryMerged.Add("toolversion", AssetStoreAPI.ToolVersion);
                extraQueryMerged.Add("xunitysession", sessionId);

                string uriPath = $"{AssetStoreAPI.AssetStoreProdUrl}/api/{apiPath}/{endPointPath}.json";
                UriBuilder uriBuilder = new UriBuilder(uriPath);

                StringBuilder queryToAppend = new StringBuilder();
                foreach (KeyValuePair<string, string> queryPair in extraQueryMerged)
                {
                    string queryName = queryPair.Key;
                    string queryValue = Uri.EscapeDataString(queryPair.Value);

                    queryToAppend.AppendFormat("&{0}={1}", queryName, queryValue);
                }
                if (!string.IsNullOrEmpty(uriBuilder.Query))
                    uriBuilder.Query = uriBuilder.Query.Substring(1) + queryToAppend;
                else
                    uriBuilder.Query = queryToAppend.Remove(0, 1).ToString();

                return uriBuilder.Uri;
            }
        }

        [UnityTest]
        public IEnumerator UploadPackage()
        {
            JsonValue packageData = new JsonValue();
            var dataTask = AssetStoreAPI.GetFullPackageDataAsync(false);
            while (!dataTask.IsCompleted)
                yield return null;

            var result = dataTask.Result;
            Assert.IsTrue(result.Success);
            packageData = result.Response;

            //var originalSize = package["extra_info"]["size"].IsString() ? package["extra_info"]["size"].AsString() : string.Empty;
            //var originalModified = package["extra_info"]["modified"].AsString();

            var timestampTask = GetLastUploadTimestamp(VersionId);
            while (!timestampTask.IsCompleted)
                yield return null;

            var originalSize = string.Empty;
            var originalTimestamp = DateTime.MinValue;

            if(timestampTask.Result.HasThisVersion)
            {
                originalSize = timestampTask.Result.Size;
                originalTimestamp = timestampTask.Result.Timestamp;
            }

            var packagePath = SelectPackageToUpload(originalSize);
            var localPackageGuid = GenerateRandomString();
            var localPackagePath = GenerateRandomString();
            var localProjectPath = GenerateRandomString();

            Assert.IsFalse(AssetStoreAPI.IsUploading, "AssetStoreAPI is uploading before the test even started");

            var uploadTask = AssetStoreAPI.UploadPackageAsync(VersionId, "TestPackage", packagePath, localPackageGuid, localPackagePath, localProjectPath);

            while (!UploadTaskComplete(uploadTask))
                yield return null;

            Assert.IsTrue(uploadTask.Status == TaskStatus.RanToCompletion, "Upload task has not completed successfully");
            Assert.IsFalse(AssetStoreAPI.IsUploading, "AssetStoreAPI is still uploading after the upload task has completed");

            var uploadResult = uploadTask.Result;

            Assert.AreEqual(PackageUploadResult.UploadStatus.Success, uploadResult.Status, result.Error?.Message);

            timestampTask = GetLastUploadTimestamp(VersionId);

            while (!timestampTask.IsCompleted)
                yield return null;

            Assert.IsTrue(timestampTask.Result.HasThisVersion, "Uploaded package did not appear in the Publisher Portal");

            // Size and modified should have changed
            Assert.AreNotEqual(originalTimestamp, timestampTask.Result.Timestamp, "Modified date before and after upload has not changed");
            Assert.AreNotEqual(originalSize, timestampTask.Result.Size, "Size before and after upload has not changed");

            dataTask = AssetStoreAPI.GetFullPackageDataAsync(false);

            while (!dataTask.IsCompleted)
                yield return null;

            Assert.IsTrue(dataTask.Result.Success);

            var package = dataTask.Result.Response["packages"][PackageId];
            var newLocalPackageGuid = package["root_guid"].AsString();
            var newLocalPackagePath = package["root_path"].AsString();
            var newLocalProjectPath = package["project_path"].AsString();

            // Package data should be identical to the sent one
            Assert.AreEqual(localPackageGuid, newLocalPackageGuid, "Local Package Guid does not match the expected one");
            Assert.AreEqual(localPackagePath, newLocalPackagePath, "Local Package Path does not match the expected one");
            Assert.AreEqual(localProjectPath, newLocalProjectPath, "Local Project Path does not match the expected one");
        }

        [UnityTest]
        public IEnumerator AbortSingleUpload()
        {
            var packagePath = LargePackageReference;

            Assert.IsFalse(AssetStoreAPI.IsUploading, "AssetStoreAPI is uploading before the test even started");

            var uploadTask = AssetStoreAPI.UploadPackageAsync(VersionId, "TestPackage", packagePath, "", "", "");
            while (!AssetStoreAPI.ActiveUploads.ContainsKey(VersionId))
                yield return null;

            AssetStoreAPI.AbortPackageUpload(VersionId);

            while (!UploadTaskComplete(uploadTask))
                yield return null;

            var uploadResult = uploadTask.Result.Status;

            Assert.AreEqual(PackageUploadResult.UploadStatus.Cancelled, uploadResult, "Upload result did not return appropriate Cancel status");
            Assert.IsFalse(AssetStoreAPI.IsUploading, "AssetStoreAPI is still uploading after the upload task has completed");
        }

        [UnityTest]
        public IEnumerator AbortAllUploads()
        {
            var packagePathA = LargePackageReference;
            var packagePathB = MultiPackageReference;

            Assert.IsFalse(AssetStoreAPI.IsUploading);

            var uploadTask1 = AssetStoreAPI.UploadPackageAsync(VersionId, "TestPackage1", packagePathA, String.Empty, String.Empty, String.Empty);
            Thread.Sleep(150); // Sleeping between two upload tasks avoid deadlock
            var uploadTask2 = AssetStoreAPI.UploadPackageAsync(VersionIdAlt, "TestPackage2", packagePathB, String.Empty, String.Empty, String.Empty);
            Thread.Sleep(150); // Sleeping between two upload tasks avoid deadlock
            
            while (!AssetStoreAPI.ActiveUploads.ContainsKey(VersionId) && !AssetStoreAPI.ActiveUploads.ContainsKey(VersionIdAlt))
                yield return null;
            
            AssetStoreAPI.AbortPackageUpload(VersionId);
            AssetStoreAPI.AbortPackageUpload(VersionIdAlt);

            while (!UploadTaskComplete(uploadTask1) && !UploadTaskComplete(uploadTask2))
                yield return null;

            var uploadResult1 = uploadTask1.Result.Status;
            var uploadResult2 = uploadTask2.Result.Status;

            Assert.AreEqual(PackageUploadResult.UploadStatus.Cancelled, uploadResult1, "Upload result 1 did not return appropriate Cancel status");
            Assert.AreEqual(PackageUploadResult.UploadStatus.Cancelled, uploadResult2, "Upload result 2 did not return appropriate Cancel status");
            Assert.IsFalse(AssetStoreAPI.IsUploading, "AssetStoreAPI is still uploading after canceling all tasks");
        }
    }
}