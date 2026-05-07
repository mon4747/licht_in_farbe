using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer 
{
    public static NetworkServer Instance { get; private set; }

    private NetworkManager networkManager;

    private Dictionary<ulong,string> clientIdToAuth = new Dictionary<ulong,string>();
    private Dictionary<string,UserData> authIdToUserData = new Dictionary<string,UserData>();
    private Dictionary<ulong,string> clientIdToUserName = new Dictionary<ulong,string>();
    private Dictionary<ulong,int> clientIdToTeamIndex = new Dictionary<ulong,int>();

    public NetworkServer(NetworkManager networkManager)
    {
        Instance = this;
        this.networkManager = networkManager;

        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.OnServerStarted += OnNetworkReady;
    }
    
    private void OnNetworkReady()
    {
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }
    
    private void OnClientDisconnect(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            clientIdToAuth.Remove(clientId);
            authIdToUserData.Remove(authId);
        }

        clientIdToUserName.Remove(clientId);
        clientIdToTeamIndex.Remove(clientId);
    }

    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request, 
        NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        if (!string.IsNullOrWhiteSpace(userData.userAuthId))
        {
            clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
            authIdToUserData[userData.userAuthId] = userData;
        }
        else
        {
            clientIdToAuth[request.ClientNetworkId] = string.Empty;
        }

        clientIdToUserName[request.ClientNetworkId] = string.IsNullOrWhiteSpace(userData.userName)
            ? $"Player {request.ClientNetworkId}"
            : userData.userName;

        clientIdToTeamIndex[request.ClientNetworkId] = userData.teamIndex;

        response.Approved = true;
        response.CreatePlayerObject = true;
    }

    public string GetPlayerName(ulong clientId)
    {
        if (clientIdToUserName.TryGetValue(clientId, out string playerName))
        {
            return playerName;
        }
        return $"Player {clientId}";
    }

    public bool TryGetTeamIndex(ulong clientId, out int teamIndex)
    {
        return clientIdToTeamIndex.TryGetValue(clientId, out teamIndex);
    }
}
 