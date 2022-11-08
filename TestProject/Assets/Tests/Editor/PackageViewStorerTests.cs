using System.Globalization;
using AssetStoreTools.Uploader;
using NUnit.Framework;

namespace Tests.Editor
{
    public class PackageViewStorerTests
    {
        [Test]
        public void GetPackage_NewPackage()
        {
            var packageId = "123";
            var versionId = "1234";
            var packageName = "NewName";
            var status = "PENDINGREVIEW";
            var category = "2D";
            var lastDate = "1337-03-08 12:23:43";
            var lastSize = (1024f * 1024f).ToString(CultureInfo.InvariantCulture);
            var isCompleteProject = false;

            var lastUploadedPath = "Assets/SomePath";
            var lastUploadedGuid = "0123456789abcdef";
            var lastDateResult = "1337-03-08";
            var lastSizeResult = "1.00 MB";

            var packageData = new PackageData(packageId, packageName, versionId, status, category, isCompleteProject, lastUploadedPath, lastUploadedGuid, lastDate, lastSize);

            var package = PackageViewStorer.GetPackage(packageData);
            
            Assert.IsTrue(package.PackageId == packageId);
            Assert.IsTrue(package.VersionId == versionId);
            Assert.IsTrue(package.PackageName == packageName);
            Assert.IsTrue(package.Status == status);
            Assert.IsTrue(package.Category == category);
            Assert.IsTrue(package.LastUpdatedDate == lastDateResult);
            Assert.IsTrue(package.LastUpdatedSize == lastSizeResult);
            Assert.IsTrue(package.IsCompleteProject == isCompleteProject);
            Assert.IsTrue(package.LastUploadedGuid == lastUploadedGuid);
            Assert.IsTrue(package.LastUploadedPath == lastUploadedPath);
        }

        [Test]
        public void GetPackage_UpdateValues()
        {
            var packageId = "123";
            var versionId = "1234";
            var packageName = "NewName";
            var status = "PENDINGREVIEW";
            var category = "2D";
            var lastDate = "1337-03-08 12:23:43";
            var lastSize = (1024f * 1024f).ToString(CultureInfo.InvariantCulture);
            var isCompleteProject = false;

            var lastUploadedPath = "Assets/SomePath";
            var lastUploadedGuid = "0123456789abcdef";

            var newStatus = "DEPRECATED";
            var newSize = (4f * 1024f * 1024f).ToString(CultureInfo.InvariantCulture);
            var newDate = "2000-09-15 12:23:43";

            var packageData = new PackageData(packageId, packageName, versionId, status, category, isCompleteProject, lastUploadedPath, lastUploadedGuid, lastDate, lastSize);
            var packageData2 = new PackageData(packageId, packageName, versionId, newStatus, category, isCompleteProject, lastUploadedPath, lastUploadedGuid, newDate, newSize);

            var package = PackageViewStorer.GetPackage(packageData);
            var package2 = PackageViewStorer.GetPackage(packageData2);

            // Should reference to the same Package
            Assert.IsTrue(package == package2);
        }

        [Test]
        public void GetPackage_ResetPackages()
        {
            var packageId = "123";
            var versionId = "1234";
            var packageName = "NewName";
            var status = "PENDINGREVIEW";
            var category = "2D";
            var lastDate = "1337-03-08 12:23:43";
            var lastSize = (1024f * 1024f).ToString(CultureInfo.InvariantCulture);
            var isCompleteProject = false;

            var lastUploadedPath = "Assets/SomePath";
            var lastUploadedGuid = "0123456789abcdef";

            var newStatus = "DEPRECATED";
            var newSize = (4f * 1024f * 1024f).ToString(CultureInfo.InvariantCulture);
            var newDate = "2000-09-15 12:23:43";

            var packageData = new PackageData(packageId, packageName, versionId, status, category, isCompleteProject, lastUploadedPath, lastUploadedGuid, lastDate, lastSize);

            var package = PackageViewStorer.GetPackage(packageData);

            PackageViewStorer.Reset();

            var packageData2 = new PackageData(packageId, packageName, versionId, newStatus, category, isCompleteProject, lastUploadedPath, lastUploadedGuid, newDate, newSize);

            var package2 = PackageViewStorer.GetPackage(packageData2);

            // Should not reference to the same Package
            Assert.IsTrue(package != package2);
        }
    }
}