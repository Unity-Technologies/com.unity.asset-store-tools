using AssetStoreTools.Uploader;
using AssetStoreTools.Utility.Json;
using NUnit.Framework;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine.TestTools;

namespace Tests.Editor
{
    public class PackageDataTests
    {
        private readonly string Username = Environment.GetEnvironmentVariable("AST_Username");
        private readonly string Password = Environment.GetEnvironmentVariable("AST_Password");

        private readonly string PackageId = "227403";

        [UnitySetUp]
        public IEnumerator Setup()
        {
            if (AssetStoreAPI.SavedSessionId == string.Empty)
            {
                var loginTask = AssetStoreAPI.LoginWithCredentialsAsync(Username, Password);

                while (!loginTask.IsCompleted)
                    yield return null;
            }
        }

        [TearDown]
        public void Teardown()
        {
            if (AssetStoreAPI.IsUploading)
                AssetStoreAPI.AbortUploadTasks();
        }

        [UnityTest]
        public IEnumerator GetFullPackageData()
        {
            var packageData = default(JsonValue);
            var dataTask = AssetStoreAPI.GetFullPackageDataAsync(false);

            while (!dataTask.IsCompleted)
                yield return null;

            var result = dataTask.Result;
            Assert.IsTrue(result.Success);

            packageData = result.Response;
            Assert.AreNotEqual(default(JsonValue), packageData, "Package data Json is a default Json");
        }

        [UnityTest]
        public IEnumerator GetRefreshedPackageData()
        {
            var packageData = default(JsonValue);
            var dataTask = AssetStoreAPI.GetRefreshedPackageData(PackageId);

            while (!dataTask.IsCompleted)
                yield return null;

            var result = dataTask.Result;
            Assert.IsTrue(result.Success);

            packageData = result.Response;

            Assert.IsTrue(packageData.ContainsKey("name"), "Package data does not contain a name key");
            Assert.IsTrue(packageData.ContainsKey("status"), "Package data does not contain a status key");
            Assert.IsTrue(packageData.ContainsKey("extra_info"), "Package data does not contain an extra info key");
            Assert.IsTrue(packageData["extra_info"].ContainsKey("modified"), "Package data does not contain a modified key");
            Assert.IsTrue(packageData["extra_info"].ContainsKey("size"), "Package data does not contain a size key");
            Assert.IsTrue(packageData["extra_info"].ContainsKey("category_info"), "Package data does not contain category info key");
            Assert.IsTrue(packageData["extra_info"]["category_info"].ContainsKey("id"), "Package data does not contain a category id key");
            Assert.IsTrue(packageData["extra_info"]["category_info"].ContainsKey("name"), "Package data does not contain a category name key");
        }

        [UnityTest]
        public IEnumerator GetThumbnails()
        {
            var packageData = default(JsonValue);
            var dataTask = AssetStoreAPI.GetFullPackageDataAsync(false);

            while (!dataTask.IsCompleted)
                yield return null;

            var result = dataTask.Result;
            Assert.IsTrue(result.Success);

            packageData = result.Response;
            Assert.AreNotEqual(default(JsonValue), packageData, "Package data Json is a default Json");


            var actualThumbnailCount = 0;
            var expectedThumbnailCount = packageData["packages"].AsDict().Count;

            AssetStoreAPI.GetPackageThumbnails(packageData, false,
                (id, tex) =>
                {
                    actualThumbnailCount++;
                    if (tex != null)
                    {
                        // Textures are created with size 1, 1 before loading a proper texture
                        Assert.AreNotEqual(1, tex.width);
                        Assert.AreNotEqual(1, tex.height);
                    }
                },
                (id, e) => { Assert.Fail($"Package thumbnail downloa failed for package {id}:\n{e.Message}"); });

            while (actualThumbnailCount != expectedThumbnailCount)
                yield return null;
        }

        [UnityTest]
        public IEnumerator PackageMetadataIsCached()
        {
            var packageMetadataPath = $"{AssetStoreCache.TempCachePath}/PackageMetadata.json";

            if (File.Exists(packageMetadataPath))
                File.Delete(packageMetadataPath);

            var dataTask = AssetStoreAPI.GetFullPackageDataAsync(false);

            while (!dataTask.IsCompleted)
                yield return null;

            var result = dataTask.Result;
            Assert.IsTrue(result.Success);

            Assert.IsTrue(File.Exists(packageMetadataPath), "Package metadata was not cached");
        }

        [UnityTest]
        public IEnumerator PackageCategoriesAreCached()
        {
            var packageCategoryPath = $"{AssetStoreCache.TempCachePath}/Categories.json";

            if (File.Exists(packageCategoryPath))
                File.Delete(packageCategoryPath);

            var dataTask = AssetStoreAPI.GetFullPackageDataAsync(false);

            while (!dataTask.IsCompleted)
                yield return null;

            var result = dataTask.Result;
            Assert.IsTrue(result.Success);

            Assert.IsTrue(File.Exists(packageCategoryPath), "Package categories were not cached");
        }

        [UnityTest]
        public IEnumerator ThumbnailsAreCached()
        {
            var packageData = default(JsonValue);
            var dataTask = AssetStoreAPI.GetFullPackageDataAsync(false);

            foreach (var file in Directory.GetFiles(AssetStoreCache.TempCachePath).Where(x => x.ToLower().EndsWith(".png")))
                File.Delete(file);

            while (!dataTask.IsCompleted)
                yield return null;

            var result = dataTask.Result;
            Assert.IsTrue(result.Success);

            packageData = result.Response;
            Assert.AreNotEqual(default(JsonValue), packageData, "Package data Json is a default Json");

            var actualThumbnailCount = 0;
            var expectedThumbnailCount = packageData["packages"].AsDict().Count;

            AssetStoreAPI.GetPackageThumbnails(packageData, false,
                (id, tex) =>
                {
                    actualThumbnailCount++;
                    if(tex != null)
                        Assert.IsTrue(File.Exists($"{AssetStoreCache.TempCachePath}/{id}.png"), $"Non-fallback thumbnail for package id {id} was not cached");
                },
                (id, e) => { Assert.Fail($"Package thumbnail downloa failed for package {id}:\n{e.Message}"); });

            while (actualThumbnailCount != expectedThumbnailCount)
                yield return null;
        }
    }
}
