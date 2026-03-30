using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpilledRing : MonoBehaviour
{
    public float lifetime = 10f;       // Disappears after 10 seconds
    public float collectDelay = 1.5f;  // Can't be collected for 1.5 seconds
    public float rotationSpeed = 300f; // Spin really fast!
    public AudioClip ringpick;
    private float age = 0f;

    void Start()
    {
        // Automatically destroy this ring after 10 seconds
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        age += Time.deltaTime;
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }

    // Because this uses a Rigidbody, we use OnCollisionEnter instead of Trigger
    private void OnCollisionEnter(Collision collision)
    {
        // Don't allow collection if the ring just spawned
        if (age < collectDelay) return;

        PlayerScore sonicScore = collision.gameObject.GetComponent<PlayerScore>();
        
        if (sonicScore != null)
        {
            sonicScore.AddRings(1);
            Camera.main.GetComponent<AudioSource>().PlayOneShot(ringpick);
            Destroy(gameObject);
        }
    }
}