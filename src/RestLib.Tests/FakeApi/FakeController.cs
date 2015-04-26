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
                new CustomerDto {CustomerId = 1, Name="John"},
                new CustomerDto {CustomerId = 2, Name="Jane"},
                new CustomerDto {CustomerId = 3, Name="John"}
            };

        public static void RegisterRoutes(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
        }

        [Route("customers/{customerId}")]
        public CustomerDto GetCustomer(int customerId)
        {
            var customer = CustomerStore.FirstOrDefault(c => c.CustomerId == customerId);

            if (customer != null)
                return customer;

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [Route("customers")]
        public List<CustomerDto> GetCustomers()
        {
            return CustomerStore;
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