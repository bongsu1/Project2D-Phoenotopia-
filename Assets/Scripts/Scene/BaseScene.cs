using System.Collections;
using UnityEngine;

public abstract class BaseScene : MonoBehaviour
{
    [SerializeField] protected Transform[] startPoint;
    [SerializeField] protected int exitPoint; // ������ ���� Ȯ���� ���� ������ ��� ������ ���ϱ� ���� ���� ����Ʈ ����

    public int ExitPoint { get { return exitPoint; } set { exitPoint = value; } }

    public abstract IEnumerator LoadingRoutine();
}
