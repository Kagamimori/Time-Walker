using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public float knockbackForce = 8f;       // 击退力度
    public float stunTime = 0.3f;           // 受击硬直时间

    private Rigidbody2D rb;
    private Animator anim;
    private MonsterController enemyController; //移动控制脚本
    private HitEffectController hitEffectController;
    private bool isStunned = false;
    private bool isInvincible = false;

    [Header("宝石掉落")]
    public GameObject gemPrefab;            // 宝石预制体
    public float gemInitialUpSpeed = 6f;    // 初始上抛速度
    public float gemGroundOffset = 0f;      // 地面Y偏移（宝石落地的相对高度，例如-0.5表示落在怪物下方）


    private Coroutine invincibleCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyController = GetComponent<MonsterController>();
        hitEffectController = GetComponent<HitEffectController>();   // 获取特效组件
    }
    public void TakeDamage(int damage, Transform attacker)//受击
    {
        if (currentHealth <= 0 || isInvincible) return;

        currentHealth -= damage;
        //isInvincible = true;

        if (hitEffectController != null)
            hitEffectController.TriggerHitEffect();

        // 播放受击动画
        //anim.SetTrigger("Hurt");
        // 击退：向攻击者的反方向弹开
        Vector2 knockbackDirection = (transform.position - attacker.position).normalized;
        rb.velocity = new Vector2(knockbackDirection.x * knockbackForce, rb.velocity.y);
        //rb.velocity = new Vector2(knockbackDirection.x * knockbackForce, knockbackForce * 0.5f); // 受击弹起

        // 进入硬直状态（暂停移动逻辑）
        StartCoroutine(StunCoroutine());

        // 检查死亡
        if (currentHealth <= 0)
        {
            Debug.Log(2);
            Die();
        }
       
    }
    
    IEnumerator StunCoroutine()
    {
        isStunned = true;
        if (enemyController != null)
            enemyController.enabled = false;   // 暂停AI移动（脚本）

        yield return new WaitForSeconds(stunTime);

        isStunned = false;
        if (enemyController != null)
            enemyController.enabled = true;
    }

    public void Die()
    {
        StopAllCoroutines();
        if (enemyController != null)
            enemyController.enabled = false;

        if (hitEffectController != null)
            hitEffectController.TriggerHitEffect(1);
        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        // ---- 生成宝石 ----
        if (gemPrefab != null)
        {
            // 实例化宝石在怪物位置
            GameObject gemObj = Instantiate(gemPrefab, transform.position, Quaternion.identity);
            Gem gem = gemObj.GetComponent<Gem>();
            if (gem != null)
            {
                // 计算地面Y：怪物位置Y + 偏移（可根据需求调整）
                float groundY = transform.position.y + gemGroundOffset;
                gem.Initialize(groundY, gemInitialUpSpeed);
            }
            else
            {
                Debug.LogWarning("宝石预制体缺少 Gem 组件！");
            }
        }

        this.enabled = false;
        Destroy(gameObject, 0.15f);
    }
    public void TriggerHitEffect()
    {
        if (hitEffectController != null)
            hitEffectController.TriggerHitEffect(0);
    }
    void Update()
    {
        
    }
}
