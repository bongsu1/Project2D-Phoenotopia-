using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeState : PlayerState
{
    public override void Enter()
    {
        player.Animator.Play("Charging");
    }

    public override void Update()
    {
        // ������
        if ((player.ChargeTime < 1f) && (Mathf.Abs(player.MoveDir.x) < 0.1f) && player.IsGrounded && player.Input.actions["Attack"].IsPressed())
        {
            player.Animator.Play("Charging");
            player.ChargeTime += Time.deltaTime;
            player.Animator.SetFloat("ChargedTime", player.ChargeTime);
        }
        // �����Ϸ�
        else if ((Mathf.Abs(player.MoveDir.x) < 0.1f) && player.IsGrounded && player.Input.actions["Attack"].IsPressed())
        {
            player.Animator.Play("Charged");
            player.Animator.SetFloat("ChargedTime", player.ChargeTime);
        }
        // ������ �̵�
        else if ((player.ChargeTime < 1f) && (Mathf.Abs(player.MoveDir.x) > 0.1f) && player.IsGrounded && player.Input.actions["Attack"].IsPressed())
        {
            player.Animator.Play("ChargingWalk");
            player.ChargeTime += Time.deltaTime;
            player.Animator.SetFloat("ChargedTime", player.ChargeTime);

            if (player.MoveDir.x != 0)
            {
                player.transform.localScale = new Vector3(player.MoveDir.x, 1, 1);
            }
        }
        // �����Ϸ� �̵�
        else if ((Mathf.Abs(player.MoveDir.x) > 0.1f) && player.IsGrounded && player.Input.actions["Attack"].IsPressed())
        {
            player.Animator.Play("ChargedWalk");
            player.Animator.SetFloat("ChargedTime", player.ChargeTime);

            if (player.MoveDir.x != 0)
            {
                player.transform.localScale = new Vector3(player.MoveDir.x, 1, 1);
            }
        }
        // ������ �Ϸ�ǰ� Ű�� ���� ��������
        else if (player.IsGrounded && !player.Input.actions["Attack"].IsPressed() && player.Input.actions["Attack"].triggered && player.ChargeTime >= 1f)
        {
            player.Animator.Play("ChargeAttack");
            player.Input.actions["Attack"].Disable();
        }
    }

    public override void FixedUpdate()
    {
        Move();
    }

    public override void Exit()
    {
        player.ChargeTime = 0;
        player.Input.actions["Attack"].Enable();
    }

    private void Move()
    {
        float target = player.MoveDir.x * player.MoveSpeed;
        float diffSpeed = target - player.Rigid.velocity.x;
        player.Rigid.AddForce(Vector2.right * diffSpeed * player.Accel);
    }

    public override void Transition()
    {
        if (!player.IsGrounded)
        {
            ChangeState(Player.State.Jump);
        }
        // ���� �Ϸᰡ �Ǳ� ���� ���� ���
        else if (!player.Input.actions["Attack"].IsPressed() && player.ChargeTime < 1f)
        {
            ChangeState(Player.State.Normal);
        }
    }

    public ChargeState(Player player)
    {
        this.player = player;
    }
}