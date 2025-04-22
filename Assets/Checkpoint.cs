using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Vector3 checkpointPosition;
    private static Vector3 lastCheckpointPosition; // ตำแหน่ง Checkpoint ล่าสุด (Static เพื่อให้เข้าถึงได้จาก Movement2D)

    private void Start()
    {
        // บันทึกตำแหน่งเริ่มต้นของ Checkpoint นี้
        checkpointPosition = transform.position;

        // ถ้ายังไม่มี Checkpoint ที่บันทึกไว้ ให้ตั้ง Checkpoint แรกเป็นจุดเริ่มต้น
        if (lastCheckpointPosition == Vector3.zero)
        {
            lastCheckpointPosition = checkpointPosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Movement2D playerMovement = other.GetComponent<Movement2D>();
        if (playerMovement != null)
        {
            // เมื่อผู้เล่นสัมผัส Checkpoint นี้ ให้บันทึกตำแหน่งเป็น Checkpoint ล่าสุด
            lastCheckpointPosition = checkpointPosition;
            Debug.Log("Checkpoint reached! Position: " + lastCheckpointPosition);

            // คุณอาจเพิ่มเอฟเฟกต์ภาพหรือเสียงเมื่อถึง Checkpoint ที่นี่
        }
    }

    // ฟังก์ชันสำหรับให้ Movement2D เข้าถึงตำแหน่ง Checkpoint ล่าสุด
    public static Vector3 GetLastCheckpointPosition()
    {
        return lastCheckpointPosition;
    }
}