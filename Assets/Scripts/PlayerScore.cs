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
    public AudioClip WinSound; 
    public AudioClip finishA;  

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
        
        // --- EXTRA FIX: Ensure rings can never go below 0 in the UI ---
        ringCount = Mathf.Max(0, ringCount);
        
        AddScore(amount * 100);
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        score += amount;
        
        // Make sure score never dips below 0 either!
        score = Mathf.Max(0, score);
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
        
        GetComponent<ImprovedPlayerController>().LockControls();

        if (WinSound != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(WinSound);

        int timeBonus = CalculateTimeBonus(timeElapsed);
        
        // --- THE MAIN FIX: Force the ring bonus to bottom out at 0 ---
        int ringBonus = Mathf.Max(0, ringCount * 100); 

        StartCoroutine(WinSequenceTiming(timeBonus, ringBonus));
    }

    private IEnumerator WinSequenceTiming(int tBonus, int rBonus)
    {
        yield return new WaitForSeconds(2f);

        if (bgmSource != null) bgmSource.Stop();
        if (finishA != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(finishA);

        if (winPanel != null) winPanel.SetActive(true);
        winTimeBonusText.text = "" + tBonus;
        winRingBonusText.text = "" + rBonus;
        winTotalScoreText.text = "" + score;

        yield return new WaitForSeconds(3f);

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

            if (tallyScoreSoundLoop != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(tallyScoreSoundLoop);

            yield return new WaitForSeconds(0.05f);
        }
        
        yield return new WaitForSeconds(0.5f);
        if (tallyScoreSound != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(tallyScoreSound);
        
        yield return new WaitForSeconds(3f); 
        
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
        #endif
    }
}