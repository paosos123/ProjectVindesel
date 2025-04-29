using UnityEngine;
using System.Collections;

public class HealItem : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player"; // Tag ของผู้เล่น
    [SerializeField] private float respawnDelay = 10f; // ระยะเวลาในการกลับมาของไอเทม (วินาที)
    private Collider2D itemCollider;
    private SpriteRenderer itemRenderer;

    void Start()
    {
        // เก็บ Component Collider2D และ SpriteRenderer ไว้ใช้งาน
        itemCollider = GetComponent<Collider2D>();
        itemRenderer = GetComponent<SpriteRenderer>();

        // ตรวจสอบว่ามี Component ที่จำเป็นหรือไม่
        if (itemCollider == null || itemRenderer == null)
        {
            Debug.LogError("HealItem GameObject ต้องมี Collider2D และ SpriteRenderer!");
            enabled = false; // ปิดสคริปต์หากไม่มี Component ที่จำเป็น
        }

        // ตรวจสอบให้แน่ใจว่า Collider เป็น Trigger
        if (itemCollider != null && !itemCollider.isTrigger)
        {
            Debug.LogError("Collider2D บน HealItem GameObject ต้องตั้งค่า Is Trigger เป็น true!");
            enabled = false; // ปิดสคริปต์หาก Collider ไม่ใช่ Trigger
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ตรวจสอบว่า GameObject ที่เข้ามาใน Trigger มี Tag ตรงกับ playerTag หรือไม่
        if (other.CompareTag(playerTag))
        {
            // หา Component Movement2D จาก GameObject ของผู้เล่น
            Movement2D playerMovement = other.GetComponent<Movement2D>();

            // ตรวจสอบว่าพบ Component Movement2D หรือไม่
            if (playerMovement != null)
            {
                // เรียกฟังก์ชันเพื่อฟื้น HP ให้เต็ม
                HealPlayer(playerMovement);

                // ปิดการใช้งาน Collider และ Renderer และเริ่ม Coroutine สำหรับการกลับมา
                DisableItemForRespawn();
            }
        }
    }

    private void HealPlayer(Movement2D player)
    {
        // เรียกใช้ฟังก์ชัน Heal() ใน Movement2D เพื่อฟื้น HP ให้เต็ม
        player.Heal();
        Debug.Log("HealItem triggered player's Heal() function.");

        // คุณอาจเพิ่ม Effect หรือเสียงเมื่อเก็บไอเทมที่นี่
    }

    private void DisableItemForRespawn()
    {
        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }
        if (itemRenderer != null)
        {
            itemRenderer.enabled = false;
        }

        // เริ่ม Coroutine เพื่อรอและเปิดใช้งานไอเทมใหม่
        StartCoroutine(RespawnItemAfterDelay(respawnDelay));
    }

    private IEnumerator RespawnItemAfterDelay(float delay)
    {
        // รอตามระยะเวลาที่กำหนด
        yield return new WaitForSeconds(delay);

        // เปิดใช้งาน Collider และ Renderer อีกครั้ง
        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }
        if (itemRenderer != null)
        {
            itemRenderer.enabled = true;
        }
    }
}