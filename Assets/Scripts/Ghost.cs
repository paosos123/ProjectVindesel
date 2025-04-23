using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    [SerializeField] private float ghostDelay;
    private float   ghostTimer;
    [SerializeField] GameObject ghostPrefab;
    private bool isGhosting = false;

    // Start is called before the first frame update
    void Start()
    {
        ghostTimer = ghostDelay;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGhosting)
        {
            if (ghostTimer > 0)
            {
                ghostTimer -= Time.deltaTime;
            }
            else
            {
                // สร้าง ghost ที่ตำแหน่งของผู้เล่น และใช้ rotation ของผู้เล่น (ตอนนี้เราจะแก้ไข localScale แทน)
                GameObject currentGhost = Instantiate(ghostPrefab, transform.position, transform.rotation);

                // ดึง SpriteRenderer ของผู้เล่น เพื่อเอา sprite ปัจจุบัน
                Sprite currentSprite = GetComponent<SpriteRenderer>().sprite;

                // ดึง SpriteRenderer และ Transform ของ ghost ที่เพิ่งสร้างขึ้นมา
                SpriteRenderer ghostSpriteRenderer = currentGhost.GetComponent<SpriteRenderer>();
                Transform ghostTransform = currentGhost.transform;

                // ตั้งค่า sprite ให้กับ ghost
                if (ghostSpriteRenderer != null)
                {
                     ghostSpriteRenderer.sprite = currentSprite;
                }

                // === ส่วนแก้ไข: ทำให้ ghost หันหน้าไปทางเดียวกับผู้เล่น ===
                // คัดลอกค่า localScale.x ของผู้เล่นไปใส่ให้ ghost
                Vector3 ghostLocalScale = ghostTransform.localScale;
                ghostLocalScale.x = transform.localScale.x; // ใช้ transform.localScale.x ของ script ตัวนี้ (ซึ่งคือผู้เล่น)
                ghostTransform.localScale = ghostLocalScale;
                // ====================================================

                ghostTimer = ghostDelay;
                Destroy(currentGhost, 0.5f); // ทำลาย ghost หลังจากผ่านไป 0.5 วินาที
            }
        }
    }

    public void StartGhosting()
    {
        isGhosting = true;
        ghostTimer = 0f; // สร้างภาพเงาทันทีที่เริ่ม
    }

    // ฟังก์ชันสำหรับหยุดการสร้างภาพเงา
    public void StopGhosting()
    {
        isGhosting = false;
    }
}