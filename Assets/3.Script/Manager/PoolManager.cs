using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoolManager : MonoBehaviour
{
    public static PoolManager instance = null;
    
    // 사슬 구현 관련 변수
    public List<Transform> chainList = new List<Transform>(); // chain 저장 리스트
    public GameObject chainPrefab; // 반복할 chain 프리팹
    public int maxChainCnt = 60;

    // 잔상 구현 관련 변수 
    public List<SpriteRenderer> traceList; // 잔상 오브젝트 리스트
    [SerializeField] private GameObject tracePrefab; // 잔상 오브젝트 프리팹
    [SerializeField] private int traceMaxCnt = 20; // 잔상 오브젝트 폴링 생성 개수

    // 현재 상태 확인 변수
    public bool isInitialized = false; // 초기화 여부

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
        Debug.Log($"{SceneManager.GetActiveScene().name} : PoolManager를 새로 생성했습니다.");
    }

    public void InitializeData()
    {
        if (isInitialized) return;

        // 사슬 프리팹 생성 및 초기화
        for (int i = 0; i < maxChainCnt; i++)
        {
            GameObject chain = Instantiate(chainPrefab, transform);
            chain.SetActive(false);
            chainList.Add(chain.transform);
        }

        // 잔상 프리팹 생성 및 초기화
        for (int i = 0; i < traceMaxCnt; i++)
        {
            GameObject trace = Instantiate(tracePrefab, transform);
            trace.SetActive(false);
            traceList.Add(trace.transform.GetComponent<SpriteRenderer>());
        }

        isInitialized = true;
        Debug.Log("PoolManager를 초기화 완료했습니다.");
    }
}
