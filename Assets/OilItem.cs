using UnityEngine;
using System.Collections;

public class OilItem : MonoBehaviour
{
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
            Debug.LogError("OilItem GameObject ต้องมี Collider2D และ SpriteRenderer!");
            enabled = false; // ปิดสคริปต์หากไม่มี Component ที่จำเป็น
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Movement2D playerMovement = other.GetComponent<Movement2D>();
        if (playerMovement != null)
        {
            // เรียกฟังก์ชันใน Movement2D เพื่อรีเซ็ตคูลดาวน์ Dash
            playerMovement.ResetDashCooldown();
            Debug.Log("Player collected Oil! Dash cooldown reset.");

            // ปิดการใช้งาน Collider และ Renderer เพื่อทำให้ไอเทม "หายไป"
            if (itemCollider != null)
            {
                itemCollider.enabled = false;
            }
            if (itemRenderer != null)
            {
                itemRenderer.enabled = false;
            }

            // เริ่ม Coroutine เพื่อรอ 5 วินาทีแล้วเปิดใช้งานไอเทมใหม่
            StartCoroutine(ResetItemAfterDelay(5f));
        }
    }

    private IEnumerator ResetItemAfterDelay(float delay)
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