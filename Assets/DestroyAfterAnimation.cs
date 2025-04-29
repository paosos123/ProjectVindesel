using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    private Animator animator;
    private float animationLength;

    void Start()
    {
        // Get the Animator component attached to this GameObject
        animator = GetComponent<Animator>();

        // Check if an Animator component exists
        if (animator == null)
        {
            Debug.LogError("Animator component not found on this GameObject!");
            enabled = false; // Disable this script if no Animator is present
            return;
        }

        // Get the AnimatorClipInfo for the base layer (layer 0)
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);

        // Check if any clip is playing on the base layer
        if (clipInfo.Length > 0)
        {
            // Get the length of the first playing clip
            animationLength = clipInfo[0].clip.length;

            // Start a coroutine to wait for the animation to finish and then destroy the GameObject
            StartCoroutine(DestroyAfterDelay(animationLength));
        }
        else
        {
            Debug.LogWarning("No animation clip is currently playing on the base layer of the Animator!");
            enabled = false; // Disable if no animation is playing
            return;
        }
    }

    private System.Collections.IEnumerator DestroyAfterDelay(float delay)
    {
        // Wait for the duration of the animation clip
        yield return new WaitForSeconds(delay);

        // Destroy this GameObject
        Destroy(gameObject);
        Debug.Log(gameObject.name + " destroyed after animation finished.");
    }
}