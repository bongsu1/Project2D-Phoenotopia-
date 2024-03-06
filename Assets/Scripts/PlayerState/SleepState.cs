using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

// ��� ������ �ȵǸ� ������
public class SleepState : PlayerState
{
    public override void Enter()
    {
        player.Input.actions["Move"].Disable();
        player.Animator.Play("Sleep");
    }

    public override void Exit()
    {
        player.Animator.Play("WakeUp"); // �ִϸ��̼��� ����Ǿ� �ϴµ� �ڷ�ƾ�� ���� ����
        player.Input.actions["Move"].Enable();
    }

    public override void Transition()
    {
        if (player.Input.actions["Jump"].IsPressed() && player.Input.actions["Jump"].triggered)
        {
            ChangeState(Player.State.Normal);
        }
    }

    public SleepState(Player player)
    {
        this.player = player;
    }
}
