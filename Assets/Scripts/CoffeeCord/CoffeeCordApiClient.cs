using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class CoffeeCordApiClient
{
    private const string BaseUrl = "http://176.123.169.111:8080";

    private async Task<T> SendRequestAsync<T>(
        string method,
        string path,
        string authToken = null,
        string bodyJson = null)
    {
        using var request = new UnityWebRequest($"{BaseUrl}{path}", method);

        if (!string.IsNullOrEmpty(bodyJson))
        {
            var bodyBytes = Encoding.UTF8.GetBytes(bodyJson);
            request.uploadHandler = new UploadHandlerRaw(bodyBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
        }
        else
        {
            request.downloadHandler = new DownloadHandlerBuffer();
        }

        if (!string.IsNullOrEmpty(authToken))
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");

        var tcs = new TaskCompletionSource<T>();

        var op = request.SendWebRequest();
        op.completed += _ =>
        {
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                var errorBody = request.downloadHandler?.text;
                ErrorResponse errorResp = null;
                try { errorResp = JsonConvert.DeserializeObject<ErrorResponse>(errorBody ?? ""); } catch { }
                tcs.TrySetException(new ApiException(
                    (int)request.responseCode,
                    errorResp?.error ?? request.error));
            }
            else
            {
                try
                {
                    var json = request.downloadHandler.text;
                    var result = JsonConvert.DeserializeObject<T>(json);
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(new ApiException(0, $"Failed to parse response: {ex.Message}"));
                }
            }
        };

        return await tcs.Task;
    }

    private async Task<List<string>> SendRequestForStringArrayAsync(
        string method,
        string path,
        string authToken)
    {
        using var request = new UnityWebRequest($"{BaseUrl}{path}", method);
        request.downloadHandler = new DownloadHandlerBuffer();

        if (!string.IsNullOrEmpty(authToken))
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");

        var tcs = new TaskCompletionSource<List<string>>();

        var op = request.SendWebRequest();
        op.completed += _ =>
        {
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                var errorBody = request.downloadHandler?.text;
                ErrorResponse errorResp = null;
                try { errorResp = JsonConvert.DeserializeObject<ErrorResponse>(errorBody ?? ""); } catch { }
                tcs.TrySetException(new ApiException(
                    (int)request.responseCode,
                    errorResp?.error ?? request.error));
            }
            else
            {
                try
                {
                    var json = request.downloadHandler.text;
                    var result = JsonConvert.DeserializeObject<List<string>>(json);
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(new ApiException(0, $"Failed to parse response: {ex.Message}"));
                }
            }
        };

        return await tcs.Task;
    }

    // ---- Authentication ----

    public async Task<CreateUserResponse> RegisterUser(string name, string lastName, string email, string password)
    {
        var body = new CreateUserRequest
        {
            name = name,
            lastName = lastName,
            email = email,
            password = password
        };
        return await SendRequestAsync<CreateUserResponse>("POST", "/register",
            bodyJson: JsonConvert.SerializeObject(body));
    }

    public async Task<LoginResponse> Login(string email, string password)
    {
        var body = new LoginRequest
        {
            email = email,
            password = password
        };
        return await SendRequestAsync<LoginResponse>("POST", "/login",
            bodyJson: JsonConvert.SerializeObject(body));
    }

    public async Task<UserResponse> GetProfile(string token)
    {
        return await SendRequestAsync<UserResponse>("POST", "/me", authToken: token);
    }

    public async Task<UserResponse> SearchUserByEmail(string token, string email)
    {
        var encoded = UnityWebRequest.EscapeURL(email);
        return await SendRequestAsync<UserResponse>("GET", $"/users?email={encoded}", authToken: token);
    }

    public async Task<UserResponse> GetUserById(string token, string userId)
    {
        return await SendRequestAsync<UserResponse>("GET", $"/users/{userId}", authToken: token);
    }

    // ---- Servers ----

    public async Task<CreateServerResponse> CreateServer(string token, string name)
    {
        var body = new CreateServerRequest { name = name };
        return await SendRequestAsync<CreateServerResponse>("POST", "/servers",
            authToken: token, bodyJson: JsonConvert.SerializeObject(body));
    }

    public async Task<List<string>> GetServerMembers(string token, string serverId)
    {
        return await SendRequestForStringArrayAsync("GET", $"/servers/{serverId}/members", authToken: token);
    }

    public async Task<StatusResponse> AddServerMember(string token, string serverId, string userId = null, string email = null)
    {
        var body = new AddMemberRequest { userId = userId, email = email };
        return await SendRequestAsync<StatusResponse>("POST", $"/servers/{serverId}/members",
            authToken: token, bodyJson: JsonConvert.SerializeObject(body));
    }

    public async Task<CreateChannelResponse> CreateChannel(string token, string serverId, string name)
    {
        var body = new CreateChannelRequest { name = name };
        return await SendRequestAsync<CreateChannelResponse>("POST", $"/servers/{serverId}/channels",
            authToken: token, bodyJson: JsonConvert.SerializeObject(body));
    }
}

public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}
