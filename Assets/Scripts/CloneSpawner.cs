using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Cinemachine;

public class CloneSpawner : MonoBehaviour
{

    public GameObject clonePrefab; //クローンのプレハブ
    //現在操作するクローン
    public GameObject currentClone;
    public HumanStats currentHumanStats; // HumanStatsクラスの参照

    //HumanStatsに渡すパス

    public TMP_Text lifespanText; // 寿命を表示するTMP（設定しておく）
    public TMP_Text lifeText; // 寿命を表示するTMP（設定しておく）

    //CinemachineCamera のtagetを次のHumanに変更するためのカメラ
    public CinemachineCamera cinemachineCamera;

    public Camera maincamera; //メインカメラの参照

    //クローンを生成する位置
    public Transform spawnPoint;

    void Start()
    {
        //初期クローンを生成
        SpawnClone();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHumanStats != null) 
        {
            UpdateUI();
        }

    }

    public void  SpawnClone()
    {

        if (currentHumanStats != null)
        {
            currentHumanStats.IsInitiative = false; //操作するクローンに設定
        }
        //クローンを生成
        currentClone = Instantiate(clonePrefab, spawnPoint.position, spawnPoint.rotation);
        //HumanStatsコンポーネントを取得
        currentHumanStats = currentClone.GetComponent<HumanStats>();

        // Clone が持つ mouthObject をカメラに追従させる
        if (cinemachineCamera != null && currentHumanStats.weaponPickup.mouthObject != null)
        {
            cinemachineCamera.Follow = currentHumanStats.weaponPickup.mouthObject;
            cinemachineCamera.LookAt = currentHumanStats.weaponPickup.mouthObject;

        }


        currentHumanStats.cloneSpawner = this; // CloneSpawnerの参照を設定
        currentHumanStats.cam = maincamera;

        currentHumanStats.lifeText = lifeText;
        currentHumanStats.lifespanText = lifespanText;

        currentHumanStats.IsInitiative = true; //操作するクローンに設定
    }

    private void UpdateUI()
    {
        // 残り寿命の表示（小数点以下0桁で表示）＋透明度調整（段階的 + 色変更）
        if (lifespanText != null && lifeText)
        {
            float remaining = Mathf.Max(0, currentHumanStats.lifespan - currentHumanStats.age);
            lifespanText.text = $"{remaining:F0}";

            // デフォルトの色（白）で初期化
            Color baseColor = Color.white;

            // 透明度処理（半分を超えるまでは見えにくい）
            float halfLife = currentHumanStats.lifespan / 2f;
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
        if (currentHumanStats.bloodGaugeObject != null)
        {
            float healthPercent = Mathf.Clamp01(currentHumanStats.currentHealth / currentHumanStats.maxHealth); // 0～1
            float y = Mathf.Lerp(currentHumanStats.emptyGaugeY, currentHumanStats.fullGaugeY, healthPercent);   // 線形補間で位置を計算

            Vector3 localPos = currentHumanStats.bloodGaugeObject.localPosition;
            localPos.y = y;
            currentHumanStats.bloodGaugeObject.localPosition = localPos;
        }
    }
}
