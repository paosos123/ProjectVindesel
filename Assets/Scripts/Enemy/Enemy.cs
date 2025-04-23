using UnityEngine;
using System.Collections; // ต้องเพิ่ม using นี้สำหรับ Coroutine

public class Enemy : MonoBehaviour
{
    [SerializeField] protected int maxHp = 5; // เพิ่ม max HP เหมือนผู้เล่น
    protected int currentHp;

    public bool IsDead { get; protected set; } // ทำให้ IsDead อ่านได้จากภายนอกและตั้งค่าได้ในคลาสนี้และคลาสลูก
    public int CurrentHp { get { return currentHp; } } //

    protected virtual void Start()
    {
        currentHp = maxHp;
        IsDead = false;
    }

    public virtual bool TakeDamage(int damageAmount)
    {
        if (IsDead) return false; // ถ้าตายแล้ว ไม่รับ damage อีก

        currentHp -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Current HP: " + currentHp);

        if (currentHp <= 0)
        {
            IsDead = true; // ตั้งค่า IsDead เป็น true ก่อนที่จะทำลาย object หรือเริ่มกระบวนการตาย
            Die(); // เรียก method การตาย
            return true; // แจ้งว่าตาย
        }

        // ถ้าไม่ตาย อาจจะมีการทำอะไรบางอย่างเมื่อโดนโจมตีแต่ไม่ตาย
        // เช่น แสดง animation โดนตี
        // ...

        return false; // แจ้งว่าไม่ตาย
    }

    protected virtual void Die()
    {
        Debug.Log(gameObject.name + " is dead.");
        // นี่คือส่วนที่คุณจะจัดการการตายของศัตรู
        // เช่น เล่น animation ตาย, ปล่อย item, ทำลาย object
        // เพื่อให้แน่ใจว่าโค้ดฝั่งผู้เล่นทำงานถูกต้อง อาจจะหน่วงเวลาก่อน Destroy สักเล็กน้อย
        // หรือใช้การตั้งค่า active(false) แทนการ Destroy ทันทีถ้าใช้ Pooling
        Destroy(gameObject, 0.1f); // ตัวอย่าง: ทำลาย object หลังจาก 0.1 วินาที
        // หรือ gameObject.SetActive(false); ถ้าใช้ object pooling
    }

    // Coroutine สำหรับหน่วงเวลาการทำลาย
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    // คุณอาจจะต้องมีฟังก์ชันสำหรับ Reset ค่าต่างๆ ถ้าใช้ Object Pooling
    // เช่น public void ResetEnemy() { ... }
}