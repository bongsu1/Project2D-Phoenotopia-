using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public enum State { Sleep, Normal, Jump, Duck, Climb, Attack, Charge, Grab, Carry, Talk }

    private StateMachine<State> stateMachine = new StateMachine<State>();

    [Header("Component")]
    [SerializeField] Rigidbody2D rigid;
    [SerializeField] Animator animator;
    [SerializeField] PlayerInput input;
    [SerializeField] BoxCollider2D playercoll; // ���̴� �ڼ����� �ݶ��̴� ����� ���̱� ���� BoxCollider2D�� �ʿ�
    [SerializeField] Transform attackPoint; // ���ݹ��� ��ġ
    [SerializeField] Transform grabPoint; // ���ڸ� ��� ��ġ
    [SerializeField] Transform talkPoint; // NPC ��ȣ�ۿ� ����Ʈ

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

    [Header("Physics")]
    [SerializeField] float accel;
    [SerializeField] float multiplier;
    [SerializeField] float lowJumpMultiplier;
    [SerializeField] float maxFall;

    [Header("LayerMask")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask platformLayer;
    [SerializeField] LayerMask LadderLayer;
    [SerializeField] LayerMask damagableLayer;
    [SerializeField] LayerMask grabbableLayer;
    [SerializeField] LayerMask npcLayer;
    [SerializeField] Vector2 talkBoxSize;

    Vector2 moveDir;
    private float moveSpeed;
    private float chargeTime;
    private float attackRange;
    private float hitPower;
    private int groundCount;
    private int ladderCount;
    private bool isGrounded;
    private bool isDucking;
    private bool isLadder;
    private bool onNPC;
    private bool onTalk;
    Collider2D platformcoll;
    Box box;

    // Property
    public Animator Animator => animator;
    public PlayerInput Input => input;
    public Vector2 MoveDir => moveDir;
    public Rigidbody2D Rigid => rigid;
    public BoxCollider2D PlayerColl => playercoll;
    public Box Box { get { return box; } set { box = value; } }

    public float Accel => accel;
    public float JumpSpeed => jumpSpeed;
    public float MoveSpeed => moveSpeed;
    public float ClimbMoveSpeed => climbMoveSpeed;
    public float ChargeTime { get { return chargeTime; } set { chargeTime = value; } }
    public float ThrowPower => throwPower;
    public float HitPower => hitPower;

    public bool IsGrounded => isGrounded;
    public bool IsDucking => isDucking;
    public bool IsLadder => isLadder;
    public bool OnNPC => onNPC;
    public bool OnTalk {get { return onTalk; } set { onTalk = value; } }

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

        //stateMachine.Start(State.Sleep); // Sleep���� ����
        stateMachine.Start(State.Normal);
    }

    private void Update()
    {
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

        hitPower = chargeTime > 1f ? chargeHitPower : normalHitPower;
        attackRange = chargeTime > 1f ? chargeAttackRange : normalAttackRange;

        stateMachine.Update();
    }

    private void FixedUpdate()
    {
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
                Rigidbody2D other = colliders[i].GetComponent<Rigidbody2D>();
                if (other != null)
                {
                    Vector2 hitDir = new Vector2(other.position.x - transform.position.x, other.position.y - transform.position.y).normalized;
                    other.AddForce(hitDir * hitPower, ForceMode2D.Impulse);
                }
                damagable.TakeDamage(damage);
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
        int size = Physics2D.OverlapBoxNonAlloc(talkPoint.position, talkBoxSize, 0, colliders, npcLayer);
        if (size > 0)
        {
            onTalk = true;
            IInteractable npc = colliders[0].GetComponent<IInteractable>();
            npc.Interact(this);
        }
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
        Gizmos.DrawWireCube(talkPoint.position, talkBoxSize);
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
        yield return new WaitForSeconds(0.2f);
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
    }
}
