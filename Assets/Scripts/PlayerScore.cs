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
    public AudioClip WinSound;
    public AudioClip finishA;

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        UpdateUI();
    }

    void Update()
    {
        if (!isLevelFinished)
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
        
        // FREEZE SONIC
        GetComponent<ImprovedPlayerController>().LockControls();

        if (bgmSource != null) bgmSource.Stop();
        if (winPanel != null) winPanel.SetActive(true);
        
        if (WinSound != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(WinSound);
        if (finishA != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(finishA);

        int timeBonus = CalculateTimeBonus(timeElapsed);
        int ringBonus = ringCount * 100; 

        winTimeBonusText.text = "" + timeBonus;
        winRingBonusText.text = "" + ringBonus;
        winTotalScoreText.text = "" + score;

        StartCoroutine(TallyScore(timeBonus, ringBonus));
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
        yield return new WaitForSeconds(1f); 

        while (tBonus > 0 || rBonus > 0)
        {
            int addAmount = 1000;

            if (tBonus > 0)
            {
                tBonus -= addAmount;
                score += addAmount;
                winTimeBonusText.text =""+ tBonus;
            }
            else if (rBonus > 0)
            {
                rBonus -= addAmount;
                score += addAmount;
                winRingBonusText.text =""+ rBonus;
            }

            winTotalScoreText.text = ""+ score;

            if (tallyScoreSound != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(tallyScoreSound);

            yield return new WaitForSeconds(0.05f);
        }
        
        yield return new WaitForSeconds(3f); // Wait a few seconds to admire the final score
        
        // EXIT THE GAME
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // This exits play mode in the Unity Editor
        #endif
    }
}