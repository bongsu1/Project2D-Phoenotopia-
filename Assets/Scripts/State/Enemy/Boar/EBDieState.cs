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
