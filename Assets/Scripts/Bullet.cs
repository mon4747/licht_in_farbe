using Unity.Netcode;

using UnityEngine;

public class Bullet : NetworkBehaviour

{

    public float speed = 12f;

    public float lifeTime = 3f;

    void Start()

    {

        if (IsServer)

        {

            Destroy(gameObject, lifeTime);

        }

    }

    void Update()

    {

        // พุ่งไปตามแกน Y (Up) ของกระสุนที่ถูก Server ตั้งองศาไว้ให้แล้ว

        transform.Translate(Vector2.up * speed * Time.deltaTime);

    }

}
