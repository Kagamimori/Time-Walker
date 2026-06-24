using System.Collections;
using UnityEngine;

[System.Serializable]
public struct HitEffectConfig
{
    [Header("特效材质（必须包含 _HitEffect 属性）")]
    public Material effectMaterial;

    [Header("特效持续时间（秒）")]
    public float duration;
}

public class HitEffectController : MonoBehaviour
{
    [Header("特效配置数组")]
    [SerializeField] private HitEffectConfig[] effectConfigs;

    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private bool currentRestoreMaterial = true;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalMaterial = spriteRenderer.material;
        else
            Debug.LogError("HitEffectController: 找不到 SpriteRenderer 组件！");
    }

    private void OnDestroy()
    {
        if (currentRestoreMaterial && spriteRenderer != null && originalMaterial != null)
        {
            spriteRenderer.material = originalMaterial;
        }
        else if (!currentRestoreMaterial && spriteRenderer != null)
        {
            // 死亡特效，强制设 _HitEffect = 0
            Material mat = spriteRenderer.material;
            if (mat != null && mat.shader.name.Contains("PixelDissolve"))
                mat.SetFloat("_HitEffect", 0f);
        }
        StopAllCoroutines();
    }

    /// <summary>
    /// 触发默认特效（数组第一个），默认恢复材质
    /// </summary>
    public void TriggerHitEffect()
    {
        TriggerHitEffect(0, true);
    }

    /// <summary>
    /// 触发指定索引的特效，默认恢复材质（受击用）
    /// </summary>
    public void TriggerHitEffect(int index)
    {
        TriggerHitEffect(index, true);
    }

    /// <summary>
    /// 触发指定索引的特效，可控制是否恢复原始材质
    /// </summary>
    /// <param name="index">特效索引</param>
    /// <param name="restoreMaterial">true=恢复原始材质（受击），false=保持特效材质（死亡溶解）</param>
    public void TriggerHitEffect(int index, bool restoreMaterial)
    {
        if (effectConfigs == null || effectConfigs.Length == 0)
        {
            Debug.LogWarning("HitEffectController: 特效配置数组为空。");
            return;
        }

        if (index < 0 || index >= effectConfigs.Length)
        {
            Debug.LogWarning($"索引 {index} 超出范围，使用默认索引 0。");
            index = 0;
        }

        if (!gameObject.activeInHierarchy)
            return;

        currentRestoreMaterial = restoreMaterial;
        StopAllCoroutines();
        StartCoroutine(PlayEffectCoroutine(index, restoreMaterial));
    }

    private IEnumerator PlayEffectCoroutine(int index, bool restoreMaterial)
    {
        // 清除任何可能存在的 MaterialPropertyBlock 干扰
        if (spriteRenderer != null)
            spriteRenderer.SetPropertyBlock(null);

        HitEffectConfig config = effectConfigs[index];
        if (config.effectMaterial == null || spriteRenderer == null)
            yield break;

        // 创建材质实例并应用
        Material tempMat = new Material(config.effectMaterial);
        spriteRenderer.material = tempMat;

        float duration = config.duration;
        float targetFPS = 120; // 假设目标帧率为 6fps
        float step = 1f / (duration * targetFPS); // 每帧增加量
        float progress = 0f;

        while (progress < 1f)
        {
            if (spriteRenderer == null || this == null)
            {
                if (!restoreMaterial && tempMat != null)
                    tempMat.SetFloat("_HitEffect", 0f);
                yield break;
            }

            // 强度从 1 线性递减到 0
            float intensity = 1f - progress;
            tempMat.SetFloat("_HitEffect", intensity);

            // 每帧增加固定步长
            progress += step;

            yield return null; // 等待下一帧
        }

        // 协程正常结束，确保最终值为 0
        if (restoreMaterial)
        {
            // 受击特效：恢复原始材质
            if (spriteRenderer != null)
            {
                tempMat.SetFloat("_HitEffect", 0f);
                spriteRenderer.material = originalMaterial;
            }
            Destroy(tempMat);
        }
        else
        {
            // 死亡特效：保持临时材质，将 _HitEffect 设为 0（完全溶解）
            if (tempMat != null)
                tempMat.SetFloat("_HitEffect", 0f);
            // 不销毁 tempMat，对象销毁时自动释放
        }
    }
}