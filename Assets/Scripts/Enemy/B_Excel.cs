using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class B_Excel : MonoBehaviour
{
    public enum Phase
    {
        Phase1,
        Phase2,
        Phase3,
        Phase4,
    }

    private Phase phase; // 1 ~ 4��������� ������ ���� ������ ����� ���� �����Ѵ�.

    [Header("Status")]
    [SerializeField]
    private int maxHp;
    private int hp;

    [Header("Action")]
    [SerializeField]
    private float attackDis = 8.8f; // �������� �÷��̾���� ������ ����
    [SerializeField]
    private float attackPosY = -0.05f; // ������ ������ ������Y
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float maxMoveTime = 2f; // �ִ� �̵��ð�. �̸� �ʰ��ϸ� �̵� ����

    [Header("Attack")]
    [SerializeField]
    private Transform emitter;
    [SerializeField]
    private float generalShotSpeed;
    [SerializeField]
    private float patternDelay; // ���� ������ ������
    [SerializeField]
    private float generalShotDelay; // �⺻ ������ ������

    // yield return time
    private WaitForSeconds patternWait;
    private WaitForSeconds generalShotWait;

    [Header("Component")]
    private Animator animator;
    private Rigidbody2D rigid;
    private BoxCollider2D collider;
    private GameObject player;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();

        Init();
    }

    void Start()
    {
        StartCoroutine("PatternCycle");
    }

    void Init()
    {
        phase = Phase.Phase1;
        hp = maxHp;
        player = GameObject.Find("Player");

        patternWait = new WaitForSeconds(patternDelay);
        generalShotWait = new WaitForSeconds(generalShotDelay);
    }

    IEnumerator PatternCycle()
    {
        yield return StartCoroutine("Move");
        yield return patternWait;
        int pattern = PatternChoice();
        
        switch (pattern)
        {
            case 0:
                StartCoroutine("GeneralShot");
                break;
        }
    }

    int PatternChoice()
    {
        int patternIndex = 0;

        switch (phase)
        {
            case Phase.Phase1:
                patternIndex = 0;
                break;
            case Phase.Phase2:
                break;
            case Phase.Phase3:
                break;
            case Phase.Phase4:
                break;
        }

        return patternIndex;
    }

    IEnumerator Move()
    {
        float destX = player.transform.position.x + attackDis;

        if (transform.position.x > destX) // �������� �̵�
        {
            while (transform.position.x > destX)
            {
                rigid.velocity = Vector2.left * moveSpeed;
                yield return null;
            }
        }
        else if (transform.position.x < destX)// ���������� �̵�
        {
            while (transform.position.x < destX)
            {
                rigid.velocity = Vector2.right * moveSpeed;
                yield return null;
            }
        }

        rigid.velocity = Vector2.zero;
    }

    IEnumerator GeneralShot()
    {
        int shotCount = Random.Range(1, 3); // �ִ� 2�߱��� ���.

        for (int i = 0; i < shotCount; i++)
        {
            yield return generalShotWait;
            GameObject spawnBullet = PoolManager.instance.Get(PoolManager.PoolType.Bullet, 2);
            spawnBullet.gameObject.SetActive(true);
            spawnBullet.transform.position = emitter.position;
            spawnBullet.GetComponent<Bullet>().Shoot(Vector2.left, generalShotSpeed);
            AudioManager.instance.PlayEnemySFX(AudioManager.EnemySfx.ExcelShot);
        }

        StartCoroutine("PatternCycle");
    }
}