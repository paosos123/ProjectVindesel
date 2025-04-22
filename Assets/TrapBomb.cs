using UnityEngine;

public class TrapBomb : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1; // กำหนดจำนวนความเสียหายที่กับระเบิดทำ
    [SerializeField] private string playerTag = "Player"; // Tag ของผู้เล่น

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag)) // ตรวจสอบ Tag ก่อน
        {
            Movement2D playerMovement = other.GetComponent<Movement2D>();
            if (playerMovement != null)
            {
                // ลด HP ของผู้เล่นโดยเรียกใช้ฟังก์ชัน TakeDamage ใน Movement2D
                playerMovement.TakeDamage(damageAmount);

                // ทำลายกับระเบิด (ทางเลือก)
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("ชนกับ GameObject ที่มี Tag 'Player' แต่ไม่มี Component 'Movement2D'!");
            }
        }
    }
}