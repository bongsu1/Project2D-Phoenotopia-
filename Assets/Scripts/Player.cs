using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IDamagable
{
    public enum State { Sleep, Normal, Jump, Duck, Climb, Attack, Charge, Grab, Carry, Talk, Use, Hit, Die }
    private StateMachine<State> stateMachine = new StateMachine<State>();

    [Header("Component")]
    [SerializeField] Rigidbody2D rigid;
    [SerializeField] Animator animator;
    [SerializeField] PlayerInput input;
    [SerializeField] BoxCollider2D playercoll; // ���̴� �ڼ����� �ݶ��̴� ����� ���̱� ���� BoxCollider2D�� �ʿ�
    [SerializeField] Transform attackPoint; // ���ݹ��� ��ġ
    [SerializeField] Transform grabPoint; // ���ڸ� ��� ��ġ
    [SerializeField] Transform interactPoint; // NPC ��ȣ�ۿ� ����Ʈ
    [SerializeField] Transform slingshotAim;

    [Header("status")]
    [SerializeField] int damage;
    [SerializeField] int hp;
    [SerializeField] int stamina;

    [Header("Normal")]
    [SerializeField] float walkSpeed;
    [SerializeField] float runSpeed;
    [SerializeField] float jumpSpeed;

    [Header("State move speed")]
    [SerializeField] float duckMoveSpeed;
    [SerializeField] float climbMoveSpeed;
    [SerializeField] float ChargerMoveSpeed;
    [SerializeField] float grabMoveSpeed;

    [Header("Attack")]
    [SerializeField] float normalAttackRange;
    [SerializeField] float chargeAttackRange;
    [SerializeField] float normalHitPower;
    [SerializeField] float chargeHitPower;

    [Header("Carry")]
    [SerializeField] float grabRange;
    [SerializeField] float throwPower;

    [Header("Use")]
    [SerializeField] float aimSpeed;
    [SerializeField] float slingshotPower;

    [Header("Physics")]
    [SerializeField] float accel;
    [SerializeField] float multiplier;
    [SerializeField] float lowJumpMultiplier;
    [SerializeField] float maxFall;
    [SerializeField] float takeHitPower;
    [SerializeField] PhysicsMaterial2D playerMaterial;
    [SerializeField] float bounciness;

    [Header("LayerMask")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask platformLayer;
    [SerializeField] LayerMask LadderLayer;
    [SerializeField] LayerMask damagableLayer;
    [SerializeField] LayerMask grabbableLayer;
    [SerializeField] LayerMask npcLayer;
    [SerializeField] Vector2 interactBoxSize;
    [SerializeField] LayerMask doorLayer;

    [Header("Prefab")]
    [SerializeField] Bullet slingshotBullet;

    Vector2 moveDir;
    private float moveSpeed;
    private float chargeTime;
    private float attackRange;
    private float hitPower;
    private float doorXPosition;
    private int groundCount;
    private int ladderCount;
    private bool isGrounded;
    private bool isDucking;
    private bool isLadder;
    private bool onNPC;
    private bool onTalk;
    private bool onDoor;
    private bool onEnter;
    private bool onExit;
    private bool onHit;
    Collider2D platformcoll;
    Box box;

    // Property
    public Animator Animator => animator;
    public PlayerInput Input => input;
    public Vector2 MoveDir => moveDir;
    public Rigidbody2D Rigid => rigid;
    public BoxCollider2D PlayerColl => playercoll;
    public Box Box { get { return box; } set { box = value; } }
    public Transform SlingshotAim => slingshotAim;
    public PhysicsMaterial2D PlayerMaterial { get { return playerMaterial; } set { playerMaterial = value; } }

    public float Accel => accel;
    public float JumpSpeed => jumpSpeed;
    public float MoveSpeed => moveSpeed;
    public float ClimbMoveSpeed => climbMoveSpeed;
    public float ChargeTime { get { return chargeTime; } set { chargeTime = value; } }
    public float ThrowPower => throwPower;
    public float HitPower => hitPower;
    public float AimSpeed => aimSpeed;
    public float TakeHitPower { get { return takeHitPower; } set { takeHitPower = value; } }
    public float Bounciness => bounciness;

    public bool IsGrounded => isGrounded;
    public bool IsDucking => isDucking;
    public bool IsLadder => isLadder;
    public bool OnNPC => onNPC;
    public bool OnTalk { get { return onTalk; } set { onTalk = value; } }
    public bool OnDoor => onDoor;
    public bool OnEnter => onEnter;
    public bool OnHit { get { return onHit; } set { onHit = value; } }

    private void Start()
    {
        stateMachine.AddState(State.Sleep, new SleepState(this));
        stateMachine.AddState(State.Normal, new NormalState(this)); // merge(Idle, walk)
        stateMachine.AddState(State.Jump, new JumpState(this));
        stateMachine.AddState(State.Duck, new DuckState(this));
        stateMachine.AddState(State.Climb, new ClimbState(this));
        stateMachine.AddState(State.Attack, new AttackState(this));
        stateMachine.AddState(State.Charge, new ChargeState(this));
        stateMachine.AddState(State.Grab, new GrabState(this));
        stateMachine.AddState(State.Carry, new CarryState(this));
        stateMachine.AddState(State.Talk, new TalkState(this));
        stateMachine.AddState(State.Use, new UseState(this));
        stateMachine.AddState(State.Hit, new HitState(this));
        stateMachine.AddState(State.Die, new DieState(this));

        stateMachine.Start(State.Normal);
    }

    float xPosition;
    private void Update()
    {
        if (onEnter)
        {
            if (onExit)
                return;

            xPosition = Mathf.Lerp(transform.position.x, doorXPosition, 0.1f);
            transform.position = new Vector2(xPosition, transform.position.y);
            return;
        }

        isGrounded = groundCount > 0;
        isDucking = moveDir.y < -0.1f && isGrounded;
        isLadder = ladderCount > 0;

        // ���� ���¸� ��������
        if (isDucking)
        {
            moveSpeed = duckMoveSpeed;
        }
        // ���� �����߿��� ��������
        else if (input.actions["Attack"].IsPressed())
        {
            moveSpeed = ChargerMoveSpeed;
        }
        // ������ �аų� ������ ��������
        else if (input.actions["Grab"].IsPressed() && box != null)
        {
            moveSpeed = grabMoveSpeed;
        }
        // "Run"Ű�� ������ �����̸� �޸���
        else if (input.actions["Run"].IsPressed())
        {
            moveSpeed = runSpeed;
        }
        else
        {
            moveSpeed = walkSpeed;
        }

        // ��¡�� �Ǹ� hitPower�� ������ attackRange�� Ŀ����
        hitPower = chargeTime > 1f ? chargeHitPower : normalHitPower;
        attackRange = chargeTime > 1f ? chargeAttackRange : normalAttackRange;

        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        if (onEnter)
            return;

        stateMachine.FixedUpdate();

        // ���� ���� ����
        if (rigid.velocity.y < 0)
        {
            rigid.velocity += Vector2.up * Physics2D.gravity.y * multiplier * Time.deltaTime;
        }
        else if (rigid.velocity.y > 0 && !input.actions["Jump"].IsPressed())
        {
            rigid.velocity += Vector2.up * Physics2D.gravity.y * lowJumpMultiplier * Time.deltaTime;
        }

        // �������� �ִ�ӵ� ����
        if (rigid.velocity.y < -maxFall)
        {
            rigid.velocity = new Vector2(rigid.velocity.x, -maxFall);
        }
    }

    Collider2D[] colliders = new Collider2D[10];
    public void Attack()
    {
        int size = Physics2D.OverlapCircleNonAlloc(attackPoint.position, attackRange, colliders, damagableLayer);
        for (int i = 0; i < size; i++)
        {
            IDamagable damagable = colliders[i].GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.TakeDamage(damage);
                damagable.Knockback(transform.position, hitPower);
            }
        }
    }

    public void Grab()
    {
        int size = Physics2D.OverlapCircleNonAlloc(grabPoint.position, grabRange, colliders, grabbableLayer);
        if (size > 0)
        {
            colliders[0].transform.SetParent(transform);
            box = colliders[0].GetComponent<Box>();
        }
    }

    public void Talk()
    {
        int size = Physics2D.OverlapBoxNonAlloc(interactPoint.position, interactBoxSize, 0, colliders, npcLayer);
        if (size > 0)
        {
            onTalk = true;
            IInteractable npc = colliders[0].GetComponent<IInteractable>();
            if (npc != null)
            {
                npc.Interact(this);
            }
            else if (npc == null)
            {
                onTalk = false;
            }
        }
    }

    IEnterable door;
    public void EnterDoor()
    {
        int size = Physics2D.OverlapBoxNonAlloc(interactPoint.position, interactBoxSize, 0, colliders, doorLayer);
        if (size > 0)
        {
            door = colliders[0].GetComponent<IEnterable>();
            if (door != null)
            {
                doorXPosition = colliders[0].transform.position.x;
                door.Enter(this);
            }
        }
    }

    public void ExitDoor()
    {
        door.Exit(this);
        onExit = true;
    }

    public void IsEnter()
    {
        onEnter = true;
    }

    public void IsExit()
    {
        onDoor = true;
        onEnter = false;
        door = null;
        onExit = false;
    }

    // ������ �����Ҷ� ������ ȣ��
    public void StartGame()
    {
        stateMachine.ChangeState(State.Sleep);
    }

    // DieScene���� ȣ���
    public void EndGame()
    {
        stateMachine.ChangeState(State.Die);
    }

    // ���� �ִϸ��̼��� ������ �������� �ִϸ��̼� �̺�Ʈ�� ȣ��
    // + ��� ������ ���оִϸ��̼� ������ ȣ��
    public void ToNormalState()
    {
        stateMachine.ChangeState(State.Normal);
    }

    // ������ ��� �ø��� �ִϸ��̼��� ������ ȣ��
    public void ToCarryState()
    {
        stateMachine.ChangeState(State.Carry);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(grabPoint.position, grabRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(interactPoint.position, interactBoxSize);
    }

    // ���ѿܿ� ��°��� �� ����� �Ѿ˵��� ��ũ���ͺ������Ʈ�� ����ڿ� �װ� �ҷ��ͼ� ��°��� ����
    public void Shot()
    {
        Vector2 shotDir = transform.localScale.x > 0 ? slingshotAim.right : -slingshotAim.right;
        Rigidbody2D bullet = Instantiate(slingshotBullet, slingshotAim.position, slingshotAim.rotation).GetComponent<Rigidbody2D>();
        bullet.velocity = shotDir * slingshotPower;
    }

    private void OnMove(InputValue value)
    {
        moveDir = value.Get<Vector2>();
    }

    private void OnJump(InputValue value)
    {
        if (isDucking && platformcoll)
        {
            StartCoroutine(DownJumpRoutine());
        }
    }

    IEnumerator DownJumpRoutine()
    {
        Physics2D.IgnoreCollision(playercoll, platformcoll);
        yield return new WaitForSeconds(0.15f);
        Physics2D.IgnoreCollision(playercoll, platformcoll, false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            groundCount++;
            if (((1 << collision.gameObject.layer) & platformLayer) != 0)
            {
                platformcoll = collision.gameObject.GetComponent<Collider2D>();
            }
        }
        else if (((1 << collision.gameObject.layer) & LadderLayer) != 0)
        {
            ladderCount++;
        }
        else if (((1 << collision.gameObject.layer) & npcLayer) != 0)
        {
            onNPC = true;
        }
        else if (((1 << collision.gameObject.layer) & doorLayer) != 0)
        {
            onDoor = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            groundCount--;
        }
        else if (((1 << collision.gameObject.layer) & LadderLayer) != 0)
        {
            ladderCount--;
        }
        else if (((1 << collision.gameObject.layer) & npcLayer) != 0)
        {
            onNPC = false;
        }
        else if (((1 << collision.gameObject.layer) & doorLayer) != 0)
        {
            onDoor = false;
        }
    }

    public void TakeDamage(int damage)
    {
        if (onHit)
            return;

        hp -= damage;
    }

    public void Knockback(Vector2 hitPoint, float hitPower)
    {
        if (onHit)
            return;

        if (hp < 0)
            return;

        onHit = true;
        float direction = Mathf.Sign(transform.position.x - hitPoint.x);
        Vector2 knockback = new Vector2(direction, hitPower > 9 ? hitPower * 0.5f : 0.5f).normalized;
        rigid.velocity = knockback * hitPower;
        transform.localScale = new Vector3(-direction, 1f, 1f);

        takeHitPower = hitPower;

        stateMachine.ChangeState(State.Hit);
        Die();
    }

    Coroutine knockbackRoutine;
    public void StartKnockbackRoutine(float takeHitPower)
    {
        knockbackRoutine = StartCoroutine(KnockbackRoutine(takeHitPower));
    }

    IEnumerator KnockbackRoutine(float takeHitPower)
    {
        yield return new WaitForSeconds(takeHitPower * 0.25f);
        ToNormalState();
    }

    public void StopKnockbackRoutine()
    {
        StopCoroutine(knockbackRoutine);
        onHit = false;
    }

    // test..
    public void Die()
    {
        if (hp > 0)
            return;

        Manager.Scene.LoadScene("DieScene");
    }

    private void OnDisable()
    {
        playerMaterial.bounciness = 0f;
    }
}
