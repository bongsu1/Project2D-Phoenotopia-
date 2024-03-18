using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneManager : Singleton<SceneManager>
{
    [SerializeField] Image fade;
    [SerializeField] float fadeTime;

    [SerializeField] int exitPoint; // �� ������ ���� ���� ���� ���� ���޿�
    [SerializeField] Vector2 battlePosition; // ���Ϳ� �ο����� �����, ������� ��Ʋ������ ���

    private BaseScene curScene;

    public BaseScene GetCurScene()
    {
        if (curScene == null)
        {
            curScene = FindObjectOfType<BaseScene>();
        }
        return curScene;
    }

    public T GetCurScene<T>() where T : BaseScene
    {
        if (curScene == null)
        {
            curScene = FindObjectOfType<BaseScene>();
        }
        return curScene as T;
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadingRoutine(sceneName));
    }

    private bool isStart;
    IEnumerator LoadingRoutine(string sceneName)
    {
        BaseScene prevScene = GetCurScene();
        if (prevScene.name == "TitleScene")
        {
            isStart = true;
        }
        yield return FadeOut();

        Manager.Pool.ClearPool();
        Manager.Sound.StopSFX();
        Manager.UI.ClearPopUpUI();
        Manager.UI.ClearWindowUI();
        Manager.UI.CloseInGameUI();

        // �������� ����
        exitPoint = GetCurScene().ExitPoint;
        battlePosition = GetCurScene().BattlePosition;

        Time.timeScale = 0f;

        AsyncOperation oper = UnitySceneManager.LoadSceneAsync(sceneName);
        yield return new WaitUntil(() => oper.isDone);

        Manager.UI.EnsureEventSystem();

        BaseScene curScene = GetCurScene();
        curScene.ExitPoint = exitPoint;           // �������� ����
        curScene.BattlePosition = battlePosition; // ���Ϳ� �ο� ���� ����
        yield return curScene.LoadingRoutine();

        Time.timeScale = 1f;

        yield return FadeIn();
        if (isStart)
        {
            isStart = false;
        }
    }

    IEnumerator FadeOut()
    {
        float rate = 0;
        Color fadeOutColor = isStart ? Color.white : Color.black;
        Color fadeInColor = isStart ? new Color(1f, 1f, 1f, 0f) : new Color(0f, 0f, 0f, 0f);

        while (rate <= 1)
        {
            rate += Time.deltaTime / fadeTime;
            fade.color = Color.Lerp(fadeInColor, fadeOutColor, rate);
            yield return null;
        }
    }

    IEnumerator FadeIn()
    {
        float rate = 0;
        Color fadeOutColor = isStart ? Color.white : Color.black;
        Color fadeInColor = isStart ? new Color(1f, 1f, 1f, 0f) : new Color(0f, 0f, 0f, 0f);

        while (rate <= 1)
        {
            rate += Time.deltaTime / fadeTime;
            fade.color = Color.Lerp(fadeOutColor, fadeInColor, rate);
            yield return null;
        }
    }
}
