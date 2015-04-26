using NUnit.Framework;
using RestLib.Tests.FakeApi;
using System.Collections.Generic;
using System.Net;

namespace RestLib.Tests
{
    [TestFixture]
    public class RestClientTests
    {
        private const string BaseAddress = "http://localhost:9001";
        private RestClient client;

        [TestFixtureSetUp]
        public void SetUp()
        {
            FakeServer.Configure(FakeController.RegisterRoutes);
            Http.ClientFactory = () => FakeServer.CreateClientForAServer(BaseAddress);
        }

        [SetUp]
        public void Before_each_test()
        {
            client = new RestClient(BaseAddress);
        }

        [Test]
        public void Make_a_get_request_without_parameters()
        {
            var result = client.Get();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Content, Is.EqualTo(null));
        }

        [Test]
        public void Make_a_get_request_without_parameters_and_deserialize()
        {
            var result = client.Get<CustomerDto>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Content, Is.EqualTo(null));
            Assert.That(result.Data, Is.EqualTo(null));
        }

        [Test]
        public void Make_a_get_request_to_a_resource_root()
        {
            var response = client.Get("customers");
            var content = response.Content;

            Assert.That(content, Contains.Substring("CustomerId"));
        }

        [Test]
        public void Make_a_get_request_to_a_resource_identifier()
        {
            var response = client.Get("customers", "1");
            var content = response.Content;

            Assert.That(content, Contains.Substring("CustomerId"));
        }

        [Test]
        public void Make_a_get_request_using_headers()
        {
            client.AddHeader("header", "value");
            var response = client.Get("customers-requires-header");
            var content = response.Content;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content, Is.Not.Null);
        }

        [Test]
        [TestCase("accept", "application/json")]
        [TestCase("accept", "text/json")]
        [TestCase("accept", "text/x-json")]
        public void Make_a_get_request_to_a_resource_root_and_deserialize(string header, string value)
        {
            client.Headers.Add(header, value);
            var response = client.Get<List<CustomerDto>>("customers");
            var data = response.Data;

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Make_a_get_request_to_a_resource_identifier_and_deserialize()
        {
            var response = client.Get<CustomerDto>("customers", "2");
            var data = response.Data;

            Assert.That(data, Is.Not.Null);
            Assert.That(data.CustomerId, Is.EqualTo(2));
        }

        [Test]
        public void Make_a_get_request_to_a_resource_that_does_not_exist()
        {
            var response = client.Get<CustomerDto>("customers", "10");
            var data = response.Data;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(data, Is.Null);
        }
    }
}