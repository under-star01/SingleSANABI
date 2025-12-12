using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalQueueCtrl : MonoBehaviour
{
    [SerializeField] private List<Transform> goals; // 저장할 위치를 받아올 리스트 (Queue는 SerializeField 사용 못함)
    [SerializeField] private GameObject goal1; // 목표 오브젝트 1
    [SerializeField] private GameObject goal2; // 목표 오브젝트 2
    private Queue<Transform> goalQueue; // 목표 위치 저장 큐
    private int activeGoalCnt = 0; // 현재 활성화된 목표 오브젝트 개수

    private void Awake()
    {
            // 리스트로 받아온 데이터를 큐 형태로 사용
            goalQueue = new Queue<Transform>(goals);
    }

    private void OnEnable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ReadyTimer += SetGoalObjects;
            GameManager.instance.ReturnPos += ReturnTrainingState;
        }
    }

    private void OnDisable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ReadyTimer -= SetGoalObjects;
            GameManager.instance.ReturnPos -= ReturnTrainingState;
        }
    }

    // Goal 오브젝트 위치 설정 메소드
    private void SetGoalObjects()
    {
        // 4초 대기후 목표 오브젝트 활성화
        Invoke("SpawnnextObjects", 4f);
    }

    // 활성화된 목표 개수 확인 메소드
    public void OnGoalDeactivated()
    {
        activeGoalCnt--;

        if (activeGoalCnt <= 0)
        {
            // 둘 다 비활성화 완료 → 다음 페어 스폰
            SpawnnextObjects();
        }
    }

    // 목표 오브젝트 활성화 메소드
    private void SpawnnextObjects()
    {
        // 모든 목표 오브젝트 클리어시 타이머 정지 실행
        if(goalQueue.Count < 2)
        {
            GameManager.instance.RequestEndTimer();
            return;
        }

        // 목표 오브젝트 위치 설정 실행
        StartCoroutine(SetGoalObjects_co(0.5f));
    }

    // 목표 오브젝트 위치 설정 코루틴
    private IEnumerator SetGoalObjects_co(float dealy)
    {
        Debug.Log("목표 오브젝트 위치 설정했어!");
        // 잠시 대기
        yield return new WaitForSeconds(dealy);

        // 목표 오브젝트 위치 설정 및 활성화
        goal1.transform.position = goalQueue.Dequeue().position;
        goal2.transform.position = goalQueue.Dequeue().position;
        
        goal1.SetActive(false);
        goal2.SetActive(false);
        goal1.SetActive(true);
        goal2.SetActive(true);

        // 목표 오브젝트 활성화 개수 초기화
        activeGoalCnt = 2;
    }

    // 훈련 상태 초기화
    private void ReturnTrainingState()
    {
        // 목표 오브젝트 비활성화
        goal1.SetActive(false);
        goal2.SetActive(false);

        // 상태 초기화
        goalQueue = new Queue<Transform>(goals);
    }
}
