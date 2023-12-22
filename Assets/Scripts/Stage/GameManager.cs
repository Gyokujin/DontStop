using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    [HideInInspector]
    public bool isLive = false;

    [Header("StageInfo")]
    public int maxScore;
    [HideInInspector]
    public int score;
    [SerializeField]
    private float scoreDelay = 0.25f;
    private bool scoreGetting = false;
    [HideInInspector]
    public bool isGameOver = false;
    private bool isArrive = false;

    [Header("Component")]
    [HideInInspector]
    public PlayerController player;
    [SerializeField]
    private PlatformControl platform;
    [SerializeField]
    private targetCamera camera;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        Application.targetFrameRate = 60;
    }

    void Start()
    {
        Init();
    }
    
    void Update()
    {
        if (isLive && platform.platformType == PlatformControl.PlatformType.Random)
        {
            ScoreProcess();
        }
    }

    void Init()
    {
        GameLive(false);
        player.Move(false);

        if (platform.platformType == PlatformControl.PlatformType.Random)
        {
            EventManager.instance.PlayTimeLine(EventManager.Timeline.Countdown);
        }
        else if (platform.platformType == PlatformControl.PlatformType.Line)
        {
            StartCoroutine("BossInitProcess");
        }
    }

    IEnumerator BossInitProcess()
    {
        yield return StartCoroutine(UIManager.instance.FadeIn());
        EventManager.instance.PlayTimeLine(EventManager.Timeline.BossAppear);
    }

    public void RunStageStart()
    {
        Time.timeScale = 1;
        GameLive(true);
        isLive = true;
        player.Move(true);
        AudioManager.instance.BgmPlay(AudioManager.StageBGM.RunStage);
    }

    void ScoreProcess()
    {
        if (!scoreGetting && !isArrive)
        {
            StartCoroutine("ProgressScore");
        }
    }

    public void AddScore(int newScore)
    {
        if (!isGameOver)
        {
            score += newScore;
            UIManager.instance.ProgressModify(score);
        }

        if (score >= maxScore && !isArrive)
        {
            ArriveGoal();
        }
    }

    IEnumerator ProgressScore()
    {
        scoreGetting = true;
        AddScore(1);

        yield return new WaitForSeconds(scoreDelay);
        scoreGetting = false;
    }

    public void GameLive(bool live)
    {
        isLive = live;

        if (platform.platformType == PlatformControl.PlatformType.Random)
        {
            UIManager.instance.ProgressCha(live);
        }
    }

    void ArriveGoal()
    {
        isArrive = true;
        GameLive(false);
        player.Move(false);
        UIManager.instance.ShowController(false);
        AudioManager.instance.MuteBgm();
        EventManager.instance.PlayTimeLine(EventManager.Timeline.Danger);
    }

    public void EnterBossStage()
    {
        StartCoroutine("EnterBossStageProcess");
    }

    IEnumerator EnterBossStageProcess()
    {
        yield return StartCoroutine(UIManager.instance.FadeOut());
        SceneManager.LoadScene(2); // ���� ������ �̵�
    }

    public void CameraPause()
    {
        camera.StopCamera();
    }

    public void GamePause()
    {
        Time.timeScale = 0;
        AudioManager.instance.PlaySystemSFX(AudioManager.SystemSFX.Click);
        AudioManager.instance.BgmVolumeControl(0.1f);
        UIManager.instance.ShowPausePanel(true);
    }

    public void GameResume()
    {
        Time.timeScale = 1;
        AudioManager.instance.PlaySystemSFX(AudioManager.SystemSFX.Click);
        AudioManager.instance.BgmVolumeControl(0.4f);
        UIManager.instance.ShowPausePanel(false);
    }

    public void GameOver()
    {
        isGameOver = true;
        CameraPause();
        Invoke("GameOverProcess", 2f);
    }

    void GameOverProcess()
    {
        Time.timeScale = 0;
        UIManager.instance.ShowGameOverPanel(true);
        UIManager.instance.restartButton.interactable = true;
        AudioManager.instance.PlaySystemSFX(AudioManager.SystemSFX.GameOver);
    }

    public void GameRestart()
    {
        if (isGameOver)
        {
            Time.timeScale = 1;
            UIManager.instance.restartButton.interactable = false;
            AudioManager.instance.PlaySystemSFX(AudioManager.SystemSFX.Click);
            Invoke("GameRestartProcess", 1f);
        }
    }

    void GameRestartProcess()
    {
        SceneManager.LoadScene(1);
        AudioManager.instance.MuteBgm();
        EventManager.instance.PlayTimeLine(EventManager.Timeline.Countdown);
    }

    public void GameQuit()
    {
        Time.timeScale = 1;
        AudioManager.instance.PlaySystemSFX(AudioManager.SystemSFX.Click);
        Invoke("GameQuitProcess", 0.5f);
    }

    void GameQuitProcess()
    {
        SceneManager.LoadScene(0);
    }
}