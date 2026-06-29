using System.Collections.Generic;
using Newtonsoft.Json;

// ============================================================
// REST API Request Models
// ============================================================

[System.Serializable]
public class CreateUserRequest
{
    public string name;
    public string lastName;
    public string email;
    public string password;
}

[System.Serializable]
public class LoginRequest
{
    public string email;
    public string password;
}

[System.Serializable]
public class CreateServerRequest
{
    public string name;
}

[System.Serializable]
public class AddMemberRequest
{
    public string userId;
    public string email;
}

[System.Serializable]
public class CreateChannelRequest
{
    public string name;
}

// ============================================================
// REST API Response Models
// ============================================================

[System.Serializable]
public class UserProfile
{
    public string id;
    public string name;
    public string lastname;
    public string email;
}

[System.Serializable]
public class CreateUserResponse
{
    public UserProfile user;
}

[System.Serializable]
public class LoginResponse
{
    public string token;
    public long expireIn;
}

[System.Serializable]
public class UserResponse
{
    public UserProfile user;
}

[System.Serializable]
public class CreateServerResponse
{
    public string serverId;
}

[System.Serializable]
public class CreateChannelResponse
{
    public string serverId;
    public string channelId;
    public string channelName;
}

[System.Serializable]
public class StatusResponse
{
    public string status;
}

[System.Serializable]
public class ErrorResponse
{
    public string error;
}

// ============================================================
// WebSocket Client -> Server Messages
// ============================================================

[System.Serializable]
public class WsMessage
{
    public string type;
}

[System.Serializable]
public class WsPing : WsMessage { }

[System.Serializable]
public class WsGetServers : WsMessage { }

[System.Serializable]
public class WsGetChannels : WsMessage
{
    public string serverId;
}

[System.Serializable]
public class WsGetDirectChats : WsMessage { }

[System.Serializable]
public class WsCreateChannel : WsMessage
{
    public string serverId;
    public string name;
}

[System.Serializable]
public class WsSendMessage : WsMessage
{
    public string channelId;
    public string text;
}

[System.Serializable]
public class WsGetHistory : WsMessage
{
    public string channelId;
    public int limit;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public long? beforeTimestamp;
}

[System.Serializable]
public class WsAddMember : WsMessage
{
    public string serverId;
    public string userId;
}

// ============================================================
// WebSocket Server -> Client Messages
// ============================================================

[System.Serializable]
public class ServerInfo
{
    public string serverId;
    public string serverName;
}

[System.Serializable]
public class WsServersList : WsMessage
{
    public List<ServerInfo> servers;
}

[System.Serializable]
public class ChannelInfo
{
    public string channelId;
    public string channelName;
}

[System.Serializable]
public class WsChannelsList : WsMessage
{
    public string serverId;
    public List<ChannelInfo> channels;
}

[System.Serializable]
public class ChatInfo
{
    public string channelId;
    public string title;
}

[System.Serializable]
public class WsDirectChatsList : WsMessage
{
    public List<ChatInfo> chats;
}

[System.Serializable]
public class WsChannelCreated : WsMessage
{
    public string channelId;
    public string serverId;
    public string channelName;
}

[System.Serializable]
public class WsSent : WsMessage
{
    public string messageId;
    public long createdAt;
}

[System.Serializable]
public class MessageData
{
    public string messageId;
    public string senderId;
    public string text;
    public long createdAt;
}

[System.Serializable]
public class WsNewMessage : WsMessage
{
    public string serverId;
    public string channelId;
    public MessageData message;
}

[System.Serializable]
public class WsServerCreated : WsMessage
{
    public string serverId;
    public string serverName;
}

[System.Serializable]
public class WsHistory : WsMessage
{
    public string channelId;
    public List<MessageData> messages;
    public bool hasMore;
}

[System.Serializable]
public class WsMemberAdded : WsMessage
{
    public string serverId;
    public string userId;
}

[System.Serializable]
public class WsError : WsMessage
{
    public string message;
}
