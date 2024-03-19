using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

// ��� ������ �ȵǸ� ������ // �ذ�
public class SleepState : PlayerState
{
    public override void Enter()
    {
        player.Animator.Play("Sleep");
    }

    public override void Update()
    {
        if (player.Input.actions["Jump"].IsPressed() && player.Input.actions["Jump"].triggered) // �ڷ�ƾ ��� �ִϸ��̼ǿ��� ChangeState�� ȣ��
        {
            player.Input.actions["Jump"].Disable();
            player.Animator.Play("WakeUp");
            player.SFX.PlaySFX(PlayerSoundManager.SFX.WakeUp);
        }
    }

    public override void Exit()
    {
        player.Input.actions["Jump"].Enable();
    }

    public SleepState(Player player)
    {
        this.player = player;
    }
}
