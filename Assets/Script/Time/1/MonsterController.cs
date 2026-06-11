using System.Collections;
using UnityEngine;

public enum EnemyState
{
    Patrol,
    Charge,
    Stun
}

public class MonsterController : MonoBehaviour
{
    [Header("状态")]
    public EnemyState currentState = EnemyState.Patrol;

    [Header("检测范围")]
    public float detectionRange = 13f;
    public float detectionBuffer = 0.2f;  // 超了一点再离开
    private float effectiveDetectionRange;

    [Header("移动速度")]
    public float patrolSpeed = 3f;          // 巡逻速度
    public float chargeMaxSpeed = 4f;      // 冲锋最大速度
    public float accelerationTime = 0.5f;  // 加速到最大速度的时间（用了时间而非加速度）
    public float decelerationTime = 0.5f;  // 自主减速的时间（追击状态）

    [Header("僵直")]
    public float stunDeceleration = 8f;    // 僵直滑行的减速度（与自主减速数值相等，但变量独立）
    public float minKnockbackForce = 5f;   // 对玩家的最小击退力
    public float maxKnockbackForce = 12f;  // 对玩家的最大击退力

    [Header("巡逻边界")]
    public Transform patrolPointA;
    public Transform patrolPointB;
    public float boundaryWaitTime = 0.2f;   // 到达边界后的停留时间（根据动画而定）

    [Header("地面检测")]
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    public float groundCheckDistance = 1.5f;

    [Header("墙壁检测")]
    public LayerMask wallLayer;
    public float wallCheckDistance = 0.5f;

    [Header("攻击")]
    public int attackDamage = 10;
    public LayerMask playerLayer;

    // 内部变量
    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;
    private bool isFacingRight = true;
    private float currentSpeed = 0f;           // 当前实际速度（绝对值）
    private bool isStateLocked = false;        // 状态锁（仅Stun时使用）
    private EnemyState pendingState;
    private bool hasPendingTransition;

    // 巡逻边界等待
    private bool isBoundaryWaiting = false;
    private float boundaryWaitTimer = 0f;
    private bool shouldFlipAfterWait = false;

    // 加速度控制
    private float accelerationPerSecond;
    private float decelerationPerSecond;
    private bool isAccelerating = false;
    private bool isDecelerating = false;
    private float targetSpeed;                 // 加速的目标速度（4）
    private float accelerationTimer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        effectiveDetectionRange = detectionRange;
        accelerationPerSecond = (chargeMaxSpeed - patrolSpeed) / accelerationTime;
        decelerationPerSecond = chargeMaxSpeed / decelerationTime; // 从4到0的减速度
        stunDeceleration = decelerationPerSecond; // 使两者数值相等，但保留独立变量方便日后调整
    }

    void Update()
    {
        if (!canMove) return;

        float distToPlayer = player ? Vector2.Distance(transform.position, player.position) : Mathf.Infinity;
        EnemyState targetState = EvaluateState(distToPlayer);

        // 状态切换（受锁控制）
        if (!isStateLocked)
        {
            if (targetState != currentState)
                TransitionToState(targetState);
        }
        else
        {
            if (targetState != currentState)
            {
                pendingState = targetState;
                hasPendingTransition = true;
            }
        }
        
            transform.localScale = new Vector3(isFacingRight ? 1 : -1, 1, 1);
        

        ExecuteCurrentState();
        UpdateAnimatorSpeed();
    }

    #region 状态评估
    private EnemyState EvaluateState(float distToPlayer)
    {
        // 如果在僵直中，任何评估都不改变状态（由锁保证）
        switch (currentState)
        {
            case EnemyState.Patrol:
                if (distToPlayer <= detectionRange)
                    return EnemyState.Charge;
                return EnemyState.Patrol;

            case EnemyState.Charge:
                // 冲锋中：如果玩家离开扩展范围或到达边界，转为巡逻
                if (distToPlayer > effectiveDetectionRange || HasReachedBoundary())
                    return EnemyState.Patrol;
                return EnemyState.Charge;

            case EnemyState.Stun:
                // 僵直由自身解除，外部不能切换
                return EnemyState.Stun;

            default:
                return EnemyState.Patrol;
        }
    }
    #endregion

    #region 状态切换
    private void TransitionToState(EnemyState newState) // 切换，进入，离开分开写
    {
        OnExitState(currentState);
        currentState = newState;
        OnEnterState(currentState);
    }

    private void OnEnterState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Patrol:
                effectiveDetectionRange = detectionRange;
                // 如果是从其他状态回来，立即恢复巡逻速度并重置加速度标志
                currentSpeed = patrolSpeed;
                isAccelerating = false;
                isDecelerating = false;
                isBoundaryWaiting = false;
                break;

            case EnemyState.Charge:
                effectiveDetectionRange = detectionRange + detectionBuffer;
                // 如果速度小于3，瞬间提升到3再开始加速
                if (currentSpeed < patrolSpeed)
                    currentSpeed = patrolSpeed;
                // 启动加速：从当前速度到4
                StartAccelerating();
                break;

            case EnemyState.Stun:
                LockState(true);
                isAccelerating = false;
                isDecelerating = false; // 使用专用的减速度
                break;
        }
    }

    private void OnExitState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Charge:
                effectiveDetectionRange = detectionRange;
                StopAccelerating();
                StopDecelerating();
                break;

            case EnemyState.Stun:
                LockState(false);
                break;

            case EnemyState.Patrol:
                
                break;
        }
    }
    #endregion

    #region 行为执行
    private void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Charge:
                Charge();
                break;
            case EnemyState.Stun:
                StunSlide();
                break;
        }
    }

    // ---------- 巡逻 ----------
    private void Patrol()
    {
        // 边界等待逻辑
        if (isBoundaryWaiting)
        {
            rb.velocity = Vector2.zero;
            currentSpeed = 0f;
            boundaryWaitTimer -= Time.deltaTime;
            if (boundaryWaitTimer <= 0f)
            {
                isBoundaryWaiting = false;
                if (shouldFlipAfterWait)
                    Flip();
                currentSpeed = patrolSpeed; // 恢复移动速度
            }
            return;
        }

        // 墙壁/悬崖检测掉头
        if (IsHittingWall() || !IsGroundAhead())
        {
            Flip();
        }

        // 边界点检测（到达时开始等待）
        if (patrolPointA && patrolPointB)
        {
            if (isFacingRight && transform.position.x >= patrolPointB.position.x)
            {
                StartBoundaryWait(true);
            }
            else if (!isFacingRight && transform.position.x <= patrolPointA.position.x)
            {
                StartBoundaryWait(true);
            }
        }

        if (!isBoundaryWaiting)
        {
            rb.velocity = new Vector2((isFacingRight ? 1 : -1) * patrolSpeed, rb.velocity.y);
            currentSpeed = patrolSpeed;
        }
    }

    private void StartBoundaryWait(bool flipAfter)
    {
        isBoundaryWaiting = true;
        boundaryWaitTimer = boundaryWaitTime;
        shouldFlipAfterWait = flipAfter;
        rb.velocity = Vector2.zero;
        currentSpeed = 0f;
    }

    // ---------- 冲锋 ----------
    private void Charge()
    {
        // 如果加速中
        if (isAccelerating)
        {
            accelerationTimer += Time.deltaTime;
            currentSpeed = Mathf.MoveTowards(currentSpeed, chargeMaxSpeed, accelerationPerSecond * Time.deltaTime);
            if (currentSpeed >= chargeMaxSpeed)
            {
                currentSpeed = chargeMaxSpeed;
                isAccelerating = false;
            }
        }
        // 如果减速中
        else if (isDecelerating)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, decelerationPerSecond * Time.deltaTime);
            if (currentSpeed <= 0f)
            {
                currentSpeed = 0f;
                isDecelerating = false;
                // 减速到0，转为巡逻
                TransitionToState(EnemyState.Patrol);
                return;
            }
        }

        // 面向玩家
        FacePlayer();

        rb.velocity = new Vector2((isFacingRight ? 1 : -1) * currentSpeed, rb.velocity.y);

        // 检查退出条件（仅在非减速时检查，减速中已决定退出，避免重复）
        if (!isDecelerating)
        {
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            if (distToPlayer > effectiveDetectionRange || HasReachedBoundary())
            {
                // 开始减速
                StartDecelerating();
            }
        }
    }

    private void StartAccelerating()
    {
        isAccelerating = true;
        isDecelerating = false;
        accelerationTimer = 0f;
        // 加速度恒定，从当前速度到4
    }

    private void StopAccelerating()
    {
        isAccelerating = false;
    }

    private void StartDecelerating()
    {
        isDecelerating = true;
        isAccelerating = false;
    }

    private void StopDecelerating()
    {
        isDecelerating = false;
    }

    // ---------- 僵直滑行 ----------
    private void StunSlide()
    {
        // 用恒定减速度降低速度
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, stunDeceleration * Time.deltaTime);
        rb.velocity = new Vector2((isFacingRight ? 1 : -1) * currentSpeed, rb.velocity.y);

        if (currentSpeed <= 0f)
        {
            currentSpeed = 0f;
            rb.velocity = Vector2.zero;
            // 僵直结束
            LockState(false);
            // 立即评估下一步状态
            float dist = player ? Vector2.Distance(transform.position, player.position) : Mathf.Infinity;
            EnemyState next = EvaluateState(dist);
            TransitionToState(next);
        }
    }
    #endregion

    #region 碰撞处理
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 仅当处于Charge状态且碰到玩家时触发
        if (currentState != EnemyState.Charge)
            return;

        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            // 伤害玩家（假设了多个）
            var playerHealth = collision.gameObject.GetComponent<MCscript>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);

            // 暂时假设主角takedamage击退没写

            // 击退玩家（力度与当前速度相关）
            float speedRatio = Mathf.InverseLerp(patrolSpeed, chargeMaxSpeed, currentSpeed); // 用了比例而非数字
            float knockbackForce = Mathf.Lerp(minKnockbackForce, maxKnockbackForce, speedRatio);
            Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
            Rigidbody2D playerRb = collision.rigidbody;
            if (playerRb != null)
                playerRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

            // 自身进入僵直（Stun）
            TransitionToState(EnemyState.Stun);
        }
    }
    #endregion

    #region 辅助方法
    private bool HasReachedBoundary()
    {
        if (patrolPointA == null || patrolPointB == null)
            return false;
        if (isFacingRight && transform.position.x >= patrolPointB.position.x)
            return true;
        if (!isFacingRight && transform.position.x <= patrolPointA.position.x)
            return true;
        return false;
    }

    private void FacePlayer()
    {
        if (player == null) return;
        if (player.position.x > transform.position.x && !isFacingRight)
            Flip();
        else if (player.position.x < transform.position.x && isFacingRight)
            Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void UpdateAnimatorSpeed()
    {
        if (anim)
            anim.SetFloat("Speed", Mathf.Abs(currentSpeed));
            anim.SetFloat("WalkSpeedMultiplier", Mathf.Abs(currentSpeed) / patrolSpeed);// 用浮点数关联动画
    }

    // 地面检测（前方是否有地面）
    private bool IsGroundAhead()
    {
        Vector2 origin = groundCheckPoint.position;
        Vector2 direction = (isFacingRight ? Vector2.right : Vector2.left) + Vector2.down;
        direction.Normalize();
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }

    // 墙壁检测
    private bool IsHittingWall()
    {
        Vector2 origin = transform.position;
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, wallCheckDistance, wallLayer);
        return hit.collider != null;
    }

    // 状态锁控制（供内部及外部查询）
    public void LockState(bool locked)
    {
        isStateLocked = locked;
        if (!locked && hasPendingTransition)
        {
            hasPendingTransition = false;
            TransitionToState(pendingState);
        }
    }

    // 一点补充
    public bool IsStateLocked => isStateLocked; // 只读访问锁定状态，主要可能用于takedamage作条件数据
    // 是否可移动（外部控制）
    [HideInInspector] public bool canMove = true;
    #endregion

    private void OnDrawGizmosSelected() // scene窗口AB点绘制辅助
    {
        if (patrolPointA && patrolPointB)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(patrolPointA.position, patrolPointB.position);
            Gizmos.DrawWireSphere(patrolPointA.position, 0.3f);
            Gizmos.DrawWireSphere(patrolPointB.position, 0.3f);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}