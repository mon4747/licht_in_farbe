using UnityEngine;

public class MouseClickSound : MonoBehaviour
{
    public AudioSource audioSource; // ลาก AudioSource มาใส่ใน Inspector

    void Update()
    {
        // ตรวจจับการคลิกเมาส์ซ้าย (0 คือคลิกซ้าย, 1 คือคลิกขวา)
        if (Input.GetMouseButtonDown(0))
        {
            audioSource.Play(); // เล่นเสียง
        }
    }
}
