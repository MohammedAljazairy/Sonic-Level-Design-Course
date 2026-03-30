using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [Header("Bounce Settings")]
    public float bounceForce = 25f; // How high Sonic gets launched
    public AudioClip bounceSound;   // The classic spring sound effect

    private void OnCollisionEnter(Collision collision)
    {
        // 1. Check if the object that landed on us is Sonic
        ImprovedPlayerController sonic = collision.gameObject.GetComponentInParent<ImprovedPlayerController>();

        if (sonic != null)
        {
            // 2. Get Sonic's physical body
            Rigidbody sonicRb = sonic.GetComponent<Rigidbody>();

            if (sonicRb != null)
            {
                // 3. Zero out his current falling speed. 
                // This ensures he bounces the exact same height whether he dropped from 1 foot or 100 feet!
                sonicRb.linearVelocity = new Vector3(sonicRb.linearVelocity.x, 0f, sonicRb.linearVelocity.z);

                // 4. Launch him! (Using transform.up means it pushes him whichever way the spring is pointing)
                sonicRb.AddForce(transform.up * bounceForce, ForceMode.Impulse);

                // 5. Play the bounce sound loudly and cleanly
                if (bounceSound != null)
                {
                    Camera.main.GetComponent<AudioSource>().PlayOneShot(bounceSound);
                }
            }
        }
    }
}