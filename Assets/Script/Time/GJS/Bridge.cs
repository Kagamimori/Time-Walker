using System.Collections;
using UnityEngine;

public class Bridge2 : MonoBehaviour, ITimeControlable
{
    // ----- 原有属性（保留）-----
    public bool CanReserveTime { get; set; }
    [SerializeField] private Vector3 StartPos;
    [SerializeField] private Vector3 EndPos;
    [SerializeField] public AnimationCurve Xcurve;
    [SerializeField] public AnimationCurve Ycurve;
    [SerializeField] private float CurrentTime;
    [SerializeField] private float EndTime;

    [Header("变亮")]
    [SerializeField] private AnimationCurve LightCurve;
    [SerializeField] private float LightTime;
    [SerializeField] private float Darkrgb;
    [SerializeField] private float Lightrbg;
    private SpriteRenderer SR;
    private Coroutine LightCorotine;
    private bool IsLighten;

    // ----- 新增合成相关属性 -----
    [Header("合成对象")]
    [SerializeField] private GameObject bridge1;   // 桥1（静态，只参与淡出）
    [SerializeField] private GameObject bridge3;   // 桥3（目标产物，淡入显示）

    [Header("过渡参数")]
    [SerializeField] private float fadeOutDuration = 1f;   // 桥1、2淡出总时长
    [SerializeField] private float fadeInDuration = 1f;    // 桥3淡入总时长
    [SerializeField] private float startDelay = 0.3f;      // 桥3开始淡入相对桥1、2开始淡出的延迟
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.Linear(0, 0, 1, 1); // 淡出曲线（默认线性）
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.Linear(0, 0, 1, 1);  // 淡入曲线

    private SpriteRenderer sr1;          // 桥1的SpriteRenderer
    private SpriteRenderer sr3;          // 桥3的SpriteRenderer
    private bool isTransitioning = false; // 是否正在过渡中
    private bool transitionTriggered = false; // 是否已触发过渡（防止重复）

    // ----- 生命周期 -----
    void Start()
    {
        CanReserveTime = true;
        SR = GetComponent<SpriteRenderer>();
        transform.position = StartPos;
        SR.color = new Color(Darkrgb, Darkrgb, Darkrgb, 1);

        // 获取桥1和桥3的SpriteRenderer（若未拖入则记录错误）
        if (bridge1 != null)
            sr1 = bridge1.GetComponent<SpriteRenderer>();
        else
            Debug.LogError("Bridge2: 桥1未拖入！");

        if (bridge3 != null)
        {
            sr3 = bridge3.GetComponent<SpriteRenderer>();
            // 初始化桥3为完全透明
            if (sr3 != null)
                sr3.color = new Color(sr3.color.r, sr3.color.g, sr3.color.b, 0);
        }
        else
            Debug.LogError("Bridge2: 桥3未拖入！");
    }

    // ----- ITimeControlable 实现 -----
    public void ChangeCurrentTime(float deltaTime)
    {
        // 过渡中不再处理位移
        if (isTransitioning) return;

        CurrentTime += deltaTime;
        CurrentTime = Mathf.Clamp(CurrentTime, 0, EndTime);

        // 更新位置（使用曲线插值）
        float progress = CurrentTime / EndTime;
        float x = Mathf.Lerp(StartPos.x, EndPos.x, Xcurve.Evaluate(progress));
        float y = Mathf.Lerp(StartPos.y, EndPos.y, Ycurve.Evaluate(progress));
        transform.position = new Vector3(x, y, transform.position.z);

        // 到达终点 → 触发合成过渡
        if (CurrentTime >= EndTime && !transitionTriggered)
        {
            StartTransition();
        }
    }

    // ----- 变亮功能（过渡中禁用）-----
    public void Lighten(bool _isLighten)
    {
        if (isTransitioning) return; // 过渡中忽略变亮请求

        if (IsLighten == _isLighten) return;

        if (LightCorotine != null)
            StopCoroutine(LightCorotine);
        LightCorotine = StartCoroutine(Light(_isLighten));
        IsLighten = _isLighten;
    }

    private IEnumerator Light(bool _isLighten)
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

    // ----- 启动合成过渡 -----
    private void StartTransition()
    {
        transitionTriggered = true;
        isTransitioning = true;

        // 若有正在运行的变亮协程则停止
        if (LightCorotine != null)
            StopCoroutine(LightCorotine);

        StartCoroutine(TransitionCoroutine());
    }

    // ----- 合成过渡协程（同时控制三个对象的透明度）-----
    private IEnumerator TransitionCoroutine()
    {
        // 获取桥1、桥2的初始颜色（保留RGB，只改alpha）
        Color color1 = sr1 != null ? sr1.color : Color.white;
        Color color2 = SR.color;

        float startTime = Time.time;
        float elapsed = 0f;
        float totalDuration = startDelay + fadeInDuration; // 总过渡时间（桥3完全显示的时刻）

        while (elapsed < totalDuration)
        {
            elapsed = Time.time - startTime;

            // ----- 桥1和桥2淡出（从alpha=1到0）-----
            float fadeOutProgress = Mathf.Clamp01(elapsed / fadeOutDuration);
            float alphaOut = Mathf.Lerp(1, 0, fadeOutCurve.Evaluate(fadeOutProgress));
            if (sr1 != null)
            {
                Color c = color1;
                c.a = alphaOut;
                sr1.color = c;
            }
            if (SR != null)
            {
                Color c = color2;
                c.a = alphaOut;
                SR.color = c;
            }

            // ----- 桥3淡入（延迟startDelay后从alpha=0到1）-----
            float fadeInElapsed = elapsed - startDelay;
            if (fadeInElapsed > 0)
            {
                float fadeInProgress = Mathf.Clamp01(fadeInElapsed / fadeInDuration);
                float alphaIn = Mathf.Lerp(0, 1, fadeInCurve.Evaluate(fadeInProgress));
                if (sr3 != null)
                {
                    Color c = sr3.color;
                    c.a = alphaIn;
                    sr3.color = c;
                }
            }
            // 未到延迟时保持桥3透明（已初始化为0，但这里重复赋值确保）
            else
            {
                if (sr3 != null)
                {
                    Color c = sr3.color;
                    c.a = 0;
                    sr3.color = c;
                }
            }

            yield return null;
        }

        // ----- 最终状态修正（确保精确）-----
        // 桥1、桥2完全透明
        if (sr1 != null) { Color c = sr1.color; c.a = 0; sr1.color = c; }
        if (SR != null) { Color c = SR.color; c.a = 0; SR.color = c; }
        // 桥3完全不透明
        if (sr3 != null) { Color c = sr3.color; c.a = 1; sr3.color = c; }

        // 隐藏桥1和桥2（设为不活动），桥3保留
        if (bridge1 != null) bridge1.SetActive(false);
        gameObject.SetActive(false); // 桥2自身隐藏

        // 注意：桥3保持活动且完全不透明，合成完成
    }

   
}