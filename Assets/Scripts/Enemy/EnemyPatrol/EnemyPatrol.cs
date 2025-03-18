using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : Enemy
{
    [Header("Patrol Variables")]
    [SerializeField] private GameObject pointA;
    [SerializeField] private GameObject pointB;
    [SerializeField] private float speed;
   
    private Rigidbody2D rb;
    private Animator anim;
    private Transform currentPoint;
    private bool isFacingRight = true;
    [Header("Layer Masks")]
    [SerializeField] private LayerMask layerMask;
    [Header("Detection")]
    public bool isDetected = false; // ตัวแปรบอกว่าตรวจพบผู้เล่นหรือไม่
    public Transform playerTransform; //
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentPoint = pointB.transform;
       // anim.SetBool("isRunning",true);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDetected)
        {
            Debug.Log("กำลังไล่ตามผู้เล่นที่ตำแหน่ง: " + playerTransform.position);
            // คุณสามารถเพิ่มโค้ดสำหรับการเคลื่อนที่เข้าหาผู้เล่นได้ที่นี่
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.velocity = direction * speed; // ตัวอย่างการเคลื่อนที่เข้าหาผู้เล่น

            // อาจจะต้องมีการปรับทิศทางให้หันไปทางผู้เล่นด้วย
            if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
            {
                Flip();
            }
        }
        else
        {
            Vector2 point = currentPoint.position - transform.position;
            if (currentPoint == pointB.transform)
            {
                rb.velocity = new Vector2(speed, 0);
            }
            else
            {
                rb.velocity = new Vector2(-speed, 0);
            }
        
            if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f)
            {
                // Switch to the other point
                if (currentPoint == pointB.transform)
                {
                    currentPoint = pointA.transform;
                    Flip(); // Call flip function when changing direction
                }
                else
                {
                    currentPoint = pointB.transform;
                    Flip(); // Call flip function when changing direction
                }
            }


        }
        
    }
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }   
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (pointA != null && pointB != null) // Check if references are set to avoid errors
        {
            Gizmos.DrawWireSphere(pointA.transform.position, 0.5f);
            Gizmos.DrawWireSphere(pointB.transform.position, 0.5f);
            Gizmos.DrawLine(pointA.transform.position, pointB.transform.position);
        }
        
    }
    void OnTriggerEnter2D(Collider2D col)
    {
       
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
