using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerEndCtrl : MonoBehaviour
{
    private bool isDetect = false;
    
    private void OnEnable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ReturnPos += ResetTrainingState;
        }
    }

    private void OnDisable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ReturnPos -= ResetTrainingState;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 타이머가 활성화되지 않은 상태일 경우에만 실행
        if (!isDetect)
        {
            Debug.LogWarning("감시병 충돌 확인했어용");
            // 타이머 실행
            isDetect = true;
            GameManager.instance.RequestEndTimer();
        }
    }

    // 훈련 상태 초기화 메소드
    private void ResetTrainingState()
    {
        isDetect = false;
    }
}
