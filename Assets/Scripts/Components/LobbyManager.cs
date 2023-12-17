using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private GameObject MainButtons;
    [SerializeField]
    private GameObject StageSelectButtons;
    [SerializeField]
    private GameObject bossStageButton;

    public void GameStart()
    {
        MainButtons.SetActive(false);
        StageSelectButtons.SetActive(true);
        int gameProgress = PlayerPrefs.GetInt("GameProgress"); // 1 : RunStage Ŭ����, 2 : BossStage Ŭ����

        if (gameProgress >= 1)
        {
            bossStageButton.SetActive(true);
        }
    }

    public void StageSelect(int stageIndex)
    {
        SceneManager.LoadScene(stageIndex);
    }

    public void GameExit()
    {
        Application.Quit();
    }

    public void Return()
    {
        MainButtons.SetActive(true);
        StageSelectButtons.SetActive(false);
    }
}