using Unity.Netcode;

using UnityEngine;
 
public class PlayerSpawner : MonoBehaviour

{

    [SerializeField] private GameObject[] playerPrefabs; // ลาก Prefab ตัวละคร 4 ตัวใส่ที่นี่
 
    private void Start()

    {

        // เมื่อมีใครกด Connect เข้ามา (ทั้ง Host และ Client)

        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>

        {

            if (NetworkManager.Singleton.IsServer)

            {

                SpawnPlayer(clientId);

            }

        };

    }
 
    private void SpawnPlayer(ulong clientId)

    {

        // เลือก Prefab ตามลำดับการเข้าเกม (0, 1, 2, 3)

        int index = (int)(clientId % (ulong)playerPrefabs.Length);

        GameObject prefabToSpawn = playerPrefabs[index];
 
        // หาจุดเกิดจาก SpawnManager ที่เราทำไว้

        Vector3 spawnPos = SpawnManager.Instance.GetSpawnPosition(clientId);
 
        // สร้างตัวละครบน Server

        GameObject playerInstance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        // สำคัญ: สั่ง Spawn พร้อมมอบสิทธิ์การเป็นเจ้าของ (Owner) ให้ Client คนนั้น

        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

    }

}
 