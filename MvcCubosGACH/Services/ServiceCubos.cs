using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using MvcCubosGACH.Models;
using System.Security.Claims;

namespace MvcCubosGACH.Services {
    public class ServiceCubos {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private MediaTypeWithQualityHeaderValue Header;
        private string UrlApi;

        public ServiceCubos(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) {
            this.UrlApi = configuration.GetValue<string>("ApiUrls:ApiCubos");
            this.Header = new MediaTypeWithQualityHeaderValue("application/json");
            _httpContextAccessor = httpContextAccessor;
        }

        #region GENERAL
        private async Task<T> CallApiAsync<T>(string request) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);

                HttpResponseMessage response = await client.GetAsync(request);
                return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<T>() : default(T);
            }
        }

        private async Task<int> InsertApiAsync<T>(string request, T objeto) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);

                string json = JsonConvert.SerializeObject(objeto);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(request, content);

                if (response.IsSuccessStatusCode) {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    int numero = JsonConvert.DeserializeObject<int>(responseContent);
                    return numero;
                } else {
                    return -1; // Si no hay ningún número a devolver en caso de error.
                }
            }
        }

        private async Task<HttpStatusCode> UpdateApiAsync<T>(string request, T objeto) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);

                string json = JsonConvert.SerializeObject(objeto);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(request, content);
                return response.StatusCode;
            }
        }

        // Se supone que en el request ya va el id. Ejemplo: http:/localhost/api/deletealgo/17
        private async Task<HttpStatusCode> DeleteApiAsync(string request) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();

                HttpResponseMessage response = await client.DeleteAsync(request);
                return response.StatusCode;
            }
        }
        #endregion

        #region GENERAL TOKEN
        private async Task<T> CallApiAsync<T>(string request, string token) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                HttpResponseMessage response = await client.GetAsync(request);
                return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<T>() : default(T);
            }
        }

        private async Task<HttpStatusCode> InsertApiAsync<T>(string request, T objeto, string token) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                string json = JsonConvert.SerializeObject(objeto);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(request, content);
                return response.StatusCode;
            }
        }

        private async Task<HttpStatusCode> UpdateApiAsync<T>(string request, T objeto, string token) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                string json = JsonConvert.SerializeObject(objeto);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(request, content);
                return response.StatusCode;
            }
        }

        // Se supone que en el request ya va el id. Ejemplo: http:/localhost/api/deletealgo/17
        private async Task<HttpStatusCode> DeleteApiAsync(string request, string token) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                HttpResponseMessage response = await client.DeleteAsync(request);
                return response.StatusCode;
            }
        }
        #endregion

        #region TOKEN
        public async Task<string?> GetToken(string email, string password) {
            LogInModel model = new LogInModel { Email = email, Password = password };
            string token = "";

            using (HttpClient client = new HttpClient()) {
                string request = "/api/auth/login";
                client.BaseAddress = new Uri(this.UrlApi + request);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string jsonModel = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(jsonModel, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode) {
                    string data = await response.Content.ReadAsStringAsync();
                    JObject jsonObject = JObject.Parse(data);
                    token = jsonObject.GetValue("response").ToString();
                }
                return (token != "") ? token : null;
            }
        }
        #endregion

        #region CUBOS
        public async Task<int> InsertCuboAsync(string nombre, string marca, string extension, int precio) {
            Cubo newCubo = new Cubo {
                IdCubo = 0,
                Nombre = nombre,
                Marca = marca,
                Imagen = extension,
                Precio = precio
            };
            string request = "/api/cubos/insertcubo";
            int newId = await this.InsertApiAsync<Cubo>(request, newCubo);
            return newId;
        }

        public async Task<List<Cubo>> GetCubosAsync() {
            string request = "/api/cubos/getcubos";
            return await this.CallApiAsync<List<Cubo>>(request);
        }
        
        public async Task<List<Cubo>> GetCubosByMarcaAsync(string marca) {
            string request = "/api/cubos/getcubosbymarca/" + marca;
            return await this.CallApiAsync<List<Cubo>>(request);
        }
        #endregion

        #region USUARIOS
        public async Task<Usuario> GetPerfil() {
            string request = "/api/usuarios/perfilusuario";
            string token = _httpContextAccessor.HttpContext.Session.GetString("TOKEN");
            return await this.CallApiAsync<Usuario>(request, token);
        }

        public async Task<int> RegisterUser(string email, string password, string extension, string name) {
            Usuario newUser = new Usuario {
                IdUsuario = 0,
                Email= email,
                Password = password,
                Imagen = extension,
                Nombre = name
            };
            string request = "/api/usuarios/insertusuario";
            int newId = await this.InsertApiAsync<Usuario>(request, newUser);
            return newId;
        }
        #endregion

        #region PEDIDOS
        public async Task<List<Pedido>> GetPedidos() {
            int idUser = int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            string token = _httpContextAccessor.HttpContext.Session.GetString("TOKEN");
            string request = "/api/pedidos/PedidosUsuario/" + idUser;
            return await this.CallApiAsync<List<Pedido>>(request, token);
        }

        public async Task InsertPedidoAsync(int idCubo) {
            string request = "/api/pedidos/insertpedido/" + idCubo;
            string token = _httpContextAccessor.HttpContext.Session.GetString("TOKEN");
            await this.InsertApiAsync<Pedido>(request, null, token);
        }
        #endregion

    }
}
