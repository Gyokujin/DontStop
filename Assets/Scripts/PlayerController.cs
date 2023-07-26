using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Status")]
    [SerializeField]
    private float jumpFoece = 500f;

    // private int jumpCount; // ���Ŀ� ���� ����
    private bool isGrounded = false; // �ٴڿ� ��Ҵ��� ��Ÿ��
    private bool onDamage = false;

    [Header("Components")]
    private Rigidbody2D rigid;
    private Animator animator;

    private PlayerAudio audio;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audio = GetComponent<PlayerAudio>();
    }

    void Update()
    {
        
    }
}