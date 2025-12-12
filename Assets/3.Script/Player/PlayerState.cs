using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    // 독립적인 플레이어 상태 정의
    None,
    Climbing,
    Ceiling,
    ChargeAttackReady,
    ChargeAttack,
}
