using Unity.Netcode;

using UnityEngine;

public class PlayerController : NetworkBehaviour

{

    [Header("Movement")]

    public float moveSpeed = 5f;

    [Header("Shooting")]

    [SerializeField] private GameObject[] bulletPrefabs;

    [SerializeField] private Transform shootPoint;

    private Rigidbody2D rb;

    void Awake()

    {

        rb = GetComponent<Rigidbody2D>();

    }

    void Update()

    {

        if (!IsOwner) return;

        // การเคลื่อนที่ปกติ (ตัวละครจะไม่หมุนตามเมาส์แล้ว)

        float moveX = Input.GetAxisRaw("Horizontal");

        float moveY = Input.GetAxisRaw("Vertical");

        rb.linearVelocity = new Vector2(moveX, moveY).normalized * moveSpeed;

        // เมื่อคลิกซ้าย

        if (Input.GetButtonDown("Fire1"))

        {

            // คำนวณหาตำแหน่งเมาส์ในโลกเกม

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            mousePos.z = 0;

            // หาเวกเตอร์ทิศทางจากจุดยิงไปหาเมาส์

            Vector2 shootDir = (mousePos - shootPoint.position).normalized;

            // ส่งทิศทางไปให้ Server สั่งยิง

            FireServerRpc(shootDir);

        }

    }

    [ServerRpc]

    void FireServerRpc(Vector2 direction)

    {

        int index = (int)(OwnerClientId % (ulong)bulletPrefabs.Length);

        // คำนวณมุมหมุนของกระสุนจากทิศทางที่ส่งมา

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // สร้างกระสุนตามมุมที่คำนวณได้

        GameObject bullet = Instantiate(bulletPrefabs[index], shootPoint.position, rotation);

        // สั่งให้ระบบ Network รู้จักกระสุน

        bullet.GetComponent<NetworkObject>().Spawn();

    }

}
