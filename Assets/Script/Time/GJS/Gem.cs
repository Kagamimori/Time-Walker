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
    public int value = 33;

    [Header("重力参数")]
    public float gravity = 15f;
    public float groundY;

    // ---- 私有字段 ----
    private Transform playerTransform;
    private MCscript playerScript;
    private bool isCollected = false;
    private float landTime;
    private float floatEaseInDuration = 0.3f;

    private Vector3 basePosition;
    private Vector3 initialPosition;

    private enum State { Thrown, Idle, Attracting }
    private State currentState = State.Thrown;

    private float verticalSpeed;

    private float smoothDistRatio = 1f;
    private float velocityRef = 0f;

    private int skipLandCheckFrames = 0;
    private float throwStartTime;
    private bool hasPeaked = false;

    // ---- 初始化方法 ----
    public void Initialize(float groundY, float initialUpSpeed)
    {
        this.groundY = groundY;
        verticalSpeed = initialUpSpeed;
        currentState = State.Thrown;
        isCollected = false;
        skipLandCheckFrames = 3;
        throwStartTime = Time.time;
        hasPeaked = false;

        basePosition = transform.position;
        initialPosition = transform.position;

        // Debug.Log($"【Initialize】传入 groundY = {groundY}, initialUpSpeed = {initialUpSpeed}");

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

        // 不覆盖 Initialize 设置的 groundY
    }

    private void Update()
    {
        if (isCollected || playerTransform == null) return;

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

        // ---- 浮动计算 ----
        float offsetY = 0f;
        if (currentState == State.Idle || currentState == State.Attracting)
        {
            Vector3 targetPos = playerTransform.position + new Vector3(0, attractoffsetY, 0);
            float distToTarget = Vector2.Distance(basePosition, targetPos);
            float rawRatio = Mathf.Clamp01(distToTarget / attractDistance);
            smoothDistRatio = Mathf.SmoothDamp(smoothDistRatio, rawRatio, ref velocityRef, 0.15f);

            float floatEase = 1f;
            if (landTime > 0)
            {
                float t = (Time.time - landTime) / floatEaseInDuration;
                floatEase = Mathf.Clamp01(t);
            }
            offsetY = amplitude * smoothDistRatio * floatEase * Mathf.Sin(Time.time * wobbleSpeed);
        }

        transform.position = new Vector3(basePosition.x, basePosition.y + offsetY, basePosition.z);
    }

    private void UpdateThrown()
    {
        // 调试初速度
        if (skipLandCheckFrames == 3)
        {
            // Debug.Log($"【上抛】初速度 = {verticalSpeed}, 生成Y = {basePosition.y}, groundY = {groundY}");
        }

        // 跳过落地检测
        if (skipLandCheckFrames > 0)
        {
            skipLandCheckFrames--;
            verticalSpeed -= gravity * Time.deltaTime;
            basePosition.y += verticalSpeed * Time.deltaTime;
            return;
        }

        // 正常更新速度
        verticalSpeed -= gravity * Time.deltaTime;
        basePosition.y += verticalSpeed * Time.deltaTime;

        // 检测是否达到最高点（速度由正转负）
        if (verticalSpeed <= 0 && !hasPeaked)
        {
            hasPeaked = true;
        }

        // 落地检测
        if (verticalSpeed <= 0 && basePosition.y <= groundY)
        {
            basePosition.y = groundY;
            verticalSpeed = 0f;
            currentState = State.Idle;
            landTime = Time.time;
            Debug.Log("Gem landed.");
            return;
        }

        // ★ 提前吸附：仅在下落阶段（hasPeaked为true）且经过一定延迟（保证上抛动作）
        if (hasPeaked && Time.time - throwStartTime > 0.2f)
        {
            float distToPlayer = Vector2.Distance(basePosition, playerTransform.position);
            if (distToPlayer <= attractDistance)
            {
                verticalSpeed = 0f;
                currentState = State.Attracting;
                // Debug.Log("Gem attracted during falling.");
            }
        }
    }

    // ---- 待机状态 ----
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
                landTime = Time.time;
            }
        }
        else
        {
            basePosition.y = groundY;
            verticalSpeed = 0f;
            if (landTime < 0) landTime = Time.time;
        }

        Vector3 targetPos = playerTransform.position + new Vector3(0, attractoffsetY, 0);
        float distance = Vector2.Distance(basePosition, targetPos);
        if (distance <= attractDistance)
        {
            currentState = State.Attracting;
            verticalSpeed = 0f;
            // Debug.Log("Gem attracted from Idle.");
        }
    }

    // ---- 吸附状态 ----
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

        if (distance > attractDistance)
        {
            currentState = State.Idle;
            verticalSpeed = 0f;
            // Debug.Log("Player left, returning to Idle.");
        }

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