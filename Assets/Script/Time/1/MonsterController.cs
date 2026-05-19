using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//状态机
public enum EnemyState
{
    Patrol,
    Chase,  
    Attack 
}

public class MonsterController : MonoBehaviour
{
    public EnemyState currentState = EnemyState.Patrol;
    public float detectionRange = 13f;   // 检测玩家的范围
    public float attackRange = 5f;     // 攻击玩家的范围
    public float detectionBuffer = 0.6f;  // 退出检测范围需要多走0.3f
    public float attackBuffer = 0.5f;     // 退出攻击范围需要多走0.2f

    // 实际生效的范围（动态计算）
    private float effectiveDetectionRange;
    private float effectiveAttackRange;

    public float moveSpeed = 3f;

    private EnemyState pendingState;           // 等待切换的状态
    private bool isStateLocked = false;        // 状态锁
    private bool hasPendingTransition = false; // 是否有等待中的切换
    public bool canMove = true;
    private bool isChaseWaiting = false;


    public Transform groundCheckPoint; // 加个子类gameobject地面检测点（在脚底位置）
    public LayerMask groundLayer;
    public LayerMask enemyLayer;
    private bool isFacingRight = true;
   
    private Transform player;
    public Transform patrolPointA; // 左边界点
    public Transform patrolPointB; // 右边界点

    public Transform AttackPoint;
    public LayerMask playerLayer;
    public float attackDamage = 10f;

    private Rigidbody2D rb;
    private Animator anim;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;//你自己给一下Player tag（我看你工程好像没搞）
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        effectiveDetectionRange = detectionRange;//动态变化的攻击范围，这里先初始化
        effectiveAttackRange = attackRange;
    }

    
    void Update()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        EnemyState targetState = EvaluateState(distToPlayer);//目标，但不马上切换

        // 如果状态没被锁，直接切换
        if (!isStateLocked)
        {
            if (targetState != currentState)
            {
                TransitionToState(targetState);
            }
        }
        else
        {
            // 状态被锁时，只记录期望状态，不立即切换
            if (targetState != currentState)
            {
                pendingState = targetState;
                hasPendingTransition = true;
            }
        }
        ExecuteCurrentState();//实际的状态执行
    }
    //判断，切换，执行分离的状态机
    #region 状态判断
    private EnemyState EvaluateState(float distToPlayer)//不同状态下去到另一个状态判断不同（主要是在距离的变化）
    {
        {
            switch (currentState)
            {
                case EnemyState.Patrol:
                    // 巡逻状态：用标准范围判断进入追逐
                    if (distToPlayer <= detectionRange)
                    {

                        return EnemyState.Chase;
                    }
                    return EnemyState.Patrol;

                case EnemyState.Chase:
                    // 追逐状态：优先判断攻击，然后用缓冲范围判断退出
                    if (distToPlayer <= attackRange)
                    {
                        return EnemyState.Attack;
                    }
                    else if (distToPlayer > effectiveDetectionRange)//要更远的距离跑出来
                    {

                        return EnemyState.Patrol;
                    }
                    return EnemyState.Chase;

                case EnemyState.Attack:
                    // 攻击状态：用缓冲范围判断退出
                    if (distToPlayer > effectiveAttackRange)
                    {
                        return EnemyState.Chase;
                    }
                    return EnemyState.Attack;

                default:
                    return EnemyState.Patrol;
            }
        }
    }
    #endregion
    
    #region 切换状态
    void TransitionToState(EnemyState newState)
    {
        // 退出当前状态
        OnExitState(currentState);

        currentState = newState;

        // 进入新状态
        OnEnterState(currentState);
    }

    void OnEnterState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Chase:
                // 从巡逻进入追逐时，扩大退出范围
                effectiveDetectionRange = detectionRange + detectionBuffer;
                break;

            case EnemyState.Attack:
                // 从追逐进入攻击时，扩大退出范围
                effectiveAttackRange = attackRange + attackBuffer;
                // 锁定状态机
                LockState(true);
                anim.SetTrigger("Attack");
                //动画命中帧，结束帧这些还没写
                //anim.SetBool("IsAttacking", true);  // 用 Bool 参数循环攻击动画
                //如果需要怪物一直攻击，直到玩家离开就不要在动画结束帧时重新评估状态的方法
                //
                break;
        }
    }

    void OnExitState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Attack:
                effectiveAttackRange = attackRange;
                if (isStateLocked)
                    LockState(false);//保证一下解锁
                // 攻击状态退出时什么都不做，锁由动画事件解除（先执行动画，再切换锁，防止攻击动画放一半锁就改了）
                break;
            case EnemyState.Patrol:
                // 退出巡逻，可能需要停止巡逻动画
                break;

            case EnemyState.Chase:
                effectiveDetectionRange = detectionRange;
                // 退出追逐，可能需要停止追逐音效
                break;

            
        }
    }
    // 供外部调用的锁控制
    public void LockState(bool locked)
    {
        isStateLocked = locked;

        // 解锁时，如果有待处理的切换，立即执行
        if (!locked && hasPendingTransition)
        {
            hasPendingTransition = false;
            TransitionToState(pendingState);
        }
    }
    #endregion

    #region 实际动作执行和行为相关判断
    void ExecuteCurrentState()
    {
        if (!canMove) return;
        //纯粹执行行为，不需要判断任何条件
        switch (currentState)
        {
            case EnemyState.Patrol:
                //Debug.Log("Patrol");
                Patrol();   // 直接执行，因为既然在这个状态，就应该巡逻
                break;

            case EnemyState.Chase:
                //Debug.Log("Chase");
                Chase();
                break;

            case EnemyState.Attack:
                //Debug.Log("Attack");
                FacePlayer();
                
                break;
        }
    }
    void Patrol()
    {
        // 注释原来的掉头方式，改为完全靠两点判定
        //// 前方是墙壁或悬崖时，立即掉头
        //if (IsHittingWall() || !IsGroundAhead())
        //    Flip();

        if (patrolPointA != null && patrolPointB != null)
        {
            if (isFacingRight && transform.position.x >= patrolPointB.position.x)
            {
                Flip();
            }
            else if (!isFacingRight && transform.position.x <= patrolPointA.position.x)
            {
                Flip();
            }
        }

        rb.velocity = new Vector2((isFacingRight ? 1 : -1) * moveSpeed, rb.velocity.y);
    }

    void Chase()
    {
        if (isChaseWaiting) return;
        // 判断玩家在左还是在右，并调整朝向
        if (player.position.x > transform.position.x && !isFacingRight)
            Flip();
        else if (player.position.x < transform.position.x && isFacingRight)
            Flip();

        if (isFacingRight && transform.position.x >= patrolPointB.position.x || !isFacingRight && transform.position.x <= patrolPointA.position.x) // 追的时候追到领地边界了
        {
            StartCoroutine(ChaseBoundaryWait());
            return;
        }

        // 向玩家移动
        Vector2 targetVelocity = new Vector2((isFacingRight ? 1 : -1) * moveSpeed, rb.velocity.y);
        rb.velocity = targetVelocity;

    }
    
    //bool IsGroundAhead()
    //{
    //    Vector2 direction = (isFacingRight ? Vector2.right : Vector2.left) + Vector2.down;
    //    direction.Normalize();  // 变成斜向下 45°
        
    //    RaycastHit2D hit = Physics2D.Raycast(groundCheckPoint.position, direction, 1.5f, groundLayer);
    //    return hit.collider != null;
    //}

    //bool IsHittingWall() { /* 类似的射线检测逻辑 */ return false; }//防止怪物穿墙
    //bool IsOtherEnemyAhead() 
    //{
    //    Vector2 direction = (isFacingRight ? Vector2.right : Vector2.left);
    //    direction.Normalize();  // 变成斜向下 45°

    //    RaycastHit2D hit = Physics2D.Raycast(groundCheckPoint.position, direction, 1.5f, enemyLayer);
    //    return hit.collider != null;
    //}//防止怪物撞怪物
    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(isFacingRight ? 4 : -4, 4, 1);//给你调了比例
    }
    void FacePlayer()
    {
        if (player.position.x > transform.position.x && !isFacingRight)
            Flip();
        else if (player.position.x < transform.position.x && isFacingRight)
            Flip();
    }
    IEnumerator ChaseBoundaryWait()
    {
        isChaseWaiting = true;
        canMove = false;
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(1.5f);

        isChaseWaiting = false;
        canMove = true;

        // 强制切换回巡逻
        TransitionToState(EnemyState.Patrol);
    }
    #endregion

    //现在进入atteck会锁住，得加了动画才能开锁


    // 由攻击动画的最后一帧调用
    public void OnAttackAnimationEnd()
    {
        LockState(false); // 解锁状态机

        float dist = Vector2.Distance(transform.position, player.position);
        EnemyState nextState = EvaluateState(dist);//主动进行状态评估，如果还是在范围内，可以继续保持攻击状态
        TransitionToState(nextState);
    }
    // 这个方法由攻击动画的命中帧调用
    public void PerformAttackHit()
    {
        // 伤害判定逻辑
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(AttackPoint.position, attackRange, playerLayer);
        foreach (var player in hitPlayers)
        {
            player.GetComponent<MCscript>()?.TakeDamage(attackDamage);
        }
    }
}
