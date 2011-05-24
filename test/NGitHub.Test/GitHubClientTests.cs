﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RestSharp;

namespace NGitHub.Test {
    [TestClass]
    public class GitHubClientTests {
        [TestMethod]
        public void CallApiAsync_ShouldExecuteRestRequestWithGivenResourceAndMethod() {
            var expectedResource = "foo/bar";
            var expectedMethod = Method.POST;
            var mockRestClient = new Mock<IRestClient>(MockBehavior.Strict);
            var mockFactory = new Mock<GitHubClient.IRestClientFactory>(MockBehavior.Strict);
            mockFactory.Setup<IRestClient>(f => f.CreateRestClient(Constants.ApiV3Url))
                       .Returns(mockRestClient.Object);
            mockRestClient.Setup(c => c.ExecuteAsync<object>(
                It.Is<RestRequest>(r => (r.Resource == expectedResource) &&
                                        (r.Method == expectedMethod)),
                It.IsAny<Action<RestResponse<object>>>()));
            var githubClient = new GitHubClient(mockFactory.Object);

            githubClient.CallApiAsync<object>(expectedResource, expectedMethod, o => { }, e => { });

            mockRestClient.VerifyAll();
        }

        [TestMethod]
        public void CallApiAsync_ShouldInvokeCallbackMethod_WhenRestRequestCompletesSuccessfully() {
            var mockRestClient = new Mock<IRestClient>(MockBehavior.Strict);
            var response = new RestResponse<object>() { ResponseStatus = ResponseStatus.Completed };
            mockRestClient
                .Setup(c => c.ExecuteAsync<object>(It.IsAny<RestRequest>(), It.IsAny<Action<RestResponse<object>>>()))
                .Callback<RestRequest, Action<RestResponse<object>>>((r, c) => c(response));
            var mockFactory = new Mock<GitHubClient.IRestClientFactory>(MockBehavior.Strict);
            mockFactory.Setup<IRestClient>(f => f.CreateRestClient(Constants.ApiV3Url)).Returns(mockRestClient.Object);

            var client = new GitHubClient(mockFactory.Object);

            var callbackInvoked = false;
            client.CallApiAsync<object>(
                "foo",
                Method.GET,
                o => callbackInvoked = true,
                e => { });

            Assert.IsTrue(callbackInvoked);
        }

        [TestMethod]
        public void CallApiAsync_ShouldPassResponseDataToCallback_WhenRestRequestCompletesSuccessfully() {
            var mockRestClient = new Mock<IRestClient>(MockBehavior.Strict);
            var expectedData = new object();
            var response = new RestResponse<object>() {
                ResponseStatus = ResponseStatus.Completed,
                Data = expectedData
            };
            mockRestClient
                .Setup(c => c.ExecuteAsync<object>(It.IsAny<RestRequest>(), It.IsAny<Action<RestResponse<object>>>()))
                .Callback<RestRequest, Action<RestResponse<object>>>((r, c) => c(response));
            var mockFactory = new Mock<GitHubClient.IRestClientFactory>(MockBehavior.Strict);
            mockFactory.Setup<IRestClient>(f => f.CreateRestClient(Constants.ApiV3Url)).Returns(mockRestClient.Object);

            var client = new GitHubClient(mockFactory.Object);
            client.CallApiAsync<object>(
                "foo",
                Method.GET,
                o => Assert.AreEqual(expectedData, o),
                e => { });
        }
    }
}