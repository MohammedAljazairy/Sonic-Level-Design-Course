using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Health System")]
    public int health = 3;
    public GameObject gameOverPanel; 

    [Header("Respawn Settings")]
    public Transform currentRespawnPoint; 
    public float fallDeathThreshold = -20f; 
    public AudioClip Deathsound;
    public AudioClip GameoverSound;
    private ImprovedPlayerController movementController;
    private bool isDead = false;
    public AudioSource bgmSource;
    
    private Vector3 defaultStartPos; 
    
    // NEW: Prevents multiple spikes from killing you instantly
    private float lastDamageTime = -10f; 
    
    void Start()
    {
        movementController = GetComponent<ImprovedPlayerController>();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        GetComponent<PlayerScore>().UpdateHealthUI(health);
        
        defaultStartPos = transform.position; 
    }

    void Update()
    {
        if (isDead) return;

        if (transform.position.y < fallDeathThreshold)
        {
            TakeLethalDamage();
        }
    }

    public void SetCheckpoint(Transform newCheckpoint)
    {
        currentRespawnPoint = newCheckpoint;
    }

    public void TakeLethalDamage()
    {
        if (isDead) return;

        // --- THE FIX: Wait at least 1 second between health drops! ---
        if (Time.time - lastDamageTime < 1.0f) return;
        lastDamageTime = Time.time;

        health--;
        GetComponent<PlayerScore>().UpdateHealthUI(health);
        
        if (health <= 0) 
        {
            GameOver();
        }
        else
        {
            if (Deathsound != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(Deathsound);
            Respawn();
        }
    }

    public void Respawn()
    {
        if (currentRespawnPoint != null)
        {
            transform.position = currentRespawnPoint.position;
        }
        else
        {
            transform.position = defaultStartPos;
        }
        
        movementController.ResetMomentum();
    }

    private void GameOver()
    {
        if (bgmSource != null) bgmSource.Stop();
        if (GameoverSound != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(GameoverSound);
        isDead = true;
        
        movementController.LockControls(); 
        
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        
        StartCoroutine(RestartLevelRoutine());
    }

    private IEnumerator RestartLevelRoutine()
    {
        yield return new WaitForSeconds(12f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }
}