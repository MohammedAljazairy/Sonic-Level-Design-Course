using UnityEngine;

public class BuzzProjectile : MonoBehaviour
{
    public float speed = 25f;
    public float lifetime = 4f;

    void Start()
    {
        // Automatically destroy the bullet after 4 seconds so it doesn't fly forever
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Fly straight forward
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        ImprovedPlayerController sonic = other.GetComponent<ImprovedPlayerController>();
        
        if (sonic != null)
        {
            // If Sonic is in ball form, the bullet harmlessly deflects/destroys itself!
            if (!sonic.IsInBallForm)
            {
                sonic.TakeDamage(transform.position); // Hurt Sonic
            }
        }
        
        // Destroy the bullet when it hits ANYTHING (Sonic, the floor, a wall)
        Destroy(gameObject);
    }
}