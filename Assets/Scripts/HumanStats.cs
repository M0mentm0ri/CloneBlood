using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections; // TextMeshProを使う場合に必要

public class HumanStats : MonoBehaviour
{
    [Header("基本ステータス")]
    public float maxBlood = 100f; // 最大HP
    public float currentBlood;    // 現在のHP
    public float lifespan = 60f;   // 寿命（秒）
    public float age = 0f;  // 経過時間（寿命に使用）

    [Header("あたり判定")]
    public Collider hitbox;

    [Header("無敵設定")]
    public float invincibleDuration = 3f; // 無敵時間（秒）
    private bool isInvincible = false;    // 無敵状態フラグ

    [Header("参照")]
    public Human human; // Humanクラスの参照
    public WeaponPickup weaponPickup; // 武器を持つスクリプト
    public CloneSpawner cloneSpawner; // クローンを生成するスクリプト

    [Header("色変化設定")]
    public SpriteRenderer[] spriteRenderers; // 色を変えたいスプライトたち
    public Color blinkColor; // 点滅時の色

    public Camera cam;
    public bool IsInitiative = false;
    public UnityEvent OnDeath; // 死亡時イベント

    private void Start()
    {
        currentBlood = maxBlood;
        StartCoroutine(InvincibilityRoutine());
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

        // Highダメージ時
        GameReferences.Instance.particleManager.PlayParticle(
            ParticleManager.ParticleType.Blood_Mid,  // 血のパーティクル（High）
            human.centerPoint.position,                                   // 衝突位置
            Quaternion.identity                      // 衝突面に基づく回転
        );

        human.isDead = true; // 死亡フラグを立てる
        human.isIKActive = false; // IKを無効化
        human.ActivateRagdoll();

        Debug.Log($"{gameObject.name} が死亡しました");

        if (cloneSpawner != null)
        {
            cloneSpawner.RespawnClone();
        }
        else
        {
            Debug.LogWarning("CloneSpawnerが設定されていません。");
        }

    }


    // トリガー衝突時に呼ばれる
    private void OnTriggerEnter(Collider other)
    {
        HandleDamage(other.tag); // 衝突したオブジェクトのタグでダメージ処理
    }

    // パーティクルとの衝突時に呼ばれる
    private void OnParticleCollision(GameObject other)
    {
        HandleDamage(other.tag); // 衝突したオブジェクトのタグでダメージ処理
    }

    // 無敵時間＋点滅風カラー変化処理
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        float elapsed = 0f;
        float blinkCycle = 1f; // 点滅サイクル（1秒で往復）
        Color originalColor = spriteRenderers[0].color;

        while (elapsed < invincibleDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.PingPong(Time.time * (1f / blinkCycle), 1f);
            Color lerpedColor = Color.Lerp(originalColor, blinkColor, t);

            foreach (var renderer in spriteRenderers)
            {
                if (renderer != null)
                    renderer.color = lerpedColor;
            }

            yield return null;
        }

        // ✨ 無敵終了後、なめらかに元の色に戻す
        float restoreDuration = 0.5f; // 徐々に戻す時間（0.5秒など）
        float restoreElapsed = 0f;
        Color currentColor = spriteRenderers[0].color;

        while (restoreElapsed < restoreDuration)
        {
            restoreElapsed += Time.deltaTime;
            float t = restoreElapsed / restoreDuration;
            Color backToOriginal = Color.Lerp(currentColor, originalColor, t);

            foreach (var renderer in spriteRenderers)
            {
                if (renderer != null)
                    renderer.color = backToOriginal;
            }

            yield return null;
        }

        // 念のため完全に戻す
        foreach (var renderer in spriteRenderers)
        {
            if (renderer != null)
                renderer.color = originalColor;
        }

        isInvincible = false;
    }


    // 共通のダメージ処理
    private void HandleDamage(string tag)
    {
        // 🔒 無敵中は処理しない
        if (isInvincible) return;

        if (currentBlood <= 0) return;

        // ダメージ処理以下は既存コードと同じ
        int damage = GameReferences.Instance.GetDamageFromTag(tag, "Human");

        if (damage <= 0) return;

        currentBlood -= damage;
        Debug.Log($"{gameObject.name} が {damage} ダメージを受けた！（残り血液量: {currentBlood}）");

        if (currentBlood <= 0)
        {
            Dead();
        }

        Vector3 hitPos = human.centerPoint.position;
        Quaternion finalRot = Quaternion.identity;

        if (damage < 10)
        {
            GameReferences.Instance.particleManager.PlayParticle(
                ParticleManager.ParticleType.Blood_Low,
                hitPos,
                finalRot
            );
        }
        else if (damage < 50)
        {
            GameReferences.Instance.particleManager.PlayParticle(
                ParticleManager.ParticleType.Blood_Mid,
                hitPos,
                finalRot
            );
        }

        if (currentBlood <= 0)
        {
            GameReferences.Instance.particleManager.PlayParticle(
                ParticleManager.ParticleType.Blood_High,
                hitPos,
                finalRot
            );

            Destroy(gameObject);
        }
    }

}
