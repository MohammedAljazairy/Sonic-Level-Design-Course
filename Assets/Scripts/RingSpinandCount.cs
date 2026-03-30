using UnityEngine;

public class RingSpinandCount : MonoBehaviour
{
    public float rotationSpeed = 150f;
    public AudioClip ringCA;
    void Update()
    {

        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0, Space.World);
    }

private void OnTriggerEnter(Collider other)
    {

        PlayerScore sonicScore = other.GetComponentInParent<PlayerScore>();
        
        if (sonicScore != null)
        {
            sonicScore.AddRings(1);
            AudioSource.PlayClipAtPoint(ringCA,transform.position);
            Destroy(gameObject);
        }
    }
}