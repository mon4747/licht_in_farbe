using Unity.Netcode;

using UnityEngine;

using Unity.Cinemachine; // ถ้าใช้ Cinemachine v3

// using Cinemachine; // ถ้าใช้ Cinemachine v2

public class PlayerCameraHandler : NetworkBehaviour

{

    private void Start()

    {

        // ถ้าไม่ใช่ตัวละครของเรา (เป็นของผู้เล่นอื่นที่เชื่อมต่อเข้ามา)

        if (!IsOwner)

        {

            // ปิดกล้องทิ้งไปเลย! ให้เหลือแค่กล้องในตัวเราที่ทำงาน

            gameObject.SetActive(false);

            // หรือถ้ามี AudioListener ติดมาด้วย ก็ควรปิดที่ Main Camera ของคนอื่นด้วย

            // แต่ปกติ Main Camera จะอยู่ในฉากหลักอยู่แล้ว จึงไม่มีปัญหา

        }

    }

}
