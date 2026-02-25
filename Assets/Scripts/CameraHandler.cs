using Unity.Netcode;

using UnityEngine;

public class CameraHandler : NetworkBehaviour

{

    public override void OnNetworkSpawn()

    {

        // ถ้าไม่ใช่ตัวเรา (IsOwner == false) ให้ปิดกล้องนี้ทิ้ง

        if (!IsOwner)

        {

            gameObject.SetActive(false);

            // ถ้ามี AudioListener อยู่ใน Prefab ด้วย ก็ควรปิดด้วยครับ

            if (TryGetComponent<AudioListener>(out var listener))

                listener.enabled = false;

        }

    }

}
