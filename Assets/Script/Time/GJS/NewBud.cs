using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBud : MonoBehaviour, ITimeControlable
{
    public bool CanReserveTime { get; set; } = true;

    [Header("孢子配置")]
    [SerializeField] private GameObject[] sporeObjects; // 在场景中预先放置的孢子对象

    [Header("透明度渐变")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private bool isActivated;
    private float currentTime;

    private List<SporeInstance> sporeInstances = new List<SporeInstance>();

    private class SporeInstance
    {
        public GameObject gameObject;
        public SpriteRenderer renderer;
        public Collider2D[] colliders;
        public float currentAlpha;
    }

    private void Awake()
    {
        // 初始化所有预置孢子，初始禁用
        foreach (GameObject obj in sporeObjects)
        {
            if (obj == null) continue;
            SporeInstance instance = new SporeInstance();
            instance.gameObject = obj;
            instance.renderer = obj.GetComponent<SpriteRenderer>();
            instance.colliders = obj.GetComponents<Collider2D>();
            // 初始状态：未激活（禁用GameObject）
            obj.SetActive(false);
            sporeInstances.Add(instance);
        }
    }

    #region 实现 ITimeControlable
    public void ChangeCurrentTime(float deltaTime)
    {
        if (!isActivated && deltaTime > 0f)
        {
            Activate();
        }

        if (!isActivated)
            return;

        currentTime += deltaTime;
        currentTime = Mathf.Clamp(currentTime, 0f, fadeDuration);

        float progress = currentTime / fadeDuration;
        float alpha = fadeCurve.Evaluate(progress);

        foreach (var spore in sporeInstances)
        {
            SetSporeAlpha(spore, alpha);
        }
    }

    public void Lighten(bool _isLighten)
    {
        // 不需要实现
    }
    #endregion

    private void Activate()
    {
        if (isActivated) return;
        isActivated = true;
        currentTime = 0f;

        // 激活所有孢子对象
        foreach (var spore in sporeInstances)
        {
            if (spore.gameObject != null)
            {
                spore.gameObject.SetActive(true);
                // 初始透明且禁用碰撞体
                SetSporeAlpha(spore, 0f);
            }
        }
    }

    private void SetSporeAlpha(SporeInstance spore, float alpha)
    {
        if (spore.renderer != null)
        {
            Color color = spore.renderer.color;
            color.a = alpha;
            spore.renderer.color = color;
        }
        spore.currentAlpha = alpha;

        // 控制碰撞体：透明度>0时启用，否则禁用
        bool enableCollider = alpha > 0.001f;
        if (spore.colliders != null)
        {
            foreach (var col in spore.colliders)
            {
                col.enabled = enableCollider;
            }
        }
        // 注意：游戏对象始终激活（除非外部手动禁用），我们不在此控制GameObject激活状态
    }

    // 可选重置方法
    public void ResetBud()
    {
        // 重置时，将所有孢子禁用，并重置状态
        foreach (var spore in sporeInstances)
        {
            if (spore.gameObject != null)
                spore.gameObject.SetActive(false);
        }
        isActivated = false;
        currentTime = 0f;
    }
}