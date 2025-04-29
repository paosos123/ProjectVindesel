using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : Enemy
{
    [Header("Patrol Variables")]
    [SerializeField] private GameObject pointA;
    [SerializeField] private GameObject pointB;
    [SerializeField] private float patrolSpeed = 2f; // ความเร็วในการลาดตระเวน
    [SerializeField] private float chaseSpeed = 4f; // ความเร็วในการไล่ตาม

    private Rigidbody2D rb;
    private Animator anim;
    private Transform currentPoint;
    private bool isFacingRight = true;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask playerLayer; // Layer ของ Player สำหรับตรวจจับ

    [Header("Detection and Explosion")]
    [SerializeField] private float detectionRange = 5f; // ระยะตรวจจับผู้เล่น
    [SerializeField] private float explosionTimer = 3f; // เวลาหน่วงก่อนระเบิด
    [SerializeField] private float explosionRadius = 2f; // รัศมีการระเบิด
    [SerializeField] private float explosionDamage = 1f; // ดาเมจจากการระเบิด
    [SerializeField] private GameObject explosionPrefab; // Prefab ที่จะสร้างเมื่อระเบิด
    private bool canExplode = false;
    private float currentExplosionTimer;
    public bool isDetected = false; // ตัวแปรบอกว่าตรวจพบผู้เล่นหรือไม่
    public Transform playerTransform; // Transform ของผู้เล่น (จะถูกตั้งค่าเมื่อตรวจพบ)

    // Start is called before the first frame update
    void Start()
    {
        base.Start(); // เรียกใช้ฟังก์ชัน Start() ของคลาส Enemy
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentPoint = pointB.transform;
        currentExplosionTimer = explosionTimer;
        // anim.SetBool("isRunning",true);
    }

    // Update is called once per frame
    void Update()
    {
        // ตรวจจับผู้เล่น
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (playerCollider != null && playerTransform == null)
        {
            isDetected = true;
            playerTransform = playerCollider.transform;
            canExplode = true; // เริ่มสถานะพร้อมระเบิดเมื่อเจอผู้เล่น
            Debug.Log("Enemy detected player!");
        }

        if (canExplode)
        {
            currentExplosionTimer -= Time.deltaTime;
            if (currentExplosionTimer <= 0f)
            {
                Explode();
            }
            else
            {
                // แสดงเวลาที่เหลือก่อนระเบิด (สำหรับ Debug)
                // Debug.Log("Exploding in: " + currentExplosionTimer);

                // ไล่ตามผู้เล่นในขณะที่นับถอยหลัง
                if (playerTransform != null)
                {
                    Vector2 direction = (playerTransform.position - transform.position).normalized;
                    rb.velocity = direction * chaseSpeed;

                    // ปรับทิศทาง
                    if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
                    {
                        Flip();
                    }
                }
                else
                {
                    // ถ้า playerTransform หายไป (เช่น ผู้เล่นตาย) ให้หยุดเคลื่อนที่
                    rb.velocity = Vector2.zero;
                }
            }
        }
        else
        {
            // ลาดตระเวนตามปกติ
            Patrol();
        }
    }

    void Patrol()
    {
        Vector2 point = currentPoint.position - transform.position;
        if (currentPoint == pointB.transform)
        {
            rb.velocity = new Vector2(patrolSpeed, 0);
        }
        else
        {
            rb.velocity = new Vector2(-patrolSpeed, 0);
        }

        if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f)
        {
            // Switch to the other point
            if (currentPoint == pointB.transform)
            {
                currentPoint = pointA.transform;
                Flip();
            }
            else
            {
                currentPoint = pointB.transform;
                Flip();
            }
        }
    }

    void Explode()
    {
        Debug.Log("Enemy Exploded!");
        // สร้าง Prefab ณ ตำแหน่งที่ศัตรูอยู่
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Explosion Prefab is not assigned!");
        }

        // ตรวจสอบผู้เล่นที่อยู่ในรัศมีการระเบิด
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, playerLayer);
        foreach (var hitCollider in hitColliders)
        {
            Movement2D playerMovement = hitCollider.GetComponent<Movement2D>();
            if (playerMovement != null)
            {
                playerMovement.TakeDamage((int)explosionDamage);
                Debug.Log("Player took " + explosionDamage + " damage from explosion!");
            }
        }

        // ทำลายตัวเองหลังจากระเบิด
        Destroy(gameObject);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (pointA != null && pointB != null)
        {
            Gizmos.DrawWireSphere(pointA.transform.position, 0.5f);
            Gizmos.DrawWireSphere(pointB.transform.position, 0.5f);
            Gizmos.DrawLine(pointA.transform.position, pointB.transform.position);
        }

        // แสดงรัศมีตรวจจับ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // แสดงรัศมีการระเบิด (สีแดงเมื่อตรวจพบผู้เล่น)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // คุณอาจมีโค้ด OnTriggerEnter2D อื่นๆ ที่นี่
    }
    /*private bool isDetected()
    {
        if ())
        {
            return true;
        }
        else
        {
            return false;
        }
    }*/
}