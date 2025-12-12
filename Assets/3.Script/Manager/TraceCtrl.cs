using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraceCtrl : MonoBehaviour
{
    public float traceDuration; // 잔상 유지기간
    private SpriteRenderer spriteRenderer;
    private Coroutine traceRoutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        spriteRenderer.color = new Color(1, 1, 1, 0.5f);

        if(traceRoutine != null)
        {
            StopCoroutine(traceRoutine);
        }
        traceRoutine = StartCoroutine(ActiveAfterImage());
    }

    // 잔상 효과 메소드
    private IEnumerator ActiveAfterImage()
    {
        float elapsed = 0f;
        Color color = spriteRenderer.color;
        
        while(elapsed < traceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / traceDuration;

            float a = Mathf.Lerp(color.a, 0f, t);
            spriteRenderer.color = new Color(1, 1, 1, a);
            yield return null;
        }

        // 시간 종료시 코루틴 정리
        traceRoutine = null;
        gameObject.SetActive(false);
    }
}
