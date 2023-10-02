using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum EnemyType
    {
        Fixed, // ������
        LineMove, // ���� �̵���
        Chase, // ������
        Patrol // ������
    }

    public EnemyType type;

    [Header("Status")]
    [SerializeField]
    private int maxHp;
    private int hp;
    
    [Header("Move")]
    [SerializeField]
    protected float moveSpeed;
    [SerializeField]
    protected Vector2 moveVec;

    [Header("Component")]
    protected Rigidbody2D rigid;
    protected BoxCollider2D collider;
    protected Animator animator;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        hp = maxHp;
    }

    void Damage()
    {
        hp--;
        
        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        animator.SetTrigger("doDie");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            Debug.Log("Enemy �ǰ�");
        }

        if (collision.gameObject.layer == 10) // Deadzone
        {
            gameObject.SetActive(false);
        }
    }
}