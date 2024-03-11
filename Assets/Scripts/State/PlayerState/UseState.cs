using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UseState : PlayerState
{
    // 장착한 아이템 종류에 따라 다르게 업데이트 (무기, 음식)
    // 임시로 새총만 구현
    Transform aim;
    Quaternion bottomTurnPoint;
    Quaternion topTurnPoint;
    float rotateDir;

    public override void Enter()
    {
        bottomTurnPoint = Quaternion.Euler(0f, 0f, -40f);
        topTurnPoint = Quaternion.Euler(0f, 0f, 50f);

        player.Animator.Play("SlingshotStart");
        player.Rigid.velocity = Vector3.zero;

        aim = player.SlingshotAim;
        aim.localRotation = Quaternion.Euler(0f, 0f, -39f);
        rotateDir = -0.1f;
        aim.gameObject.SetActive(true);
    }

    public override void Update()
    {
        if (aim.localRotation.z < bottomTurnPoint.z)
        {
            player.Animator.Play("AimUp");
            rotateDir = 1f;   
        }
        else if (aim.localRotation.z > topTurnPoint.z)
        {
            player.Animator.Play("AimDown");
            rotateDir = -1f;
        }
        aim.Rotate(0, 0, player.AimSpeed * rotateDir * Time.deltaTime);

        if (player.Input.actions["Use"].IsPressed() && player.Input.actions["Use"].triggered)
        {
            rotateDir = 0f;
            player.Animator.Play("SlingshotEnd");
            player.Shot();
        }
    }

    public override void Exit()
    {
        aim.gameObject.SetActive(false);
    }

    public override void Transition()
    {
        if (player.Input.actions["Jump"].IsPressed() && player.Input.actions["Jump"].triggered)
        {
            ChangeState(Player.State.Normal);
        }
    }

    public UseState(Player player)
    {
        this.player = player;
    }
}
