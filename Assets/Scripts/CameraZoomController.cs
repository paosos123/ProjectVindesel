using UnityEngine;
 using Cinemachine;
 using System.Collections;

 public class CameraZoomController : MonoBehaviour
 {
     public CinemachineVirtualCamera vcam;
     public float targetOrthographicSize = 4f;
     public float zoomSpeed = 5f;
 

     private float originalOrthographicSize;
     private Vector3 originalPosition; // เปลี่ยนชื่อเพื่อให้สื่อถึง World Position

     void Start()
     {
         if (vcam == null)
         {
             Debug.LogError("Cinemachine Virtual Camera is not assigned!");
             enabled = false;
             return;
         }

         originalOrthographicSize = vcam.m_Lens.OrthographicSize;
         originalPosition = vcam.transform.position; // เก็บตำแหน่งเริ่มต้นใน World Space
     }

     public void ZoomInAndOut(float duration)
     {
         StartCoroutine(ZoomSequence(duration));
     }

     IEnumerator ZoomSequence(float duration)
     {
         Debug.Log("ZoomSequence started with duration: " + duration);
         float startTime = Time.time;

         // ซูมเข้า
         while (Time.time < startTime + duration)
         {
             float t = (Time.time - startTime) / duration;
             vcam.m_Lens.OrthographicSize = Mathf.Lerp(originalOrthographicSize, targetOrthographicSize, t);
        
             yield return null;
         }
         vcam.m_Lens.OrthographicSize = targetOrthographicSize;
         ResetShake();

         // ซูมกลับ
         startTime = Time.time;
         while (Time.time < startTime + duration)
         {
             float t = (Time.time - startTime) / duration;
             vcam.m_Lens.OrthographicSize = Mathf.Lerp(targetOrthographicSize, originalOrthographicSize, t);
          
             yield return null;
         }
         vcam.m_Lens.OrthographicSize = originalOrthographicSize;
         ResetShake();
     }

     

     void ResetShake()
     {
         vcam.transform.position = originalPosition; // รีเซ็ตตำแหน่ง World Space
     }
 }