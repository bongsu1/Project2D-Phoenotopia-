using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalState : PlayerState
{
    public override void Enter()
    {
        player.Animator.Play("Idle");
    }

    public override void Update()
    {
        player.Animator.SetFloat("MoveSpeed", Mathf.Abs(player.Rigid.velocity.x));

        if (player.MoveDir.x != 0)
        {
            player.transform.localScale = new Vector3(player.MoveDir.x, 1, 1);
        }
    }

    public override void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        float target = player.MoveDir.x * player.MoveSpeed;
        float diffSpeed = target - player.Rigid.velocity.x;
        player.Rigid.AddForce(Vector2.right * diffSpeed * player.Accel);
    }

    private void Jump()
    {
        player.Rigid.velocity = new Vector2(player.Rigid.velocity.x, player.JumpSpeed);
    }

    public override void Transition()
    {
        // ����Ű�� ������ AttackState
        if (!player.OnNPC && player.Input.actions["Attack"].IsPressed() && player.Input.actions["Attack"].triggered)
        {
            ChangeState(Player.State.Attack);
        }
        // NPC�տ��� ����Ű�� ������ TalkState
        else if (player.OnNPC && player.Input.actions["Attack"].IsPressed() && player.Input.actions["Attack"].triggered)
        {
            ChangeState(Player.State.Talk);
        }
        // �Ʒ�Ű�� ������ ������ DuckState
        else if (player.MoveDir.y < -0.1f)
        {
            ChangeState(Player.State.Duck);
        }
        // JumpŰ�� ������ ������ JumpState
        else if (player.Input.actions["Jump"].IsPressed() && player.Input.actions["Jump"].triggered)
        {
            Jump();
            if (!player.IsGrounded)
            {
                ChangeState(Player.State.Jump);
            }
        }
        // �������� JumpState
        else if (!player.IsGrounded)
        {
            ChangeState(Player.State.Jump);
        }
        // ���� ��ٸ��� �ְ� ��Ű�� ������ ClimbState
        else if (player.IsLadder && player.MoveDir.y > 0f)
        {
            ChangeState(Player.State.Climb);
        }
        // ���Ű�� ������ GrabState
        else if (player.Input.actions["Grab"].IsPressed() && player.Input.actions["Grab"].triggered)
        {
            ChangeState(Player.State.Grab);
        }
    }

    public NormalState(Player player)
    {
        this.player = player;
    }
}
