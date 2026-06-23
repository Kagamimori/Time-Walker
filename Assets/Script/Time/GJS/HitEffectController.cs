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

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
        }
        else
        {
            Debug.LogError("HitEffectController: 找不到 SpriteRenderer 组件！");
        }
    }

    /// <summary>
    /// 触发默认特效（数组第一个）
    /// </summary>
    public void TriggerHitEffect()
    {
        TriggerHitEffect(0);
    }

    /// <summary>
    /// 触发指定索引的特效
    /// </summary>
    public void TriggerHitEffect(int index)
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

        StopAllCoroutines();
        StartCoroutine(PlayEffectCoroutine(index));
    }

    private IEnumerator PlayEffectCoroutine(int index)
    {
        HitEffectConfig config = effectConfigs[index];
        if (config.effectMaterial == null)
        {
            Debug.LogError($"配置 {index} 的材质为空！");
            yield break;
        }

        // 实例化材质副本，避免影响其他对象
        Material tempMat = new Material(config.effectMaterial);
        spriteRenderer.material = tempMat;

        float elapsed = 0f;
        float duration = config.duration;

        // 从强度 1 逐渐降到 0
        while (elapsed < duration)
        {
            float intensity = 1f - (elapsed / duration);
            tempMat.SetFloat("_HitEffect", intensity);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 确保最终强度为 0 并恢复原始材质
        tempMat.SetFloat("_HitEffect", 0f);
        spriteRenderer.material = originalMaterial;
        Destroy(tempMat);
    }
}