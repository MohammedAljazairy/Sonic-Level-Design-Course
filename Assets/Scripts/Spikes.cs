using UnityEngine;

public class Spikes : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object that touched the spikes is Sonic
        ImprovedPlayerController sonic = collision.gameObject.GetComponent<ImprovedPlayerController>();

        if (sonic != null)
        {
            // Hurt Sonic, and tell him exactly where the spikes are so he bounces away from them!
            sonic.TakeDamage(transform.position); 
        }
    }
}