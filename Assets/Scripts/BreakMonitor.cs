using UnityEngine;

public class BreakMonitor : MonoBehaviour
{
    public AudioClip breakS;
public AudioClip RingS;
    void OnCollisionEnter(Collision collision)
    {
        PlayerScore ps = collision.gameObject.GetComponentInParent<PlayerScore>();
        ImprovedPlayerController sonic = collision.gameObject.GetComponent<ImprovedPlayerController>();
        if (ps != null&sonic.IsInBallForm)
        {
            ps.AddRings(10);
            if (breakS != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(breakS);
             if (breakS != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(RingS);
            Destroy(gameObject);
        }
    }
}