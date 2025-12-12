using System.Collections;
using UnityEngine;

public class TargetCtrl : MonoBehaviour
{
    [SerializeField] private Transform returnPos;
    [SerializeField] private GameObject player;
    [SerializeField] private bool _isCatched = false;
    private Rigidbody2D rigid2D;
    private Animator animator;
    public bool isCatched
    {
        get => _isCatched;
        set
        {
            _isCatched = value;

            // 제압 해제시, 복귀 코루틴 실행
            if (!value)
            {
                OnReturnCool();
            }
        }
    }

    private Coroutine returnRoutine;
    public float returnTime;
    public bool canReturn = true;

    private void Awake()
    {
        rigid2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        // 제압 상태일 경우, 플레이어 위치로 위치 초기화
        if (isCatched)
        {
            transform.position = player.transform.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 목표 오브젝트와 충돌시 비활성화
        if (collision.gameObject.CompareTag("Goal"))
        {
            rigid2D.linearVelocity = Vector3.zero;
            animator.SetTrigger("Disappear");
        }
        // 플레이어와 충돌했을 경우
        else if (collision.gameObject.CompareTag("Player"))
        {
            animator.SetTrigger("Hit");
        }
    }

    // 기존 위치 복귀 메소드
    private void OnReturnCool()
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
        }
        returnRoutine = StartCoroutine(ReturnCool_co());
    }

    // 기존 위치 복귀 코루틴
    private IEnumerator ReturnCool_co()
    {
        // 복귀 시간 대기
        canReturn = false;
        yield return new WaitForSeconds(returnTime);

        // 기존 위치 복귀 실행
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, returnPos.position, elapsed);
            yield return null;
        }

        // 복귀 상태 초기화
        rigid2D.linearVelocity = Vector3.zero;
        transform.position = returnPos.position;
        canReturn = true;
    }

    // 애니메이션 호출 메소드 -----------------------

    // 오브젝트 비활성화 메소드
    public void DeActivateObject()
    {
        // 재생성 위치에서 다시 활성화
        gameObject.SetActive(false);
        gameObject.transform.position = returnPos.position;
        gameObject.SetActive(true);
    }

}
