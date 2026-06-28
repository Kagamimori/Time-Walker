using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public MCscript MC;
    public float PlayerMaxHP;
    private float _playerHP;
    public NewHeart newHeartHP;

    public bool TestMode;
    public float PlayerHP
    {
        get { return _playerHP; }
        set
        {
            _playerHP = Mathf.Clamp(value, 0, PlayerMaxHP);

            if (newHeartHP == null)
            {
                BindUIReferences();
            }
            if (newHeartHP != null)
            {
                newHeartHP.SetValue(_playerHP / PlayerMaxHP);
            }
        }
    }

    public float PlayerMaxMP;
    private float _playerMP;
    public NewHeart newHeartMP;
    public float PlayerMP
    {
        get { return _playerMP; }
        set
        {
            _playerMP = Mathf.Clamp(value, 0, PlayerMaxMP);

            if (newHeartMP == null)
            {
                BindUIReferences();
            }
            if (newHeartMP != null)
            {
                newHeartMP.SetValue(_playerMP / PlayerMaxMP);
            }
        }
    }

    [Range(0f, 100f)]
    public float TestHP;
    [Range(0f, 100f)]
    public float TestMP;

    public static PlayerManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 订阅场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // 取消订阅，防止内存泄漏
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        BindUIReferences();
        PlayerHP = PlayerMaxHP;
        PlayerMP = PlayerMaxMP;
    }

    void Update()
    {
        if (TestMode)
        {
            if (PlayerHP != TestHP)
            {
                PlayerHP = TestHP;
            }
            if (PlayerMP != TestMP)
            {
                PlayerMP = TestMP;
            }
        }
    }

    // 场景加载完成后调用
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 重新绑定 UI（新场景的 UI 对象）
        BindUIReferences();
        // 重置血量到满值
        PlayerHP = PlayerMaxHP;
        PlayerMP = PlayerMaxMP;
    }

    // 每次启用时检查引用，确保 UI 有效（比如场景激活时）
    void OnEnable()
    {
        if (newHeartHP == null || newHeartMP == null)
        {
            BindUIReferences();
        }
    }

    // 绑定 UI 引用
    private void BindUIReferences()
    {
        // 如果血条引用为空，尝试查找
        if (newHeartHP == null)
        {
            newHeartHP = FindObjectOfType<NewHeart>();
        }
        // 如果蓝条引用为空，尝试查找（注意：如果血条蓝条是同一个脚本的不同对象，这里可能找到同一个，需要区分）
        if (newHeartMP == null)
        {
            newHeartMP = FindObjectOfType<NewHeart>();
        }
        // 如果场景中血条和蓝条是不同的 GameObject，但挂载相同的 NewHeart 脚本，
        // 则上面的 FindObjectOfType 只会返回第一个，可能导致重复。
        // 如果你有单独的蓝条组件，可以单独查找或通过 Inspector 手动赋值。
        // 这里建议在 Inspector 中直接拖拽赋值，更加可靠。
        // 若无法拖拽，可改用更精确的查找方式（如通过名称或标签）。
    }
}