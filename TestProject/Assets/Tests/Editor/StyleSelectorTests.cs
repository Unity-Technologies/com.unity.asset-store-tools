using AssetStoreTools.Uploader;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace Tests.Editor
{
    public class StyleSelectorTests
    {
        [Test]
        public void SetStyle_BaseWindow_Light()
        {
            var element = new VisualElement();
            var window = StyleSelector.Style.Base;
            var isLightTheme = true;
            
            var stylesPath = "Packages/com.unity.asset-store-tools/Editor/AssetStoreUploader/Styles/";
            var windowMain = "Base/BaseWindow_Main.uss";
            var windowLight = "Base/BaseWindow_Light.uss";
            
            var styleAssetMain = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesPath + windowMain);
            var styleAssetLight = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesPath + windowLight);

            StyleSelector.SetStyle(element, window, isLightTheme);
            
            if (element.styleSheets.Contains(styleAssetMain) && element.styleSheets.Contains(styleAssetLight))
                Assert.Pass();
            
            Assert.Fail($"StyleSheets found: {element.styleSheets.count}\n" +
                        $"Main: {element.styleSheets.Contains(styleAssetMain)}\n" +
                        $"Colors: {element.styleSheets.Contains(styleAssetLight)}");
        }
        
        [Test]
        public void SetStyle_BaseWindow_Dark()
        {
            var element = new VisualElement();
            var window = StyleSelector.Style.Base;
            var isLightTheme = false;
            
            var stylesPath = "Packages/com.unity.asset-store-tools/Editor/AssetStoreUploader/Styles/";
            var windowMain = "Base/BaseWindow_Main.uss";
            var windowDark = "Base/BaseWindow_Dark.uss";
            
            var styleAssetMain = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesPath + windowMain);
            var styleAssetDark = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesPath + windowDark);

            StyleSelector.SetStyle(element, window, isLightTheme);
            
            if (element.styleSheets.Contains(styleAssetMain) && element.styleSheets.Contains(styleAssetDark))
                Assert.Pass();
            
            Assert.Fail($"StyleSheets found: {element.styleSheets.count}\n" +
                        $"Main: {element.styleSheets.Contains(styleAssetMain)}\n" +
                        $"Colors: {element.styleSheets.Contains(styleAssetDark)}");
        }
        
        [Test]
        public void SetStyle_LoginWindow_Light()
        {
            var element = new VisualElement();
            var window = StyleSelector.Style.Login;
            var isLightTheme = true;
            
            var stylesPath = "Packages/com.unity.asset-store-tools/Editor/AssetStoreUploader/Styles/";
            var windowMain = "Login/Login_Main.uss";
            var windowLight = "Login/Login_Light.uss";
            
            var styleAssetMain = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesPath + windowMain);
            var styleAssetLight = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesPath + windowLight);

            StyleSelector.SetStyle(element, window, isLightTheme);
            
            if (element.styleSheets.Contains(styleAssetMain) && element.styleSheets.Contains(styleAssetLight))
                Assert.Pass();
            
            Assert.Fail($"StyleSheets found: {element.styleSheets.count}\n" +
                        $"Main: {element.styleSheets.Contains(styleAssetMain)}\n" +
                        $"Colors: {element.styleSheets.Contains(styleAssetLight)}");
        }
        
        [Test]
        public void SetStyle_LoginWindow_Dark()
        {
            var element = new VisualElement();
            var window = StyleSelector.Style.Login;
            var isLightTheme = false;
            
            var stylesPath = "Packages/com.unity.asset-store-tools/Editor/AssetStoreUploader/Styles/";
            var windowMain = "Login/Login_Main.uss";
            var windowDark = "Login/Login_Dark.uss";
            
            var styleAssetMain = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesPath + windowMain);
            var styleAssetDark = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesPath + windowDark);

            StyleSelector.SetStyle(element, window, isLightTheme);
            
            if (element.styleSheets.Contains(styleAssetMain) && element.styleSheets.Contains(styleAssetDark))
                Assert.Pass();
            
            Assert.Fail($"StyleSheets found: {element.styleSheets.count}\n" +
                        $"Main: {element.styleSheets.Contains(styleAssetMain)}\n" +
                        $"Colors: {element.styleSheets.Contains(styleAssetDark)}");
        }
        
        [Test]
        public void SetStyle_UploadWindow_Light()
        {
            var element = new VisualElement();
            var window = StyleSelector.Style.UploadWindow;
            var isLightTheme = true;
            
            var stylesPath = "Packages/com.unity.asset-store-tools/Editor/AssetStoreUploader/Styles/";
            var windowMain = "Upload/UploadWindow_Main.uss";
            var windowLight = "Upload/UploadWindow_Light.uss";
            
            var styleAssetMain = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesPath + windowMain);
            var styleAssetLight = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesPath + windowLight);

            StyleSelector.SetStyle(element, window, isLightTheme);
            
            if (element.styleSheets.Contains(styleAssetMain) && element.styleSheets.Contains(styleAssetLight))
                Assert.Pass();
            
            Assert.Fail($"StyleSheets found: {element.styleSheets.count}\n" +
                        $"Main: {element.styleSheets.Contains(styleAssetMain)}\n" +
                        $"Colors: {element.styleSheets.Contains(styleAssetLight)}");
        }
        
        [Test]
        public void SetStyle_UploadWindow_Dark()
        {
            var element = new VisualElement();
            var window = StyleSelector.Style.UploadWindow;
            var isLightTheme = false;
            
            var stylesPath = "Packages/com.unity.asset-store-tools/Editor/AssetStoreUploader/Styles/";
            var windowMain = "Upload/UploadWindow_Main.uss";
            var windowDark = "Upload/UploadWindow_Dark.uss";
            
            var styleAssetMain = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesPath + windowMain);
            var styleAssetDark = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesPath + windowDark);

            StyleSelector.SetStyle(element, window, isLightTheme);
            
            if (element.styleSheets.Contains(styleAssetMain) && element.styleSheets.Contains(styleAssetDark))
                Assert.Pass();
            
            Assert.Fail($"StyleSheets found: {element.styleSheets.count}\n" +
                        $"Main: {element.styleSheets.Contains(styleAssetMain)}\n" +
                        $"Colors: {element.styleSheets.Contains(styleAssetDark)}");
        }
        
                [Test]
        public void SetStyle_AllPackagesWindow_Light()
        {
            var element = new VisualElement();
            var window = StyleSelector.Style.AllPackages;
            var isLightTheme = true;
            
            var stylesPath = "Packages/com.unity.asset-store-tools/Editor/AssetStoreUploader/Styles/";
            var windowMain = "Upload/AllPackages/AllPackages_Main.uss";
            var windowLight = "Upload/AllPackages/AllPackages_Light.uss";
            
            var styleAssetMain = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesPath + windowMain);
            var styleAssetLight = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesPath + windowLight);

            StyleSelector.SetStyle(element, window, isLightTheme);
            
            if (element.styleSheets.Contains(styleAssetMain) && element.styleSheets.Contains(styleAssetLight))
                Assert.Pass();
            
            Assert.Fail($"StyleSheets found: {element.styleSheets.count}\n" +
                        $"Main: {element.styleSheets.Contains(styleAssetMain)}\n" +
                        $"Colors: {element.styleSheets.Contains(styleAssetLight)}");
        }
        
        [Test]
        public void SetStyle_AllPackagesWindow_Dark()
        {
            var element = new VisualElement();
            var window = StyleSelector.Style.AllPackages;
            var isLightTheme = false;
            
            var stylesPath = "Packages/com.unity.asset-store-tools/Editor/AssetStoreUploader/Styles/";
            var windowMain = "Upload/AllPackages/AllPackages_Main.uss";
            var windowDark = "Upload/AllPackages/AllPackages_Dark.uss";
            
            var styleAssetMain = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesPath + windowMain);
            var styleAssetDark = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesPath + windowDark);

            StyleSelector.SetStyle(element, window, isLightTheme);
            
            if (element.styleSheets.Contains(styleAssetMain) && element.styleSheets.Contains(styleAssetDark))
                Assert.Pass();
            
            Assert.Fail($"StyleSheets found: {element.styleSheets.count}\n" +
                        $"Main: {element.styleSheets.Contains(styleAssetMain)}\n" +
                        $"Colors: {element.styleSheets.Contains(styleAssetDark)}");
        }
        
    }
}