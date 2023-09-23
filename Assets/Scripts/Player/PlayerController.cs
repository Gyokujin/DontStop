using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    private int maxLife;
    private float knockback;
    private float hitTime;
    private float invincibleTime;

    // Action
    private int jumpCount = 0;
    private bool onJumping = false;
    private bool jumpAble = true;
    private Transform[] landVec = new Transform[2];
    private bool onGround = false;
    private bool onFall = false;
    private bool onSlide = false;
    private bool slideAble = true;
    private int playerLayer = 6; // Player 레이어
    private int invincibleLayer = 7; // Invincible 레이어

    // Hit
    private int life;
    private bool onDamage = false;
    private bool isDead = false;

    // Component
    private PlayerStatus status;
    private PlayerAudio audio;
    private SpriteRenderer sprite;
    private BoxCollider2D collider;
    private Rigidbody2D rigid;
    private Animator animator;

    void Awake()
    {
        status = GetComponent<PlayerStatus>();
        audio = GetComponent<PlayerAudio>();
        sprite = GetComponent<SpriteRenderer>();
        collider = GetComponent<BoxCollider2D>();
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        StatusSetting();
        StartSetting();
    }

    void StatusSetting()
    {
        landDis = status.RayDistance;
        jumpTime = status.JumpTime;
        jumpCool = status.JumpCoolTime;
        jumpFoece = status.JumpFoce;
        slideTime = status.SlideTime;
        slideCool = status.SlideCoolTime;
        maxLife = status.MaxLife;
        knockback = status.KnockbackForce;
        hitTime = status.HitTime;
        invincibleTime = status.InvincibleTime;
    }

    void StartSetting()
    {
        life = maxLife;
        landVec[0] = transform.GetChild(0);
        landVec[1] = transform.GetChild(1);
        gameObject.layer = playerLayer;

        Move(true);
    }

    void Update()
    {
        if (isDead)
            return;

        if (!onGround && !onJumping && !onSlide && GroundCheck())
        {
            Land();
        }

        Fall(rigid.velocity.y < 0 ? true : false);
    }

    void Move(bool move)
    {
        animator.SetBool("onMove", move);
    }

    void Land()
    {
        onGround = true;
        jumpCount = 0;
        animator.SetBool("onGround", true);
        animator.SetBool("onFall", false);
    }

    bool GroundCheck()
    {
        Vector2 landPos1 = new Vector2(transform.position.x + landVec[0].localPosition.x, transform.position.y + landVec[0].localPosition.y);
        Vector2 landPos2 = new Vector2(transform.position.x + landVec[1].localPosition.x, transform.position.y + landVec[1].localPosition.y);
        Debug.DrawRay(landPos1, Vector2.down * landDis, Color.green);
        Debug.DrawRay(landPos2, Vector2.down * landDis, Color.green);
        RaycastHit2D platCheck1 = Physics2D.Raycast(landPos1, Vector2.down, landDis, LayerMask.GetMask("Ground"));
        RaycastHit2D platCheck2 = Physics2D.Raycast(landPos2, Vector2.down, landDis, LayerMask.GetMask("Ground"));

        return platCheck1 || platCheck2 ? true : false;
    }
    
    public void Jump() // 버튼으로 사용하기 때문에 public
    {
        if (!isDead && jumpCount < 2 && jumpAble && !onDamage)
        {
            if (onSlide)
            {
                SlideCancel();
            }

            onGround = false;
            onJumping = true;
            onFall = false;
            jumpAble = false;
            jumpCount++;
            animator.SetBool("onGround", false);

            rigid.velocity = Vector2.zero;
            rigid.AddForce(Vector2.up * jumpFoece);
            animator.SetTrigger("doJump");
            audio.PlaySound("jump");
            Invoke("JumpCool", jumpCool);
            Invoke("JumpTime", jumpTime); // 연속적인 Jump와 착지 오류를 막기위해 onJump에 딜레이를 준다.
        }
    }

    public void OnJump(bool click) // 버튼으로 사용하기 때문에 public
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
        if (!isDead && GroundCheck() && !onDamage && !onSlide && slideAble)
        {
            StartCoroutine("SlideProcess");
            Invoke("SlideCool", slideCool); // 슬라이드의 쿨타임을 추가
        }
    }

    IEnumerator SlideProcess()
    {
        onSlide = true;
        slideAble = false;
        gameObject.layer = invincibleLayer;
        animator.SetBool("onSlide", true);
        audio.PlaySound("slide");
        float time = slideTime;

        while (time > 0)
        {
            time -= Time.deltaTime;
            yield return null;
        }

        onSlide = false;
        gameObject.layer = playerLayer;
        animator.SetBool("onSlide", false);
    }

    void SlideCool()
    {
        slideAble = true;
    }

    void SlideCancel()
    {
        StopCoroutine("SlideProcess"); // 슬라이드중 점프로 내부의 bool을 초기화한다.
        onSlide = false;
        slideAble = true;
        animator.SetBool("onSlide", false);
        gameObject.layer = playerLayer;
    }

    void Damage()
    {
        GameManager.instance.GamePause(); // 캐릭터외의 진행을 멈춘다. 스크롤링 일시정지
        life--;
        rigid.velocity = Vector2.zero;
        animator.SetBool("onMove", false);
        audio.PlaySound("damage");

        if (life <= 0)
        {
            StartCoroutine("Die");
        }
        else
        {
            StartCoroutine("DamageProcess");
        }
    }

    /*
    IEnumerator DamageProcess()
    {
        onDamage = true;
        
        animator.SetTrigger("onHit");
        sprite.color = new Color(1, 1, 1, 0.7f);
        rigid.AddForce(Vector2.left * knockbackForce);

        

        yield return new WaitForSeconds(1f); // 1초 뒤에 바닥을 검사해서 있을 경우 그대로 진행 없을 경우 새로 진행한다.
        Invoke("StandUp", damageTime);
    }
    

    void StandUp()
    {
        animator.SetBool("onMove", true);
        onDamage = false;
        StartCoroutine("Invincible");
        GameManager.instance.GameRestart();
    }

    IEnumerator Invincible()
    {
        onInvincible = true;

        yield return new WaitForSeconds(invincibleTime);
        onInvincible = false;
        sprite.color = new Color(1, 1, 1, 1);
    }

    IEnumerator Die()
    {
        collider.enabled = false;
        rigid.velocity = Vector2.zero;
        rigid.AddForce(Vector2.up * 600f);
        animator.SetTrigger("onDie");
        isDead = true;
        GameManager.instance.GameOver();

        yield return new WaitForSeconds(1f);
        rigid.gravityScale *= 2;

        yield return new WaitForSeconds(2f);
        rigid.simulated = false;
    }
    */

}