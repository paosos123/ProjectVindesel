using UnityEngine;

public class OilItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Movement2D playerMovement = other.GetComponent<Movement2D>();
        if (playerMovement != null)
        {
            // เรียกฟังก์ชันใน Movement2D เพื่อรีเซ็ตคูลดาวน์ Dash
            playerMovement.ResetDashCooldown();
            Debug.Log("Player collected Oil! Dash cooldown reset.");

            // คุณอาจต้องการทำลาย GameObject ของไอเทมหลังจากผู้เล่นเก็บ
            Destroy(gameObject);
        }
    }
}