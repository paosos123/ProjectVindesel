using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolDetect : MonoBehaviour
{
    private EnemyPatrol enemyPatrolScript;
    // Start is called before the first frame update
    void Start()
    {
        enemyPatrolScript = GetComponentInParent<EnemyPatrol>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            // ตรวจสอบว่า enemyPatrolScript ถูกต้องหรือไม่ก่อนใช้งาน
            if (enemyPatrolScript != null)
            {
             // ตั้งค่า isDetected ใน EnemyPatrol เป็น true
                enemyPatrolScript.playerTransform = col.transform;// เก็บตำแหน่งของผู้เล่น
                Debug.Log("ตรวจพบผู้เล่น!");
            }
        }
    }

}
