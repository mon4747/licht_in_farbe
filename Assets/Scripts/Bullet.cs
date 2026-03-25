using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float speed = 12f;
    public float lifeTime = 3f;
    public int damageAmount = 10;
    
    // ตัวแปรเก็บ ID ของผู้เล่นที่เป็นคนยิง
    [HideInInspector] public ulong shooterId; 

    void Start()
    {
        if (IsServer) Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out PlayerHealth targetHealth))
        {
            // ตรวจสอบ: ถ้า ID ของเป้าหมาย "ตรงกับ" ID คนยิง ให้ข้ามไป (ไม่ชน)
            if (targetHealth.OwnerClientId == shooterId) return;

            // ถ้าไม่ใช่คนยิงเอง ให้ลดเลือดและทำลายกระสุน
            targetHealth.TakeDamage(damageAmount);
            Destroy(gameObject);
        }
    }
}