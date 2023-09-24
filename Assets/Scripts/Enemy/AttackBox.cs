using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBox : MonoBehaviour
{
    public enum AttackType
    {
        Obstacle, Bullet
    }

    public AttackType attackType;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.gameObject.layer == 6) // 6 : �÷��̾�, 7 : ���� ����
        {
            collision.GetComponent<PlayerController>().Damage();

            if (attackType == AttackType.Bullet) // ���� �Ѿ��� ��� �÷��̾�� �������� �ְ� �ı��Ѵ�
            {
                Destroy(gameObject);
            }
        }
    }
}