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
                GameObject currentGhost = Instantiate(ghostPrefab,transform.position,transform.rotation);
                Sprite currentSprite = GetComponent<SpriteRenderer>().sprite;
                currentGhost.GetComponent<SpriteRenderer>().sprite = currentSprite;
                ghostTimer = ghostDelay;
                Destroy(currentGhost,0.5f);
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
