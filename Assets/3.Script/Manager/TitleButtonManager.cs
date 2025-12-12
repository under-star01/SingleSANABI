using UnityEngine;

public class TitleButtonManager : MonoBehaviour
{
    public void OnClick_LoadScene(string targetSceneName)
    {
        GameManager.instance.LoadScene(targetSceneName);
    }

    public void OnClick_ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnPointEnter()
    {
        AudioManager.instance.PlaySFX_Obj(AudioManager.instance.UISFX);
    }
}
