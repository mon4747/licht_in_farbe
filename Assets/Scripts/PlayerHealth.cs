using Unity.Netcode;

using UnityEngine;

public class PlayerHealth : NetworkBehaviour

{

    // NetworkVariable ทำให้ทุกคนเห็นเลือดตรงกันเสมอ

    public NetworkVariable<int> currentHp = new NetworkVariable<int>(100);

    public void TakeDamage(int damage)

    {

        if (!IsServer) return;

        currentHp.Value -= damage;

        if (currentHp.Value <= 0)

        {

            currentHp.Value = 100;

            // วาร์ปกลับไปจุดเกิด

            transform.position = SpawnManager.Instance.GetSpawnPosition(OwnerClientId);

        }

    }

}
