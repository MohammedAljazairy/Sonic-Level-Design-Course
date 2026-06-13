using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool isActivated = false;
    public AudioClip ckS;
    private Animator Spin;
    void Start()
    {
        Spin=GetComponent<Animator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (isActivated) return; 

        PlayerRespawn respawnScript = other.GetComponent<PlayerRespawn>();
        
        if (respawnScript != null)
        {
            Spin.SetTrigger("CheckPoint");
            Spin.Play("Spinorooony");
            respawnScript.SetCheckpoint(transform);
            if (ckS != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(ckS);
            isActivated = true;
            Debug.Log("Checkpoint Reached!");
        }
    }
}