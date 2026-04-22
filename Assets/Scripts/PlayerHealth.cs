using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    // ใช้ NetworkVariable เพื่อให้เลือด Sync ตรงกันทุกคน
    public NetworkVariable<int> currentHp = new NetworkVariable<int>(100);

    /// <summary>
    /// ฟังก์ชันรับความเสียหายและคืนค่าว่าตายหรือไม่ (สำหรับระบบ Leaderboard)
    /// </summary>
    public bool TryTakeDamage(int damage)
    {
        // การคำนวณพลังชีวิตต้องเกิดขึ้นที่ Server เท่านั้น
        if (!IsServer) return false;

        currentHp.Value -= damage;

        if (currentHp.Value <= 0)
        {
            currentHp.Value = 100; // Reset เลือดเมื่อตาย
            
            // สั่งให้ Client เครื่องที่เป็นเจ้าของตัวละครวาร์ปกลับจุดเกิด
            RespawnClientRpc(OwnerClientId);
            
            return true; // คืนค่าว่า "ตายแล้ว"
        }
        return false; // ยังไม่ตาย
    }

    /// <summary>
    /// ฟังก์ชันเดิม (ถ้ายังมี Script อื่นเรียกใช้อยู่)
    /// </summary>
    public void TakeDamage(int damage)
    {
        TryTakeDamage(damage);
    }

    [ClientRpc]
    private void RespawnClientRpc(ulong clientId)
    {
        // เฉพาะเจ้าของตัวละคร (Owner) เท่านั้นที่เป็นคนจัดการตำแหน่งของตัวเอง
        if (IsOwner)
        {
            // ดึงตำแหน่งจาก SpawnManager
            Vector3 spawnPos = SpawnManager.Instance.GetSpawnPosition(clientId);
            transform.position = spawnPos;
        }
    }
}