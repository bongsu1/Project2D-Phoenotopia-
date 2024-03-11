using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabState : PlayerState
{
    float facing; // �ٶ󺸴� ������ ����
    BoxCollider2D boxColl;
    Vector2 offset;

    public override void Enter()
    {
        player.Grab();
        if (player.Box != null)
        {
            player.Animator.Play("Push");
            player.Animator.speed = 0;

            facing = player.transform.localScale.x;

            boxColl = player.Box.gameObject.GetComponent<BoxCollider2D>();
            offset = new Vector2(Mathf.Abs(boxColl.size.x + player.PlayerColl.size.x) / 2, (boxColl.size.y / 2) + 0.01f);
            player.Box.transform.localPosition = offset;
        }
        else
        {
            player.Animator.Play("Grab");
        }
    }

    public override void Update()
    {
        if (player.Box == null)
            return;

        player.Box.transform.localPosition = offset;

        // ����ִ� ���¿��� ������Ű�� ������ ������ ���ø���
        if (player.MoveDir.y > 0.1f)
        {
            player.Animator.Play("BoxUp");
            player.Animator.speed = 1f;
        }
        else if (player.MoveDir.x == 0)
        {
            player.Animator.Play("Push");
            player.Animator.speed = 0f;
        }
        // �ٶ󺸴� ����� �����̴� ������ ������ �б� �ִϸ��̼�
        else if (Mathf.Abs(player.MoveDir.x - facing) < 0.1f)
        {
            player.Animator.Play("Push");
            player.Animator.speed = 1f;
        }
        // �ݴ�� ����
        else if (Mathf.Abs(player.MoveDir.x - facing) > 1f)
        {
            player.Animator.Play("Pull");
            player.Animator.speed = 1f;
        }
    }

    public override void FixedUpdate()
    {
        Move();
    }

    public override void Exit()
    {
        player.Animator.speed = 1f;
    }

    private void Move()
    {
        float target = player.MoveDir.x * player.MoveSpeed;
        float diffSpeed = target - player.Rigid.velocity.x;
        player.Rigid.AddForce(Vector2.right * diffSpeed * player.Accel);
    }

    public override void Transition()
    {
        if (player.Box ==  null)
            return;

        if (!player.Input.actions["Grab"].IsPressed() && player.Input.actions["Grab"].triggered)
        {
            ChangeState(Player.State.Normal);
            player.Box.transform.parent = null;
            player.Box = null;
        }
        else if (!player.IsGrounded)
        {
            ChangeState(Player.State.Jump);
            player.Box.transform.parent = null;
            player.Box = null;
        }
    }

    public GrabState(Player player)
    {
        this.player = player;
    }
}
