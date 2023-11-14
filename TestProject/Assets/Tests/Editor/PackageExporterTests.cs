using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.TestTools;
using AssetStoreTools.Utility.Json;
using AssetStoreTools.Exporter;

namespace Tests.Editor
{
    public class PackageExporterTests
    {
        private const string CachePath = "Temp/TestCache";

        private const string MultiPackageReference = "Assets/Tests/Resources/MultiPackage.unitypackage";
        private const string HybridPackageReference = "Assets/Tests/Resources/HybridPackage.unitypackage";

        private readonly string[] MultiPackagePathInProject = { "Assets/SmallPackage", "Assets/StreamingAssets", "Assets/WebGLTemplates" };
        private readonly string[] HybridPackagePathInProject = { "Packages/com.hybrid.package" };

        private readonly string[] PackmanDependencies = { "com.unity.ide.visualstudio", "com.unity.ide.vscode" };

        private readonly string[] ProjectSettingsExcludeList =
        { 
            "ProjectSettings/boot.config", "ProjectSettings/MemorySettings.asset", "ProjectSettings/PackageManagerSettings.asset", 
            "ProjectSettings/ProjectVersion.txt", "ProjectSettings/SceneTemplateSettings.json", "ProjectSettings/VersionControlSettings.asset", 
            "ProjectSettings/XRSettings.asset"
        };

        [SetUp]
        public void Setup()
        {
            if (!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(CachePath))
                Directory.Delete(CachePath, true);
        }

        private void ExtractPackage(string packagePath, out string extractedPath)
        {
#if UNITY_EDITOR_WIN
            string _7zPath = Path.Combine(EditorApplication.applicationContentsPath, "Tools", "7z.exe");
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            string _7zPath = Path.Combine(EditorApplication.applicationContentsPath, "Tools", "7za");
#endif  
            extractedPath = string.Empty;
            var workingDirectory = Path.GetFullPath(CachePath);
            var inputArgument = Path.GetFullPath(packagePath);
            var outputArgument = $"Uncompressed_{Path.GetFileNameWithoutExtension(packagePath)}";
            var arguments = $"x \"{inputArgument}\" -o\"{outputArgument}\"";
            
            UnityEngine.Debug.Log($"{_7zPath} {arguments}");
            
            var info = new ProcessStartInfo()
            {
                FileName = _7zPath, 
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            
            using (Process process = Process.Start(info))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                    throw new Exception($"7-zip process has returned non-zero exit code ({process.ExitCode})\nOutput:\n{process.StandardOutput.ReadToEnd()}");
            }

            workingDirectory = Path.Combine(workingDirectory, outputArgument);
            inputArgument = new DirectoryInfo(workingDirectory).GetFiles()[0].Name;
            outputArgument = $"Unzipped_{Path.GetFileNameWithoutExtension(packagePath)}";

            var info2 = new ProcessStartInfo()
            {
                FileName = _7zPath,
                Arguments = $"x {inputArgument} -o\"{outputArgument}\"",
                WorkingDirectory = workingDirectory,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            
            using (Process process = Process.Start(info2))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                    throw new Exception($"7-zip process has returned non-zero exit code ({process.ExitCode})\nOutput:\n{process.StandardOutput.ReadToEnd()}");
            }

            extractedPath = Path.Combine(CachePath, $"Uncompressed_{Path.GetFileNameWithoutExtension(packagePath)}",
                $"Unzipped_{Path.GetFileNameWithoutExtension(packagePath)}");
        }

        private void CompareFilesInPaths(DirectoryInfo expectedDir, DirectoryInfo actualDir, bool expectManifest = false, bool expectProjectSettings = false)
        {
            var actualFiles = new Dictionary<string, string[]>();
            var expectedFiles = new Dictionary<string, string[]>();

            foreach (var folder in actualDir.GetDirectories())
            {
                if (folder.Name == "packagemanagermanifest")
                {
                    actualFiles.Add("packagemanagermanifest", folder.GetFiles().Select(x => x.Name).ToArray());
                    continue;
                }
                
                var filePathAsset = $"{folder.FullName}/pathname";
                var fileName = File.ReadAllText(filePathAsset).Trim();
                var files = folder.GetFiles().Select(x => x.Name).ToArray();
                actualFiles.Add(fileName, files);
            }

            foreach(var folder in expectedDir.GetDirectories())
            {
                if (folder.Name == "packagemanagermanifest")
                {
                    expectedFiles.Add("packagemanagermanifest", folder.GetFiles().Select(x => x.Name).ToArray());
                    continue;
                }

                var filePathAsset = $"{folder.FullName}/pathname";
                var fileName = File.ReadAllLines(filePathAsset)[0].Trim(); // Some lines may end with a '00' in newline
                var files = folder.GetFiles().Select(x => x.Name).ToArray();
                expectedFiles.Add(fileName, files);
            }

            if (expectManifest)
                CheckForManifest(expectedFiles, actualFiles);

            if (expectProjectSettings)
                CheckForProjectSettings(expectedFiles, actualFiles);

            CompareFiles(expectedFiles, actualFiles);
        }

        private void CheckForManifest(Dictionary<string, string[]> expectedFiles, Dictionary<string, string[]> actualFiles)
        {
            if (!actualFiles.ContainsKey("packagemanagermanifest"))
                Assert.Fail("Exported package does not contain a package manager manifest");

            Assert.IsTrue(actualFiles["packagemanagermanifest"][0] == "asset");

            actualFiles.Remove("packagemanagermanifest");

            // Remove manifest from expected files as well since it's been checked
            if (expectedFiles.ContainsKey("packagemanagermanifest"))
                expectedFiles.Remove("packagemanagermanifest");
        }

        private void CheckForProjectSettings(Dictionary<string, string[]> expectedFiles, Dictionary<string, string[]> actualFiles)
        {
            var expectedProjectSettings = Directory.GetFiles("ProjectSettings").Where(x => !ProjectSettingsExcludeList.Contains(x.Replace("\\", "/"))).Select(x => x.Replace("\\", "/")).ToArray();
                var missingFiles = new List<string>();

            foreach(var file in expectedProjectSettings)
            {
                if (!actualFiles.ContainsKey(file))
                {
                    missingFiles.Add(file);
                    continue;
                }

                actualFiles.Remove(file);

                // Remove this file from expected files as well since it's been checked
                if (expectedFiles.ContainsKey(file))
                    expectedFiles.Remove(file);
            }

            if(missingFiles.Count > 0)
            {
                var message = "Exported package has missing project settings:";
                foreach (var f in missingFiles)
                    message += $"\n{f}";
                Assert.Fail(message);
            }
        }

        private void CompareFiles(Dictionary<string, string[]> expectedFiles, Dictionary<string, string[]> actualFiles)
        {
            var additionalFolders = new List<string>();
            var missingFolders = new List<string>();
            var additionalFiles = new List<string>();
            var missingFiles = new List<string>();

            // Populate missing files
            foreach(var kvp in expectedFiles)
            {
                if (!actualFiles.ContainsKey(kvp.Key))
                {
                    missingFolders.Add(kvp.Key);
                    continue;
                }

                // Check for pathname
                if (kvp.Value.Any(x => x == "pathname") && !actualFiles[kvp.Key].Any(x => x == "pathname"))
                    missingFiles.Add($"{kvp.Key}/pathname");

                // Check for asset
                if (kvp.Value.Any(x => x == "asset") && !actualFiles[kvp.Key].Any(x => x == "asset"))
                    missingFiles.Add($"{kvp.Key}/asset");

                // Check for asset meta
                if (kvp.Value.Any(x => x == "asset.meta") && !actualFiles[kvp.Key].Any(x => x == "asset.meta"))
                    missingFiles.Add($"{kvp.Key}/asset.meta");

                // Check for asset preview
                if (kvp.Value.Any(x => x == "preview.png") && !actualFiles[kvp.Key].Any(x => x == "preview.png"))
                    missingFiles.Add($"{kvp.Key}/preview.png");
            }

            // Populate additional files
            foreach (var kvp in actualFiles)
            {
                if (!expectedFiles.ContainsKey(kvp.Key))
                {
                    additionalFolders.Add(kvp.Key);
                    continue;
                }

                // Check for pathname
                if (kvp.Value.Any(x => x == "pathname") && !expectedFiles[kvp.Key].Any(x => x == "pathname"))
                    additionalFiles.Add($"{kvp.Key}/pathname");

                // Check for asset
                if (kvp.Value.Any(x => x == "asset") && !expectedFiles[kvp.Key].Any(x => x == "asset"))
                    additionalFiles.Add($"{kvp.Key}/asset");

                // Check for asset meta
                if (kvp.Value.Any(x => x == "asset.meta") && !expectedFiles[kvp.Key].Any(x => x == "asset.meta"))
                    additionalFiles.Add($"{kvp.Key}/asset.meta");

                // Check for asset preview
                if (kvp.Value.Any(x => x == "preview.png") && !expectedFiles[kvp.Key].Any(x => x == "preview.png"))
                    additionalFiles.Add($"{kvp.Key}/preview.png");
            }

            if(missingFolders.Count != 0)
            {
                var message = "Exported package contains the following missing folders:";
                foreach (var f in missingFolders)
                    message += $"\n{f}";
                Assert.Fail(message);
            }

            if (missingFiles.Count != 0)
            {
                var message = "Exported package contains the following missing files:";
                foreach (var f in missingFiles)
                    message += $"\n{f}";
                Assert.Fail(message);
            }

            if (additionalFolders.Count != 0)
            {
                var message = "Exported package contains the following additional folders:";
                foreach (var f in additionalFolders)
                    message += $"\n{f}";
                UnityEngine.Debug.LogWarning(message);
            }

            if (additionalFiles.Count != 0)
            {
                var message = "Exported package contains the following additional files:";
                foreach (var f in additionalFiles)
                    message += $"\n{f}";
                UnityEngine.Debug.LogWarning(message);
            }

        }

        [UnityTest]
        public IEnumerator ExportPackage_Native_Basic()
        {
            var exportPaths = MultiPackagePathInProject;
            var outputPath = Path.Combine(CachePath, "Actual.unitypackage");

            var settings = new NativeExporterSettings()
            {
                ExportPaths = exportPaths,
                OutputFilename = outputPath,
                IncludeDependencies = false
            };
            var exportTask = PackageExporter.ExportPackage(settings);

            while (!exportTask.IsCompleted)
                yield return null;

            var result = exportTask.Result;
            Assert.IsTrue(result.Success);
            Assert.IsTrue(File.Exists(outputPath));

            ExtractPackage(MultiPackageReference, out string extractedExpectedPath);
            ExtractPackage(outputPath, out string extractedActualPath);

            var expectedDir = new DirectoryInfo(extractedExpectedPath);
            var actualDir = new DirectoryInfo(extractedActualPath);

            CompareFilesInPaths(expectedDir, actualDir, false, false);
        }

        [UnityTest]
        public IEnumerator ExportPackage_Native_ManifestAndProjectSettings()
        {
            var exportPaths = MultiPackagePathInProject;
            var tempArray = new string[exportPaths.Length + 1];
            Array.Copy(exportPaths, tempArray, exportPaths.Length);
            tempArray[tempArray.Length - 1] = "ProjectSettings";
            exportPaths = tempArray;

            var outputPath = Path.Combine(CachePath, "Actual.unitypackage");

            var settings = new NativeExporterSettings()
            {
                ExportPaths = exportPaths,
                OutputFilename = outputPath,
                IncludeDependencies = true
            };
            var exportTask = PackageExporter.ExportPackage(settings);

            while (!exportTask.IsCompleted)
                yield return null;

            var result = exportTask.Result;
            Assert.IsTrue(result.Success);
            Assert.IsTrue(File.Exists(outputPath));

            ExtractPackage(MultiPackageReference, out string extractedExpectedPath);
            ExtractPackage(outputPath, out string extractedActualPath);

            var expectedDir = new DirectoryInfo(extractedExpectedPath);
            var actualDir = new DirectoryInfo(extractedActualPath);

            CompareFilesInPaths(expectedDir, actualDir, true, true);
        }

        [UnityTest]
        public IEnumerator ExportPackage_Native_InvalidPath()
        {
            var exportPaths = new string[0];
            var outputPath = Path.Combine(CachePath, "Actual.unitypackage");

            // Empty export path check
            var settings = new NativeExporterSettings()
            {
                ExportPaths = exportPaths,
                OutputFilename = outputPath,
                IncludeDependencies = false
            };
            var exportTask = PackageExporter.ExportPackage(settings);

            while (!exportTask.IsCompleted)
                yield return null;

            var result = exportTask.Result;
            Assert.IsFalse(result.Success);


            // Non-existent export path check
            settings.ExportPaths = new string[] { "This/Path/Does/Not/Exist" };
            exportTask = PackageExporter.ExportPackage(settings);

            while (!exportTask.IsCompleted)
                yield return null;

            result = exportTask.Result;
            Assert.IsFalse(result.Success);


            // Empty output path check
            settings.ExportPaths = MultiPackagePathInProject;
            settings.OutputFilename = string.Empty;
            exportTask = PackageExporter.ExportPackage(settings);

            while (!exportTask.IsCompleted)
                yield return null;

            result = exportTask.Result;
            Assert.IsFalse(result.Success);


            // Null output path check
            settings.ExportPaths = MultiPackagePathInProject;
            settings.OutputFilename = null;
            exportTask = PackageExporter.ExportPackage(settings);

            while (!exportTask.IsCompleted)
                yield return null;

            result = exportTask.Result;
            Assert.IsFalse(result.Success);
        }

        [UnityTest]
        public IEnumerator ExportPackage_Custom_Basic()
        {
            var exportPaths = MultiPackagePathInProject;
            var outputPath = Path.Combine(CachePath, "Actual.unitypackage");

            var settings = new CustomExporterSettings()
            {
                ExportPaths = exportPaths,
                OutputFilename = outputPath
            };
            var exportTask = PackageExporter.ExportPackage(settings);

            while (!exportTask.IsCompleted)
                yield return null;

            var result = exportTask.Result;
            Assert.IsTrue(result.Success);
            Assert.IsTrue(File.Exists(outputPath));

            ExtractPackage(MultiPackageReference, out string extractedExpectedPath);
            ExtractPackage(outputPath, out string extractedActualPath);

            var expectedDir = new DirectoryInfo(extractedExpectedPath);
            var actualDir = new DirectoryInfo(extractedActualPath);

            CompareFilesInPaths(expectedDir, actualDir, false, false);
        }

        [UnityTest]
        public IEnumerator ExportPackage_Custom_ManifestAndProjectSettings()
        {
            var exportPaths = MultiPackagePathInProject;
            var outputPath = Path.Combine(CachePath, "Actual.unitypackage");

            var tempArray = new string[exportPaths.Length + 1];
            Array.Copy(exportPaths, tempArray, exportPaths.Length);
            tempArray[tempArray.Length - 1] = "ProjectSettings";
            exportPaths = tempArray;

            var settings = new CustomExporterSettings()
            {
                ExportPaths = exportPaths,
                OutputFilename = outputPath,
                Dependencies = PackmanDependencies
            };
            var exportTask = PackageExporter.ExportPackage(settings);

            while (!exportTask.IsCompleted)
                yield return null;

            var result = exportTask.Result;
            Assert.IsTrue(result.Success);
            Assert.IsTrue(File.Exists(outputPath));

           ExtractPackage(MultiPackageReference, out string extractedExpectedPath);
           ExtractPackage(outputPath, out string extractedActualPath);

            var expectedDir = new DirectoryInfo(extractedExpectedPath);
            var actualDir = new DirectoryInfo(extractedActualPath);

            CompareFilesInPaths(expectedDir, actualDir, true, true);

            var manifestFileText = File.ReadAllText($"{actualDir.FullName}/packagemanagermanifest/asset");
            var manifestFileJson = JSONParser.SimpleParse(manifestFileText);
            var actualDependencies = manifestFileJson["dependencies"].AsDict();
            Assert.AreEqual(2, actualDependencies.Count, "Dependency count in created package does not match the expected count");
            foreach (var d in PackmanDependencies)
                Assert.IsTrue(actualDependencies.ContainsKey(d));
        }

        [UnityTest]
        public IEnumerator ExportPackage_Custom_InvalidPath()
        {
            var exportPaths = new string[0];
            var outputPath = Path.Combine(CachePath, "Actual.unitypackage");

            // Empty export path check
            var settings = new CustomExporterSettings()
            {
                ExportPaths = exportPaths,
                OutputFilename = outputPath,
            };
            var exportTask = PackageExporter.ExportPackage(settings);

            while (!exportTask.IsCompleted)
                yield return null;

            var result = exportTask.Result;
            Assert.IsFalse(result.Success);


            // Non-existent export path check
            exportPaths = new string[] { "This/Path/Does/Not/Exist" };

            settings.ExportPaths = exportPaths;
            exportTask = PackageExporter.ExportPackage(settings);

            while (!exportTask.IsCompleted)
                yield return null;

            result = exportTask.Result;
            Assert.IsFalse(result.Success);


            // Empty output path check
            settings.ExportPaths = MultiPackagePathInProject;
            settings.OutputFilename = string.Empty;
            exportTask = PackageExporter.ExportPackage(settings);

            while (!exportTask.IsCompleted)
                yield return null;

            result = exportTask.Result;
            Assert.IsFalse(result.Success);


            // Null output path check
            settings.ExportPaths = MultiPackagePathInProject;
            settings.OutputFilename = null;
            exportTask = PackageExporter.ExportPackage(settings);

            while (!exportTask.IsCompleted)
                yield return null;

            result = exportTask.Result;
            Assert.IsFalse(result.Success);
        }

        [UnityTest]
        public IEnumerator ExportPackage_Custom_Hybrid()
        {
            var exportPaths = HybridPackagePathInProject;
            var outputPath = Path.Combine(CachePath, "Actual.unitypackage");

            var settings = new CustomExporterSettings()
            {
                ExportPaths = exportPaths,
                OutputFilename = outputPath,
            };
            var exportTask = PackageExporter.ExportPackage(settings);

            while (!exportTask.IsCompleted)
                yield return null;

            var result = exportTask.Result;
            Assert.IsTrue(result.Success);
            Assert.IsTrue(File.Exists(outputPath));

            ExtractPackage(HybridPackageReference, out string extractedExpectedPath);
            ExtractPackage(outputPath, out string extractedActualPath);

            var expectedDir = new DirectoryInfo(extractedExpectedPath);
            var actualDir = new DirectoryInfo(extractedActualPath);

            CompareFilesInPaths(expectedDir, actualDir, false, false);
        }

        // [UnityTest, Timeout(3600000)]
        // public IEnumerator ExportPackage_Custom_LargePackage()
        // {
        //     TestHelper.AssertIgnoreIfYamato(); // Ignore on Yamato due to long run times
        //     
        //     yield return ImportPackage(LargePackageReference);
        //
        //     var exportPaths = LargePackagePathInProject;
        //     var outputPath = Path.Combine(CachePath, "Actual.unitypackage");
        //
        //     var exportTask = PackageExporter.ExportPackage(exportPaths, outputPath, false, false, true);
        //
        //     while (!exportTask.IsCompleted)
        //         yield return null;
        //
        //     var result = exportTask.Result;
        //     Assert.IsTrue(result.Success);
        //     Assert.IsTrue(File.Exists(outputPath));
        //
        //     ExtractPackage(LargePackageReference, out string extractedExpectedPath);
        //     ExtractPackage(outputPath, out string extractedActualPath);
        //
        //     var expectedDir = new DirectoryInfo(extractedExpectedPath);
        //     var actualDir = new DirectoryInfo(extractedActualPath);
        //
        //     CompareFilesInPaths(expectedDir, actualDir, false, false);
        // }
    }
}
