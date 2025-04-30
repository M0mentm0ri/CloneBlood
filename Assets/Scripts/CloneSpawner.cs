using System.Collections;
using TMPro;
using UnityEngine;
using Unity.Cinemachine;

public class CloneSpawner : MonoBehaviour
{
    public GameObject clonePrefab;
    public GameObject currentClone;
    public HumanStats currentHumanStats;

    [Header("UI")]
    public TMP_Text lifespanText;
    public TMP_Text lifeText;
    public TMP_Text HealthText;
    public Transform bloodGaugeObject;

    [Header("ゲージ設定")]
    public float fullGaugeY = 1.0f;
    public float emptyGaugeY = 0.0f;

    public CinemachineCamera cinemachineCamera;
    public Camera maincamera;
    public Transform spawnPoint;

    [Header("耐久値")]
    public int maxHealth = 100; // 最大耐久
    public int currentHealth = 100; // 現在の耐久

    void Start()
    {
        currentHealth = maxHealth;
        SpawnClone();
    }

    void Update()
    {
        if (currentHumanStats != null)
        {
            UpdateUI();
        }
    }

    public void SpawnClone()
    {
        if (currentHumanStats != null)
        {
            currentHumanStats.IsInitiative = false;
        }

        currentClone = Instantiate(clonePrefab, spawnPoint.position, spawnPoint.rotation);
        currentHumanStats = currentClone.GetComponent<HumanStats>();

        if (cinemachineCamera != null && currentHumanStats != null && currentHumanStats.weaponPickup.mouthObject != null)
        {
            cinemachineCamera.Follow = currentHumanStats.weaponPickup.mouthObject;
            cinemachineCamera.LookAt = currentHumanStats.weaponPickup.mouthObject;
        }

        currentHumanStats.cloneSpawner = this;
        currentHumanStats.cam = maincamera;
        currentHumanStats.IsInitiative = true;
    }

    public void RespawnClone()
    {
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(1f);
        SpawnClone();
    }

    private void UpdateUI()
    {
        if (lifespanText != null && lifeText != null)
        {
            float remaining = Mathf.Max(0, currentHumanStats.lifespan - currentHumanStats.age);
            lifespanText.text = $"{remaining:F0}";

            Color baseColor = Color.white;
            float halfLife = currentHumanStats.lifespan / 2f;
            float alpha = 0.1f;

            if (remaining <= halfLife)
            {
                float t = Mathf.InverseLerp(halfLife, 0f, remaining);
                alpha = Mathf.Lerp(0.1f, 1f, t);
            }

            if (remaining <= 10f) baseColor = Color.yellow;
            if (remaining <= 3f) baseColor = Color.red;

            baseColor.a = alpha;
            lifespanText.color = baseColor;
            lifeText.color = baseColor;
        }

        if (bloodGaugeObject != null)
        {
            float bloodPercent = Mathf.Clamp01(currentHumanStats.currentBlood / currentHumanStats.maxBlood);
            float y = Mathf.Lerp(emptyGaugeY, fullGaugeY, bloodPercent);
            Vector3 localPos = bloodGaugeObject.localPosition;
            localPos.y = y;
            bloodGaugeObject.localPosition = localPos;
        }

        if (HealthText != null)
        {
            float percentage = ((float)currentHealth / (float)maxHealth) * 100f;
            HealthText.text = $"{Mathf.RoundToInt(percentage)}%";
        }


    }

    // ✅ OnTriggerEnterによるダメージ処理
    private void OnTriggerEnter(Collider other)
    {
        ApplyDamageFromCollider(other.tag);
    }

    // ✅ パーティクル衝突によるダメージ処理
    private void OnParticleCollision(GameObject other)
    {
        ApplyDamageFromCollider(other.tag);
    }

    // 共通ダメージ処理
    private void ApplyDamageFromCollider(string otherTag)
    {
        int damage = GameReferences.Instance.GetDamageFromTag(otherTag, "Human");
        if (damage > 0)
        {
            currentHealth -= damage;
            Debug.Log($"CloneSpawnerが{damage}のダメージを受けた！ 残りHP: {currentHealth}");

            if (currentHealth <= 0)
            {
                GameOver();
            }
        }
    }

    // ✅ ゲーム終了処理（仮）
    private void GameOver()
    {
        Debug.Log("ゲームオーバー！");
        // TODO: 実際のゲーム終了演出などを追加（UI遷移、シーン遷移など）
        Time.timeScale = 1f;
    }
}