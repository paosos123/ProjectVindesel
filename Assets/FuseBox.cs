using UnityEngine;

public class Fusebox : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player"; // Tag ของผู้เล่น
    [SerializeField] private Sprite fuseboxOpenSprite; // Sprite สำหรับสถานะเปิด
    [SerializeField] private GameObject doorToOpen;
    [SerializeField] private GameObject qButton;// GameObject ประตูที่จะเปิด (ถ้าไม่ได้เป็น Child โดยตรง)
    private bool isPlayerInRange = false;
    private GameObject playerObjectInRange; // อ้างอิง GameObject ของผู้เล่นที่อยู่ในระยะ
    private SpriteRenderer fuseboxRenderer; // Component SpriteRenderer ของ Fusebox เอง

    void Start()
    {
        // GetComponent หา SpriteRenderer เมื่อเริ่มเกม
        fuseboxRenderer = GetComponent<SpriteRenderer>();
        if (fuseboxRenderer == null)
        {
            Debug.LogError("Fusebox GameObject ต้องมี Component SpriteRenderer!");
            enabled = false; // ปิดสคริปต์ถ้าไม่มี SpriteRenderer
            return;
        }
    }

    void Update()
    {
        if (isPlayerInRange )
        {
            qButton.SetActive(true);
        }
        else
        {
            qButton.SetActive(false);
        }
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
        // เปลี่ยน Sprite ของ Fusebox เป็นสถานะเปิด
        if (fuseboxOpenSprite != null && fuseboxRenderer != null)
        {
            fuseboxRenderer.sprite = fuseboxOpenSprite;
        }
        else
        {
            Debug.LogWarning("Fusebox Open Sprite ไม่ได้ถูกกำหนด หรือไม่พบ SpriteRenderer!");
        }

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
}