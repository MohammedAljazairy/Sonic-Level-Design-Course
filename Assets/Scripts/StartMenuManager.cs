using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class StartMenuManager : MonoBehaviour
{
    [Header("Cameras & Player")]
    public ImprovedPlayerController sonic;
    public GameObject introCamera;    
    public GameObject gameplayCamera; 

    [Header("UI & Media")]
    public VideoPlayer introVideo;
    public GameObject titleCanvas;      
    public GameObject gameplayUI;       
    public GameObject loadingBlackScreen; // Hides the flash
    public float videoDuration = 4f;  
    public float cameraBlendTime = 2f;    // How long the camera takes to fly to Sonic

    [Header("Audio")]
    public AudioSource titleMusic;
    public AudioSource levelBGM;

    private bool canPressEnter = false;
    private bool gameStarted = false;

    void Start()
    {
        if (sonic != null)
        {
            sonic.ResetMomentum();
            sonic.LockControls();
            Animator anim = sonic.GetComponentInChildren<Animator>();
            if (anim != null) 
            {
                anim.SetFloat("CurrentSpeed", 0f);
                anim.SetBool("isRunning", false);
                anim.Play("Idle"); 
            }
        }

        if (introCamera != null) introCamera.SetActive(true);
        if (gameplayCamera != null) gameplayCamera.SetActive(false);
        
        if (titleCanvas != null) titleCanvas.SetActive(false); 
        if (gameplayUI != null) gameplayUI.SetActive(false);
        
        if (loadingBlackScreen != null) loadingBlackScreen.SetActive(true);
        if (levelBGM != null) levelBGM.Stop();
        
        if (titleMusic != null) 
        {
            titleMusic.loop = false; 
            StartCoroutine(TitleMusicLoop());
        }
        
        // --- THE WEBGL VIDEO FIX ---
        if (introVideo != null) 
        {
            // 1. Tell it we are using a URL, not a clip
            introVideo.source = UnityEngine.Video.VideoSource.Url;
            
            // 2. Build the correct path with a forward slash for WebGL!
            string videoPath = Application.streamingAssetsPath + "/SegaIntro.mp4";
            introVideo.url = videoPath;
            
            // 3. Play it!
            introVideo.Play();
        }

        // CRITICAL: Start the timer so the game actually lets you press Enter!
        StartCoroutine(VideoTimerRoutine());
    }

    private IEnumerator TitleMusicLoop()
    {
        yield return new WaitForSeconds(5f); // Initial 5 second delay

        while (!gameStarted) // Keep doing this until you press Enter
        {
            titleMusic.Play();
            
            // Wait for the song to finish, plus 2 extra seconds of silence
            yield return new WaitForSeconds(titleMusic.clip.length + 2f);
        }
    }

    private IEnumerator VideoTimerRoutine()
    {
        // Give the video a split second to load before hiding the black screen
        yield return new WaitForSeconds(0.5f);
        if (loadingBlackScreen != null) loadingBlackScreen.SetActive(false);

        // Wait the rest of the video duration
        yield return new WaitForSeconds(videoDuration - 0.5f);

        if (introVideo != null) 
        {
            introVideo.Stop();
            introVideo.gameObject.SetActive(false);
        }

        if (titleCanvas != null) titleCanvas.SetActive(true);
        
        canPressEnter = true;
    }

    void Update()
    {
        if (canPressEnter && !gameStarted && Input.GetKeyDown(KeyCode.Return))
        {
            gameStarted = true;
            canPressEnter = false;
            
            // Start the sequence!
            StartCoroutine(StartGameSequence());
        }
    }

    private IEnumerator StartGameSequence()
    {
        // 1. Hide Title UI immediately
        if (titleCanvas != null) titleCanvas.SetActive(false);
        
        // 2. Start the Camera Fly-over
        if (introCamera != null) introCamera.SetActive(false);
        if (gameplayCamera != null) gameplayCamera.SetActive(true);

        // Swap Music
        if (titleMusic != null) titleMusic.Stop();
        if (levelBGM != null) levelBGM.Play();

        // 3. WAIT for the camera to finish flying before giving control!
        yield return new WaitForSeconds(cameraBlendTime);

        // 4. NOW show the gameplay UI, start the timer, and unlock Sonic!
        if (gameplayUI != null) gameplayUI.SetActive(true);

        if (sonic != null) 
        {
            PlayerScore ps = sonic.GetComponent<PlayerScore>();
            if (ps != null) ps.isGameActive = true; 
            
            sonic.canControl = true; 
            Animator anim = sonic.GetComponentInChildren<Animator>();
            if (anim != null) anim.speed = 1f;
        }
    }
}