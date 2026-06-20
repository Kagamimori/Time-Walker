using UnityEngine;

public class Gem : MonoBehaviour
{
    [Header("吸附参数")]
    public float attractoffsetY = 0.2f;
    public float attractDistance = 1f;
    public float maxSpeed = 5f;
    public float minSpeed = 0.5f;

    [Header("浮动参数")]
    public float wobbleSpeed = 2f;
    public float A = 0.09f;

    [Header("补充能量值")]
    public int value = 1;

    
    private Transform playerTransform;
    private MCscript playerScript;
    private bool isCollected = false;

    // 核心位置（不含浮动），用于物理移动
    private Vector3 basePosition;
    private Vector3 initialPosition; // 仅用于初始值，不再重置

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

        initialPosition = transform.position;
        basePosition = transform.position;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("宝石的 Collider2D 未设为 Trigger，已自动修正。");
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        if (isCollected || playerTransform == null) return;

        // 目标点 = 玩家位置 + Y偏移
        Vector3 targetPos = playerTransform.position + new Vector3(0, attractoffsetY, 0);

        // 计算 basePosition 到目标点的距离
        float distance = Vector2.Distance(basePosition, targetPos);

        // ---- 距离 > 吸附距离：不移动 basePosition（保持当前位置） ----
        if (distance > attractDistance)
        {
            // ★ 不重置，basePosition 保持不动
        }
        // ---- 距离 <= 吸附距离：向目标点移动（包含垂直） ----
        else
        {
            Vector2 direction = (targetPos - basePosition).normalized;
            float t = Mathf.Clamp01(distance / attractDistance);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            float currentSpeed = Mathf.Lerp(maxSpeed, minSpeed, smoothT);

            // 移动 basePosition（包含垂直）
            basePosition += (Vector3)(direction * currentSpeed * Time.deltaTime);
        }

        // ---- 计算浮动偏移（振幅随到目标点的距离变化） ----
        float distToTarget = Vector2.Distance(basePosition, targetPos);
        float distRatio = Mathf.Clamp01(distToTarget / attractDistance);
        float offsetY = A * distRatio * Mathf.Sin(Time.time * wobbleSpeed);

        // ---- 最终显示位置 = basePosition + 浮动偏移 ----
        transform.position = new Vector3(basePosition.x, basePosition.y + offsetY, basePosition.z);

        // ---- 拾取判定：使用真实位置到目标点的距离 ----
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
        {
            playerScript.CollectGem(value);
        }
        else
        {
            Debug.LogWarning("玩家脚本不存在，无法拾取。");
            Destroy(gameObject);
            return;
        }

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