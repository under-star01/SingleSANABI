using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEngine.UI.Image;

public class PlayerCtrl : MonoBehaviour
{
    #region 변수 목록
    // 플레이어 컴포넌트 연결 변수
    public GameObject weapon;
    private Rigidbody2D rigid2D;
    private Animator animator;
    private Animator weaponAnimator;
    private SpriteRenderer spriteRenderer;
    private DistanceJoint2D joint2D;
    private AudioSource audioSource;

    [Header("직접 연결 필요 변수")]
    public GameObject anchor;
    public GameObject chargeAttackAim;
    public SpriteRenderer FadeImage;
    public Transform savePoint;
    public RectTransform cursorPos;
    public LineRenderer line;

    [Header("플레이어 조작 관련 변수")]
    // 플레이어 이동 관련 변수
    public bool inputBlock = false;
    public float inputX;
    public float inputY;
    public float maxSpeed = 15f;
    public float speed;

    [Header("점프 관련 변수")]
    // 점프 관련 변수
    public int jump_Cnt = 0;
    public float jump_Force;
    public float jumpTimer = 0f;

    [Header("사슬 조작 관련 변수")]
    // 사슬 조작 관련 변수
    public float detectRange = 8f;
    public float maxChainDistance = 6f;
    public Vector3 defaultWeaponPos = new Vector3(-0.08f, -0.015f, 0);
    public Coroutine chargeAttackRoutine; // 실행중인 차지어택 코루틴

    private float _chainDistance;
    public float chainDistance
    {
        get => _chainDistance;

        set
        {
            _chainDistance = Mathf.Clamp(value, 0.5f, maxChainDistance);
        }
    }

    [Header("사슬 구현 관련 변수")]
    // 사슬 구현 관련 변수
    public List<Transform> chainList; // chain 저장 리스트
    public int maxChainCnt = 60;
    public float chainLength = 0.1f;

    [Header("잔상 구현 관련 변수")]
    // 잔상 구현 관련 변수 
    public List<SpriteRenderer> traceList; // 잔상 오브젝트 리스트
    public int traceActiveCnt; // 잔상 오브젝트 활성화 개수
    public float traceInterval;

    [Header("플레이어 상태 관련 변수")]
    // 플레이어 상태 관련 변수
    public PlayerState playerState = PlayerState.None;
    public bool isGrounded = false; // 착지 여부
    [SerializeField] private bool _isClimb = false; // 벽타기 여부
    public bool isClimb
    {
        get => _isClimb;
        set
        {
            // 변경값 적용
            _isClimb = value;

            // 벽타기 상태시, weapon 위치 변경
            if (_isClimb)
            {
                weapon.transform.localPosition = new Vector3(0, 0.02f, 0); // 사슬팔 위치 조절
                weapon.transform.localRotation = Quaternion.Euler(Vector3.zero); // 사슬팔 각도 초기화
            }
            // 벽타기 상태 해제시, weapon 위치 복구
            else
            {
                weapon.transform.localPosition = defaultWeaponPos;
            }
        }
    }
    [SerializeField] private bool _isWired = false; // 와이어 연결 여부
    public bool isWired
    {
        get => _isWired;
        set
        {
            // 변경값 적용
            _isWired = value;

            // 사슬 연결 상태시, weapon 위치 변경
            if (_isWired)
            {
                weapon.transform.localPosition = new Vector3(-0.06f, 0.04f, 0); // 사슬팔 위치 조절
            }
            // 사슬 연결 상태 해제시, weapon 위치 복구
            else
            {
                // 벽타기 상태가 아닐때만 초기화
                if (!isClimb)
                {
                    weapon.transform.localPosition = defaultWeaponPos;
                }
            }
        }
    }
    public bool isCeiling = false; // 천장 연결 여부
    public bool canAccel = true; // 가속 스킬 사용 여부
    public bool canChargeAttack = true; // 가속 스킬 사용 여부
    public bool isInteractRange = false; // 상호작용 버튼 여부(E버튼)

    [Header("플레이어 스킬 관련 변수")]
    // 플레이어 스킬 관련 변수
    public bool isCharging = false;
    public bool isCatched = false;
    public bool isDash = false;
    public float chargeTimer = 0f;
    public float maxChargeTime = 1.5f;
    public float maxDashTime = 1.5f;
    public GameObject target = null;
    public GameObject chargeDetect; // 차지 어택 탐지 오브젝트
    public GameObject hitBox; // 차지 어택시 충돌 확인할 오브젝트
    private Vector3 chargeDetect_ScaleMin = Vector3.one * 5; // 최소 Scale값
    private Vector3 chargeDetect_ScaleMax = Vector3.one * 10; // 최대 Scale값
    private Coroutine dashCoolCoroutine; // 대시 쿨타임 코루틴

    [Header("그외 조작 변수")]
    // 그외 조작 변수
    public LayerMask detectLayerMask; // 충돌을 확인할 레이어
    RaycastHit2D hit; // 현재 충돌한 collider
    GameObject catchObejct = null; // 제압한 오브젝트

    #endregion

    private void Awake()
    {
        weapon = transform.GetChild(0).gameObject;

        animator = GetComponent<Animator>();
        weaponAnimator = weapon.GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigid2D = GetComponent<Rigidbody2D>();
        joint2D = GetComponent<DistanceJoint2D>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // Anchor 및 UI 정리
        Cursor.visible = false;
        anchor.SetActive(false);
        line.enabled = false;

        // PoolManager의 사슬 리스트와 연결
        chainList = PoolManager.instance.chainList;

        // PoolManager의 잔상 리스트와 연결
        traceList = PoolManager.instance.traceList;
    }

    private void OnEnable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ReturnPos += ActiveExitEffect;
        }
    }

    private void OnDisable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ReturnPos -= ActiveExitEffect;
        }
    }

    private void FixedUpdate()
    {
        if (inputBlock || isCharging) return;

        // 대시 공격의 경우
        if (isDash)
        {
            rigid2D.linearVelocityX = inputX * 2f;
            rigid2D.linearVelocityY = inputY * 2f;
            
            return;
        }

        // 와이어o, 바닥x
        if (isWired && !isGrounded)
        {
            rigid2D.AddForce(new Vector2(inputX, 0f) * 3);
        }
        // 와이어x, 바닥x
        else if (!isWired && !isGrounded)
        {
            // 천장 연결시
            if (isCeiling)
            {
                rigid2D.linearVelocityX = inputX * speed;
            }
            // 공중 이동시
            else
            {
                rigid2D.linearVelocityX += inputX * 3 * Time.deltaTime;
            }
        }
        // 바닥o
        else
        {
            rigid2D.linearVelocityX = inputX * speed;
        }

        // 벽타기 or 천장 연결시
        if (isClimb || isCeiling)
        {
            rigid2D.linearVelocityY = inputY * speed;
        }

        rigid2D.linearVelocity = Vector2.ClampMagnitude(rigid2D.linearVelocity, maxSpeed);
    }

    private void Update()
    {
        // 사슬 연결 표현 갱신
        ChainVisualUpdate();

        // 마우스 거리 계산 및 이동 확인
        MouseMove();

        if (inputBlock) return;

        // 플레이어 이동 확인
        Move();

        // Space 버튼 입력 확인
        SpaceButtonClick();

        // 좌클릭 입력 확인
        LeftMouseClick();

        // Shift 버튼 입력 확인
        ShiftButtonClick();

        // 상호작용(E) 버튼 입력 확인
        InteractButtonClick();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDash) return;

        // 바닥과 충돌했을 경우
        if (collision.gameObject.CompareTag("Floor"))
        {
            jump_Cnt = 0;

            // 위에서 아래로 충돌시 (착지)
            if (collision.contacts[0].normal.y > 0.7f)
            {
                isGrounded = true;
                rigid2D.linearVelocity = Vector2.zero;

                animator.SetTrigger("Landing");
                weaponAnimator.SetTrigger("Landing");
            }
            // 옆으로 충돌시 (벽타기)
            else if (collision.contacts[0].normal.y <= 0.7f && collision.contacts[0].normal.y >= -0.7f)
            {
                // 바라보는 방향 초기화
                if (collision.contacts[0].normal.x > 0)
                {
                    gameObject.transform.localScale = new Vector3(-1, 1, 1);
                }
                else
                {
                    gameObject.transform.localScale = new Vector3(1, 1, 1);
                }

                isClimb = true;
                rigid2D.linearVelocity = Vector2.zero;
                rigid2D.gravityScale = 0f;
                playerState = PlayerState.Climbing;
                animator.SetTrigger("Climbing");
                weaponAnimator.SetTrigger("Climbing");
            }
            // 아래에서 위로 충돌시 (천장 타기)
            else
            {
                isCeiling = true;
                AudioManager.instance.PlayDefaultSFX(AudioManager.instance.ceilingSFX);
                rigid2D.linearVelocity = Vector2.zero;
                rigid2D.gravityScale = 0f;
                playerState = PlayerState.Ceiling;
                animator.SetTrigger("Ceiling");
                weaponAnimator.SetTrigger("Ceiling");
                weapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
        }
        // DeadLine 충돌했을 경우
        else if (collision.gameObject.CompareTag("DeadLine"))
        {
            transform.position = savePoint.position;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (isDash) return;

        // 바닥에서 빠져나올 경우
        if (collision.gameObject.CompareTag("Floor"))
        {
            // 착지상태에서 벽타기 -> 변수만 전환
            if (isGrounded && isClimb)
            {
                isGrounded = false;
                return;
            }

            // 상태 초기화 및 애니메이션 실행
            isGrounded = false;
            isClimb = false;
            isCeiling = false;
            rigid2D.gravityScale = 1f;
            jump_Cnt++; // fall 상태 -> 점프 제한

            animator.SetTrigger("Falling");
            weaponAnimator.SetTrigger("Falling");
            playerState = PlayerState.None;

            // 위로 빠져나올 경우, 살짝 힘을 더해줌
            if (inputY > 0)
            {
                rigid2D.linearVelocity = Vector2.zero;
                rigid2D.AddForce(Vector2.up * 150f);
            }
        }
    }

    // 연결된 오브젝트 TriggerEnter 감지 메소드
    public void OnPlayerTriggerEnter(TriggerObject.Role role, Collider2D collision, string tag)
    {
        if (isDash) return;

        switch (role)
        {
            // ChargeDetect에서 탐지
            case TriggerObject.Role.ChargeDetect:

                // Target과 충돌한 경우
                if (tag == "Target")
                {
                    // 차지 어택 타겟 설정
                    target = collision.gameObject;
                }
                break;

            // AttackHitBox에서 탐지
            case TriggerObject.Role.HitBox:

                // Target과 충돌한 경우
                if(tag == "Target")
                {
                    // 차지어택 중 피격시
                    if (playerState == PlayerState.ChargeAttack)
                    {
                        AudioManager.instance.PlaySFX(AudioManager.instance.chargeAttackSFX);
                     
                        // ChargeAttack 상태 복구
                        playerState = PlayerState.None;
                        ChargeAttackStateBack();
                        collision.gameObject.GetComponent<TargetCtrl>().isCatched = false; // 복귀 코루틴 호출

                        // 플레이어 반작용 구현
                        Vector3 dir = (transform.position - collision.transform.position).normalized;
                        rigid2D.linearVelocity = Vector2.zero;
                        rigid2D.AddForce(dir * 100f);
                        collision.gameObject.GetComponent<Rigidbody2D>().AddForce(-dir * 300f);
                    }
                }
                // Anchor와 충돌한 경우
                else if (tag == "Anchor")
                {
                    // 사슬 연결 상태 초기화
                    inputBlock = false;
                    isWired = false;
                    joint2D.enabled = false;
                    anchor.SetActive(false);
                    animator.SetBool("isWired", false);
                    weaponAnimator.SetBool("isWired", false);
                }
                // ReturnTitle에 들어간 경우
                else if (tag == "ReturnTitle")
                {
                    isInteractRange = true;
                }
                break;
        }
    }

    // 연결된 오브젝트 TriggerExit 감지 메소드
    public void OnPlayerTriggerExit(TriggerObject.Role role, Collider2D collision, string tag)
    {
        if (isDash) return;

        switch (role)
        {
            // ChargeDetect에서 탐지
            case TriggerObject.Role.ChargeDetect:
                break;

            // AttackHitBox에서 탐지
            case TriggerObject.Role.HitBox:

                // ReturnTitle에서 빠져나온 경우
                if (tag == "ReturnTitle")
                {
                    isInteractRange = false;
                }
                break;
        }
    }

    // 마우스 이동 메소드
        private void MouseMove()
    {
        // 차지 or 대시 중일 때는 입력x
        if (isCharging) return;

        // 커서 위치 초기화
        cursorPos.position = Input.mousePosition;

        // 마우스 위치 및 방향 계산
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(cursorPos.position);
        Vector2 dir = (mouseWorld - (Vector2)transform.position).normalized;

        // 마우스 방향으로 RayCast 발사
        hit = Physics2D.Raycast(transform.position, dir, detectRange, detectLayerMask);

        // 현재 연결 가능한 벽 안내선 표시
        if (hit.collider != null && hit.collider.gameObject.CompareTag("Floor") && !isWired)
        {
            line.enabled = true;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, hit.point);
        }
        else if (hit.collider != null && hit.collider.gameObject.CompareTag("Target") && !isWired)
        {
            line.enabled = true;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, hit.point);
        }
        // 없을 경우, 선 숨기기
        else
        {
            line.enabled = false;
        }
    }

    // 플레이어 이동 메소드
    private void Move()
    {
        // 차지 중일 때는 입력x
        if (isCharging) return;

        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");
        jumpTimer += Time.deltaTime;

        animator.SetFloat("VelocityY", rigid2D.linearVelocityY);
        animator.SetFloat("JumpTimer", jumpTimer);

        weaponAnimator.SetFloat("VelocityY", rigid2D.linearVelocityY);
        weaponAnimator.SetFloat("JumpTimer", jumpTimer);

        // 대시 중일 때, 속도 및 방향 설정
        if (isDash)
        {
            if (inputX > 0)
            {
                speed = 2f;
                gameObject.transform.localScale = new Vector3(1, 1, 1);
            }
            else if (inputX < 0)
            {
                speed = 2f;
                gameObject.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                speed = 0f;
            }

            return;
        }

        // 사슬 연결시, 연결 위치에 따른 사슬팔 각도 조절
        if (isWired)
        {
            Vector2 weaponDir = anchor.transform.position - weapon.transform.position;
            float weaponAngle = Mathf.Atan2(weaponDir.y, weaponDir.x) * Mathf.Rad2Deg;

            if (gameObject.transform.localScale.x > 0)
            {
                weapon.transform.localRotation = Quaternion.Euler(0, 0, weaponAngle);
            }
            else
            {
                weapon.transform.localRotation = Quaternion.Euler(0, 0, 180 - weaponAngle);
            }
        }

        if (isGrounded)
        {
            // 사슬 연결o / 바닥 이동시
            if (isWired)
            {
                // 걸어서 이동 (우측)
                if (inputX > 0)
                {
                    // 속도 설정
                    speed = 2f;
                    // 모션 설정
                    gameObject.transform.localScale = new Vector3(1, 1, 1);
                    animator.SetBool("isMoving", true);
                    weaponAnimator.SetBool("isMoving", true);
                }
                // 걸어서 이동 (좌측)
                else if (inputX < 0)
                {
                    // 속도 설정
                    speed = 2f;
                    // 모션 설정
                    gameObject.transform.localScale = new Vector3(-1, 1, 1);
                    animator.SetBool("isMoving", true);
                    weaponAnimator.SetBool("isMoving", true);
                }
                // 걸어서 이동 (정지)
                else
                {
                    // 속도 설정
                    speed = 0f;
                    // 모션 설정
                    animator.SetBool("isMoving", false);
                    weaponAnimator.SetBool("isMoving", false);
                }
            }
            // 사슬 연결x / 바닥 이동시
            else
            {
                // 뛰어서 이동 (우측)
                if (inputX > 0)
                {
                    // 속도 설정
                    speed = 4f;
                    // 모션 설정
                    gameObject.transform.localScale = new Vector3(1, 1, 1);
                    animator.SetBool("isMoving", true);
                    weaponAnimator.SetBool("isMoving", true);
                }
                // 뛰어서 이동 (좌측)
                else if (inputX < 0)
                {
                    // 속도 설정
                    speed = 4f;
                    // 모션 설정
                    gameObject.transform.localScale = new Vector3(-1, 1, 1);
                    animator.SetBool("isMoving", true);
                    weaponAnimator.SetBool("isMoving", true);
                }
                // 뛰어서 이동 (정지)
                else
                {
                    // 속도 설정
                    speed = 0f;
                    // 모션 설정
                    animator.SetBool("isMoving", false);
                    weaponAnimator.SetBool("isMoving", false);
                }
            }
        }
        else
        {
            // 사슬 연결o / 공중 상태
            if (isWired)
            {
                // 오른쪽 회전 
                if (inputX > 0)
                {
                    // 속도 설정
                    speed = 4f;
                    // 모션 설정
                    gameObject.transform.localScale = new Vector3(1, 1, 1);
                    animator.SetBool("isMoving", true);
                    weaponAnimator.SetBool("isMoving", true);
                }
                // 왼쪽 회전
                else if (inputX < 0)
                {
                    // 속도 설정
                    speed = 4f;
                    // 모션 설정
                    gameObject.transform.localScale = new Vector3(-1, 1, 1);
                    animator.SetBool("isMoving", true);
                    weaponAnimator.SetBool("isMoving", true);
                }
                else
                {
                    animator.SetBool("isMoving", false);
                    weaponAnimator.SetBool("isMoving", false);
                }
            }
            // 사슬 연결x / 공중 상태 
            else
            {
                // 오른쪽 회전 
                if (inputX > 0)
                {
                    // 속도 설정
                    speed = 4f;
                    // 모션 설정
                    gameObject.transform.localScale = new Vector3(1, 1, 1);
                    animator.SetBool("isMoving", true);
                    weaponAnimator.SetBool("isMoving", true);
                }
                // 왼쪽 회전
                else if (inputX < 0)
                {
                    // 속도 설정
                    speed = 4f;
                    // 모션 설정
                    gameObject.transform.localScale = new Vector3(-1, 1, 1);
                    animator.SetBool("isMoving", true);
                    weaponAnimator.SetBool("isMoving", true);
                }
                else
                {
                    animator.SetBool("isMoving", false);
                    weaponAnimator.SetBool("isMoving", false);
                }
            }
        }

        // 벽타기 이동 조작
        if (isClimb)
        {
            // 위로 이동
            if (inputY > 0)
            {
                // 속도 설정
                speed = 4f;

                // 모션 설정
                animator.SetInteger("ClimbState", 1);
                weaponAnimator.SetInteger("ClimbState", 1);
            }
            // 아래로 이동
            else if (inputY < 0)
            {
                // 속도 설정
                speed = 4f;

                // 모션 설정
                animator.SetInteger("ClimbState", -1);
                weaponAnimator.SetInteger("ClimbState", -1);
            }
            else
            {
                // 속도 설정
                speed = 0f;

                // 모션 설정
                animator.SetInteger("ClimbState", 0);
                weaponAnimator.SetInteger("ClimbState", 0);
            }
        }
    }

    // 플레이어 점프 메소드
    private void SpaceButtonClick()
    {
        // 차지 중 or 대시 중일 때는 입력x
        if (isCharging || isDash || isClimb || isCeiling) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isWired)
            {
                // 와이어 회수 실행
                WireAction_Wind();
            }
            else
            {
                // 점프 입력 확인 
                Jump();
            }
        }
        // 낙하 속도 처리
        else if (Input.GetKeyUp(KeyCode.Space) && rigid2D.linearVelocity.y > 0)
        {
            rigid2D.linearVelocity *= 0.6f;
        }
    }

    // 플레이어 좌클릭 확인 메소드
    private void LeftMouseClick()
    {
        // 차지 중일 때는 입력x
        if (isCharging) return;

        // 대시 중이 아닐 경우
        if (!isDash)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ConncectWire();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                DisconnectWire();
            }
        }
        // 대시 중일 경우
        else
        {
            if (Input.GetMouseButtonUp(0))
            {
                StopCoroutine(dashCoolCoroutine); // 대시 유지시간 확인 코루틴 종료
                StartCoroutine(Dash_End());
            }
        }
    }

    // 플레이어 Shift 버튼 확인 메소드
    private void ShiftButtonClick()
    {
        // 대시 중일 떄는 입력x
        if (isDash) return;

        // 사슬 연결 중 shift 입력 -> 가속
        if (isWired)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                WireAction_Accel();
            }
        }
        // 사슬 연결x 상태에서 shift 입력 -> 강공격 실행
        else
        {
            // 사용 가능 상태가 아닐 경우, return
            if (!canChargeAttack) return;

            // 차지 어택 충전 시작
            if (Input.GetKeyDown(KeyCode.LeftShift) && !isCharging)
            {
                chargeAttackRoutine = StartCoroutine(ChargeAttack_Ready_co());
            }
            // 차지 어택 충전 중
            else if (Input.GetKey(KeyCode.LeftShift) && isCharging)
            {
                ChargeAttack_Charging();
            }
            // 차지 어택 실행
            else if (Input.GetKeyUp(KeyCode.LeftShift) && isCharging)
            {
                // 실행중인 충전 코루틴이 있을경우 종료
                if (chargeAttackRoutine != null)
                {
                    StopCoroutine(chargeAttackRoutine);
                }

                ChargeAttack_Active();
            }
        }
    }
    
    // 상호작용 버튼(E) 확인 메소드
    private void InteractButtonClick()
    {
        // 복귀 포인트에서 상호작용 키(E)를 누를경우, 타이틀 씬으로 복귀
        if (Input.GetKeyDown(KeyCode.E) && isInteractRange)
        {
            Cursor.visible = true;
            GameManager.instance.LoadScene("Title");
        }
    }

    // 잔상 표현 메소드
    private IEnumerator TraceVisualUpdate(float traceInterval)
    {
        WaitForSeconds delay = new WaitForSeconds(traceInterval);
        int cnt = 0;

        // 리스트에 사용 가능한 잔상 오브젝트 사용
        foreach (SpriteRenderer trace in traceList)
        {
            // 플레이어 특정 상태 변경시 중지 (벽타기, 천장 연결, 차지어택 준비)
            if (!(playerState == PlayerState.None || playerState == PlayerState.ChargeAttack)) yield break;

            // 정해진 개수만큼 잔상 활성화
            if (!trace.gameObject.activeSelf && cnt < traceActiveCnt)
            {
                // 잔상 오브젝트 상태 초기화
                trace.gameObject.SetActive(true);
                trace.sprite = spriteRenderer.sprite;
                trace.transform.position = transform.position;

                cnt++;
                yield return delay;
            }
        }
    }

    // 사슬 연결 표현 메소드
    private void ChainVisualUpdate()
    {
        // 사슬 연결x -> 비활성화 -> 이거 좀 있다가 프로퍼티로 isWire = false될때 메소드 실행해서 관리하자.
        if (!isWired)
        {
            foreach (Transform chain in chainList)
            {
                chain.gameObject.SetActive(false);
            }

            return;
        }

        // 사슬이 바라볼 회전값 계산
        Vector3 distance = (anchor.transform.position - weapon.transform.position); // 목적지를 바라보는 벡터 생성
        Vector3 dir = distance.normalized;
        Quaternion rot = Quaternion.FromToRotation(Vector3.right, dir); // 해당 벡터와 right 벡터의 차이를 각도로 변환 (right 벡터의 반시계로 각도가 증가하니까!)

        // 사슬 활성화 관리
        int needCnt = Mathf.FloorToInt(distance.magnitude / chainLength); // 필요 사슬 개수 계산(소숫점 내림)
        needCnt = Mathf.Clamp(needCnt, 0, maxChainCnt); // 최대거리일 때 사슬 개수: 35개 // 최대 사슬 개수: 60개

        for (int i = 0; i < maxChainCnt; i++)
        {
            if (i < needCnt)
            {
                chainList[i].gameObject.SetActive(true);
                Vector3 pos = weapon.transform.position + dir * chainLength * i;
                chainList[i].position = pos;
                chainList[i].rotation = rot;
            }
            else
            {
                chainList[i].gameObject.SetActive(false);
            }
        }
    }

    // 플레이어 점프 메소드
    private void Jump()
    {
        if (jump_Cnt < 1)
        {
            AudioManager.instance.PlaySFX(AudioManager.instance.jumpSFX);
            jumpTimer = 0f;
            rigid2D.linearVelocity = Vector2.zero;
            rigid2D.AddForce(new Vector2(0, jump_Force));
        }
    }

    // 사슬 연결 메소드
    private void ConncectWire()
    {
        // 바닥과 연결시
        if (hit && hit.collider.gameObject.CompareTag("Floor"))
        {
            AudioManager.instance.PlaySFX(AudioManager.instance.shootSFX);

            // 연결 위치로 바라보는 방향 전환 
            if (hit.transform.position.x - transform.position.x > 0)
            {
                gameObject.transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                gameObject.transform.localScale = new Vector3(-1, 1, 1);
            }

            // 사슬 연결 상태 초기화
            isWired = true;
            canAccel = true;
            anchor.SetActive(true);
            animator.SetBool("isWired", true);
            weaponAnimator.SetBool("isWired", true);
            weaponAnimator.SetTrigger("Shooting");
            AudioManager.instance.PlaySFX(AudioManager.instance.anchorSFX);

            // 충돌시, 해당 오브젝트 위치로 Anchor 이동 및 각도 변경
            anchor.transform.position = hit.point;

            Vector2 annchorDir = anchor.transform.position - transform.position;
            float anchorAngle = Mathf.Atan2(annchorDir.y, annchorDir.x) * Mathf.Rad2Deg;
            anchor.transform.rotation = Quaternion.Euler(0, 0, anchorAngle);

            // 사슬 길이 초기화
            chainDistance = Vector2.Distance(anchor.transform.position, transform.position) - 0.3f; // 살짝 뜰 수 있도록, 길이를 조금 작게 설정
            joint2D.distance = chainDistance;

            // 연결 컴포넌트 활성화
            joint2D.enabled = true;
        }
        // 상호작용 오브젝트와 연결시
        else if (hit && hit.collider.gameObject.CompareTag("Target"))
        {
            // 연결 위치로 바라보는 방향 전환 
            if (hit.transform.position.x - transform.position.x > 0)
            {
                gameObject.transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                gameObject.transform.localScale = new Vector3(-1, 1, 1);
            }

            // 대시어택 실행 코루틴 실행
            StartCoroutine(Dash_Start_co(hit.collider.gameObject));
        }
    }

    // 사슬 연결 해제 메소드
    private void DisconnectWire()
    {
        // 사슬 연결 상태 초기화
        weapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
        isWired = false;
        joint2D.enabled = false;
        anchor.SetActive(false);
        animator.SetBool("isWired", false);
        weaponAnimator.SetBool("isWired", false);

        // 일반 회전시 속도 보정
        if (canAccel)
        {
            rigid2D.linearVelocity *= 1.5f;
        }

        // 와이어 해제에 따른 감속
        rigid2D.linearVelocity = new Vector2(rigid2D.linearVelocity.x * 0.4f, rigid2D.linearVelocity.y * 0.7f);
    }

    // 사슬 회전 가속 메소드
    private void WireAction_Accel()
    {
        if (canAccel)
        {
            AudioManager.instance.PlaySFX(AudioManager.instance.accelSFX);

            canAccel = false;

            if (rigid2D.linearVelocityX > 0)
            {
                rigid2D.AddForce(Vector2.down * 100f);
                rigid2D.AddForce(Vector2.right * 450f);
            }
            else
            {
                rigid2D.AddForce(Vector2.down * 100f);
                rigid2D.AddForce(Vector2.left * 450f);
            }

            // 잔상 표시 실행
            StartCoroutine(TraceVisualUpdate(traceInterval));
        }
    }

    // 사슬 회수 및 이동 메소드
    private void WireAction_Wind()
    {
        AudioManager.instance.PlaySFX(AudioManager.instance.windSFX);
        
        // 사슬 회수
        joint2D.distance = 0f;
        inputBlock = true;
    }

    // 대시어택 시작 코루틴
    private IEnumerator Dash_Start_co(GameObject hit)
    {
        // 상태 초기화 및 애니메이션 길이만큼 대기
        inputBlock = true;
        animator.SetTrigger("Dash");
        AudioManager.instance.PlaySFX(AudioManager.instance.dashSFX);
        dashCoolCoroutine = StartCoroutine(Dash_Cool()); // 대시 유지시간 쿨타임 시작

        rigid2D.linearVelocity = Vector2.zero;
        hit.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        rigid2D.gravityScale = 0f;
        yield return new WaitForSeconds(0.25f);

        // 상태 초기화 및 제압 변수 활성화
        inputBlock = false;
        isDash = true;
        hitBox.SetActive(false);
        FadeImage.color = new Color(0.35f, 0.35f, 0.35f, 0.2f);
        
        catchObejct = hit;
        catchObejct.GetComponent<TargetCtrl>().isCatched = true;

        animator.SetTrigger("DashHold");
        weaponAnimator.SetTrigger("DashHold");

        // 위치 이동 및 사용자 입력 활성화
        transform.position = hit.transform.position;

        // 대기중 마우스가 떼어져 있을 경우, 종료 함수 실행
        if (!Input.GetMouseButton(0))
        {
            StartCoroutine(Dash_End());
        }
    }

    // 대시어택 최대 유지 시간 확인 코루틴
    private IEnumerator Dash_Cool()
    {
        // 최대 지속 시간까지 대기
        isCatched = true;
        yield return new WaitForSeconds(maxDashTime);

        // 대시 종료 코루틴 실행
        StartCoroutine(Dash_End());
    }

    // 대시어택 종료 코루틴
    private IEnumerator Dash_End()
    {
        // 제압 상태일 때만 실행 (중복 실행x)
        if (!isCatched) yield break;

        // 일정 시간 대기
        isCatched = false;
        inputBlock = true;
        rigid2D.linearVelocity = Vector2.zero;

        // 마우스 위치로 추가 이동 방향 설정
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouseWorld - (Vector2)transform.position).normalized;

        yield return new WaitForSeconds(0.25f);

        // 대시 상태 초기화
        AudioManager.instance.PlaySFX(AudioManager.instance.excDashSFX);
        inputBlock = false;
        isGrounded = false;
        isClimb = false;
        isCeiling = false;
        rigid2D.gravityScale = 1f;
        FadeImage.color = new Color(1, 1, 1, 0);
        playerState = PlayerState.None;
        rigid2D.linearVelocity = Vector2.zero;
        hitBox.SetActive(true);

        // 애니메이션 적용
        animator.SetTrigger("DashEnd");
        weaponAnimator.SetTrigger("DashEnd");

        // Target의 제압 변수 비활성화 및 정리
        catchObejct.GetComponent<TargetCtrl>().isCatched = false;
        catchObejct = null;

        // 추가 이동 실행
        rigid2D.AddForce(dir * 350f);

        // 대시 상태 해제
        isDash = false; 
    }

    // 차지어택 실행 준비 메소드
    private IEnumerator ChargeAttack_Ready_co()
    {
        isCharging = true;
        target = null;

        // 착지 상태의 경우, 살짝 위로 힘을 줌
        if (isGrounded)
        {
            rigid2D.AddForce(Vector3.up * 100f);
            yield return new WaitForSeconds(0.05f);
        }
        // 천장 연결 상태의 경우, 살짝 아래로 힘을 줌
        else if (isCeiling)
        {
            rigid2D.AddForce(Vector3.down * 100f);
            yield return new WaitForSeconds(0.05f);
        }
        // 벽타기 상태의 경우, 살짝 옆으로 힘을 줌
        else if (isClimb)
        {
            if (transform.localScale.x > 0)
            {
                rigid2D.AddForce(Vector3.left * 100f);
            }
            else
            {
                rigid2D.AddForce(Vector3.right * 100f);
            }
            yield return new WaitForSeconds(0.05f);
        }

        // 상태 초기화 및 애니메이션 실행

        chargeTimer = 0f;
        speed = 0f;
        rigid2D.gravityScale = 0f;

        weapon.transform.localPosition = Vector3.zero; // 사슬팔 위치 조절
        rigid2D.linearVelocity = Vector2.zero;

        cursorPos.gameObject.SetActive(false);
        animator.SetTrigger("ChargeAttackReady");
        weaponAnimator.SetTrigger("ChargeAttackReady");
        playerState = PlayerState.ChargeAttackReady;

        // 탐지 범위 활성화
        chargeDetect.transform.localScale = chargeDetect_ScaleMin;
        chargeDetect.SetActive(true);
    }

    // 차지어택 실행 충전중 메소드
    private void ChargeAttack_Charging()
    {
        // Mathf.Clamp01()는 안에 있는 값을 0~1사이로 제한하는 함수! / Lerp 사용을 위해 변화량 계산!
        // 정수끼리 나눌때는 몫만 나오지만, 실수끼리 나눌때는 정상적으로 나눗셈 값이 계산됨!
        chargeTimer += Time.deltaTime;
        float t = Mathf.Clamp01(chargeTimer / maxChargeTime);

        Vector3 targetScale = Vector3.Lerp(chargeDetect_ScaleMin, chargeDetect_ScaleMax, t);
        chargeDetect.transform.localScale = targetScale;

        // 탐지된 타겟이 있을 경우, 안내선 표시
        if (target != null)
        {
            chargeAttackAim.transform.position = target.transform.position;
            chargeAttackAim.SetActive(true);

            line.enabled = true;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, target.transform.position);
        }
        // 없을 경우, 선 숨기기
        else
        {
            line.enabled = false;
        }
    }

    // 차지어택 실행 메소드
    private void ChargeAttack_Active()
    {
        // 차지 상태 초기화
        isCharging = false;
        playerState = PlayerState.ChargeAttack;

        cursorPos.gameObject.SetActive(true);
        chargeDetect.SetActive(false);
        chargeAttackAim.SetActive(false);

        weapon.transform.localPosition = defaultWeaponPos; // 사슬팔 위치 복구
        
        // 쿨타임 실행
        StartCoroutine(ChargeAttackCool_co(0.25f));

        // 탐지 여부에 따른 기술 실행
        if (target != null)
        {
            // 탐지o -> 차지어택 실행
            animator.SetTrigger("ChargeAttack");
            weaponAnimator.SetTrigger("ChargeAttack");

            hitBox.transform.localScale = Vector3.one * 0.8f;
            gameObject.layer = LayerMask.NameToLayer("ChargeAttack"); // 잠시 발판과 충돌하지 않는 레이어로 변경

            Vector2 dir = target.transform.position - transform.position;
            rigid2D.AddForce(dir * 500f);

            // 잔상 표시 실행
            StartCoroutine(TraceVisualUpdate(traceInterval));

            // 타켓 초기화
            target = null;
        }
        else
        {
            // 탐지x -> Falling 상태로 전환
            if (isGrounded)
            {
                playerState = PlayerState.None;
                animator.SetTrigger("Landing");
                weaponAnimator.SetTrigger("Landing");
            }
            else
            {
                playerState = PlayerState.None;
                animator.SetTrigger("Falling");
                weaponAnimator.SetTrigger("Falling");
            }

            rigid2D.gravityScale = 1f;
        }
    }

    // GameManager 구독 이벤트 실행 메소드 ----------------------------------------
    
    // 퇴장 모션 효과 메소드
    private void ActiveExitEffect()
    {
        StartCoroutine(ActiveExitEffect_co(1.5f));
    }

    // 퇴장 모션 효과 코루틴
    private IEnumerator ActiveExitEffect_co(float delay)
    {
        // 입력 제한 on
        animator.SetBool("isMoving", false);
        weaponAnimator.SetBool("isMoving", false);
        inputBlock = true;
        
        // 2초 동안 암전 효과 실행
        float elapsed = 0;

        while (elapsed < delay)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, elapsed / delay);

            FadeImage.color = new Color(0f, 0f, 0f, a);

            yield return null;
        }
        FadeImage.color = new Color(0f, 0f, 0f, 1f);

        // 플레이어 위치 초기화
        transform.position = savePoint.position;
        yield return new WaitForSeconds(1f);

        // 조명 효과 실행
        elapsed = 0;

        while (elapsed < delay)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, elapsed / delay);

            FadeImage.color = new Color(0f, 0f, 0f, a);

            yield return null;
        }
        FadeImage.color = new Color(0f, 0f, 0f, 0f);

        // 입력 제한 off
        inputBlock = false;
    }

    // 애니메이션 중 실행 메소드---------------------------------------------------

    // Rolling 애니메이션에 따른 사슬팔 애니메이션 동기화 메소드
    public void RollingAnimation()
    {
        weaponAnimator.SetTrigger("Rolling");
    }

    // ChargeAttack 종료에 따른 상태 초기화 메소드
    public void ChargeAttackStateBack()
    {
        // 상태 초기화
        target = null;
        playerState = PlayerState.None;
        rigid2D.gravityScale = 1f;
        hitBox.transform.localScale = Vector3.one * 0.5f;
        gameObject.layer = LayerMask.NameToLayer("PlayerBody"); // 플레이어 레이어 복구
    }

    // 이동 소리 동기화 메소드
    public void WalkSound()
    {
        AudioManager.instance.PlayDefaultSFX(AudioManager.instance.RunSFX);
    }

    // 벽 올라가는 소리 동기화 메소드
    public void ClimbSound()
    {
        AudioManager.instance.PlayDefaultSFX(AudioManager.instance.climbingSFX);
    }

    // 벽 내려가는 소리 동기화 메소드
    public void SlidingSound()
    {
        AudioManager.instance.StopPlayDefaultSFX();
        AudioManager.instance.PlayDefaultSFX(AudioManager.instance.slidingSFX);
    }

    // 천장 이동 소리 동기화 메소드
    public void CeilingSound()
    {
        AudioManager.instance.StopPlayDefaultSFX();
        AudioManager.instance.PlayDefaultSFX(AudioManager.instance.ceilingSFX);
    }

    // 기본 소리 정지 메소드
    public void StopDefaultSound()
    {
        AudioManager.instance.StopPlayDefaultSFX();
    }

    // 스킬 쿨타임 조절 메소드-----------------------------------------------------

    // 차지어택 쿨타임 코루틴
    private IEnumerator ChargeAttackCool_co(float delay)
    {
        canChargeAttack = false;
        yield return new WaitForSeconds(delay);

        canChargeAttack = true;
    }
}
