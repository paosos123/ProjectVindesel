using UnityEngine;

public class PlayerProximityActivator : MonoBehaviour
{
    public float detectionDistance = 20f; // ระยะที่ผู้เล่นจะต้องเข้ามาใกล้
    public GameObject targetObject; // GameObject ที่คุณต้องการให้เปิด/ปิด

    private Transform playerTransform;

    void Start()
    {
        // ค้นหา GameObject ที่มี Tag เป็น "Player" ในตอนเริ่มต้น
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("ไม่พบ GameObject ที่มี Tag 'Player' ใน Scene!");
            enabled = false; // ปิดการทำงานของสคริปต์หากไม่พบ Player
        }

        // ตรวจสอบว่าได้กำหนด Target Object แล้วหรือไม่
        if (targetObject == null)
        {
            Debug.LogError("ไม่ได้กำหนด Target Object ใน Inspector!");
            enabled = false; // ปิดการทำงานของสคริปต์หากไม่ได้กำหนด Target Object
        }
    }

    void Update()
    {
        // ตรวจสอบว่า playerTransform และ targetObject ถูกกำหนดแล้วหรือไม่
        if (playerTransform != null && targetObject != null)
        {
            // คำนวณระยะห่างระหว่าง GameObject หลักนี้กับ Player
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // ตั้งค่า Active ของ Target Object ตามระยะห่าง
            targetObject.SetActive(distanceToPlayer <= detectionDistance);
        }
    }

    // วาด Gizmos ใน Scene View เพื่อแสดงระยะการตรวจจับ
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
    }
}