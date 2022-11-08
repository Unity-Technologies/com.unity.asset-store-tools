using System.IO;
using AssetStoreTools.Uploader;
using AssetStoreTools.Utility.Json;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Editor
{
    public class AssetStoreCacheTests
    {
        [SetUp]
        public void Setup()
        {
            if (Directory.Exists(AssetStoreCache.TempCachePath))
                Directory.Delete(AssetStoreCache.TempCachePath, true);
            if (Directory.Exists(AssetStoreCache.PersistentCachePath))
                Directory.Delete(AssetStoreCache.PersistentCachePath, true);
        }

        [Test]
        public void CacheCategories_NewJson()
        {
            JsonValue categories = new JsonValue
            {
                ["test"] = "testCategory",
                ["test2"] = "testCategory2"
            };

            AssetStoreCache.CacheCategories(categories);
            
            var path = Path.Combine("Temp/AssetStoreToolsCache", "Categories.json");
            
            if (!File.Exists(path))
                Assert.Fail($"Categories file does not exist");

            Assert.Pass();
        }
        
        [Test]
        public void GetCachedCategories_FileExists()
        {
            var testResult = "testCategory";
            var testResult2 = "testCategory2";
            
            JsonValue categories = new JsonValue
            {
                ["test"] = "testCategory",
                ["test2"] = "testCategory2"
            };

            var path = Path.Combine("Temp/AssetStoreToolsCache", "Categories.json");

            AssetStoreCache.CacheCategories(categories);
            
            if (!File.Exists(path))
                Assert.Fail($"Categories file does not exist");
            
            if (!AssetStoreCache.GetCachedCategories(out var data))
                Assert.Fail($"Categories file does not exist");

            if (data["test"].AsString() != testResult && data["test2"].AsString() != testResult2)
                Assert.Fail($"Scraped data was not the same as the stored one.\n{data["test"]} {data["test2"]}");

            Assert.Pass();
        }
        
        [Test]
        public void GetCachedCategories_NoFile()
        {
            var path = Path.Combine("Temp/AssetStoreToolsCache", "Categories.json");
            
            if (File.Exists(path))
                File.Delete(path);
            
            Assert.IsFalse(AssetStoreCache.GetCachedCategories(out _));
        }
        
        [Test]
        public void CachePackageMetadata_NewJson()
        {
            JsonValue metadata = new JsonValue
            {
                ["test"] = "testMetadata",
                ["test2"] = "testMetadata2"
            };
            
            var path = Path.Combine("Temp/AssetStoreToolsCache", "PackageMetadata.json");
            
            AssetStoreCache.CachePackageMetadata(metadata);
            
            if (!File.Exists(path))
                Assert.Fail($"Metadata file does not exist");

            Assert.Pass();
        }
        
        [Test]
        public void GetCachedPackageMetadata_FileExists()
        {
            var testResult = "testMetadata";
            var testResult2 = "testMetadata2";
            
            JsonValue metadata = new JsonValue
            {
                ["test"] = "testMetadata",
                ["test2"] = "testMetadata2"
            };
            
            var path = Path.Combine("Temp/AssetStoreToolsCache", "PackageMetadata.json");
            
            AssetStoreCache.CachePackageMetadata(metadata);
            
            if (!File.Exists(path))
                Assert.Fail($"Metadata file does not exist");
            
            if (!AssetStoreCache.GetCachedPackageMetadata(out var data))
                Assert.Fail($"Metadata file does not exist");

            if (data["test"].AsString() != testResult && data["test2"].AsString() != testResult2)
                Assert.Fail($"Scraped data was not the same as the stored one.\n{data["test"]} {data["test2"]}");

            Assert.Pass();
        }

        [Test]
        public void GetCachedPackageMetadata_NoFile()
        {
            var path = Path.Combine("Temp/AssetStoreToolsCache", "PackageMetadata.json");
            
            if (File.Exists(path))
                File.Delete(path);
            
            Assert.IsFalse(AssetStoreCache.GetCachedPackageMetadata(out _));
        }

        private Texture2D GenerateDefaultTexture()
        {
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
         
            texture.SetPixel(0, 0, Color.white);
            texture.SetPixel(1, 1, Color.white);
            texture.SetPixel(0, 1, Color.black);
            texture.SetPixel(1, 0, Color.black);
         
            texture.Apply();

            return texture;
        }

        [Test]
        public void CacheTexture_Checkers()
        {
            var path = Path.Combine("Temp/AssetStoreToolsCache", "696969.png");
            var tex = GenerateDefaultTexture();
            
            AssetStoreCache.CacheTexture("696969", tex);

            if (!File.Exists(path))
                Assert.Fail($"Texture file does not exist");

            Assert.Pass();
        }
        
        [Test]
        public void GetCachedTexture_Checkers()
        {
            var path = Path.Combine("Temp/AssetStoreToolsCache", "696969.png");
            var tex = GenerateDefaultTexture();
            
            AssetStoreCache.CacheTexture("696969", tex);

            if (!File.Exists(path))
                Assert.Fail($"Texture file does not exist");
            
            AssetStoreCache.GetCachedTexture("696969", out var outTex);

            if (outTex.GetPixel(0, 0) != tex.GetPixel(0, 0) && outTex.GetPixel(1, 1) != tex.GetPixel(1, 1))
                Assert.Fail($"Textures are not the same");

            Assert.Pass();
        }
        
        [Test]
        public void GetCachedTexture_NoFile()
        {
            Assert.IsFalse(AssetStoreCache.GetCachedTexture("999999999", out _));
        }

        [Test]
        public void CacheUploadSelections_NewJson()
        {
            var packageId = "123456";
            var path = Path.Combine(AssetStoreCache.PersistentCachePath, $"{packageId}-uploadselection.asset");

            JsonValue selection = new JsonValue
            {
                [FolderUploadWorkflowView.WorkflowName] = "testMetadata",
                [UnityPackageUploadWorkflowView.WorkflowName] = "testMetadata2",
                [HybridPackageUploadWorkflowView.WorkflowName] = "testMetadata3",
            };

            AssetStoreCache.CacheUploadSelections(packageId, selection);

            if (!File.Exists(path))
                Assert.Fail($"Upload selection cache file does not exist");

            Assert.Pass();
        }

        [Test]
        public void GetCachedUploadSelections_FileExists()
        {
            var packageId = "123456";
            var path = Path.Combine(AssetStoreCache.PersistentCachePath, $"{packageId}-uploadselection.asset");

            JsonValue selection = new JsonValue
            {
                [FolderUploadWorkflowView.WorkflowName] = "testMetadata",
                [UnityPackageUploadWorkflowView.WorkflowName] = "testMetadata2",
                [HybridPackageUploadWorkflowView.WorkflowName] = "testMetadata3",
            };

            AssetStoreCache.CacheUploadSelections(packageId, selection);

            if (!File.Exists(path))
                Assert.Fail($"Upload selection cache file does not exist");

            if (!AssetStoreCache.GetCachedUploadSelections(packageId, out var data))
                Assert.Fail($"Metadata file does not exist");

            if (data[FolderUploadWorkflowView.WorkflowName].AsString() != selection[FolderUploadWorkflowView.WorkflowName].AsString()
                && data[UnityPackageUploadWorkflowView.WorkflowName].AsString() != selection[UnityPackageUploadWorkflowView.WorkflowName].AsString())
                Assert.Fail($"Scraped data was not the same as the stored one." +
                            $"\n{data[FolderUploadWorkflowView.WorkflowName]} " +
                            $"{data[UnityPackageUploadWorkflowView.WorkflowName]}");

            Assert.Pass();
        }

        [Test]
        public void GetCachedUploadSelections_NoFile()
        {
            var packageId = "123456";
            
            var path = Path.Combine(AssetStoreCache.PersistentCachePath, $"{packageId}-uploadselection.asset");

            if (File.Exists(path))
                File.Delete(path);
            
            Assert.IsFalse(AssetStoreCache.GetCachedUploadSelections(packageId, out _));
        }
        
    }
}