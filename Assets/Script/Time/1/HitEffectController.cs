using System.Collections;
using UnityEngine;

public class HitEffectController : MonoBehaviour // 可以在不同对象上复用
{
    [Header("特效材质设置")]
    [Tooltip("用于实现受击特效的材质")]
    [SerializeField] private Material hitMaterial;

    // 引用角色的SpriteRenderer
    private SpriteRenderer spriteRenderer;

    // 保存角色原始材质，以便特效结束后恢复
    private Material originalMaterial;

    [Header("特效持续时间")]
    [Tooltip("闪红和压缩效果总共持续多长时间")]
    [SerializeField] private float effectDuration = 0.15f;

    void Start()
    {
        // 获取角色身上的SpriteRenderer组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        

        // 记录原始材质
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
        }
        else
        {
            Debug.LogError("HitEffectController: 在 " + gameObject.name + " 上找不到 SpriteRenderer 组件！");
        }
    }

    // 对外公开的接口，角色受到攻击时，其他地方调用这个方法即可
    public void TriggerHitEffect()
    {
        // 如果已经有效果在运行，先停止之前的，再重新开始
        if (gameObject.activeInHierarchy)
        {
            
            StopAllCoroutines();
            StartCoroutine(PlayHitEffectCoroutine());
        }
    }

    private IEnumerator PlayHitEffectCoroutine()
    {
        // 检查必要组件是否都已正确赋值
        if (spriteRenderer == null || hitMaterial == null)
        {
            Debug.LogError("HitEffectController: SpriteRenderer 或 HitMaterial 未赋值！");
            yield break;
        }

        spriteRenderer.material = new Material(hitMaterial); // 实例化副本

        float elapsedTime = 0f;

        // 2. 在effectDuration时间内，将Shader里的_HitEffect属性从1逐渐变为0
        while (elapsedTime < effectDuration)
        {
            // 计算当前特效的强度，随时间的推移线性减弱
            float intensity = 1.0f - (elapsedTime / effectDuration);
            // 将计算出的强度值传给我们Shader里的_HitEffect参数
            spriteRenderer.material.SetFloat("_HitEffect", intensity);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 3. 确保在结束时特效强度为0，并恢复原始材质
        spriteRenderer.material.SetFloat("_HitEffect", 0);
        spriteRenderer.material = originalMaterial;
    }
}