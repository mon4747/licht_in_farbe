using Unity.Netcode;

using UnityEngine;

public class Bullet : NetworkBehaviour

{

    public float speed = 12f;

    public ulong ownerId; // รับค่ามาจาก ServerRpc ตอนยิง

    void Update()

    {

        transform.Translate(Vector2.up * speed * Time.deltaTime);

    }

    private void OnTriggerEnter2D(Collider2D other)

    {

        if (!IsServer) return;

        // เช็กว่าชน Player หรือไม่

        if (other.TryGetComponent(out PlayerHealth health))

        {

            // รับ ID ของผู้เล่นที่โดนชน

            ulong hitPlayerId = other.GetComponent<NetworkObject>().OwnerClientId;

            // ถ้าไม่ใช่คนยิงเอง ให้ลดเลือดและทำลายกระสุน

            if (hitPlayerId != ownerId)

            {

                health.TakeDamage(25);

                GetComponent<NetworkObject>().Despawn();

            }

        }

    }

}
