using System.Collections;
using UnityEngine;

public abstract class BaseScene : MonoBehaviour
{
    [SerializeField] protected CharacterStatusRender statusRender;
    [SerializeField] protected Transform[] startPoint;
    [SerializeField] protected int exitPoint; // ������ ���� Ȯ���� ���� ������ ��� ������ ���ϱ� ���� ���� ����Ʈ ����
    [SerializeField] protected Vector2 battlePosition; // ���Ϳ� ����� ����Ǵ� ����Ʈ, ������� ��Ʋ������ ���

    public int ExitPoint { get { return exitPoint; } set { exitPoint = value; } }
    public Vector2 BattlePosition { get { return battlePosition; } set { battlePosition = value; } }

    public abstract IEnumerator LoadingRoutine();
}
