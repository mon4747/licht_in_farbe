using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports;
using UnityEngine;
using Unity.Collections;

public class PlayerSpawner : MonoBehaviour

{

        public struct TeamSelectionMessage : INetworkSerializable
    {
        public int teamIndex;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref teamIndex);
        }
    }

    [SerializeField] private GameObject[] playerPrefabs; // ลาก Prefab ตัวละคร 4 ตัวใส่ที่นี่

    private const string PlayerTeamKey = "SelectedTeamIndex";
    private Dictionary<ulong, int> clientTeams = new Dictionary<ulong, int>();
    private readonly HashSet<ulong> spawnedClients = new HashSet<ulong>();

    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("PlayerSpawner: NetworkManager not found on start.");
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("TeamSelection", OnTeamSelectionReceived);

        if (NetworkManager.Singleton.IsServer)
        {
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                SpawnPlayer(clientId);
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            SpawnPlayer(clientId);
        }
    }

    private void OnTeamSelectionReceived(ulong senderClientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out TeamSelectionMessage message);
        clientTeams[senderClientId] = message.teamIndex;
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (spawnedClients.Contains(clientId))
        {
            Debug.LogWarning($"PlayerSpawner: client {clientId} already spawned, skipping duplicate.");
            return;
        }

        spawnedClients.Add(clientId);

        int index;
        if (clientTeams.TryGetValue(clientId, out int teamIndex))
        {
            index = teamIndex;
        }
        else if (NetworkServer.Instance != null && NetworkServer.Instance.TryGetTeamIndex(clientId, out int approvedTeamIndex))
        {
            index = approvedTeamIndex;
            Debug.Log($"PlayerSpawner: spawn using connection payload team index {index} for client {clientId}");
        }
        else if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId)
        {
            index = PlayerPrefs.GetInt(PlayerTeamKey, 0);
            Debug.Log($"PlayerSpawner: local client spawn fallback team index {index}");
        }
        else
        {
            index = 0;
            Debug.Log($"PlayerSpawner: no team selection message for client {clientId}, defaulting to prefab 0");
        }

        index = Mathf.Abs(index) % playerPrefabs.Length;
        GameObject prefabToSpawn = playerPrefabs[index];
 
        // หาจุดเกิดจาก SpawnManager ที่เราทำไว้

        Vector3 spawnPos = SpawnManager.Instance.GetSpawnPosition(clientId);
 
        // สร้างตัวละครบน Server

        GameObject playerInstance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        // สำคัญ: สั่ง Spawn พร้อมมอบสิทธิ์การเป็นเจ้าของ (Owner) ให้ Client คนนั้น

        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

    }

}
 