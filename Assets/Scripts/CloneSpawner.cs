using System.Collections;
using TMPro;
using UnityEngine;
using Unity.Cinemachine;

public class CloneSpawner : MonoBehaviour
{
    public GameObject clonePrefab; //クローンのプレハブ
    public GameObject currentClone; //現在操作するクローン
    public HumanStats currentHumanStats; // HumanStatsクラスの参照

    [Header("UI")]
    public TMP_Text lifespanText; // 寿命を表示するTMP（設定しておく）
    public TMP_Text lifeText; // 寿命を表示するTMP（設定しておく）
    public Transform bloodGaugeObject; // 血液ゲージ

    [Header("ゲージ設定")]
    public float fullGaugeY = 1.0f;   // 体力100%時のローカルY
    public float emptyGaugeY = 0.0f;  // 体力0%時のローカルY

    public CinemachineCamera cinemachineCamera; // カメラ
    public Camera maincamera; //メインカメラの参照
    public Transform spawnPoint; //クローンを生成する位置

    void Start()
    {
        // 初期クローンを生成
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
            currentHumanStats.IsInitiative = false; // 操作するクローンに設定
        }

        // クローンを生成
        currentClone = Instantiate(clonePrefab, spawnPoint.position, spawnPoint.rotation);
        currentHumanStats = currentClone.GetComponent<HumanStats>();

        // Cloneが持つmouthObjectをカメラに追従させる
        if (cinemachineCamera != null && currentHumanStats != null && currentHumanStats.weaponPickup.mouthObject != null)
        {
            cinemachineCamera.Follow = currentHumanStats.weaponPickup.mouthObject;
            cinemachineCamera.LookAt = currentHumanStats.weaponPickup.mouthObject;
        }

        currentHumanStats.cloneSpawner = this; // CloneSpawnerの参照を設定
        currentHumanStats.cam = maincamera;
        currentHumanStats.IsInitiative = true; // 操作するクローンに設定
    }

    public void RespawnClone()
    {
        // クローンが死んだら1秒後に再生成
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        // 1秒待機
        yield return new WaitForSeconds(1f);

        // 新しいクローンを生成
        SpawnClone();
    }

    private void UpdateUI()
    {
        if (lifespanText != null && lifeText != null)
        {
            float remaining = Mathf.Max(0, currentHumanStats.lifespan - currentHumanStats.age);
            lifespanText.text = $"{remaining:F0}";

            // 色変更処理
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

        // 血液ゲージ
        if (bloodGaugeObject != null)
        {
            float bloodPercent = Mathf.Clamp01(currentHumanStats.currentBlood / currentHumanStats.maxBlood);
            float y = Mathf.Lerp(emptyGaugeY, fullGaugeY, bloodPercent);
            Vector3 localPos = bloodGaugeObject.localPosition;
            localPos.y = y;
            bloodGaugeObject.localPosition = localPos;
        }
    }
}
