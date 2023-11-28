using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class B_Excel : MonoBehaviour
{
    public enum Phase
    {
        Phase1,
        Phase2,
        Phase3,
    }

    private Phase phase; // 1 ~ 3��������� ������ ���� ������ ����� ���� �����Ѵ�.

    [Header("Status")]
    [SerializeField]
    private int maxHp;
    private int hp;
    [SerializeField]
    private float patternDelay; // ���� ������ ������

    [Header("Move")]
    [SerializeField]
    private float attackDisMin = 7.2f; // �������� ������ ���� �ּҰ�
    [SerializeField]
    private float attackDisMax = 8.8f; // �������� ������ ���� �ִ밪
    [SerializeField]
    private float[] attackPosY; // ������ ������ ������Y
    [SerializeField]
    private float moveSpeed;

    [Header("Attack")]
    [SerializeField]
    private Transform emitter;
    [SerializeField]
    private float attackDelay; // ������ ������. � �����̵� ���� �ð��� �ߵ��Ѵ�.
    [SerializeField]
    private float generalShotSpeed;
    [SerializeField]
    private float impactShotSpeed;
    [SerializeField]
    private float shotDelay; // �⺻ ������ ������
    [SerializeField]
    private int comboShotCount;
    [SerializeField]
    private float comboShotDelay = 0.3f; // Ʈ���ü� ���� ���� ������

    // yield return time
    private WaitForSeconds attackWait;
    private WaitForSeconds patternWait;
    private WaitForSeconds shotWait;
    private WaitForSeconds comboShotWait;

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
        phase = Phase.Phase2;
        hp = maxHp;
        player = GameObject.Find("Player");

        attackWait = new WaitForSeconds(attackDelay);
        patternWait = new WaitForSeconds(patternDelay);
        shotWait = new WaitForSeconds(shotDelay);
        comboShotWait = new WaitForSeconds(comboShotDelay);
    }

    IEnumerator PatternCycle()
    {
        yield return patternWait;

        if (rigid.position.x - player.transform.position.x > attackDisMax) // �÷��̾�� �ָ� ���� �̵�
        {
            StartCoroutine("Move", Vector2.left);
        }
        else if (rigid.position.x - player.transform.position.x < attackDisMin) // �÷��̾�� ������ ������ �̵�
        {
            StartCoroutine("Move", Vector2.right);
        }
        else // ���� ��ġ�� ��� ���� ���
        {
            int pattern = PatternChoice();

            switch (pattern) // 0 : GeneralShot / 1 : ImpactShot / 2 : TripleShot
            {
                case 0:
                    StartCoroutine("GeneralShot");
                    break;
                case 1:
                    StartCoroutine("ImpactShot");
                    break;
                case 2:
                    StartCoroutine("ComboShot");
                    break;
            }
        }
    }

    int PatternChoice()
    {
        int patternMin = 0;
        int patternMax = 0;

        switch (phase)
        {
            case Phase.Phase1:
                patternMin = 0;
                patternMax = 1;
                break;
            case Phase.Phase2:
                patternMin = 1;
                patternMax = 3;
                break;
            case Phase.Phase3:
                break;
        }

        int patternIndex = Random.Range(patternMin, patternMax);
        return patternIndex;
    }

    IEnumerator Move(Vector2 dir)
    {
        while (true)
        {
            rigid.velocity = dir * moveSpeed;
            float distance = rigid.position.x - player.transform.position.x;

            if (distance <= attackDisMax && distance >= attackDisMin)
            {
                break;
            }

            yield return null;
        }

        rigid.velocity = Vector2.zero;
        StartCoroutine("PatternCycle");
    }

    IEnumerator GeneralShot()
    {
        int shotCount = Random.Range(1, 3); // �ִ� 2�߱��� ���.

        for (int i = 0; i < shotCount; i++)
        {
            yield return shotWait;
            GameObject spawnBullet = PoolManager.instance.Get(PoolManager.PoolType.Bullet, 2);
            spawnBullet.transform.position = emitter.position;
            spawnBullet.GetComponent<Bullet>().Shoot(Vector2.left, generalShotSpeed);
            AudioManager.instance.PlayEnemySFX(AudioManager.EnemySfx.ExcelGeneralShot);
        }

        yield return attackWait;
        StartCoroutine("PatternCycle");
    }

    IEnumerator ImpactShot()
    {
        float movePosX = player.transform.position.x + attackDisMax; // ����Ʈ���� �ִ� ��Ÿ��� �̵��� ���.
        
        while (rigid.position.x <= movePosX)
        {
            rigid.velocity = Vector2.right * moveSpeed;
            yield return null;
        }

        rigid.velocity = Vector2.zero;

        yield return shotWait;
        GameObject spawnBullet = PoolManager.instance.Get(PoolManager.PoolType.Bullet, 3);
        spawnBullet.transform.position = emitter.position;
        spawnBullet.GetComponent<Bullet>().Shoot(Vector2.left, impactShotSpeed);
        AudioManager.instance.PlayEnemySFX(AudioManager.EnemySfx.ExcelImpactShot);

        yield return attackWait;
        StartCoroutine("PatternCycle");
    }

    IEnumerator ComboShot()
    {
        int randomNum = -1;

        for (int i = 0; i < comboShotCount; i++) // �� 8�� ���
        {
            if (i == 0)
            {
                randomNum = 0; // ù ���� ���ڸ����� ���
            }
            else
            {
                randomNum = ComboShotPos(randomNum);
            }
            
            float posY = attackPosY[randomNum];
            
            if (rigid.position.y > posY) // �Ʒ��� �̵�
            {
                while (true)
                {
                    rigid.velocity = Vector2.down * moveSpeed;

                    if (rigid.position.y <= posY)
                    {
                        break;
                    }

                    yield return null;
                }
            }
            else if (rigid.position.y < posY) // ���� �̵�
            {
                while (true)
                {
                    rigid.velocity = Vector2.up * moveSpeed;

                    if (rigid.position.y >= posY)
                    {
                        break;
                    }

                    yield return null;
                }
            }

            rigid.velocity = Vector2.zero;
            GameObject spawnBullet = PoolManager.instance.Get(PoolManager.PoolType.Bullet, 2);
            spawnBullet.transform.position = emitter.position;
            spawnBullet.GetComponent<Bullet>().Shoot(Vector2.left, generalShotSpeed);
            AudioManager.instance.PlayEnemySFX(AudioManager.EnemySfx.ExcelGeneralShot);
            yield return comboShotWait;
        }

        if (rigid.position.y > attackPosY[0]) // ������ ���ڸ��� �̵�
        {
            while (true)
            {
                rigid.velocity = Vector2.down * moveSpeed;

                if (rigid.position.y <= attackPosY[0])
                {
                    break;
                }

                yield return null;
            }
        }

        rigid.velocity = Vector2.zero;
        yield return attackWait;
        StartCoroutine("PatternCycle");
    }

    int ComboShotPos(int num)
    {
        int randomNum = 0;

        while (true)
        {
            randomNum = Random.Range(0, attackPosY.Length);

            if (randomNum == num)
                continue;
            else
                break;
        }

        return randomNum;
    }
}