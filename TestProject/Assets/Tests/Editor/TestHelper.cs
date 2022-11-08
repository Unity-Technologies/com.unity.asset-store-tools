using System;
using NUnit.Framework;

namespace Tests.Editor
{
    public static class TestHelper
    {
        public static void AssertIgnoreIfYamato()
        {
            var result = Environment.GetEnvironmentVariable("YAMATO_PROJECT_NAME");
            if(result != null && result == "asset-store-tools-v2")
                Assert.Ignore("This test has been executed on Yamato. Ignoring...");
        }
    }
}