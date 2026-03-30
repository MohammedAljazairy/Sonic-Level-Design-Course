using UnityEngine;

public class PostWin : MonoBehaviour
{
    private Animator animator;
    public AudioClip posthit;
    
    private bool hasTriggered = false; 

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        PlayerScore ps = other.GetComponentInParent<PlayerScore>();

        if (ps != null)
        {
            hasTriggered = true; 

            if (animator != null)
            {
                animator.SetTrigger("hasWon");
                animator.Play("PostSpin"); 
            }

            if (posthit != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(posthit);

            ps.TriggerWin();
        }
    }
}