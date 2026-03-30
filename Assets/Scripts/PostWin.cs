using UnityEngine;

public class PostWin : MonoBehaviour
{
    private Animator animator;
    public AudioClip posthit;
    
    // Use a boolean instead of an int. It's the standard way to do "only once" checks!
    private bool hasTriggered = false; 

    void Start()
    {
        // We only get the Animator here, because the Animator IS attached to the Signpost.
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. If we already won, stop right here.
        if (hasTriggered) return;

        // 2. Ask the thing that hit the signpost: "Do you have the PlayerScore script?"
        PlayerScore ps = other.GetComponentInParent<PlayerScore>();

        // 3. If ps is NOT null, it means Sonic touched us! (Not a ring or an enemy)
        if (ps != null)
        {
            hasTriggered = true; // Lock it so it can't be triggered again

            // Play the animations
            if (animator != null)
            {
                animator.SetTrigger("hasWon");
                animator.Play("PostSpin"); 
            }

            // Play the sound loudly without fading!
            if (posthit != null)
            {
                Camera.main.GetComponent<AudioSource>().PlayOneShot(posthit);
            }

            // Tell Sonic's script to run the Win math
            ps.TriggerWin();
        }
    }
}