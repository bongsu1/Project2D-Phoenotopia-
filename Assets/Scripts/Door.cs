using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour, IEnterable
{
    public UnityEvent OnEnter;

    [SerializeField] Animator animator;
    [SerializeField] BoxCollider2D doorColl;
    [SerializeField] Transform exitPoint;
    [SerializeField] bool isOpen;

    Door exitDoor;

    private float ySize;

    private void Start()
    {
        exitDoor = exitPoint.GetComponent<Door>();
        doorColl = exitPoint.GetComponent<BoxCollider2D>();
        ySize = doorColl.size.y;

        string open = isOpen ? "Opened" : "Closed";

        animator.Play(open);
    }

    public void Enter(Player player)
    {
        if (!isOpen)
        {
            animator.SetTrigger("Open");
            return;
        }

        player.Animator.Play("EnterDoor");
        player.Rigid.velocity = Vector2.zero;

        // ������ ���� �ְ� �� ���� �ȿ��������� ����
        if (exitDoor != null)
        {
            exitDoor.animator.SetTrigger("Open");
        }
    }

    public void Exit(Player player)
    {
        OnEnter?.Invoke();
        player.transform.position = new Vector3(exitPoint.position.x, exitPoint.position.y - ySize);
        player.Animator.Play("ExitDoor");
    }

    // ���� ���� �ִϸ��̼ǿ��� ȣ��
    public void IsOpen()
    {
        isOpen = true;
    }
}
