using UnityEngine;

public class SpawnManager : MonoBehaviour

{

    public static SpawnManager Instance;

    [SerializeField] private Transform[] spawnPoints; // ลากจุดเกิด 4 จุดใส่ตรงนี้

    private void Awake() => Instance = this;

    public Vector3 GetSpawnPosition(ulong clientId)

    {

        if (spawnPoints == null || spawnPoints.Length == 0) return Vector3.zero;

        int index = (int)(clientId % (ulong)spawnPoints.Length);

        return spawnPoints[index].position;

    }

}
