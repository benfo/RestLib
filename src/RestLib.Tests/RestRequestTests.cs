using System.Collections.Specialized;
using Moq;
using NUnit.Framework;
using System.Net;

namespace RestLib.Tests
{
    [TestFixture]
    public class RestRequestTests
    {
        private Mock<IHttp> http;
        private const string EndPoint = "http://endpoint.com";

        [SetUp]
        public void Before_each_test()
        {
            http = new Mock<IHttp>();
        }

        [Test]
        public void Make_a_get_request_to_a_resource_root()
        {
            http.Setup(x => x.Request(EndPoint + "/resource", Method.GET, It.IsAny<NameValueCollection>()))
                .Returns(DefaultResponse());
            var request = GetRestRequest("resource");

            request.Get();

            http.VerifyAll();
        }

        [Test]
        public void Make_a_get_request_to_a_resource_identifier()
        {
            http.Setup(x => x.Request(EndPoint + "/resource/1", Method.GET, It.IsAny<NameValueCollection>()))
              .Returns(DefaultResponse());
            var request = GetRestRequest("resource");

            request.Get("1");

            http.VerifyAll();
        }

        [Test]
        public void Can_add_headers()
        {
            var request = GetRestRequest("resource");

            request.AddHeader("header", "value");

            Assert.That(request.Headers.Count, Is.EqualTo(1));
            Assert.That(request.Headers["header"], Is.EqualTo("value"));
        }

        [Test]
        public void Can_add_matrix_parameters()
        {
            var request = GetRestRequest("resource");

            const string parmName = "matrix";
            const string parmValue = "matrix-value";
            request.AddMatrixParameter(parmName, parmValue);

            Assert.That(request.Parameters.Count, Is.EqualTo(1));
            AssertParameter(request.Parameters[0], ParameterType.Matrix, parmName, parmValue);
        }

        [Test]
        public void Can_add_query_parameters()
        {
            var request = GetRestRequest("resource");

            const string parmName = "query";
            const string parmValue = "query-value";
            request.AddQueryParameter(parmName, parmValue);

            Assert.That(request.Parameters.Count, Is.EqualTo(1));
            AssertParameter(request.Parameters[0], ParameterType.QueryString, parmName, parmValue);
        }

        [Test]
        public void Omit_empty_matrix_parameters_when_making_a_request()
        {
            http.Setup(x => x.Request(EndPoint + "/resource;matrixparam1=value1;matrixparam2=value2", Method.GET, It.IsAny<NameValueCollection>()))
             .Returns(DefaultResponse());
            var request = GetRestRequest("resource")
                .AddMatrixParameter("matrixparam1", "value1")
                .AddMatrixParameter("matrixparam2", "value2")
                .AddMatrixParameter("matrixparam3", "");

            request.Get();

            http.VerifyAll();
        }

        [Test]
        public void Make_a_request_using_matrix_parameters()
        {
            http.Setup(x => x.Request(EndPoint + "/resource;matrixparam1=value1;matrixparam2=value2", Method.GET, It.IsAny<NameValueCollection>()))
              .Returns(DefaultResponse());
            var request = GetRestRequest("resource")
                .AddMatrixParameter("matrixparam1", "value1")
                .AddMatrixParameter("matrixparam2", "value2");

            request.Get();

            http.VerifyAll();
        }

        private static void AssertParameter(Parameter matrixParameter, ParameterType type, string name, string value)
        {
            Assert.That(matrixParameter.Type, Is.EqualTo(type));
            Assert.That(matrixParameter.Name, Is.EqualTo(name));
            Assert.That(matrixParameter.Value, Is.EqualTo(value));
        }

        private IRestRequest GetRestRequest(string resourceName)
        {
            var request = new RestRequest(EndPoint, resourceName, http.Object);
            return request;
        }

        private static HttpResponse DefaultResponse()
        {
            return new HttpResponse
            {
                Content = "content",
                ContentType = "content-type",
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}