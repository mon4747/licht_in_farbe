using Unity.Netcode;
using UnityEngine;
using System;

// 1. ปรับปรุง struct ให้รองรับการอ่านเขียนข้อมูล (สำคัญมาก)
[Serializable]
public struct PlayerScoreData : INetworkSerializable, IEquatable<PlayerScoreData>
{
    public ulong ClientId;
    public int Score;

    // ฟังก์ชันนี้คือหัวใจที่ทำให้ Error สีแดงหายไป
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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        // กำหนด Permission ให้ชัดเจน
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
        for (int i = 0; i < PlayerScores.Count; i++)
        {
            if (PlayerScores[i].ClientId == clientId)
            {
                var data = PlayerScores[i];
                data.Score += points;
                PlayerScores[i] = data; // บรรทัดนี้จะทำงานได้เมื่อ Serialize ผ่านแล้ว
                Debug.Log($"Server updated score for {clientId}: {data.Score}");
                break;
            }
        }
    }
}