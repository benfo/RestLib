using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace RestLib.Tests.FakeApi
{
    public static class FakeServer
    {
        private static string baseAddress;
        private static HttpConfiguration config;
        private static Action<HttpSelfHostConfiguration> configure;

        public static HttpClient CreateClientForAServer(string serverBaseAddress)
        {
            baseAddress = serverBaseAddress;
            config = config ?? ConfigureHost();
            var server = new HttpServer(config);
            var client = new HttpClient(server);
            return client;
        }

        private static HttpSelfHostConfiguration ConfigureHost()
        {
            var selfHostConfiguration = new HttpSelfHostConfiguration(baseAddress);
            if (configure != null)
            {
                configure(selfHostConfiguration);
            }
            return selfHostConfiguration;
        }

        public static void Configure(Action<HttpSelfHostConfiguration> config)
        {
            configure = config;
        }
    }
}