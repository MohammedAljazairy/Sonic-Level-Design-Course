using UnityEngine;
using UnityEngine.SceneManagement; // Required to restart the level
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
    void Start()
    {
        movementController = GetComponent<ImprovedPlayerController>();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        GetComponent<PlayerScore>().UpdateHealthUI(health);
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
        if (health < 0) 
        {
            GameOver();
        }
        else
        {
            AudioSource.PlayClipAtPoint(Deathsound,transform.position);
            Respawn();
        }
    }

    public void Respawn()
    {
        if (currentRespawnPoint != null)
        {
            
            transform.position = currentRespawnPoint.position;
            movementController.ResetMomentum();
        }
    }

    private void GameOver()
    {
        if (bgmSource != null) bgmSource.Stop();
        AudioSource.PlayClipAtPoint(GameoverSound,transform.position);
        isDead = true;
        movementController.ResetMomentum(); 
        
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        
        StartCoroutine(RestartLevelRoutine());
    }

    private IEnumerator RestartLevelRoutine()
    {
        yield return new WaitForSeconds(13f);
    
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }
}