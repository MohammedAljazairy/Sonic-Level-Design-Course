using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool isActivated = false;
    public AudioClip ckS;
    private void OnTriggerEnter(Collider other)
    {
        if (isActivated) return; // Only activate once!

        PlayerRespawn respawnScript = other.GetComponent<PlayerRespawn>();
        
        if (respawnScript != null)
        {
            // Tell Sonic his new spawn point is right here
            respawnScript.SetCheckpoint(transform);
            AudioSource.PlayClipAtPoint(ckS,transform.position);
            isActivated = true;
            
            Debug.Log("Checkpoint Reached!");
            // (You can play a sound effect or a particle system here later!)
        }
    }
}