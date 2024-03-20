using UnityEngine;

public class AttackState : PlayerState
{
    float attackPressTime = 0;

    public override void Enter()
    {
        Manager.Data.Stamina -= player.UseStamina;
        if (player.IsGrounded)
        {
            player.Animator.Play("Attack");
            player.SFX.PlaySFX(PlayerSoundManager.SFX.NormalAttack);
        }
        else
        {
            player.Animator.Play("JumpAttack");
            player.SFX.PlaySFX(PlayerSoundManager.SFX.AirAttack);
        }
    }

    public override void Update()
    {
        if (player.Input.actions["Attack"].IsPressed())
        {
            attackPressTime += Time.deltaTime;
        }
    }

    public override void FixedUpdate()
    {
        Move();
    }

    public override void Exit()
    {
        attackPressTime = 0;
    }

    public override void Transition()
    {
        if (player.IsGrounded && player.Input.actions["Attack"].IsPressed() && attackPressTime > 0.3f)
        {
            ChangeState(Player.State.Charge);
        }
    }

    private void Move()
    {
        float target = player.MoveDir.x * player.MoveSpeed;
        float diffSpeed = target - player.Rigid.velocity.x;
        player.Rigid.AddForce(Vector2.right * diffSpeed * player.Accel);
    }

    public AttackState(Player player)
    {
        this.player = player;
    }
}
