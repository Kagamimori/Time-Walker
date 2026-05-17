using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public MCscript MC;
    public float PlayerMaxHP;
    private float _playerHP;
    public NewHeart newHeartHP;
    public float PlayerHP
    {
        get
        {
            return _playerHP;
        }
        set
        {
            _playerHP = value;
            newHeartHP.SetValue(value/PlayerMaxHP);
        }
    }
    [Range(0f,100f)]
    public float TestHP;


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
        TestHP = PlayerMaxHP;
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerHP != TestHP)
        {
            PlayerHP = TestHP;
        }
    }
}
