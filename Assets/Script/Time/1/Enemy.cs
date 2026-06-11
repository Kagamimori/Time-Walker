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
    private bool isStunned = false;
    private bool isInvincible = false;

    private Coroutine invincibleCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyController = GetComponent<MonsterController>();
    }
    public void TakeDamage(int damage, Transform attacker)//受击
    {
        if (currentHealth <= 0 || isInvincible) return;

        currentHealth -= damage;
        isInvincible = true;

        // 播放受击动画
        anim.SetTrigger("Hurt");

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

    void Die()
    {
        StopAllCoroutines();
        if (enemyController != null)
            enemyController.enabled = false;  //停脚本

        anim.SetTrigger("Die");
        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;  //关碰撞

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        this.enabled = false; //停脚本
        // 延迟销毁（等死亡动画播完）
        Destroy(gameObject, 1f);
    }

    void Update()
    {
        
    }
}
