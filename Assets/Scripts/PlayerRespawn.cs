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
    
    void Start()
    {
        movementController = GetComponent<ImprovedPlayerController>();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        GetComponent<PlayerScore>().UpdateHealthUI(health);
        
        // Save his exact starting position the moment the game loads!
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
        // If we hit a checkpoint, go there
        if (currentRespawnPoint != null)
        {
            transform.position = currentRespawnPoint.position;
        }
        // If we HAVEN'T hit a checkpoint yet, go back to the very beginning!
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