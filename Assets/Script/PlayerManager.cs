using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public MCscript MC;
    public float PlayerMaxHP;
    private float _playerHP;
    public NewHeart newHeartHP;

    public bool TestMode;
    public float PlayerHP
    {
        get
        {
            return _playerHP;
        }
        set
        {
            _playerHP = Mathf.Clamp(value,0,PlayerMaxHP);
            newHeartHP.SetValue(value/PlayerMaxHP);
        }
    }

    public float PlayerMaxMP;
    private float _playerMP;
    public NewHeart newHeartMP;
    public float PlayerMP
    {
        get
        {
            return _playerMP;
        }
        set
        {
            _playerMP = Mathf.Clamp(value, 0, PlayerMaxMP);
            newHeartMP.SetValue(value/PlayerMaxMP);
        }
    }
    [Range(0f,100f)]
    public float TestHP;
    [Range(0f, 100f)]
    public float TestMP;

    public static PlayerManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        PlayerHP = PlayerMaxHP;
        PlayerMP = PlayerMaxMP;
    }

    // Update is called once per frame
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
}
