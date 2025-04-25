using UnityEngine;
using UnityEngine.Events;
using TMPro; // TextMeshProを使う場合に必要

public class HumanStats : MonoBehaviour
{
    [Header("基本ステータス")]
    public float maxBlood = 100f; // 最大HP
    public float currentBlood;    // 現在のHP
    public float lifespan = 60f;   // 寿命（秒）
    public float age = 0f;  // 経過時間（寿命に使用）

    [Header("あたり判定")]
    public Collider hitbox;

    [Header("参照")]
    public Human human; // Humanクラスの参照
    public WeaponPickup weaponPickup; // 武器を持つスクリプト
    public CloneSpawner cloneSpawner; // クローンを生成するスクリプト



    public Camera cam;
    public bool IsInitiative = false;
    public UnityEvent OnDeath; // 死亡時イベント

    private void Start()
    {
        currentBlood = maxBlood;
    }

    private void Update()
    {
        if (!human.isDead || IsInitiative)
        {
            HandleLifespan();

            if (currentBlood <= 0)
            {
                Dead();
            }
        }
    }

    private void HandleLifespan()
    {
        age += Time.deltaTime;

        if (age >= lifespan)
        {
            Dead();
        }
    }

    public void Dead()
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
