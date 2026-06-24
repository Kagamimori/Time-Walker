using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public float knockbackForce = 8f;
    public float stunTime = 0.3f;

    private Rigidbody2D rb;
    private Animator anim;
    private MonsterController enemyController;
    private HitEffectController hitEffectController;
    private bool isStunned = false;
    private bool isInvincible = false;

    [Header("宝石掉落")]
    public GameObject gemPrefab;
    public float gemInitialUpSpeed = 6f;
    public float gemGroundOffset = 0.3f;      // 落地Y偏移（相对于怪物位置）
    public float gemSpawnOffsetY = 0.5f;      // 生成Y偏移（相对于怪物位置）
    public float gemSpawnDelay = 0.4f;        // ★ 新增：死亡后延迟多久生成宝石

    private Coroutine invincibleCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyController = GetComponent<MonsterController>();
        hitEffectController = GetComponent<HitEffectController>();
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        if (currentHealth <= 0 || isInvincible) return;

        currentHealth -= damage;

        if (hitEffectController != null)
            hitEffectController.TriggerHitEffect();

        Vector2 knockbackDirection = (transform.position - attacker.position).normalized;
        rb.velocity = new Vector2(knockbackDirection.x * knockbackForce, rb.velocity.y);

        StartCoroutine(StunCoroutine());

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
            enemyController.enabled = false;

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

        // 播放死亡特效（不恢复材质）
        if (hitEffectController != null)
            hitEffectController.TriggerHitEffect(1, false);

        // 禁用碰撞体
        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        // ★ 延迟生成宝石
        if (gemPrefab != null)
        {
            StartCoroutine(SpawnGemAfterDelay(gemSpawnDelay));
        }

        this.enabled = false;
        Destroy(gameObject, 0.93f);  // 销毁延时需大于宝石延迟 + 特效时长
    }

    private IEnumerator SpawnGemAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 生成在怪物位置 + Y偏移
        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y + gemSpawnOffsetY, transform.position.z);
        GameObject gemObj = Instantiate(gemPrefab, spawnPos, Quaternion.identity);
        Gem gem = gemObj.GetComponent<Gem>();
        if (gem != null)
        {
            float groundY = transform.position.y + gemGroundOffset;
            gem.Initialize(groundY, gemInitialUpSpeed);
        }
        else
        {
            Debug.LogWarning("宝石预制体缺少 Gem 组件！");
        }
    }

    public void TriggerHitEffect()
    {
        if (hitEffectController != null)
            hitEffectController.TriggerHitEffect(0);
    }

    void Update() { }
}