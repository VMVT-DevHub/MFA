
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Graph {
	public class Request {
		public string Tenant { get; set; }
		public string ClientId { get; set; }
		private HttpClient HClient { get; set; }
		private string ClientSecret { get; set; }
		private string AuthUrl => $"https://login.microsoftonline.com/{Tenant}/oauth2/v2.0/token";

		private string? AT { get; set; }
		private DateTime Expire { get; set; }

		public Request(string tenant, string clientId, string clientSecret){
			ClientId = clientId; ClientSecret = clientSecret; Tenant=tenant; 
			HClient = new HttpClient();
			HClient.DefaultRequestHeaders.Add("Accept-Language", "lt-LT,en-US;q=0.8");
		}

		private async Task<bool> Login() {

			var content = new FormUrlEncodedContent([
			new ("grant_type", "client_credentials"),
			new ("client_id", ClientId),
			new ("client_secret", ClientSecret),
			new ("scope", "https://graph.microsoft.com/.default"),
			]);
			content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

			var response = await HClient.PostAsync(AuthUrl, content);

			if(response.IsSuccessStatusCode){
				var at = await response.Content.ReadFromJsonAsync<AccessToken>();
				if(at is not null){
					AT = at.Token;
					Expire = DateTime.UtcNow.AddSeconds(at.Expires-60);
					
					HClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AT);

					return true;
				}
			} else {
				//Bad
			}

			var rsp = await response.Content.ReadFromJsonAsync<AccessToken>();

			return false;
		}


		public async Task<T?> Post<T>(string url, object data) where T : Responses.Response {
			if(Expire>DateTime.UtcNow || await Login()){
				var content = JsonSerializer.Serialize(data);
				var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

				var response = await HClient.PostAsync(url, stringContent);

				return await response.Content.ReadFromJsonAsync<T>();
				
			} else {
				//bad (login)
			}

			return default;
		}

	}

	public class AccessToken {
		[JsonPropertyName("access_token")] public string? Token { get; set; }
		[JsonPropertyName("expires_in")] public int Expires { get; set; }
		[JsonPropertyName("ext_expires_in")] public int ExtExpires { get; set; }
		[JsonPropertyName("token_type")] public string? Type { get; set; }
	}


	namespace Responses{
		public class Response {
			public Error? Error { get; set; }
		}

		public class Error {
			public string? Code { get; set; }
			public string? Message { get; set; }
			public InnerError? InnerError { get; set; }
		}

		public class InnerError {
			public string? Message { get; set; }
			public DateTime? Date { get; set; }
			[JsonPropertyName("request-id")] public Guid RequestId { get; set; }
			[JsonPropertyName("client-request-id")] public Guid ClientRequestId { get; set; }
		}


		public class TemporaryAccessPass : Response {
			public Guid ID { get; set; }
			public bool IsUsable { get; set; }
			public string? MethodUsabilityReason { get; set; }
			[JsonPropertyName("temporaryAccessPass")] public string? AccessPass { get; set; }
			public DateTime? CreatedDateTime { get; set; }
			public DateTime? StartDateTime { get; set; }
			public int LifetimeInMinutes { get; set; }
			public bool IsUsableOnce { get; set; }
		}
	}

	namespace Requests {
		public class TemporaryAccessPass(Request req) {
			public bool IsUsableOnce { get; set; }
			public DateTime? StartDateTime { get; set; }
			public int LifetimeInMinutes { get; set; }

			private Request Req { get; set; } = req;

			public async Task<Responses.TemporaryAccessPass> Process(string user) =>
				await Req.Post<Responses.TemporaryAccessPass>($"https://graph.microsoft.com/v1.0/users/{user}/authentication/temporaryAccessPassMethods",this) ?? new();
		}
	}
}