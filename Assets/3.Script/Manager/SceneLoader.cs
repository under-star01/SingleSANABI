using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader
{
    public static string nextSceneName;

    // 프로그램 실행 시, 맨 처음 한번만 실행
    [RuntimeInitializeOnLoadMethod]
    private static void LoadScene()
    {
        // 현재 씬 이름 저장
        nextSceneName = SceneManager.GetActiveScene().name;

        // 로딩씬으로 이동
        SceneManager.LoadScene("Loading");
    }
}
