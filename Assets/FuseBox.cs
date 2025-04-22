using UnityEngine;

public class Fusebox : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player"; // Tag ของผู้เล่น
    [SerializeField] private float interactionRadius = 2f; // รัศมีที่ผู้เล่นสามารถ Interact กับกล่องฟิวส์ได้
    [SerializeField] private GameObject doorToOpen; // GameObject ประตูที่จะเปิด (ถ้าไม่ได้เป็น Child โดยตรง)
    private bool isPlayerInRange = false;
    private GameObject playerObjectInRange; // อ้างอิง GameObject ของผู้เล่นที่อยู่ในระยะ

    void Update()
    {
        // ตรวจสอบว่าผู้เล่นอยู่ในระยะและกดปุ่ม 'Q'
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.Q))
        {
            ActivateFusebox();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ตรวจสอบว่า GameObject ที่เข้ามาใน Trigger มี Tag ตรงกับ playerTag หรือไม่
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            playerObjectInRange = other.gameObject;
            Debug.Log("ผู้เล่นเข้าสู่ระยะกล่องฟิวส์");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // ตรวจสอบว่า GameObject ที่ออกจาก Trigger มี Tag ตรงกับ playerTag หรือไม่
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            playerObjectInRange = null;
            Debug.Log("ผู้เล่นออกจากระยะกล่องฟิวส์");
        }
    }

    private void ActivateFusebox()
    {
        if (doorToOpen != null)
        {
            // กรณีที่ประตูถูกกำหนดไว้ใน Inspector โดยตรง
            doorToOpen.SetActive(false); // ปิดการใช้งาน GameObject ประตู (ทำให้ "เปิด")
            Debug.Log("เปิดประตู (เปิดใช้งานกล่องฟิวส์ - อ้างอิงโดยตรง)");
        }
        else if (transform.childCount > 0)
        {
            // กรณีที่ประตูเป็น Child ของกล่องฟิวส์
            GameObject doorChild = transform.GetChild(0).gameObject; // สมมติว่าประตูเป็น Child แรก
            doorChild.SetActive(false); // ปิดการใช้งาน GameObject ประตู (ทำให้ "เปิด")
            Debug.Log("เปิดประตู (เปิดใช้งานกล่องฟิวส์ - เป็น Child)");
        }
        else
        {
            Debug.LogWarning("ไม่พบประตูที่จะเปิด!");
        }

        // คุณอาจเพิ่มการทำงานอื่นๆ ที่นี่ เช่น เล่นเสียง หรือเปลี่ยนอนิเมชันกล่องฟิวส์
    }

    // วาด Gizmos เพื่อแสดงรัศมีการ Interact ใน Scene View
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}