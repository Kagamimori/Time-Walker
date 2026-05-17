using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBud : MonoBehaviour, ITimeControlable
{
    public bool CanReserveTime { get; set; }
    [System.Serializable]
    public struct SporeMotionData
    {
        public Vector3 endPosition;      // 孢子运动的终点世界坐标
        public AnimationCurve xCurve;    // X 轴运动曲线
        public AnimationCurve yCurve;    // Y 轴运动曲线
    }

    [Header("开花动画")]
    [SerializeField] private string bloomStateName = "Bloom"; // Animator 中开花动画状态名 //
    private float bloomDuration;                               // 动画片段实际时长（动态获取）

    [Header("孢子配置")]
    [SerializeField] private GameObject sporePrefab;
    [SerializeField] private int sporeCount = 5;
    [SerializeField] private SporeMotionData[] sporeMotions;   // 每个孢子的终点和运动曲线

    [Header("变亮")]
    [SerializeField] private AnimationCurve LightCurve;        // 亮度变化曲线
    [SerializeField] private float LightTime;                  // 亮度过渡时间
    [SerializeField] private float Darkrgb;                    // 暗时的 RGB 值（灰度乘法，0~1）
    [SerializeField] private float Lightrbg;                   // 亮时的 RGB 值（灰度乘法，0~1）
    private Coroutine LightCorotine;
    private bool IsLighten;

    [Header("重置开关")]
    [SerializeField] private bool resetOnReverseComplete = true; // 逆向归零后是否回到未激活状态并销毁孢子

    private bool isActivated;
    private float currentTime;
    private int bloomStateHash;

    private Animator anim;
    private SpriteRenderer SR;
    private List<Transform> sporeList = new List<Transform>();

    private void Awake()
    {
        anim = GetComponent<Animator>();
        SR = GetComponent<SpriteRenderer>();

        anim.speed = 0f;

        bloomStateHash = Animator.StringToHash(bloomStateName); // 用哈希查找那个动画的名字
    }

    private void Start()
    {
        // 设置初始暗色（灰度乘法，保留贴图本身色相）
        SR.color = new Color(Darkrgb, Darkrgb, Darkrgb, 1);
    }

    #region 实现接口

    public void ChangeCurrentTime(float deltaTime)
    {
        // 未激活时只响应正 deltaTime 以激活，负 deltaTime 忽略
        if (!isActivated)
        {
            if (deltaTime > 0f)
                Activate();
            else
                return;
        }

        currentTime += deltaTime;
        currentTime = Mathf.Clamp(currentTime, 0f, bloomDuration);

        float progress = currentTime / bloomDuration; // 用百分比管理这个动画和孢子（CurrentTime / EndTime）

        // 根据进度更新 Animator 的归一化时间，实现正/逆向播放。
        anim.Play(bloomStateHash, 0, progress);
        UpdateSpores(progress);

        // 逆向归零时，根据配置决定是否重置
        if (progress <= 0f && deltaTime < 0f && resetOnReverseComplete)
        {
            Deactivate();
        }
    }

    // 高亮：严格使用 NewBlock 的灰度乘法方式
    public void Lighten(bool _isLighten)
    {
        if (IsLighten == _isLighten)
        {
            return;
        }

        if (LightCorotine != null)
        {
            StopCoroutine(LightCorotine);
        }
        LightCorotine = StartCoroutine(Light(_isLighten));

        IsLighten = _isLighten;
    }

    #endregion

    #region 核心逻辑

    // 激活状态，生成孢子
    private void Activate()
    {
        Debug.Log("花苞激活，孢子数量：" + sporeCount);
        isActivated = true;
        currentTime = 0f;

        // 获取 Animator 中指定状态的动画片段时长
        anim.Play(bloomStateHash, 0, 0f);
        AnimatorClipInfo[] clipInfo = anim.GetCurrentAnimatorClipInfo(0); // 这个数组里有动画时间信息
        bloomDuration = (clipInfo.Length > 0) ? clipInfo[0].clip.length : 1f; // 后备值，防止没查到崩溃

        // 实例化孢子
        if (sporePrefab != null && sporeCount > 0)
        {
            for (int i = 0; i < sporeCount; i++)
            {
                GameObject spore = Instantiate(sporePrefab, transform.position, Quaternion.identity); 
                sporeList.Add(spore.transform);
            }
        }
    }

    // 更新所有孢子的位置，完全由进度量驱动Lerp
    private void UpdateSpores(float progress)
    {
        Vector3 startPos = transform.position; // 所有孢子的起点均为当前花苞位置

        for (int i = 0; i < sporeList.Count; i++)
        {
            // 若配置了该孢子的运动数据，则使用其终点和曲线；否则停留在起点
            if (sporeMotions != null && i < sporeMotions.Length)
            {
                SporeMotionData data = sporeMotions[i];

                // 保护：曲线为空则使用线性进度（匀速移动）
                float xT = (data.xCurve != null) ? data.xCurve.Evaluate(progress) : progress;
                float yT = (data.yCurve != null) ? data.yCurve.Evaluate(progress) : progress;

                float x = Mathf.Lerp(startPos.x, data.endPosition.x, xT);
                float y = Mathf.Lerp(startPos.y, data.endPosition.y, yT);
                sporeList[i].position = new Vector3(x, y, startPos.z);
            }
            else
            {
                sporeList[i].position = startPos;
            }
        }
    }

    // 重置到未激活状态：销毁所有孢子，时间归零，动画归零，关闭高亮。
    private void Deactivate()
    {
        isActivated = false;
        currentTime = 0f;

        foreach (Transform t in sporeList)
            Destroy(t.gameObject);
        sporeList.Clear();

        anim.Play(bloomStateHash, 0, 0f);
        Lighten(false);
    }

    #endregion

    #region 高亮协程（严格使用 NewBlock 的灰度乘法方式）

    IEnumerator Light(bool _isLighten)
    {
        float orginrbg = SR.color.r;
        float targetrbg = _isLighten ? Lightrbg : Darkrgb;
        float timer = 0;
        while (timer < LightTime)
        {
            timer += Time.deltaTime;
            float rbg = Mathf.Lerp(orginrbg, targetrbg, LightCurve.Evaluate(timer / LightTime));
            SR.color = new Color(rbg, rbg, rbg, 1);
            yield return null;
        }
        SR.color = new Color(targetrbg, targetrbg, targetrbg, 1);
    }

    #endregion

}