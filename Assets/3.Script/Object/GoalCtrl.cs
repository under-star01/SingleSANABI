using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalCtrl : MonoBehaviour
{
    [SerializeField] private GameObject vfx;
    Animator vfxAnimator;
    Animator animator;
    CircleCollider2D vfxCollider;

    [SerializeField] GoalQueueCtrl goalQueueCtrl; // 목표 오브젝트 컨트롤러
    private bool isCollidable = false; // 충돌 가능 여부
    private float rotationSpeed = 50f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        vfxAnimator = vfx.GetComponent<Animator>();
        vfxCollider = vfx.GetComponent<CircleCollider2D>();
    }

    private void OnEnable()
    {
        isCollidable = true;
        vfxCollider.enabled = true;
        animator.SetTrigger("Appear");
        AudioManager.instance.PlaySFX_Obj(AudioManager.instance.goalAppearSFX);
    }

    private void Update()
    {
        if (isCollidable)
        {
            vfx.transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 타겟 또는 바닥과 충돌했을 경우, 비활성화
        if(collision.gameObject.CompareTag("Target") && isCollidable)
        {
            isCollidable = false;
            vfxCollider.enabled = false;

            animator.SetTrigger("Damaged");
            vfxAnimator.SetTrigger("Damaged");

            goalQueueCtrl.OnGoalDeactivated();
        }
    }

    // 애니메이션 호출 메소드 -----------------------

    // vfx 콜라이더 활성화 메소드
    public void ActiveVfxCollider()
    {
        // vfx 활성화
        vfx.SetActive(true);
        isCollidable = true;
    }
}
