using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Collections;

[Serializable]
public struct PlayerScoreData : INetworkSerializable, IEquatable<PlayerScoreData>
{
    public ulong ClientId;
    public int Score;
    public FixedString32Bytes Name;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref Score);
        serializer.SerializeValue(ref Name);
    }

    public bool Equals(PlayerScoreData other)
    {
        return ClientId == other.ClientId && Score == other.Score && Name.Equals(other.Name);
    }
}

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance;
    public NetworkList<PlayerScoreData> PlayerScores;
    
    [HideInInspector]
    public bool canAddScore = true; // ใช้ควบคุมการหยุดบวกคะแนน

    private void Awake()
    {
        if (Instance == null) Instance = this;
        PlayerScores = new NetworkList<PlayerScoreData>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            AddPlayer(NetworkManager.Singleton.LocalClientId);
            NetworkManager.Singleton.OnClientConnectedCallback += AddPlayer;
        }
    }

    private void AddPlayer(ulong clientId)
    {
        string playerName = GetPlayerNameForClient(clientId);
        AddPlayer(clientId, playerName);
    }

    private void AddPlayer(ulong clientId, string playerName)
    {
        foreach (var p in PlayerScores) if (p.ClientId == clientId) return;
        PlayerScores.Add(new PlayerScoreData { ClientId = clientId, Score = 0, Name = new FixedString32Bytes(playerName) });
    }

    private string GetPlayerNameForClient(ulong clientId)
    {
        if (NetworkServer.Instance != null)
        {
            string serverName = NetworkServer.Instance.GetPlayerName(clientId);
            if (!string.IsNullOrWhiteSpace(serverName))
            {
                return serverName;
            }
        }
        return $"Player {clientId}";
    }

    [Rpc(SendTo.Server)]
    public void AddScoreRpc(ulong clientId, int points)
    {
        if (!canAddScore) return; // ถ้าเกมจบ Server จะไม่บวกคะแนนให้

        for (int i = 0; i < PlayerScores.Count; i++)
        {
            if (PlayerScores[i].ClientId == clientId)
            {
                var data = PlayerScores[i];
                data.Score += points;
                PlayerScores[i] = data;
                break;
            }
        }
    }

    // ฟังก์ชันหาที่ 1
    public PlayerScoreData GetWinner()
    {
        PlayerScoreData winner = new PlayerScoreData { ClientId = 999, Score = -1 };
        foreach (var p in PlayerScores)
        {
            if (p.Score > winner.Score) winner = p;
        }
        return winner;
    }
}