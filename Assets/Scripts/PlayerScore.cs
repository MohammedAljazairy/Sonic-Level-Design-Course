using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerScore : MonoBehaviour
{
    [Header("Stats")]
    public int ringCount = 0;
    public int score = 0;
    public float timeElapsed = 0f;
    public bool isLevelFinished = false;
    public bool isGameActive = false;
    public AudioSource bgmSource;
    
    [Header("In-Game UI")]
    public TextMeshProUGUI ringText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText; 
    public TextMeshProUGUI healthText;

    [Header("Win Screen UI")]
    public GameObject winPanel; 
    public TextMeshProUGUI winTimeBonusText;
    public TextMeshProUGUI winRingBonusText;
    public TextMeshProUGUI winTotalScoreText;

    [Header("Audio")]
    public AudioClip tallyScoreSound;
    public AudioClip tallyScoreSoundLoop;
    public AudioClip WinSound; // The victory theme song
    public AudioClip finishA;  // The sound when you first touch the signpost

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        UpdateUI();
    }

    void Update()
    {
        if (isGameActive && !isLevelFinished)
        {
            timeElapsed += Time.deltaTime;
            UpdateTimeUI();
        }
    }

    public void UpdateHealthUI(int currentHealth)
    {
        if (healthText != null) healthText.text = "" + currentHealth;
    }

    public void AddRings(int amount)
    {
        ringCount += amount;
        AddScore(amount * 100);
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
    }

    public int LoseAllRings()
    {
        int droppedRings = ringCount;
        ringCount = 0;
        UpdateUI();
        return droppedRings;
    }

    private void UpdateUI()
    {
        if (ringText != null) ringText.text = "" + ringCount;
        if (scoreText != null) scoreText.text = "" + score;
    }

    private void UpdateTimeUI()
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(timeElapsed / 60F);
            int seconds = Mathf.FloorToInt(timeElapsed % 60F);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void TriggerWin()
    {
        if (isLevelFinished) return;
        isLevelFinished = true;
        
        // Freeze Sonic instantly
        GetComponent<ImprovedPlayerController>().LockControls();

        // 1. Play the instant "hit signpost" win sound. BGM keeps playing!
        if (WinSound != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(finishA);

        // Calculate final bonuses
        int timeBonus = CalculateTimeBonus(timeElapsed);
        int ringBonus = ringCount * 100; 

        // Start the timed sequence
        StartCoroutine(WinSequenceTiming(timeBonus, ringBonus));
    }

    private IEnumerator WinSequenceTiming(int tBonus, int rBonus)
    {
        // 2. Wait 2 seconds while the level BGM is still playing
        yield return new WaitForSeconds(2f);

        // 3. Stop the BGM and play the Victory Song
        if (bgmSource != null) bgmSource.Stop();
        if (finishA != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(WinSound);

        // Show the UI panel with the static numbers
        if (winPanel != null) winPanel.SetActive(true);
        winTimeBonusText.text = "" + tBonus;
        winRingBonusText.text = "" + rBonus;
        winTotalScoreText.text = "" + score;

        // 4. Wait 3 more seconds (making it exactly 5 seconds since winning)
        yield return new WaitForSeconds(3f);

        // 5. Start the rapid tally loop
        StartCoroutine(TallyScore(tBonus, rBonus));
    }

    private int CalculateTimeBonus(float time)
    {
        if (time <= 29f) return 50000;
        if (time <= 44f) return 10000;
        if (time <= 59f) return 5000;
        if (time <= 89f) return 4000; 
        if (time <= 119f) return 3000; 
        if (time <= 179f) return 2000; 
        if (time <= 239f) return 1000; 
        if (time <= 299f) return 500;  
        return 0;
    }

    private IEnumerator TallyScore(int tBonus, int rBonus)
    {
        while (tBonus > 0 || rBonus > 0)
        {
            int addAmount = 500;

            if (tBonus > 0)
            {
                tBonus -= addAmount;
                score += addAmount;
                winTimeBonusText.text = "" + tBonus;
            }
            else if (rBonus > 0)
            {
                rBonus -= addAmount;
                score += addAmount;
                winRingBonusText.text = "" + rBonus;
            }

            winTotalScoreText.text = "" + score;
            UpdateUI(); 

            // Play the repeating tally loop sound
            if (tallyScoreSoundLoop != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(tallyScoreSoundLoop);

            yield return new WaitForSeconds(0.05f);
        }
        
        // Play one final "Ping" at the very end when numbers hit 0
        yield return new WaitForSeconds(0.2f);
        if (tallyScoreSound != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(tallyScoreSound);
        
        // Wait a few seconds to admire the final score before exiting
        yield return new WaitForSeconds(3f); 
        
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
        #endif
    }
}