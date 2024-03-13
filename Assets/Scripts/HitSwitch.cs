using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HitSwitch : MonoBehaviour, IDamagable
{
    [SerializeField] Animator animator;

    public UnityEvent OnSwitch;
    public UnityEvent OffSwitch;

    private bool isPlay;       // ����Ǿ� �ִ� ������Ʈ�� ���ۻ���
    private bool onSwitching;  // ����ġ�� ���� ����(�ִϸ��̼� ����)

    public void TakeDamage(int damage)
    {
        if (isPlay || onSwitching)
            return;

        onSwitching = true;
        animator.SetTrigger("Hit");
    }

    public void OnSwitchngSet(string value)
    {
        switch (value)
        {
            case "true":
                onSwitching = true;
                break;
            case "false":
                onSwitching = false;
                break;
        }
    }

    public void SwitchOn()
    {
        OnSwitch?.Invoke();
    }

    public void SwitchOff()
    {
        OffSwitch?.Invoke();
    }

    public void IsPlaySet(bool value)
    {
        isPlay = value;
    }
}
