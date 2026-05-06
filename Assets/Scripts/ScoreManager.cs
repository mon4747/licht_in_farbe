using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public struct PlayerScoreData : INetworkSerializable, IEquatable<PlayerScoreData>
{
    public ulong ClientId;
    public int Score;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref Score);
    }

    public bool Equals(PlayerScoreData other)
    {
        return ClientId == other.ClientId && Score == other.Score;
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
        foreach (var p in PlayerScores) if (p.ClientId == clientId) return;
        PlayerScores.Add(new PlayerScoreData { ClientId = clientId, Score = 0 });
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