using System.Collections;
using UnityEngine;

public class GateCtrl : MonoBehaviour
{
    Animator animator;
    BoxCollider2D col2D;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        col2D = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ReadyTimer += OpenGate;
            GameManager.instance.ReturnPos += ReturnTrainingState;
        }
    }
    private void OnDisable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ReadyTimer -= OpenGate;
            GameManager.instance.ReturnPos -= ReturnTrainingState;
        }
    }

    // 게이트 조작 메소드
    private void OpenGate()
    {
        // 대기후 게이트 활성화
        StartCoroutine(ReadyAndOpen_co());
    }

    // 게이트 활성화 코루틴
    private IEnumerator ReadyAndOpen_co()
    {
        yield return new WaitForSeconds(1f);

        // 대기 모션 실행
        animator.SetTrigger("Ready");
        yield return new WaitForSeconds(3f);

        // 게이트 열림 및 콜라이더 비활성화
        AudioManager.instance.PlaySFX_Obj(AudioManager.instance.gateOpenSFX);
        col2D.enabled = false;
    }

    // 훈련 상태 초기화
    private void ReturnTrainingState()
    {
        // 애니메이션 및 콜라이더 초기화
        animator.SetTrigger("Closing");
        col2D.enabled = true;
    }

    // 타임어택 출발 신호 재생 메소드
    public void PlayTimeBeep()
    {
        AudioManager.instance.PlaySFX_Obj(AudioManager.instance.gateWarningSFX);
    }
}
