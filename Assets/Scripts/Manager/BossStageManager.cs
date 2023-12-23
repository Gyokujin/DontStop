using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameManager))]
public class BossStageManager : MonoBehaviour
{
    [SerializeField]
    private B_Excel excel;
    [SerializeField]
    private Vector2 bossPos;

    public void BossStageStart()
    {
        // System
        GameManager.instance.isLive = true;
        UIManager.instance.ShowController(true);
        AudioManager.instance.BgmPlay(AudioManager.StageBGM.BossStage);

        // Player
        GameManager.instance.player.Move(true);

        // Excel
        excel.transform.position = bossPos;
        excel.enabled = true;
        excel.GetComponent<BoxCollider2D>().enabled = true;
    }

    //public void BossDefeat()
    //{
    //    GameLive(false);
    //    player.gameObject.layer = 7; // �� �κ� ������ Player�� ���� ���� �Լ��� ����
    //    player.Move(false);
    //    UIManager.instance.ShowController(false);
    //    StartCoroutine("BossDefeatProcess");
    //}

    //IEnumerator BossDefeatProcess()
    //{
    //    yield return StartCoroutine(UIManager.instance.FadeOut());
    //    yield return new WaitForSeconds(1f);
    //    yield return StartCoroutine(UIManager.instance.FadeIn());
    //    yield return null;

    //    yield return StartCoroutine(EventManager.instance.BossDefeat());
    //    yield return StartCoroutine(UIManager.instance.FadeOut());
    //    yield return StartCoroutine(UIManager.instance.GameFinishMessage());

    //    SceneManager.LoadScene(0);
    //}
}