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

    [Header("色変化設定")]
    public SpriteRenderer[] spriteRenderers; // 色を変えたいスプライトたち
    public Color maxHealthColor = Color.white; // 血液MAX時の色
    public Color minHealthColor = Color.red;   // 血液がゼロに近い時の色

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

        UpdateSpriteColor();
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

    // ここが色を変える処理
    private void UpdateSpriteColor()
    {
        // 血液量の割合を計算する
        float bloodRatio = currentBlood / maxBlood;

        // 0〜1の間に制限（万が一0未満や1超えを防ぐ）
        bloodRatio = Mathf.Clamp01(bloodRatio);

        // 血液が多いと白に近く、減ると赤に近づく
        Color currentColor = Color.Lerp(minHealthColor, maxHealthColor, Mathf.Pow(bloodRatio, 0.5f));

        // 複数のスプライトがあれば全て色を変える
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer != null)
            {
                renderer.color = currentColor;
            }
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

    // 共通のダメージ処理
    private void HandleDamage(string tag)
    {
        // すでに死亡していたら何もしない
        if (currentBlood <= 0) return;

        // 衝突したオブジェクトのタグに基づきダメージを計算
        int damage = GetDamageFromTag(tag, "Human");

        if(damage <= 0) return; // ダメージが0なら何もしない

        // ダメージがあれば血液量を減らす
        if (damage > 0)
        {
            currentBlood -= damage;
            Debug.Log($"{gameObject.name} が {damage} ダメージを受けた！（残り血液量: {currentBlood}）");
        }

        // 血液量が0以下になったら死亡
        if (currentBlood <= 0)
        {
            Dead();
        }

        // 血液パーティクルを再生
        Vector3 hitPos = human.centerPoint.position; // 衝突位置（例として現在位置を使用）
        Quaternion finalRot = Quaternion.identity; // 衝突面の回転（必要に応じて計算する）

        // ダメージ量に応じて血液パーティクルを変更
        if (damage < 10)
        {
            // Lowダメージ時
            GameReferences.Instance.particleManager.PlayParticle(
                ParticleManager.ParticleType.Blood_Low,  // 血のパーティクル（Low）
                hitPos,                                   // 衝突位置
                finalRot                                  // 衝突面に基づく回転
            );
        }
        else if (damage < 50)
        {
            // Midダメージ時
            GameReferences.Instance.particleManager.PlayParticle(
                ParticleManager.ParticleType.Blood_Mid,  // 血のパーティクル（Mid）
                hitPos,                                   // 衝突位置
                finalRot                                  // 衝突面に基づく回転
            );
        }


        // 血液量が0以下になったらオブジェクトを破壊する
        if (currentBlood <= 0)
        {
            // Highダメージ時
            GameReferences.Instance.particleManager.PlayParticle(
                ParticleManager.ParticleType.Blood_High,  // 血のパーティクル（High）
                hitPos,                                   // 衝突位置
                finalRot                                  // 衝突面に基づく回転
            );

            Destroy(gameObject);  // ここで即削除
            return; // これ以上なにもしない（パーティクル処理とかスキップ）
        }
    }

    // 【追加】タグからダメージを取得する関数
    private int GetDamageFromTag(string tag, string targetType)
    {
        // タグがnull、または"@"から始まってないなら無視
        if (string.IsNullOrEmpty(tag) || tag[0] != '@') return 0;

        // "@"を除いた部分を分解
        string[] parts = tag.Substring(1).Split('_');
        if (parts.Length == 2)
        {
            string tagType = parts[0];
            int dmg;
            if (int.TryParse(parts[1], out dmg))
            {
                // 自分に向けた攻撃か判定（Allはすべてに当たる）
                if (tagType == "All" || tagType == targetType)
                {
                    return dmg;
                }
            }
        }
        return 0; // 条件を満たさなければダメージ0
    }

}
