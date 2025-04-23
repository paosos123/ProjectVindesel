using UnityEngine;

public class BreakableWall : MonoBehaviour
{
  
     private void OnCollisionEnter2D(Collision2D collision)
     {
        Movement2D playerMovement = collision.gameObject.GetComponent<Movement2D>();
      if (playerMovement != null && playerMovement.IsLongDashing)
        {
           Destroy(gameObject);
       }
    }
     private void OnCollisionStay2D(Collision2D collision)
     {
         Movement2D playerMovement = collision.gameObject.GetComponent<Movement2D>();
         if (playerMovement != null && playerMovement.IsLongDashing)
         {
             Destroy(gameObject);
         }
     }
}