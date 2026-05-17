using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class InputManager : MonoBehaviour
{
    #region ScreenSystem
    public enum ScreenType
    {
        GameScreen,
        MachineInfoScreen,
        MenuScreen
    }
    public ScreenType ActiveScreenType = ScreenType.GameScreen;
    #endregion
    #region InputSystem
    [System.Serializable]
    public struct DictionaryPair
    {
        public string name;
        public KeyCode keyCode;
        public ScreenType screenType;
        [HideInInspector]public UnityEvent IsKeyPressed;
    }
    public DictionaryPair[] NormalKeyDictionarypPair;
    public Dictionary<string, DictionaryPair> NormalKeyDictionary = new Dictionary<string, DictionaryPair>();
    public float ExtraMouseWindowTime;
    [HideInInspector]public float LastKeyDownTime=-100;
    [HideInInspector]public KeyCode LastKeyCode;

    public UnityEvent GetNormalKeyEvent(string name)
    {
        return NormalKeyDictionary[name].IsKeyPressed;
    }
    public bool GetMouseInGame(int i)
    {
        return ActiveScreenType == ScreenType.GameScreen && Input.GetMouseButton(i);
    }
    public bool GetMouseDownInGame(int i)
    {
        return ActiveScreenType == ScreenType.GameScreen && Input.GetMouseButtonDown(i);
    }
    public bool GetMouseUpInGame(int i)
    {
        return ActiveScreenType == ScreenType.GameScreen && Input.GetMouseButtonUp(i);
    }
    public bool GetNormalKeyUp(string name)
    {
        return Input.GetKeyUp(NormalKeyDictionary[name].keyCode);
    }
    #endregion
    #region public
    public static InputManager Instance { get; private set; }
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
        }
        foreach (DictionaryPair pair in NormalKeyDictionarypPair)
        {
            NormalKeyDictionary[pair.name] = pair;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            foreach (DictionaryPair pair in NormalKeyDictionary.Values)
            {
                if (Input.GetKeyDown(pair.keyCode) && ActiveScreenType == pair.screenType)
                {
                    pair.IsKeyPressed.Invoke();
                }
            }
        }
    }
    #endregion
}