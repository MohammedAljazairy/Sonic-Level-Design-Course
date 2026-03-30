using UnityEngine;

public class BreakMonitor : MonoBehaviour
{
    public AudioClip breakS;

    // We don't need Start() or Update() anymore!

    void OnCollisionEnter(Collision collision)
    {
        // 1. Check if the thing that bumped into the monitor is Sonic
        PlayerScore ps = collision.gameObject.GetComponentInParent<PlayerScore>();

        // 2. If 'ps' is NOT null, it means Sonic hit us! (Not a random enemy or the floor)
        if (ps != null)
        {
            // Give him the 10 rings
            ps.AddRings(10);

            // Play the break sound loudly on the camera (No 3D fading!)
            if (breakS != null)
            {
                Camera.main.GetComponent<AudioSource>().PlayOneShot(breakS);
            }

            // Destroy the monitor
            Destroy(gameObject);
        }
    }
}