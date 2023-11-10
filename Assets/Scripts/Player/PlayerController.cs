using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Status
    private float landDis;
    private float jumpTime;
    private float jumpCool;
    private float jumpFoece;
    private float slideTime;
    private float slideCool;
    private float attackCool;
    private float putGunCool;
    private int maxLife;
    private float knockback;
    private float bounce;
    private float invincibleTime;
    private float respawnTime;

    // Action
    private int jumpCount = 2;
    private bool onJumping = false;
    private bool jumpAble = true;
    private Transform[] landVec = new Transform[2];
    private bool onGround = false;
    private bool onFall = false;
    private bool onSlide = false;
    [SerializeField]
    private int defaultLayer; // �÷��̾� ���̾�
    [SerializeField]
    private int invincibleLayer; // �������� ���̾�
    private float leftInvincible = 0; // ���� ���� ���� �ð��� ���
    [SerializeField]
    private float respawnPosY = 7f; // ����� ���� �������ÿ� �̵���ų Y ��ǥ

    // Attack
    private bool onAttack = false;
    [SerializeField]
    private Transform emitter;
    [SerializeField]
    private GameObject bullet;
    [SerializeField]
    private float bulletSpeed;

    // Hit
    private int life;
    [HideInInspector]
    public bool onDamage = false;
    private bool isDead = false;

    // yield return time
    private WaitForSeconds slideWait;
    private WaitForSeconds attackWait;
    private WaitForSeconds putGunWait;
    private WaitForSeconds respawnWait;
    private WaitForSeconds invincibleWait;

    // Component
    private PlayerStatus status;
    private SpriteRenderer sprite;
    private Rigidbody2D rigid;
    private BoxCollider2D collider;
    private Animator animator;

    void Awake()
    {
        status = GetComponent<PlayerStatus>();
        sprite = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        StatusSetting();
        StartSetting();
        WaitTimeSetting();
    }

    void StatusSetting()
    {
        landDis = status.RayDistance;
        jumpTime = status.JumpTime;
        jumpCool = status.JumpCoolTime;
        jumpFoece = status.JumpFoce;
        slideTime = status.SlideTime;
        slideCool = status.SlideCoolTime;
        attackCool = status.AttackCoolTime;
        putGunCool = status.PutGunTime;
        maxLife = status.MaxLife;
        knockback = status.KnockbackForce;
        bounce = status.BounceForce;
        invincibleTime = status.InvincibleTime;
        respawnTime = status.RespawnTime;
    }

    void StartSetting()
    {
        life = maxLife;
        landVec[0] = transform.GetChild(0);
        landVec[1] = transform.GetChild(1);
        gameObject.layer = defaultLayer;

        Move(true);
    }

    void WaitTimeSetting()
    {
        slideWait = new WaitForSeconds(slideTime);
        attackWait = new WaitForSeconds(attackCool);
        putGunWait = new WaitForSeconds(putGunCool);
        respawnWait = new WaitForSeconds(respawnTime);
        invincibleWait = new WaitForSeconds(invincibleTime);
    }

    void Update()
    {
        if (isDead)
            return;

        if (!onDamage && !onGround && !onJumping && !onSlide && GroundCheck(landDis))
        {
            Land();
        }

        Fall(rigid.velocity.y < -0.1f ? true : false); // 0�� ���� ���� �ݵ����� �ִϸ��̼� ������ ��Ÿ����.
    }

    public void Move(bool move)
    {
        animator.SetBool("onMove", move);
    }

    void Land()
    {
        onGround = true;
        AttackCancel();

        if (jumpCount != 2)
        {
            jumpCount = 2;
            UIManager.instance.JumpCount(jumpCount);
        }

        animator.SetBool("onGround", true);
        animator.SetBool("onFall", false);
    }

    bool GroundCheck(float distance)
    {
        Vector2 landPos1 = new Vector2(transform.position.x + landVec[0].localPosition.x, transform.position.y + landVec[0].localPosition.y);
        Vector2 landPos2 = new Vector2(transform.position.x + landVec[1].localPosition.x, transform.position.y + landVec[1].localPosition.y);
        Debug.DrawRay(landPos1, Vector2.down * distance, Color.green);
        Debug.DrawRay(landPos2, Vector2.down * distance, Color.green);
        RaycastHit2D platCheck1 = Physics2D.Raycast(landPos1, Vector2.down, distance, LayerMask.GetMask("Ground"));
        RaycastHit2D platCheck2 = Physics2D.Raycast(landPos2, Vector2.down, distance, LayerMask.GetMask("Ground"));

        return platCheck1 || platCheck2 ? true : false;
    }
    
    public void Jump() // ��ư���� ����ϱ� ������ public
    {
        if (!isDead && jumpCount > 0 && jumpAble && !onDamage) // 2�� �������� �����ϴ�.
        {
            if (onSlide)
            {
                SlideCancel();
            }

            AttackCancel();
            onGround = false;
            onJumping = true;
            onFall = false;
            jumpAble = false;
            jumpCount--;
            UIManager.instance.JumpCount(jumpCount);
            animator.SetBool("onGround", false);

            rigid.velocity = Vector2.zero;
            rigid.AddForce(Vector2.up * jumpFoece);
            animator.SetTrigger("doJump");
            AudioManager.instance.PlayPlayerSFX(AudioManager.PlayerSFX.Jump);
            Invoke("JumpCool", jumpCool);
            Invoke("JumpTime", jumpTime); // �������� Jump�� ���� ������ �������� onJump�� �����̸� �ش�.
        }
    }

    public void OnJump(bool click) // ��ư���� ����ϱ� ������ public
    {
        rigid.gravityScale = click ? 0.75f : 1.1f;
    }

    void JumpCool()
    {
        jumpAble = true;
    }

    void JumpTime()
    {
        onJumping = false;
    }

    void Fall(bool fall)
    {
        onFall = fall;
        animator.SetBool("onFall", fall);
    }

    public void Slide()
    {
        if (!isDead && GroundCheck(landDis) && !onDamage && !onSlide)
        {
            StartCoroutine("SlideProcess");
        }

        if (onAttack)
        {
            AttackCancel();
        }
    }

    IEnumerator SlideProcess()
    {
        onSlide = true;
        animator.SetBool("onSlide", true);
        StartCoroutine("InvincibleTime", slideTime);
        AudioManager.instance.PlayPlayerSFX(AudioManager.PlayerSFX.Slide);
        UIManager.instance.ButtonCooldown("slide", slideCool); // �����̵��� ��Ÿ�� ���

        yield return slideWait;
        onSlide = false;
        animator.SetBool("onSlide", false);
    }

    void SlideCancel()
    {
        StopCoroutine("SlideProcess"); // �����̵��� ������ ������ bool�� �ʱ�ȭ�Ѵ�.
        onSlide = false;
        animator.SetBool("onSlide", false);
        gameObject.layer = defaultLayer;
    }

    public void Attack()
    {
        if (!onAttack && !isDead && !onDamage && !onSlide)
        {
            StopCoroutine("AttackProcess");
            StartCoroutine("AttackProcess");
        }
    }

    IEnumerator AttackProcess()
    {
        onAttack = true;
        animator.SetBool("onAttack", true);
        GameObject spawnBullet = PoolManager.instance.Get(PoolManager.PoolType.Bullet, 0);
        spawnBullet.transform.position = emitter.position;
        spawnBullet.GetComponent<Bullet>().Shoot(Vector2.right, bulletSpeed);
        AudioManager.instance.PlayPlayerSFX(AudioManager.PlayerSFX.Shoot);

        yield return attackWait;
        onAttack = false;

        yield return putGunWait;
        animator.SetBool("onAttack", false);
    }

    void AttackCancel()
    {
        StopCoroutine("AttackProcess");
        onAttack = false;
        animator.SetBool("onAttack", false);
    }

    public void Hit()
    {

    }

    public void FallDown()
    {
        if (!onDamage)
        {
            LoseHP();
        }

        onDamage = true;
        StartCoroutine("Respawn");
    }

    void LoseHP()
    {
        life--;
        UIManager.instance.DamageUI(life);

        if (life <= 0)
        {
            Die();
        }
    }

    IEnumerator Respawn()
    {
        yield return respawnWait;

        if (!isDead)
        {
            rigid.simulated = false; // ������ ó�� �߿��� rigidbody�� ��Ȱ��ȭ�Ѵ�.
            sprite.enabled = false;
            transform.position = new Vector2(transform.position.x, respawnPosY);
            GameManager.instance.GameLive(true);

            while (!GroundCheck(10f))
            {
                yield return null;
            }

            onDamage = false;
            rigid.simulated = true;
            sprite.enabled = true;
            StartCoroutine("InvincibleTime", invincibleTime);
            UIManager.instance.RespawnFX(transform.position);
            AudioManager.instance.PlayPlayerSFX(AudioManager.PlayerSFX.Respawn);
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;
        collider.enabled = false;
        animator.SetTrigger("doDie");
        AudioManager.instance.PlayPlayerSFX(AudioManager.PlayerSFX.Die);
        StartCoroutine("DieProcess");
    }

    IEnumerator DieProcess()
    {
        rigid.velocity = Vector2.zero;
        rigid.AddForce(Vector2.up * bounce);
        GameManager.instance.GameOver();

        yield return new WaitForSeconds(1f);
        rigid.gravityScale *= 2;

        yield return new WaitForSeconds(2f); // ĳ���Ͱ� ȭ�� ������ ���� �������� �߶��� ��Ȱ��ȭ �Ѵ�.
        rigid.simulated = false;
    }

    IEnumerator InvincibleTime(float time)
    {
        leftInvincible = Mathf.Max(leftInvincible, time);
        gameObject.layer = invincibleLayer;

        while (leftInvincible > 0) // ������ �����ð��� ������� ���� �����Ѵ�.
        {
            leftInvincible -= Time.deltaTime;
            yield return null;
        }

        gameObject.layer = defaultLayer;
    }

    //public void Damage(bool outSide)
    //{
    //    if (outSide || !onDamage) // �����̰ų� �ǰ� ���°� �ƴҶ��� ����
    //    {
    //        GameManager.instance.GameLive(false); // ĳ���Ϳ��� ������ �����. ��ũ�Ѹ� �Ͻ�����

    //        if (!onDamage) // �������� ������ ó�� ����
    //        {
    //            life--;
    //            UIManager.instance.DamageUI(life);
    //        }

    //        onDamage = true;
    //        rigid.velocity = Vector2.zero;
    //        Move(false);
    //        AudioManager.instance.PlayPlayerSFX(AudioManager.PlayerSFX.Hit);
    //        AttackCancel();

    //        if (life <= 0)
    //        {
    //            StartCoroutine("Die");
    //        }
    //        else
    //        {
    //            StopCoroutine("DamageProcess");
    //            StartCoroutine("DamageProcess", outSide);
    //        }
    //    }
    //}

    //IEnumerator DamageProcess(bool onOut)
    //{
    //    sprite.color = new Color(1, 1, 1, 0.7f);

    //    if (onOut) // ����(DeadZone)�� ���� �����
    //    {
    //        yield return StartCoroutine("Respawn");
    //    }
    //    else // �Ϲ����� �ǰ�
    //    {
    //        yield return StartCoroutine("HitProcess");
    //        GameManager.instance.GameLive(true); // �ǰ��� �ٷ� ������ ���� / ����� Ÿ�̹��� ���� ���ؼ� �����Ѵ�.
    //    }

    //    onDamage = false;
    //    Move(true);

    //    yield return StartCoroutine("InvincibleTime", damageTime);
    //    sprite.color = new Color(1, 1, 1, 1);
    //}



    //IEnumerator HitProcess()
    //{
    //    animator.SetTrigger("onHit");
    //    rigid.AddForce(Vector2.left * knockback);

    //    yield return hitWait; // 1�� �ڿ� �ٴ��� �˻��ؼ� ���� ��� �״�� ���� ���� ��� ���� �����Ѵ�.
    //}




}