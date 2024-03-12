using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EBDieState : BoarState
{
    public override void Enter()
    {
        boar.Animator.Play("Die");
        boar.DestroyGameObject();
        // ������ ������
    }

    public EBDieState(Boar boar)
    {
        this.boar = boar;
    }
}
