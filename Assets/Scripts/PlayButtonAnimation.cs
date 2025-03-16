using UnityEngine;

public class PlayNowButtonScript : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();  // Get the Animator component
    }

    public void PlayAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("ClickTrigger"); // Trigger the animation
        }
        else
        {
            Debug.LogError("Animator not found on Play Now button!");
        }
    }
}
