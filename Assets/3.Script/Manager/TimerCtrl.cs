using System.Collections;
using TMPro;
using UnityEngine;

public class TimerCtrl : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    private bool isTimeAttack = false; // 타임어택 시작 여부
    private float timer = 0f;

    private void OnEnable()
    {
        if (GameManager.instance != null)
        {
            isTimeAttack = false;
            GameManager.instance.ReadyTimer += OnReadyTimer;
            GameManager.instance.EndTimer += OnEndTimer;
            GameManager.instance.ReturnPos += ResetTrainingState;
        }
    }
    private void OnDisable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ReadyTimer -= OnReadyTimer;
            GameManager.instance.EndTimer -= OnEndTimer;
            GameManager.instance.ReturnPos -= ResetTrainingState;
        }
    }

    private void Update()
    {
        if (isTimeAttack)
        {
            // 시간을 TimeSpan 형태로 적용(해당 float를 시간 계산하기 좋은 형태로 바꿔줘)
            timer += Time.deltaTime;
            System.TimeSpan ts = System.TimeSpan.FromSeconds(timer);
            timerText.text = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
        }
    }

    // 타이머 카운트 다운 메소드
    private void OnReadyTimer()
    {
        if (!isTimeAttack) 
        {
            // 타임어택 준비 시작
            StartCoroutine(DeayStartTimer_co());
        }
    }

    // 타이머 카운트 다운 메소드
    private void OnEndTimer()
    {
        if (isTimeAttack)
        {
            // 타임어택 종료 시작
            isTimeAttack = false;
            StartCoroutine(EndTimer_co());
        }
    }


    // 타이머 시작 코루틴
    private IEnumerator DeayStartTimer_co()
    {
        Color originColor = timerText.color;

        // 일정 시간 대기
        yield return new WaitForSeconds(4f); // 4초 대기

        isTimeAttack = true;
        timerText.gameObject.SetActive(true);

        // 깜빡이는 효과 추가
        for (int i=0; i<4; i++)
        {
            timerText.color = new Color(originColor.r, originColor.g, originColor.b, 1f);
            yield return new WaitForSeconds(0.2f);

            timerText.color = new Color(originColor.r, originColor.g, originColor.b, 0f);
            yield return new WaitForSeconds(0.1f);
        }
        timerText.color = originColor;
    }

    // 타이머 종료 코루틴
    private IEnumerator EndTimer_co()
    {
        Color originColor = timerText.color;

        isTimeAttack = false;

        // 깜빡이는 효과 추가
        for (int i = 0; i < 4; i++)
        {
            timerText.color = new Color(originColor.r, originColor.g, originColor.b, 1f);
            yield return new WaitForSeconds(0.2f);

            timerText.color = new Color(originColor.r, originColor.g, originColor.b, 0f);
            yield return new WaitForSeconds(0.1f);
        }
        timerText.color = originColor;
        
        // 3초 대기
        yield return new WaitForSeconds(3f);

        // 타이머 비활성화
        timerText.gameObject.SetActive(false);

        // 복귀 함수
        GameManager.instance.RequestReturnPos();
    }

    // 훈련 상태 초기화 메소드
    private void ResetTrainingState()
    {
        timer = 0f;
        timerText.text = "00:00:00.00";
    }
}
