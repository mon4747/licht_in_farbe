using Unity.Netcode;

using UnityEngine;

public class PlayerController : NetworkBehaviour

{

    [Header("Movement")]

    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;

    private Vector2 moveInput;

    [Header("Weapon System")]

    [SerializeField] private Transform shootPoint;

    // ลาก Bullet Prefab ทั้ง 4 สีมาใส่ในช่องนี้ (Element 0-3)

    [SerializeField] private GameObject[] bulletPrefabs;

    public override void OnNetworkSpawn()

    {

        rb = GetComponent<Rigidbody2D>();

        if (IsOwner)

        {

            // วาร์ปไปจุดเกิดตาม ID

            transform.position = SpawnManager.Instance.GetSpawnPosition(OwnerClientId);

        }

    }

    void Update()

    {

        if (!IsOwner) return;

        moveInput.x = Input.GetAxisRaw("Horizontal");

        moveInput.y = Input.GetAxisRaw("Vertical");

        // ยิงกระสุน

        if (Input.GetButtonDown("Fire1"))

        {

            FireServerRpc();

        }

    }

    void FixedUpdate()

    {

        if (!IsOwner) return;

        rb.linearVelocity = moveInput.normalized * moveSpeed;

    }

    [ServerRpc]

    private void FireServerRpc()

    {

        // คำนวณ Index: ถ้า ID 0 จะได้กระสุนช่อง 0, ID 1 ได้ช่อง 1...

        // % bulletPrefabs.Length ช่วยกัน Error ถ้ามีผู้เล่นมากกว่าจำนวนกระสุน

        int bulletIndex = (int)(OwnerClientId % (ulong)bulletPrefabs.Length);

        GameObject prefabToSpawn = bulletPrefabs[bulletIndex];

        // สร้างกระสุนบน Server

        GameObject bulletInstance = Instantiate(prefabToSpawn, shootPoint.position, shootPoint.rotation);

        // สั่งให้เกิดใน Network เพื่อให้ทุกคนเห็น

        NetworkObject nObj = bulletInstance.GetComponent<NetworkObject>();

        nObj.Spawn();

        // ส่ง ID เจ้าของไปที่กระสุน (ป้องกันยิงโดนตัวเอง)

        if (bulletInstance.TryGetComponent(out Bullet bulletScript))

        {

            bulletScript.ownerId = OwnerClientId;

        }

    }

}
