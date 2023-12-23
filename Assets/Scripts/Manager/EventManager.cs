using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class EventManager : MonoBehaviour
{
    public static EventManager instance = null;

    public enum Timeline
    {
        Countdown,
        Danger,
        BossAppear,
        BossDefeat
    }

    private PlayableDirector director;
    [SerializeField]
    private TimelineAsset[] timelines;

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

        director = GetComponent<PlayableDirector>();
    }

    void Update()
    {
        if (director.playableAsset == null)
            return;

        if (director.playableAsset.name == "BossAppear" || director.playableAsset.name == "BossDefeat")
        {
            SkipButtonAct();
        }
    }

    public void PlayTimeLine(Timeline index)
    {
        director.playableAsset = timelines[(int)index];
        director.Play();
    }

    public void EndTimeLine()
    {
        director.playableAsset = null;
    }

    void SkipButtonAct()
    {
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0)) // PC �̿��� ��쵵 �ֱ� ������ ������ ���� ������ �ο��Ѵ�.
        {
            UIManager.instance.ShowSkipButton(true);
        }
    }

    public void Skip()
    {
        Debug.Log("SKIP MESSAGE");
    }
}