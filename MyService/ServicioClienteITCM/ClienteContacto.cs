using Entidades;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Util;

namespace ServicioClienteITCM
{
    public class ClienteContacto
    {
        private HttpClient client;
        private TokenResponse token;
        private int testCounter;
        private CancellationTokenSource cancelSource;

        public TokenResponse Token
        {
            get
            {
                if (this.token == null)
                    this.token = GetAccessToken();
                return this.token;
            }
        }

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

        public ClienteContacto(TokenResponse _token = null, int _testCounter = 10)
        {
            this.client = new HttpClient();
            if (_token == null)
            {
                _token = GetAccessToken();
            }
            
            //this.client.BaseAddress = new Uri("http://10.1.2.103:8080/MiddlewareIntranet/");
            this.client.BaseAddress = new Uri("http://localhost:8080/MiddlewareIntranet/");
            this.token = _token;
            this.testCounter = _testCounter;
            this.cancelSource = new CancellationTokenSource();
        }
        
        private TokenResponse GetAccessToken()
        {
            ClienteToken tokenClient = new ClienteToken();
            Task<TokenResponse> task = tokenClient.RunAsync();
            task.Wait();
            return task.Result;
        }

        public async Task<IEnumerable<Cliente>> GetContactos()
        {
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            IEnumerable<Cliente> clientes = null;
            MiddewareResponse middlewareResponse = null;
            string controllerName = "intranet/itsm/reporteContactos";
            NameValueCollection query = new NameValueCollection();
            query["access_token"] = this.token.Access_token;
            string relativePath = controllerName + query.ToQueryString();
            //bool isAvalible = await HttpUtils.CheckIfServiceIsAvailableAsync($"{this.client.BaseAddress.ToString()}{relativePath}", _counter: this.TestCounter);
            //if (isAvalible)
            //{                
            //}
            HttpResponseMessage response = await this.client.PostAsync(relativePath, null, cancelSource.Token);
            if (response.IsSuccessStatusCode)
            {
                middlewareResponse = await response.Content.ReadAsAsync<MiddewareResponse>();
                if (middlewareResponse.Estado.Equals(Constantes.ESTADO_200))
                {
                    object data_clientes = null;
                    bool done = middlewareResponse.Data.TryGetValue("contactos", out data_clientes);
                    if (done)
                    {
                        JArray c = (JArray)data_clientes;
                        clientes = c.ToObject<IEnumerable<Cliente>>();
                    }
                }
            }
            else
            {
                middlewareResponse = await response.Content.ReadAsAsync<MiddewareResponse>();
                if (middlewareResponse.Estado.Equals(Constantes.ESTADO_400))
                {
                    Console.WriteLine(middlewareResponse.Estado);
                    Console.WriteLine(middlewareResponse.Descripcion);
                }
                else if (middlewareResponse.Estado.Equals(Constantes.ESTADO_500))
                {
                    Console.WriteLine(middlewareResponse.Estado);
                    Console.WriteLine(middlewareResponse.Descripcion);
                }
            }
            return clientes;
        }

        private void Cancel()
        {
            if (this.cancelSource != null)
            {
                this.cancelSource.Cancel();
            }
        }
    }
}
