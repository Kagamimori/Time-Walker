using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MCscript : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator anim;
    private SpriteRenderer spriteRenderer;
    public float Movecontroller;
    public float OringinMoveSpeed;
    public float Movespeed;
    [SerializeField] private float Accelerationspeed;
    [SerializeField] private float Jumpspeed;

    [SerializeField] private float JumpAcceleration;
    [SerializeField] private float FallAcceleration;

    [SerializeField] private float dashspeed;
    [SerializeField] private float DashCD;
    [SerializeField] private float JumpAddTime;
    [SerializeField] private float BiteGroundSpeed;
    private float JumpTime;
    public bool IsJumping;
    public MCFoot RF;
    public MCFoot LF;
    private bool Candash = true;
    public bool IsGround = true;
    public float LocalScaleLock = 1;
    public bool IsDashing = false;

    private float dashdirection;
    private float LastDashTime = -1;
    public int Xdirection;
    private float Xspeed;

    public Vector2 MouseDirection;
    public float MouseDistance;
    public float MouseAngle;

    public enum Anim { wait, run, jump, fall, dash, open, openrun };
    public Anim state;


    [Header("咬合")]
    public bool Openmouth = false;
    public bool Closemouth = false;

    public BITE bite;
    public float BiteDirection;

    [Header("其他玩意的调用")]
    public MCCollider MCcollider;
    public GameObject MCshade;
    private InputManager inputManager;
    private PlayerManager playerManager;

    private int JumpNum = 0;
    public int JumpMaxNum;

    public float ScaleRate = 1;
    public bool IsInPlatform = false;
    // 新增字段
    private Transform currentPlatform;
    private Vector3 PlatformLastPos;
    private float PlatformXSpeed;
    public bool IsInGrass = false;
    public bool IsHitStun = false;
    public float HitStunDuration = 0.3f;
    private float preHitFacing = 1; // 记录受击前的朝向

    private HitEffectController hitEffectController; // 特效控制器
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        inputManager = InputManager.Instance;
        playerManager = PlayerManager.Instance;
        inputManager.GetNormalKeyEvent("Jump").AddListener(Jump);
        inputManager.GetNormalKeyEvent("Dash").AddListener(Dash);
        bool SB = Candash;//解除SB报错

        hitEffectController = GetComponent<HitEffectController>();
    }

    void OnDestroy()
    {
        inputManager.GetNormalKeyEvent("Jump").RemoveListener(Jump);
        inputManager.GetNormalKeyEvent("Dash").RemoveListener(Dash);
    }
    // Update is called once per frame
    void Update()
    {
        // Movespeed=OringinMoveSpeed*(1+eatscore/MaxScore/2);
        Movespeed = OringinMoveSpeed;
        Movecontroller = Input.GetAxisRaw("Horizontal");
        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 MCpos = new Vector2(transform.position.x, transform.position.y + 3f);
        MouseDistance = (mouse - MCpos).magnitude;
        MouseDirection = (mouse - MCpos).normalized;
        MouseAngle = Mathf.Atan2(MouseDirection.x, MouseDirection.y) * Mathf.Rad2Deg;
        Xspeed = rb.velocity.x;
        Xdirection = Math.Sign(Xspeed);
        IsGround = RF.IsGround || LF.IsGround;


        if (Math.Abs(Xspeed) <= Movespeed + 0.3f)
        {
            IsDashing = false;
        }

        if (IsGround == true)
        {

            if (Movecontroller == 0)
            {
                if (Math.Abs(rb.velocity.x) < 0.3f)
                {
                    rb.velocity = new Vector2(0, rb.velocity.y);
                }
                else
                {
                    rb.velocity -= new Vector2(3f * Accelerationspeed * Time.deltaTime * Xdirection, 0);
                }
            }
            if (IsDashing)
            {
                rb.velocity -= new Vector2(15 * Accelerationspeed * Xdirection * Time.deltaTime, 0);
            }
            else
            {
                if (Movecontroller != 0 && Movecontroller + Xdirection == 0 || Math.Abs(Xspeed) <= Movespeed)
                {
                    rb.velocity += new Vector2(10 * Accelerationspeed * Movecontroller * Time.deltaTime, 0);
                }

                if ((Math.Abs(Xspeed) > Movespeed) && Movecontroller == Xdirection)
                {
                    rb.velocity = new Vector2(Movecontroller * Movespeed, rb.velocity.y);
                }
            }
        }
        else
        {

            if (Movecontroller == 0)
            {                                                                        //移动
                if (Math.Abs(rb.velocity.x) < 0.3f)
                {
                    rb.velocity = new Vector2(0, rb.velocity.y);
                }
                else
                {
                    rb.velocity -= new Vector2(0.4f * Accelerationspeed * Time.deltaTime * Xdirection, 0);
                }
            }
            if (IsDashing)
            {
                rb.velocity -= new Vector2(5 * Accelerationspeed * Xdirection * Time.deltaTime, 0);
            }
            else
            {
                if (Movecontroller != 0 && Movecontroller + Xdirection == 0 || Math.Abs(Xspeed) <= Movespeed)
                {
                    rb.velocity += new Vector2(5 * Accelerationspeed * Movecontroller * Time.deltaTime, 0);
                }
                if ((Math.Abs(Xspeed) > Movespeed) && Movecontroller == Xdirection)
                {
                    rb.velocity = new Vector2(Movecontroller * Movespeed, rb.velocity.y);
                }
            }

        }

        Xspeed = rb.velocity.x;
        Xdirection = Math.Sign(Xspeed);


        if (inputManager.GetNormalKeyUp("Jump"))
        {
            IsJumping = false;
        }
        if (IsGround)
        {
            JumpNum = 0;
        }
        else if (IsInGrass)
        {
            if (IsJumping)
            {
                rb.velocity = new Vector2(rb.velocity.x, 5f);
            }
            else
            {
                rb.velocity -= new Vector2(0, 0.4f * FallAcceleration * Time.deltaTime);
            }
        }
        else if (IsJumping && Time.time - JumpTime < JumpAddTime)
        {
            rb.velocity += new Vector2(0, JumpAcceleration * Time.deltaTime);
        }
        if (!IsGround && !IsInGrass)
        {
            rb.velocity -= new Vector2(0, FallAcceleration * Time.deltaTime);
        }



        if (Input.GetKeyDown(KeyCode.J))
        {
            //KeyCode keyCode = inputManager.LastKeyCode;   //还没实现，以后做冲刺攻击和跳跃攻击用
            GenerateShade();

            Openmouth = true;
            Closemouth = true;
            if (Input.GetKey(KeyCode.W))
            {
                BiteDirection = 1;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                BiteDirection = 3;
            }
            else if (Movecontroller == 1)
            {
                BiteDirection = 2;
            }
            else if (Movecontroller == -1)
            {
                BiteDirection = 4;
            }
            else
            {
                BiteDirection = LocalScaleLock == 1 ? 2 : 4;
            }
            bite.Bite(BiteDirection);
        }                                                                                 //咬合

        if (Input.GetKeyUp(KeyCode.J))
        {
            Openmouth = false;
        }

        if (IsGround)
        {
            Candash = true;
        }

      
        UpdateCharacterFacing();
       

        state = Anim.wait;
        if (Movecontroller != 0)
        {
            state = Anim.run;
        }
        //if(!IsGround){
        //    if(rb.velocity.y>0.3f){
        //        state=Anim.jump;
        //    }
        //    else{                                                               //动画
        //        state=Anim.fall;
        //    }
        //}
        //if(IsDashing){
        //    state=Anim.dash;
        //}
        //if(Openmouth){
        //    if(IsGround&&Movecontroller!=0){
        //        state=Anim.openrun;
        //    }
        //    else{
        //        state=Anim.open;
        //    }

        //}
        anim.SetInteger("State", (int)state);


        if (!IsInPlatform && (LF.PlatForm != null || RF.PlatForm != null) && Movecontroller == 0)
        {
            IsInPlatform = true;
            Collider2D plat = LF.PlatForm != null ? LF.PlatForm : RF.PlatForm;
            currentPlatform = plat.transform;
            //PlatformLastPos = currentPlatform.position;
            transform.parent = currentPlatform;
            ScaleRate = 1 / currentPlatform.localScale.x;
        }
        else if (IsInPlatform && ((LF.PlatForm == null && RF.PlatForm == null) || Movecontroller != 0))
        {
            ScaleRate = 1;
            IsInPlatform = false;
            currentPlatform = null;
            transform.parent = null;
        }
        //if (currentPlatform != null)
        //{
        //    transform.position += (currentPlatform.position - PlatformLastPos);
        //    PlatformLastPos = currentPlatform.position;
        //}
    }

    /// <summary>
    /// 统一的朝向更新函数：直接翻转，无平滑过渡
    /// </summary>
    private void UpdateCharacterFacing()
    {
        // 受击时禁止改变朝向
        if (IsHitStun)
        {
            return;
        }

        // 1. 冲刺状态：根据冲刺方向直接翻转（带拉伸效果，但朝向是直接翻转）
        if (IsDashing)
        {
            float dashcal = (Math.Abs(Xspeed) - Movespeed) / (dashspeed - Movespeed);
            // 冲刺缩放：X轴方向直接使用 dashdirection（1或-1），无平滑过渡
            float scaleX = dashdirection * Mathf.Max(0.1f, 1 + 0.3f * dashcal);
            float scaleY = Mathf.Clamp(1 - 0.2f * dashcal, 0.2f, 1.5f);
            transform.localScale = ScaleRate * new Vector2(scaleX, scaleY);
            return;
        }

        int targetDirection = (int)LocalScaleLock;
        if (Movecontroller != 0)
        {
            targetDirection = (int)Movecontroller;
        }

        if (LocalScaleLock != targetDirection)
        {
            LocalScaleLock = targetDirection;
        }

        transform.localScale = ScaleRate * new Vector2(LocalScaleLock, 1);
    }

    private void Jump()
    {
        if (JumpNum > JumpMaxNum || !IsGround)
        {
            return;
        }
        GenerateShade();
        //rb.velocity = new Vector2(Xspeed, Jumpspeed * (((JumpMaxNum - JumpNum) * (JumpMaxNum - JumpNum)) / (JumpMaxNum * JumpMaxNum)));
        rb.velocity = new Vector2(Xspeed, Jumpspeed);
        JumpNum++;
        JumpTime = Time.time;
        IsJumping = true;
    }
    private void Dash()
    {
        if (Time.time - LastDashTime < DashCD)
        {
            return;
        }
        GenerateShade();
        anim.SetInteger("State", 4);
        dashdirection = (int)LocalScaleLock;
        if (Movecontroller != 0)
        {
            dashdirection = (int)Movecontroller;
            LocalScaleLock = (int)Movecontroller;                                        //冲刺
            // 冲刺瞬间直接翻转
            transform.localScale = ScaleRate * new Vector2(LocalScaleLock, 1);
        }
        rb.velocity = new Vector2(dashspeed * dashdirection, 0);
        IsDashing = true;
        LastDashTime = Time.time;
    }
    public void BiteGround(float rate)
    {
        if (!(MouseAngle < -110 || MouseAngle > 110))
        {
            return;
        }
        rb.velocity = new Vector2(-rate * BiteGroundSpeed * MouseDirection.x + rb.velocity.x, -rate * BiteGroundSpeed * MouseDirection.y);
        if (Math.Abs(rb.velocity.x) > Movespeed + 0.3f)
        {
            IsDashing = true;
            dashdirection = Math.Sign(rb.velocity.x);
        }
        Candash = true;
    }
    public void AddSpeedDirected(float AddAmount)
    {
        Vector2 AddSpeed = AddAmount * MouseDirection;
        rb.velocity += AddSpeed;
        bool XNeed = false;
        bool YNeed = false;
        if (AddSpeed.x > 0 ? rb.velocity.x < AddSpeed.x : rb.velocity.x > AddSpeed.x)
        {
            XNeed = true;
        }
        if (AddSpeed.y > 0 ? rb.velocity.y < AddSpeed.y : rb.velocity.y > AddSpeed.y)
        {
            YNeed = true;
        }
        rb.velocity = new Vector2(XNeed ? AddSpeed.x : rb.velocity.x, YNeed ? AddSpeed.y : rb.velocity.y);
        if (Math.Abs(rb.velocity.x) > Movespeed + 0.3f)
        {
            IsDashing = true;
            dashdirection = Math.Sign(rb.velocity.x);
        }
    }
    IEnumerator DieBlack()
    {
        Color originalColor = spriteRenderer.color;
        float elapsedTime = 0f;
        while (elapsedTime < 2.5f)
        {
            float progress = elapsedTime / 2.5f;
            spriteRenderer.color = Color.Lerp(originalColor, Color.black, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = Color.black;
    }

    public void LetUp(int UpDirection, float UpSpeed)
    {
        Candash = true;
        switch (UpDirection)
        {
            case 1:
                rb.velocity = new Vector2(0, UpSpeed);
                break;
            case 2:
                rb.velocity = new Vector2(0, -UpSpeed);                     //被弹射
                break;
            case 3:
                IsDashing = true;
                dashdirection = -1;
                rb.velocity = new Vector2(-UpSpeed, 0);
                break;
            case 4:
                IsDashing = true;
                dashdirection = 1;
                rb.velocity = new Vector2(UpSpeed, 0);
                break;
        }
    }
    public void GenerateShade()
    {
        GameObject shade = Instantiate(MCshade, transform.position, transform.rotation);
        shade.transform.localScale = transform.localScale / ScaleRate;
        shade.GetComponent<MCShade>().Shadef(spriteRenderer.sprite);
    }
    public void TakeDamage(float damage)
    {
        float currentHP = playerManager.PlayerHP;
        if (currentHP - damage <= 0)
        {
            Debug.LogError("Death");
        }
        else
        {
            playerManager.PlayerHP = currentHP - damage;
        }

        // 记录受击前的朝向
        preHitFacing = LocalScaleLock;

        IsHitStun = true;
        Invoke(nameof(EndHitStun), HitStunDuration);

        if (hitEffectController != null)
        {
            hitEffectController.TriggerHitEffect(0);
        }

        // 受击时锁定当前朝向
        LocalScaleLock = Math.Sign(transform.localScale.x);
        transform.localScale = ScaleRate * new Vector2(LocalScaleLock, 1);
    }
    private void EndHitStun()
    {
        IsHitStun = false;
        LocalScaleLock = preHitFacing;
        transform.localScale = ScaleRate * new Vector2(LocalScaleLock, 1);
    }

    public void CollectGem(int value)
    {
        if (hitEffectController != null)
            hitEffectController.TriggerHitEffect(1);
        playerManager.PlayerMP += value;
    }
}