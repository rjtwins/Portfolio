using Microsoft.Extensions.Configuration;
using BCT.Application.ServiceInterfaces;
using BCT.Application.AuthEntities;
using static System.Text.Json.JsonSerializer;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using BCT.Application.Exceptions;
using Polly.Registry;

namespace BCT.Auth0Api
{
	public class Auth0ManagementApi : IAuthManagementService
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<Auth0ManagementApi> _logger;
		private readonly AuthToken _auth0Token;
        private readonly ResiliencePipelineProvider<string> resiliencePipelineProvider;

        private string domain => _configuration["Auth0:Domain"];
		private string clientId => _configuration["Auth0:ClientId"];
		private string secret => _configuration["Auth0:ClientSecret"];
		
		public Auth0ManagementApi(IConfiguration configuration, ILogger<Auth0ManagementApi> logger, AuthToken auth0Token, ResiliencePipelineProvider<string> resiliencePipelineProvider)
		{
			this._configuration = configuration;
			this._logger = logger;
			this._auth0Token = auth0Token;
            this.resiliencePipelineProvider = resiliencePipelineProvider;
        }
		
		private HttpClient GetManagementClient()
		{
			var client = new HttpClient();
			client.BaseAddress = new Uri($"https://{domain}/api/v2/");
			return client;
		}
		
		private HttpClient GetAuthClient()
		{
			var client = new HttpClient();
			client.BaseAddress = new Uri($"https://{domain}/");
			return client;
		}
		
		/// <summary>
		/// Adds a role to a user.
		/// </summary>
		/// <param name="userId">The ID of the user.</param>
		/// <param name="roleId">The ID of the role.</param>
		public async Task AddUserRole(string userId, string roleId)
		{
			var endpoint = $"users/{userId}/roles";
			var bodyObject = new
			{
				roles = new string[] { roleId }
			};
			var jsonString = Serialize(bodyObject);
			await PostRequestToEndpoint_Save(endpoint, new StringContent(jsonString, Encoding.UTF8, "application/json"));
		}

		/// <summary>
		/// Creates a new user.
		/// </summary>
		/// <param name="userEmail">The email of the user to create.</param>
        /// <returns>string user_id of created user.
        /// Empty string if no user was created.</returns>
		public async Task<(bool result, string userId)> TryCreateUser(string userEmail)
		{
			var endpoint = $"users";
			AuthUserCreateModel userCreate = new AuthUserCreateModel()
			{
				email = userEmail,
				connection = "Username-Password-Authentication",
				password = Guid.NewGuid().ToString()
			};
			
			var jsonString = Serialize(userCreate);
			var response = await PostRequestToEndpoint_Save(endpoint, new StringContent(jsonString, Encoding.UTF8, "application/json"));
            
            if(response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                _logger.LogInformation($"User with email {userEmail} already exists in Auth0.");
                return (false, string.Empty);
            }

            var responseText = await response.Content.ReadAsStringAsync();
            var responseObject = Deserialize<AuthUserCreateResponse>(responseText);

            if(responseObject == null || string.IsNullOrEmpty(responseObject.user_id))
            {
                _logger.LogError($"Failed to create user with email {userEmail}. Response was null or did not contain user_id. Response content: {responseText}");
                return (false, string.Empty);
            }
            return (true, responseObject.user_id);
		}

		/// <summary>
		/// Deletes a user.
		/// </summary>
		/// <param name="userId">The ID of the user to delete.</param>
		public async Task DeleteUser(string userId)
		{
			var endpoint = $"users/{userId}";
			await DeleteRequestToEndpoint_Save(endpoint);
		}

		/// <summary>
		/// Retrieves all roles.
		/// </summary>
		/// <returns>An array of Auth0 roles.</returns>
		public async Task<AuthRole[]> GetAllRoles()
		{
			var endpoint = $"roles";
			var response = await GetRequestToEndpoint_Safe(endpoint);
			
			var jsonResponse = await response.Content.ReadAsStringAsync();
			try
			{
				return Deserialize<AuthRole[]>(jsonResponse);
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to deserialize roles", e);
				throw new Exception("Failed to deserialize roles", e);
			}
		}

		/// <summary>
		/// Retrieves all users from the Auth0 API.
		/// </summary>
		/// <returns>An array of Auth0 users.</returns>
		public async Task<AuthUser[]> GetAllUsers()
		{
			var endpoint = $"users";
			var response = await GetRequestToEndpoint_Safe(endpoint);
				
			var jsonResponse = await response.Content.ReadAsStringAsync();
			try
			{
				return Deserialize<AuthUser[]>(jsonResponse);
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to deserialize users", e);
				throw new Exception("Failed to deserialize users", e);
			}
		}

		/// <summary>
		/// Retrieves an Auth0 user by their Auth0 ID.
		/// </summary>
		/// <param name="auth0Id">The Auth0 ID of the user to retrieve.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains the Auth0 user if found; otherwise, null.</returns>
		/// <exception cref="Exception">Thrown when the request to Auth0 fails or the response cannot be deserialized.</exception>
		public async Task<AuthUser?> GetUserByAuthId(string auth0Id)
		{				
			var endpoint = $"users/{auth0Id}";
			var response = await GetRequestToEndpoint_Safe(endpoint);
				
			var jsonResponse = await response.Content.ReadAsStringAsync();
			try
			{
				return Deserialize<AuthUser>(jsonResponse);
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to deserialize user by Auth0 id", e);
				throw new Exception("Failed to deserialize user by Auth0 id", e);
			}
		}

		/// <summary>
		/// Retrieves the roles of a user.
		/// </summary>
		/// <param name="userId">The ID of the user.</param>
		/// <returns>An array of Auth0 roles.</returns>
		public async Task<AuthRole[]> GetUserRoles(string userId)
		{
			var endpoint = $"users/{userId}/roles";
			var response = await GetRequestToEndpoint_Safe(endpoint);
			
			var jsonResponse = await response.Content.ReadAsStringAsync();
			try
			{
				return Deserialize<AuthRole[]>(jsonResponse);
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to deserialize user roles", e);
				throw new ApplicationException("Failed to deserialize user roles", e);
			}
		}

        /// <summary>
        /// Poly wrapper
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> GetRequestToEndpoint_Safe(string endpoint)
        {
            var pipeline = resiliencePipelineProvider.GetPipeline("retry-Auth0API-call");
            return await pipeline.ExecuteAsync<HttpResponseMessage>(async token =>
            {
                return await GetRequestToEndpoint(endpoint);
            });
        }

        /// <summary>
        /// Poly wrapper
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="content"></param>
        /// <param name="useAuthClient"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> PostRequestToEndpoint_Save(string endpoint, StringContent content, bool useAuthClient = false)
        {
            var pipeline = resiliencePipelineProvider.GetPipeline("retry-Auth0API-call");
            return await pipeline.ExecuteAsync<HttpResponseMessage>(async token =>
            {
                return await PostRequestToEndpoint(endpoint, content, useAuthClient);
            });
        }

        /// <summary>
        /// Poly wrapper
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> DeleteRequestToEndpoint_Save(string endpoint, StringContent? content = null, bool useAuthClient = false)
        {
            var pipeline = resiliencePipelineProvider.GetPipeline("retry-Auth0API-call");
            return await pipeline.ExecuteAsync<HttpResponseMessage>(async token =>
            {
                return await DeleteRequestToEndpoint(endpoint, content, useAuthClient);
            });
        }

        /// <summary>
        /// Retrieves an Auth0 Management API token using client credentials.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the Auth0 token.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when the request to get the Auth0 management API token fails or when deserialization of the token fails.
        /// </exception>
        public async Task<AuthToken> GetAuth0ManagementApiToken()
		{
			using var httpClient = GetAuthClient();
			var endpoint = "oauth/token";
			
			var jsonObject = new
			{
				client_id = clientId,
				client_secret = secret,
				audience = $"https://{domain}/api/v2/",
				grant_type = "client_credentials"
			};
			
			var content = new StringContent(Serialize(jsonObject), Encoding.UTF8, "application/json");
			var response = await httpClient.PostAsync(endpoint, content);
			if (!response.IsSuccessStatusCode)
				throw new AuthServiceFailedToGetTokenException("Request was not successfull");
			
			var jsonResponse = await response.Content.ReadAsStringAsync();
			
			if(jsonResponse == null)
			{
				_logger.LogError("Failed to get Auth0 management api token");
				throw new AuthServiceFailedToGetTokenException("Token was null or empty");
			}
			
			try
			{
				return Deserialize<AuthToken>(jsonResponse);
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to deserialize Auth0 management api token", e);
				throw new AuthServiceFailedToGetTokenException("Unable to deserialize token json", e);
			}
		}

		/// <summary>
		/// Removes a role from a user.
		/// </summary>
		/// <param name="userId">The ID of the user.</param>
		/// <param name="roleId">The ID of the role.</param>
		public async Task RemoveUserRole(string userId, string roleId)
		{
			var endpoint = $"users/{userId}/roles";
			var bodyObject = new
			{
				roles = new string[] { roleId }
			};

			var jsonString = Serialize(bodyObject);
			await DeleteRequestToEndpoint_Save(endpoint, new StringContent(jsonString, Encoding.UTF8, "application/json"));
		}

		/// <summary>
		/// Resets the password of a user by their email.
		/// </summary>
		/// <param name="userEmail">The email of the user.</param>
		public async Task ResetPasswordByEmail(string userEmail)
		{
			var endpoint = $"dbconnections/change_password";
			var jsonObject = new
			{
				client_id = clientId,
				email = userEmail,
				connection = "Username-Password-Authentication"
			};
			
			var jsonString = Serialize(jsonObject);
			await PostRequestToEndpoint_Save(endpoint, new StringContent(jsonString, Encoding.UTF8, "application/json"), true);
		}
	
		/// <summary>
		/// Sends a GET request to the specified endpoint.
		/// </summary>
		/// <param name="endpoint">The endpoint to send the request to.</param>
		/// <returns>The HTTP response message.</returns>
		/// <exception cref="Exception">Thrown when the request fails.</exception>
		private async Task<HttpResponseMessage> GetRequestToEndpoint(string endpoint)
		{
			using var httpClient = GetManagementClient();
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _auth0Token.access_token);
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            httpClient.Timeout = TimeSpan.FromSeconds(5);
			var response = await httpClient.GetAsync(endpoint);

            switch (response.IsSuccessStatusCode, response.StatusCode)
            {
                case (true, _):
                    return response;
                case (false, System.Net.HttpStatusCode.TooManyRequests):
                    throw new AuthServiceToManyRequestsException("Too many requests to Auth0 Management API");
                case (false, System.Net.HttpStatusCode.RequestTimeout):
                    throw new AuthServiceToManyRequestsException("Too many requests to Auth0 Management API");
                default:
                    _logger.LogError($"Failed call GET on management api endpoint {endpoint} with status code {response.StatusCode}");
                    throw new Exception($"Failed call GET on management api endpoint {endpoint}");
            }
        }
		
		/// <summary>
		/// Sends a POST request to the specified endpoint.
		/// </summary>
		/// <param name="endpoint">The endpoint to send the request to.</param>
		/// <param name="content">The content of the request.</param>
		/// <param name="useAuthClient">Whether to use the Auth client.</param>
		/// <returns>The HTTP response message.</returns>
		/// <exception cref="Exception">Thrown when the request fails.</exception>
		private async Task<HttpResponseMessage> PostRequestToEndpoint(string endpoint, StringContent content, bool useAuthClient = false)
		{
			using var httpClient = useAuthClient ? GetAuthClient() : GetManagementClient();
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _auth0Token.access_token);
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			
			var response = await httpClient.PostAsync(endpoint, content);

            switch (response.IsSuccessStatusCode, response.StatusCode)
            {
                case (true, _):
                    return response;
                case (false, System.Net.HttpStatusCode.TooManyRequests):
                case (false, System.Net.HttpStatusCode.RequestTimeout):
                    throw new AuthServiceToManyRequestsException("Too many requests to Auth0 Management API");
                //We have to return here because confict will not be resolved by retry.
                case (false, System.Net.HttpStatusCode.Conflict):
                    _logger.LogError($"Failed call POST on management api endpoint {endpoint} with status code {response.StatusCode}");
                    return response;
                default:
                    _logger.LogError($"Failed call POST on management api endpoint {endpoint} with status code {response.StatusCode}");
                    throw new Exception($"Failed call POST on management api endpoint {endpoint}");
            }
        }

		/// <summary>
		/// Sends a DELETE request to the specified endpoint.
		/// </summary>
		/// <param name="endpoint">The endpoint to send the request to.</param>
		/// <param name="content">The content of the request.</param>
		/// <param name="useAuthClient">Whether to use the Auth client.</param>
		/// <returns>The HTTP response message.</returns>
		/// <exception cref="Exception">Thrown when the request fails.</exception>
		private async Task<HttpResponseMessage> DeleteRequestToEndpoint(string endpoint, StringContent? content = null, bool useAuthClient = false)
		{
			using var httpClient = useAuthClient ? GetAuthClient() : GetManagementClient();
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _auth0Token.access_token);
			HttpResponseMessage response;
			
			if(content != null)
			{
				var requestMessage = new HttpRequestMessage(HttpMethod.Delete, endpoint);
				requestMessage.Content = content;
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				response = await httpClient.SendAsync(requestMessage);
			}else
			{
				response = await httpClient.DeleteAsync(endpoint);
			}
					
			switch (response.IsSuccessStatusCode, response.StatusCode)
			{
				case (true, _):
					return response;
				case (false, System.Net.HttpStatusCode.TooManyRequests):
					throw new AuthServiceToManyRequestsException("Too many requests to Auth0 Management API");
				default:
					_logger.LogError($"Failed call DELETE on management api endpoint {endpoint} with status code {response.StatusCode}");
					throw new Exception($"Failed call DELETE on management api endpoint {endpoint}");
			}
		}
	}
}
