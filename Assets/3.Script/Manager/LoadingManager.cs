using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;
using static Unity.Burst.Intrinsics.X86;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private float textInterval = 0.25f;
    [SerializeField] private string titleSceneName = "Title";
    
    private void Start()
    {
        StartCoroutine(StartRoutine());
    }

    private IEnumerator StartRoutine()
    {
        WaitForSeconds delay = new WaitForSeconds(textInterval);
        int dotCnt = 0;

        // GameManager 확인
        if (GameManager.instance != null)
        {
            // 초기화 상태 확인
            if (!GameManager.instance.isInitialized)
            {
                // GameManager 초기화
                GameManager.instance.InitializeData();
                GameManager.instance.isInitialized = true;
            }
        }
        else
        {
            // GameManager가 없으면, Title로 보내거나 기본 씬으로 처리
            Debug.LogWarning("GameManager가 없어서 nextScene을 Title로 대체합니다.");
            SceneLoader.nextSceneName = titleSceneName;
        }

        // AudioManager 확인
        if (AudioManager.instance != null)
        {
            // AudioManager 초기화
            AudioManager.instance.InitializeData();
        }

        // PoolManager 확인
        if (PoolManager.instance != null)
        {
            // 초기화 상태 확인
            if (!PoolManager.instance.isInitialized)
            {
                // PoolManager 초기화
                PoolManager.instance.InitializeData();
                PoolManager.instance.isInitialized = true;
            }
        }

        // 목표씬 비동기 로딩 및 로딩 텍스트 갱신
        AsyncOperation nextScene = SceneManager.LoadSceneAsync(SceneLoader.nextSceneName);
        nextScene.allowSceneActivation = false; // 씬 활성화 방지

        while (nextScene.progress < 0.9f)
        {
            loadingText.text = "Loading" + new string('.', dotCnt);
            dotCnt = (dotCnt + 1) % 4;
            yield return delay;
        }

        // 다음씬 활성화
        AudioManager.instance.PlayBGM(AudioManager.instance.currentBGM);
        nextScene.allowSceneActivation = true;
    }
}