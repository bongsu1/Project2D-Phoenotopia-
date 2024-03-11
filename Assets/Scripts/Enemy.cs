using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamagable
{
    [SerializeField] protected Animator animator;
    [SerializeField] protected Rigidbody2D rigid;
    [SerializeField] protected CircleCollider2D playerCheck;
    [SerializeField] protected int hp;
    [SerializeField] protected float checkSize;
    [SerializeField] LayerMask playerLayer;

    protected Player player;
    protected bool onPlayerCheck;

    // property
    public Animator Animator => animator;
    public Rigidbody2D Rigid => rigid;
    public Player Player => player;
    public bool OnPlayerCheck { get { return onPlayerCheck; } set { onPlayerCheck = value; } }

    public virtual void TakeDamage(int damage)
    {
        hp -= damage;
    }

    public void PlayerCheck()
    {
        if (onPlayerCheck)
            return;

        if (player == null)
            return;

        // �÷��̾��� �������� �߷� �Ǿ� �־ 0.4��ŭ ����
        Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.y + 0.4f);
        if (Vector2.Distance(playerPos, transform.position) > checkSize)
            return;

        if (((player.transform.position.x - transform.position.x) > 0) // �÷��̾ �����ʿ� ������
            && transform.localScale.x < 0) // ������ �ٶ� ��
            return;
        else if (((player.transform.position.x - transform.position.x) < 0) // �÷��̾ ���ʿ� ���� ��
            && transform.localScale.x > 0) // �������� �ٶ� ��
            return;

        onPlayerCheck = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (player != null)
            return;

        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            player = collision.GetComponent<Player>();
            playerCheck.gameObject.SetActive(false);
        }
    }
}
