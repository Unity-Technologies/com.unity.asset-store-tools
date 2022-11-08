using System;
using System.Net;
using System.Net.Http;
using NUnit.Framework;
using AssetStoreTools.Uploader;

namespace Tests.Editor
{
    public class ASErrorTests
    {
        [Test]
        public void GenericError_WithMessage()
        {
            ASError asError = ASError.GetGenericError(new Exception("Ctrl-Z"));

            Assert.AreEqual("Ctrl-Z", asError.Exception.Message);
            Assert.AreEqual("Ctrl-Z", asError.Message);
        }

        [Test]
        public void GetLoginError_NoException()
        {
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            ASError asError = ASError.GetLoginError(message);

            Assert.AreEqual("Incorrect email and/or password. Please try again.", asError.Message);
        }
        
        [Test]
        public void GetLoginError_WithException()
        {
            HttpRequestException ex = new HttpRequestException("CTRL-A");
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.Forbidden);
            HttpContent content = new StringContent("<!DOCTYPE HTML\n<a>Hello World</a> <p>Incorrect email and/or password. Please try again.</p>");
            message.Content = content;
            
            ASError asError = ASError.GetLoginError(message, ex);

            Assert.AreEqual("An undefined error has been encountered with the following message:\n\nIncorrect " +
                            "email and/or password. Please try again.\n\nIf this error message is not very " +
                            "informative, please report this to Unity", asError.Message);
        }
        
        [Test]
        public void GetPublisherNullError()
        {
            ASError asError = ASError.GetPublisherNullError("BigHead");

            Assert.AreEqual($"Your Unity ID BigHead is not currently connected to a publisher account. " +
                            $"Please create a publisher profile.", asError.Message);
        }
    }
}