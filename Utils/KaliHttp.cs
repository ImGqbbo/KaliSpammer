using Leaf.xNet;
using System;
using System.Collections.Generic;

namespace KaliSpammer
{
    class KaliHttp
    {
        public static HttpResponse Post(string Url, Dictionary<string, string> Headers, string Data, string ContentType, HttpProxyClient proxyClient)
        {
            try
            {
                HttpRequest request = new HttpRequest();
                request.IgnoreProtocolErrors = true;
                request.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                request.Proxy = proxyClient;
                foreach (var Header in Headers) request.AddHeader(Header.Key, Header.Value);
                if (Data != "")
                {
                    request.Post(Url, Data, ContentType);
                }
                else request.Post(Url);

                return request.Response;
            }
            catch
            {
                return null;
            }
        }

        public static HttpResponse Put(string Url, Dictionary<string, string> Headers, string Data, string ContentType, HttpProxyClient proxyClient)
        {
            try
            {
                HttpRequest request = new HttpRequest();
                request.IgnoreProtocolErrors = true;
                request.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                request.Proxy = proxyClient;
                foreach (var Header in Headers) request.AddHeader(Header.Key, Header.Value);
                if (Data != "")
                {
                    request.Put(Url, Data, ContentType);
                }
                else request.Put(Url);

                return request.Response;
            }
            catch
            {
                return null;
            }
        }

        public static HttpResponse Get(string Url, Dictionary<string, string> Headers, HttpProxyClient proxyClient)
        {
            try
            {
                HttpRequest request = new HttpRequest();
                request.IgnoreProtocolErrors = true;
                request.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                request.Proxy = proxyClient;
                foreach (var Header in Headers) request.AddHeader(Header.Key, Header.Value);
                var res = request.Get(Url);

                return request.Response;
            }
            catch
            {
                return null;
            }
        }

        public static HttpResponse Delete(string Url, Dictionary<string, string> Headers, string Data, string ContentType, HttpProxyClient proxyClient)
        {
            try
            {
                HttpRequest request = new HttpRequest();
                request.IgnoreProtocolErrors = true;
                request.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                request.Proxy = proxyClient;
                foreach (var Header in Headers) request.AddHeader(Header.Key, Header.Value);
                if (Data != "")
                {
                    request.Delete(Url, Data, ContentType);
                }
                else request.Delete(Url);

                return request.Response;
            }
            catch
            {
                return null;
            }
        }

        public static HttpResponse Patch(string Url, Dictionary<string, string> Headers, string Data, string ContentType, HttpProxyClient proxyClient)
        {
            try
            {
                HttpRequest request = new HttpRequest();
                request.IgnoreProtocolErrors = true;
                request.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                request.Proxy = proxyClient;
                foreach (var Header in Headers) request.AddHeader(Header.Key, Header.Value);
                if (Data != "")
                {
                    request.Patch(Url, Data, ContentType);
                }
                else request.Patch(Url);

                return request.Response;
            }
            catch
            {
                return null;
            }
        }
    }
}
