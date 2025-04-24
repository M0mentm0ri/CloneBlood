using UnityEngine;
using System.Collections.Generic;

public class HitDecal : MonoBehaviour
{
    public GameObject decalPrefab;
    public ParticleSystem particleSystem;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    public float decalMergeRadius = 0.5f;       // 近くのデカールと統合する半径
    public string decalTargetTag = "DecalTarget";     // デカールを貼っていい対象のタグ（例: "Wall" など）
    public string decalTag = "Decal";     // デカールを貼っていい対象のタグ（例: "Wall" など）
    public float sizeIncrement = 0.2f;          // デカールが統合されたときのサイズ増加量
    public float maxSize = 3f;                  // デカールの最大サイズ
    public float knockbackForce = 10f;          // 吹っ飛ばし力の強さ

    void OnParticleCollision(GameObject other)
    {
        int numEvents = particleSystem.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < numEvents; i++)
        {
            Vector3 hitPos = collisionEvents[i].intersection; // パーティクル衝突位置
            Vector3 hitNormal = collisionEvents[i].normal;    // 衝突した面の法線方向
            Quaternion finalRot = Quaternion.LookRotation(-hitNormal); // デカールの向きを法線の逆向きに設定

            // ✅ タグが一致するものにだけデカール処理を行う
            if (other.CompareTag(decalTargetTag))
            {
                MergeOrCreateDecal(hitPos, finalRot);
            }

            // ✅ Rigidbody付きだったらノックバックさせる
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 forceDirection = -hitNormal.normalized;
                rb.AddForceAtPosition(forceDirection * knockbackForce, hitPos, ForceMode.Impulse);
            }
        }
    }

    // 近くに既存のデカールがあるかチェックして統合する処理
    void MergeOrCreateDecal(Vector3 spawnPos, Quaternion rotation)
    {
        Collider[] hitColliders = Physics.OverlapSphere(spawnPos, decalMergeRadius);
        GameObject closestDecal = null;
        float closestDistance = Mathf.Infinity;

        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag(decalTag)) // デカール自体には "Decal" タグを使用
            {
                float distance = Vector3.Distance(spawnPos, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDecal = collider.gameObject;
                }
            }
        }

        if (closestDecal != null)
        {
            float currentSize = closestDecal.transform.localScale.x;
            float newSize = Mathf.Min(currentSize + sizeIncrement, maxSize);
            closestDecal.transform.localScale = new Vector3(newSize, newSize, newSize);
        }
        else
        {
            GameObject newDecal = Instantiate(decalPrefab, spawnPos, rotation);
            newDecal.transform.localScale = Vector3.one;
            newDecal.tag = decalTag; // ← 生成したデカールには常に "Decal" タグを付ける
        }
    }
}
