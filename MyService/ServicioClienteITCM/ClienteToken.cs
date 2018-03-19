using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Entidades;
using System.Net.Http.Headers;
using System.Collections.Specialized;
using Util;
using System.Threading;

namespace ServicioClienteITCM
{
    public class ClienteToken
    {
        private HttpClient client;
        private int testCounter;
        private CancellationTokenSource cancelSource;

        public int TestCounter
        {
            get
            {
                return testCounter;
            }

            set
            {
                testCounter = value;
            }
        }

        public ClienteToken(int _testCounter = 10)
        {
            this.client = new HttpClient();
            //this.client.BaseAddress = new Uri("http://10.1.2.103:8080/MiddlewareIntranet/");
            this.client.BaseAddress = new Uri("http://localhost:8080/MiddlewareIntranet/");
            this.testCounter = _testCounter;
            this.cancelSource = new CancellationTokenSource();
        }

        public async Task<TokenResponse> GetRefreshToken()
        {            
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            TokenResponse token = null;
            string controllerName = "oauth/token";            
            NameValueCollection query = new NameValueCollection();            
            query["grant_type"] = "password";
            query["client_id"] = "optical-client-id";
            query["client_secret"] = "optical";
            query["username"] = "optical";
            query["password"] = "password";
            string relativePath = controllerName + query.ToQueryString();            
            bool isAvalible = await HttpUtils.CheckIfServiceIsAvailableAsync($"{this.client.BaseAddress.ToString()}{relativePath}", _counter: this.TestCounter);
            if (isAvalible)
            {
                HttpResponseMessage response = await this.client.GetAsync(relativePath);
                if (response.IsSuccessStatusCode)
                {
                    token = await response.Content.ReadAsAsync<TokenResponse>();
                }
            }            
            return token;
        }

        public async Task<TokenResponse> GetAccessToken(TokenResponse refreshToken)
        {            
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            TokenResponse token = null;
            string controllerName = "oauth/token";            
            NameValueCollection query = new NameValueCollection();
            query["grant_type"] = "refresh_token";
            query["client_id"] = "optical-client-id";
            query["refresh_token"] = refreshToken.Refresh_token;
            query["client_secret"] = "optical";
            string relativePath = controllerName + query.ToQueryString();
            //bool isAvalible = await HttpUtils.CheckIfServiceIsAvailableAsync($"{this.client.BaseAddress.ToString()}{relativePath}", _counter: this.TestCounter);
            //if (isAvalible)
            //{                
            //}
            HttpResponseMessage response = await this.client.GetAsync(relativePath);
            if (response.IsSuccessStatusCode)
            {
                token = await response.Content.ReadAsAsync<TokenResponse>();
            }
            return token;
        }

        public async Task<TokenResponse> RunAsync()
        {
            TokenResponse refreshToken = await this.GetRefreshToken();            
            TokenResponse accessToken = await this.GetAccessToken(refreshToken);
            return accessToken;
        }
    }
}
