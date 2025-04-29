using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeTable : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player"; // Tag ของผู้เล่น
    // รัศมีที่ผู้เล่นสามารถ Interact ได้
    [SerializeField] private GameObject upgradeUI; 
    [SerializeField] private GameObject qButton;// GameObject UI อัปเกรด
    private bool isPlayerInRange = false;
    private GameObject playerObjectInRange; // อ้างอิง GameObject ของผู้เล่นที่อยู่ในระยะ
    private bool hasActivated = false; // เพิ่ม Flag สำหรับตรวจสอบว่าเปิดใช้งานไปแล้วหรือไม่

    void Update()
    {
        // ตรวจสอบว่าผู้เล่นอยู่ในระยะ, กดปุ่ม 'Q' และยังไม่ได้เปิดใช้งาน
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.Q) && !hasActivated)
        {
            ActivateUpgrade();
            hasActivated = true; // ตั้งค่า Flag เป็น true หลังจากเปิดใช้งานครั้งแรก
        }

        if (isPlayerInRange)
        {
            qButton.SetActive(true);
        }
        else
        {
            qButton.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ตรวจสอบว่า GameObject ที่เข้ามาใน Trigger มี Tag ตรงกับ playerTag หรือไม่
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            playerObjectInRange = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // ตรวจสอบว่า GameObject ที่ออกจาก Trigger มี Tag ตรงกับ playerTag หรือไม่
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            playerObjectInRange = null;
        }
    }

    private void  ActivateUpgrade()
    {
        upgradeUI.SetActive(true);
        Debug.Log("เปิด UI อัปเกรด");
        // คุณอาจต้องการปิดการใช้งาน Collider ของ UpgradeTable หลังจากเปิดใช้งานแล้ว
        // GetComponent<Collider2D>().enabled = false;
    }

    // วาด Gizmos เพื่อแสดงรัศมีการ Interact ใน Scene View
    
}