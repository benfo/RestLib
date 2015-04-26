using NUnit.Framework;
using RestLib.Tests.FakeApi;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace RestLib.Tests
{
    [TestFixture]
    public class RestClientTests
    {
        private const string BaseAddress = "http://localhost:9001";
        private IRestClient client;

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
            var response = client.Resource("customers").Get();
            var content = response.Content;

            Assert.That(content, Contains.Substring("CustomerId"));
        }

        [Test]
        public void Make_a_get_request_to_a_resource_identifier()
        {
            var response = client.Resource("customers").Get("1");
            var content = response.Content;

            Assert.That(content, Contains.Substring("CustomerId"));
        }

        [Test]
        public void Make_a_get_request_using_headers()
        {
            client.AddHeader("header", "value");
            var response = client.Resource("customers-requires-header").Get();
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
            var response = client.Resource("customers").Get<List<CustomerDto>>();
            var data = response.Data;

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Count(), Is.GreaterThan(0));
        }

        [Test]
        public void Make_a_get_request_to_a_resource_identifier_and_deserialize()
        {
            var response = client.Resource("customers").Get<CustomerDto>("2");
            var data = response.Data;

            Assert.That(data, Is.Not.Null);
            Assert.That(data.CustomerId, Is.EqualTo(2));
        }

        [Test]
        public void Make_a_get_request_to_a_resource_that_does_not_exist()
        {
            var response = client.Resource("customers").Get<CustomerDto>("10");
            var data = response.Data;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(data, Is.Null);
        }

        [Test]
        public void Make_a_get_request_using_query_string_parameters()
        {
            var response = client.Resource("customers")
                .AddQueryParameter("name", "Jane")
                .AddQueryParameter("surname", "Wade")
                .Get<List<CustomerDto>>();

            var customer = response.Data.FirstOrDefault();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(customer, Is.Not.Null);
            Assert.That(customer.Name, Is.EqualTo("Jane"));
        }
    }
}