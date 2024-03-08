using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseState : PlayerState
{
    // ������ ������ ������ ���� �ٸ��� ������Ʈ (����, ����)
    // �ӽ÷� ���Ѹ� ����

    float time;

    public override void Enter()
    {
        player.Animator.Play("SlingShot");
        player.AimRotateAngle = -10;
        player.Rigid.velocity = Vector3.zero;
    }

    public override void Update()
    {
        time += Time.deltaTime / player.useTime;
        // z�� �ʱⰪ -10
        player.AimRotateAngle -= 60 / player.useTime;
        // z�� ������ 50
        if (time > 1.2f)
        {
            player.Animator.Play("SlingShotEnd");
        }
    }

    public override void Exit()
    {
        time = 0;
    }

    public UseState(Player player)
    {
        this.player = player;
    }
}
