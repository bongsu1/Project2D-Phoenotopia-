using UnityEngine;

public class EBAttackState : BoarState
{
    private enum Type { Run, Jump }
    private Type type;

    public override void Enter()
    {
        boar.Rigid.velocity = Vector3.zero;
        int attackType = Random.Range(0, 3);
        type = attackType < 2 ? Type.Run : Type.Jump;

        float direction = Mathf.Sign(boar.Player.transform.position.x - boar.transform.position.x);
        if (type == Type.Run)
        {
            boar.StartRunAttackRoutine(direction);
            boar.AttackCount--;
        }
        else if (type == Type.Jump)
        {
            boar.StartJumpAttackRoutine(boar.Player.transform.position);
            boar.AttackCount--;
        }
    }

    public override void Update()
    {
        if (type == Type.Jump)
        {
            boar.Animator.SetFloat("Fall", boar.Rigid.velocity.y);
        }
    }

    public override void Exit()
    {
        if (type == Type.Run)
        {
            boar.StopRunAttackRoutine();
        }
        else if (type == Type.Jump)
        {
            boar.StopJumpAttackRoutine();
        }
    }

    public EBAttackState(Boar boar)
    {
        this.boar = boar;
    }
}
