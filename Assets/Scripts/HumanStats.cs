using UnityEngine;
using UnityEngine.Events;
using TMPro; // TextMeshProを使う場合に必要

public class HumanStats : MonoBehaviour
{
    [Header("基本ステータス")]
    public float maxHealth = 100f; // 最大HP
    public float currentHealth;    // 現在のHP
    public float lifespan = 60f;   // 寿命（秒）

    [Header("あたり判定")]
    public Collider hitbox;

    private float age = 0f;  // 経過時間（寿命に使用）

    [Header("参照")]
    public Human human; // Humanクラスの参照

    [Header("UI表示")]
    public TMP_Text lifespanText; // 寿命を表示するTMP（設定しておく）
    public TMP_Text lifeText; // 寿命を表示するTMP（設定しておく）
    public Transform bloodGaugeObject; // ゲージを動かすオブジェクト（例：赤いバーの中身）

    [Header("ゲージ設定")]
    public float fullGaugeY = 1.0f;   // 体力100%時のローカルY
    public float emptyGaugeY = 0.0f;  // 体力0%時のローカルY

    public UnityEvent OnDeath; // 死亡時イベント

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (!human.isDead)
        {
            HandleLifespan();
            UpdateUI();
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

    private void Die()
    {
        if (human.isDead) return;

        Debug.Log($"{gameObject.name} が死亡しました");

        OnDeath?.Invoke();
    }

    private void UpdateUI()
    {
        // 残り寿命の表示（小数点以下0桁で表示）＋透明度調整（段階的 + 色変更）
        if (lifespanText != null && lifeText)
        {
            float remaining = Mathf.Max(0, lifespan - age);
            lifespanText.text = $"{remaining:F0}";

            // デフォルトの色（白）で初期化
            Color baseColor = Color.white;

            // 透明度処理（半分を超えるまでは見えにくい）
            float halfLife = lifespan / 2f;
            float alpha = 0.1f; // 最低透明度

            if (remaining <= halfLife)
            {
                // 半分以下なら透明度を徐々に上げる（0.1〜1.0）
                float t = Mathf.InverseLerp(halfLife, 0f, remaining); // 0～1
                alpha = Mathf.Lerp(0.1f, 1f, t); // 線形補間で透明度上昇
            }

            // 色の変更（残り10秒以下なら黄色）
            if (remaining <= 10f)
            {
                baseColor = Color.yellow;
            }

            // 色の変更（残り10秒以下なら黄色）
            if (remaining <= 3f)
            {
                baseColor = Color.red;
            }

            // 色に透明度を反映
            baseColor.a = alpha;
            lifespanText.color = baseColor;
            lifeText.color = baseColor;
        }

        // 血液ゲージ（体力バー）のX位置更新
        if (bloodGaugeObject != null)
        {
            float healthPercent = Mathf.Clamp01(currentHealth / maxHealth); // 0～1
            float y = Mathf.Lerp(emptyGaugeY, fullGaugeY, healthPercent);   // 線形補間で位置を計算

            Vector3 localPos = bloodGaugeObject.localPosition;
            localPos.y = y;
            bloodGaugeObject.localPosition = localPos;
        }
    }
}
