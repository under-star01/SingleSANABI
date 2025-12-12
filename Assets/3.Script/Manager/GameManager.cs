using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    // 이벤트들
    public event Action ReadyTimer;
    public event Action EndTimer;
    public event Action ReturnPos;

    // 현재 상태 확인 변수
    public bool isInitialized = false; // 초기화 여부
    public bool isTimeAttack = false;  // 타임어택 여부

    private void Awake()
    {
        // 이미 인스턴스가 있으면 자기 자신 파괴
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // 싱글톤 설정
        instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log($"{SceneManager.GetActiveScene().name} : GameManager를 새로 생성했습니다.");
    }

    public void InitializeData()
    {
        if (isInitialized) return;

        // ex) playerData 초기화, audio 초기화 등등
        
        isInitialized = true;
        Debug.Log("GameManager 초기화 완료했습니다.");
    }

    public void LoadScene(string sceneName)
    {
        // 목표 씬 이름 저장
        SceneLoader.nextSceneName = sceneName;

        // 로딩씬으로 이동
        SceneManager.LoadScene("Loading");
    }

    // 이벤트 호출 함수들
    public void RequestReadyTimer() => ReadyTimer?.Invoke();
    public void RequestEndTimer() => EndTimer?.Invoke();
    public void RequestReturnPos() => ReturnPos?.Invoke();
}