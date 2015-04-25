using RestLib.Tests.Properties;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RestLib.Tests.FakeApi
{
    public class FakeController : ApiController
    {
        public static void RegisterRoutes(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
        }

        [Route("")]
        public HttpResponseMessage GetRoot()
        {
            return new HttpResponseMessage();
        }

        [Route("customers")]
        public List<CustomerDto> GetCustomers()
        {
            var customers = new List<CustomerDto>
            {
                new CustomerDto {CustomerId = 1},
                new CustomerDto {CustomerId = 2}
            };
            return customers;
        }

        [Route("customers/{customerId}")]
        public CustomerDto GetCustomer(int customerId)
        {
            return new CustomerDto { CustomerId = customerId };
        }

        [Route("customers-requires-header")]
        public HttpResponseMessage GetCustomersRequiresHeader()
        {
            var header = Request.GetHeader("header");

            if (string.IsNullOrEmpty(header))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            return new HttpResponseMessage
            {
                Content = new StringContent(Resources.CustomersDto)
            };
        }
    }
}