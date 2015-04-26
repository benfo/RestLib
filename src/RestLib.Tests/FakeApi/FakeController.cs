using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RestLib.Tests.FakeApi
{
    public class FakeController : ApiController
    {
        private static readonly List<CustomerDto> CustomerStore = new List<CustomerDto>
            {
                new CustomerDto {Id = 1, Name="John", Surname = "Slow"},
                new CustomerDto {Id = 2, Name="Jane", Surname = "Wade"},
                new CustomerDto {Id = 3, Name="John", Surname = "Fast"}
            };

        public static void RegisterRoutes(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
        }

        [Route("customers/{id}")]
        public CustomerDto GetCustomer(int id)
        {
            var customer = CustomerStore.FirstOrDefault(c => c.Id == id);

            if (customer != null)
                return customer;

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [Route("customers", Name = "customers")]
        public HttpResponseMessage PostCustomer(CustomerDto customer)
        {
            var response = Request.CreateResponse(HttpStatusCode.Created, customer);

            // dummy save customer
            customer.Id = 11;

            string uri = Url.Link("customers", new { id = customer.Id });
            response.Headers.Location = new Uri(uri);
            return response;
        }

        [Route("customers")]
        public IEnumerable<CustomerDto> GetCustomers(string name = null, string surname = null)
        {
            return CustomerStore
                .Where(
                    c =>
                        (name == null || c.Name.Equals(name)) &&
                        (surname == null || c.Surname.Equals(surname)));
        }

        [Route("customers-requires-header")]
        public CustomerDto GetCustomersRequiresHeader()
        {
            var header = Request.GetHeader("header");

            if (string.IsNullOrEmpty(header))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            return CustomerStore[0];
        }

        [Route("")]
        public HttpResponseMessage GetRoot()
        {
            return new HttpResponseMessage();
        }
    }
}