using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public NetworkVariable<int> currentHp = new NetworkVariable<int>(100);

    public void TakeDamage(int damage)
    {
        if (!IsServer) return;

        currentHp.Value -= damage;

        if (currentHp.Value <= 0)
        {
            currentHp.Value = 100;
            
            // แทนที่จะวาร์ปตรงๆ ให้เรียก ClientRpc เพื่อสั่ง Client คนนั้น
            RespawnClientRpc(OwnerClientId);
        }
    }

    [ClientRpc]
    private void RespawnClientRpc(ulong clientId)
    {
        // เฉพาะเจ้าของตัวละคร (Owner) เท่านั้นที่เป็นคนวาร์ปตัวเอง
        if (IsOwner)
        {
            Vector3 spawnPos = SpawnManager.Instance.GetSpawnPosition(clientId);
            transform.position = spawnPos;
        }
    }
}