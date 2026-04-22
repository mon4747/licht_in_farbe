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
        if (targetHealth.OwnerClientId == shooterId) return;

        // เปลี่ยนจาก TakeDamage เป็น TryTakeDamage (ถ้าคุณแก้ไขใน PlayerHealth แล้ว)
        if (targetHealth.TryTakeDamage(damageAmount)) 
        {
            // เปลี่ยนชื่อเรียกให้ตรงกับ ScoreManager
            ScoreManager.Instance.AddScoreRpc(shooterId, 0); 
        }
        Destroy(gameObject);
    }
}
}