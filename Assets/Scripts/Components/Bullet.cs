using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public enum BulletType
    {
        Player, Enemy
    }

    [SerializeField]
    private BulletType type;
    [SerializeField]
    private float launchTime;
    private float currentLaunch;

    private SpriteRenderer sprite;
    private Rigidbody2D rigid;
    private BoxCollider2D collider;
    private Animator animator;

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    public void Shoot(Vector2 direction, float speed)
    {
        currentLaunch = launchTime; // Ǯ������ ȸ���� �Ѿ��� �߻� �ð��� �ʱ�ȭ�Ѵ�.
        rigid.velocity = (direction * speed);
        collider.enabled = true;
        animator.SetBool("onHit", false);
    }

    void Update()
    {
        currentLaunch -= Time.deltaTime;

        if (currentLaunch <= 0)
        {
            Hide();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (type == BulletType.Player && collision.GetComponent<Enemy>())
        {
            Enemy enemy = collision.GetComponent<Enemy>();

            if (!enemy.onDie)
            {
                enemy.Damage();
                animator.SetBool("onHit", true);
                collider.enabled = false;
                Hide();
                AudioManager.instance.PlaySystemSFX(AudioManager.SystemSFX.Hit);
            }
        }
        else if (type == BulletType.Enemy && collision.GetComponent<PlayerController>())
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            if (!player.onDamage)
            {
                player.Hit();
                animator.SetBool("onHit", true);
                Hide();
            }
        }
    }

    void Hide()
    {
        gameObject.transform.position = Vector2.zero;
        PoolManager.instance.Return(gameObject);
    }
}