using UnityEngine;
using UnityEngine.Events;
using TMPro; // TextMeshProを使う場合に必要

public class HumanStats : MonoBehaviour
{
    [Header("基本ステータス")]
    public float maxHealth = 100f; // 最大HP
    public float currentHealth;    // 現在のHP
    public float lifespan = 60f;   // 寿命（秒）
    public float age = 0f;  // 経過時間（寿命に使用）

    [Header("あたり判定")]
    public Collider hitbox;


    [Header("参照")]
    public Human human; // Humanクラスの参照
    public WeaponPickup weaponPickup; // 武器を持つスクリプト
    public CloneSpawner cloneSpawner; // クローンを生成するスクリプト

    [Header("UI表示")]
    public TMP_Text lifespanText; // 寿命を表示するTMP（設定しておく）
    public TMP_Text lifeText; // 寿命を表示するTMP（設定しておく）
    public Transform bloodGaugeObject; // ゲージを動かすオブジェクト（例：赤いバーの中身）

    [Header("ゲージ設定")]
    public float fullGaugeY = 1.0f;   // 体力100%時のローカルY
    public float emptyGaugeY = 0.0f;  // 体力0%時のローカルY

    public Camera cam;
    public bool IsInitiative = false;
    public UnityEvent OnDeath; // 死亡時イベント

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (!human.isDead || IsInitiative)
        {
            HandleLifespan();
        }
    }

    private void HandleLifespan()
    {
        age += Time.deltaTime;

        if (age >= lifespan)
        {
            Die();
        }
    }

    public void TakeDamage(float amount)
    {
        if (human.isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (human.isDead) return;

        OnDeath?.Invoke();


        human.isDead = true; // 死亡フラグを立てる
        human.isIKActive = false; // IKを無効化
        human.ActivateRagdoll();

        Debug.Log($"{gameObject.name} が死亡しました");

        if (cloneSpawner != null)
        {
            cloneSpawner.SpawnClone();
        }
        else
        {
            Debug.LogWarning("CloneSpawnerが設定されていません。");
        }

    }
}
