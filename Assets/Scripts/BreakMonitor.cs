using UnityEngine;

public class BreakMonitor : MonoBehaviour
{
    public AudioClip breakS;

    void OnCollisionEnter(Collision collision)
    {
        PlayerScore ps = collision.gameObject.GetComponentInParent<PlayerScore>();

        if (ps != null)
        {
            ps.AddRings(10);
            if (breakS != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(breakS);
            Destroy(gameObject);
        }
    }
}