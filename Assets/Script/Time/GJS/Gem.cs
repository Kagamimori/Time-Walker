using UnityEngine;

public class Gem : MonoBehaviour
{
    // ---- 原有参数 ----
    [Header("吸附参数")]
    public float attractoffsetY = 0.2f;
    public float attractDistance = 1f;
    public float maxSpeed = 5f;
    public float minSpeed = 0.5f;

    [Header("浮动参数")]
    public float wobbleSpeed = 2f;
    public float amplitude = 0.12f;

    [Header("补充能量值")]
    public int value = 1;

    [Header("重力参数")]
    public float gravity = 15f;          // 下落加速度（正值，向下）
    public float groundY;                // 地面Y坐标（由外部初始化设置）

    // ---- 私有字段 ----
    private Transform playerTransform;
    private MCscript playerScript;
    private bool isCollected = false;
    private float landTime;             // 落地时间
    private float floatEaseInDuration = 0.3f; // 浮动渐变时长（可调）

    private Vector3 basePosition;        // 不含浮动的基础位置（用于移动和物理）
    private Vector3 initialPosition;     // 仅用于初始化

    // 状态机
    private enum State { Thrown, Idle, Attracting }
    private State currentState = State.Thrown;

    // 垂直速度（用于重力，所有状态通用）
    private float verticalSpeed;

    // 平滑振幅过渡（用于Idle/Attracting状态）
    private float smoothDistRatio = 1f;
    private float velocityRef = 0f;

    // ---- 初始化方法（由外部调用） ----
    public void Initialize(float groundY, float initialUpSpeed)
    {
        this.groundY = groundY;
        verticalSpeed = initialUpSpeed;
        currentState = State.Thrown;
        isCollected = false;

        basePosition = transform.position;
        initialPosition = transform.position;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerScript = player.GetComponent<MCscript>();
        }
        else
        {
            Debug.LogError("未找到 Tag 为 'Player' 的游戏对象！");
        }

        if (groundY == 0f && currentState == State.Thrown)
            groundY = transform.position.y;
    }

    private void Update()
    {
        if (isCollected || playerTransform == null) return;

        // ---- 根据状态执行不同逻辑 ----
        switch (currentState)
        {
            case State.Thrown:
                UpdateThrown();
                break;
            case State.Idle:
                UpdateIdle();
                break;
            case State.Attracting:
                UpdateAttracting();
                break;
        }

        // ---- 浮动计算（仅 Idle / Attracting 状态） ----
        float offsetY = 0f;
        if (currentState == State.Idle || currentState == State.Attracting)
        {
            Vector3 targetPos = playerTransform.position + new Vector3(0, attractoffsetY, 0);
            float distToTarget = Vector2.Distance(basePosition, targetPos);
            float rawRatio = Mathf.Clamp01(distToTarget / attractDistance);
            smoothDistRatio = Mathf.SmoothDamp(smoothDistRatio, rawRatio, ref velocityRef, 0.15f);

            // 浮动淡入（落地后逐渐增加振幅）
            float floatEase = 1f;
            if (landTime > 0)
            {
                float t = (Time.time - landTime) / floatEaseInDuration;
                floatEase = Mathf.Clamp01(t);
            }
            offsetY = amplitude * smoothDistRatio * floatEase * Mathf.Sin(Time.time * wobbleSpeed);
        }



        // 最终位置 = basePosition + 浮动偏移（Y轴）
        transform.position = new Vector3(basePosition.x, basePosition.y + offsetY, basePosition.z);
    }

    // ---- 抛出状态：受重力影响，无浮动，检测提前吸附 ----
    private void UpdateThrown()
    {
        verticalSpeed -= gravity * Time.deltaTime;
        basePosition.y += verticalSpeed * Time.deltaTime;

        if (basePosition.y <= groundY)
        {
            basePosition.y = groundY;
            verticalSpeed = 0f;
            currentState = State.Idle;
            landTime = Time.time; // ★ 记录落地时间
            Debug.Log("Gem landed, entering Idle.");
            return;
        }

        float distToPlayer = Vector2.Distance(basePosition, playerTransform.position);
        if (distToPlayer <= attractDistance)
        {
            verticalSpeed = 0f;
            currentState = State.Attracting;
            // 注意：如果落地前就吸附，landTime 未设置，浮动仍无淡入（因为未落地）
            Debug.Log("Gem attracted before landing.");
        }
    }

    // ---- 待机状态：重力持续作用，保持在地面，检测玩家 ----
    private void UpdateIdle()
    {
        if (basePosition.y > groundY)
        {
            verticalSpeed -= gravity * Time.deltaTime;
            basePosition.y += verticalSpeed * Time.deltaTime;
            if (basePosition.y <= groundY)
            {
                basePosition.y = groundY;
                verticalSpeed = 0f;
                landTime = Time.time; // 重新落地也更新
            }
        }
        else
        {
            basePosition.y = groundY;
            verticalSpeed = 0f;
            // 如果已经落地且 landTime 未设置（理论上不会），可补设
            if (landTime < 0) landTime = Time.time;
        }

        Vector3 targetPos = playerTransform.position + new Vector3(0, attractoffsetY, 0);
        float distance = Vector2.Distance(basePosition, targetPos);
        if (distance <= attractDistance)
        {
            currentState = State.Attracting;
            verticalSpeed = 0f;
            Debug.Log("Gem attracted from Idle.");
        }
    }

    // ---- 吸附状态：重力禁用，向玩家移动 ----
    private void UpdateAttracting()
    {
        Vector3 targetPos = playerTransform.position + new Vector3(0, attractoffsetY, 0);
        float distance = Vector2.Distance(basePosition, targetPos);

        if (distance > 0.01f)
        {
            Vector2 direction = (targetPos - basePosition).normalized;
            float t = Mathf.Clamp01(distance / attractDistance);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            float currentSpeed = Mathf.Lerp(maxSpeed, minSpeed, smoothT);
            basePosition += (Vector3)(direction * currentSpeed * Time.deltaTime);
        }

        // 如果玩家离开吸附范围，回到 Idle（启用重力）
        if (distance > attractDistance)
        {
            currentState = State.Idle;
            verticalSpeed = 0f; // 重新启用重力时初始速度0，会自然下落
            Debug.Log("Player left, returning to Idle.");
        }

        // 吸附时仍检测拾取（使用真实距离）
        float realDistance = Vector2.Distance(transform.position, targetPos);
        if (realDistance < 0.05f)
        {
            Collect();
        }
    }

    private void Collect()
    {
        if (isCollected) return;
        isCollected = true;

        if (playerScript != null)
            playerScript.CollectGem(value);
        else
            Debug.LogWarning("玩家脚本不存在，无法拾取。");

        StartCoroutine(FadeOutAndDestroy());
    }

    private System.Collections.IEnumerator FadeOutAndDestroy()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Destroy(gameObject);
            yield break;
        }

        float duration = 0.07f;
        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        sr.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        Destroy(gameObject);
    }
}