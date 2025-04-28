using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class HitDecal : MonoBehaviour
{
    public GameObject decalPrefab;
    public ParticleSystem particleSystem;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    private float decalMergeRadius = 1f;       // 近くのデカールと統合する半径
    public string decalTargetTag = "DecalTarget";     // デカールを貼っていい対象のタグ（例: "Wall" など）
    private string decalTag = "Blood";     // 血痕デカールのタグ（例: "Blood" など）
    private string enemydecalTag = "Blood_Enemy";     // 敵の血痕デカールのタグ（例: "Blood" など）

    public bool IsEnemyBlood = false; // 血痕が敵のものかどうか
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

            // ✅ ぶつかったオブジェクトがEnemyレイヤーかつ自分のタグが"Blood"の場合にEnemyBloodを生成
            if (other.layer == LayerMask.NameToLayer("Enemy"))
            {
                // 自身のオブジェクトのタグが"@Enemy"で始まるか確認
                if (gameObject.tag.StartsWith("@Enemy"))
                {
                    // 血のパーティクルを再生（必要なパーティクルを指定）
                    GameReferences.Instance.particleManager.PlayParticle(
                        ParticleManager.ParticleType.BloodEnemy_Low,  // 血のパーティクル（適切なものを選択）
                        hitPos,                                      // 衝突位置
                        finalRot                                     // 衝突面に基づく回転
                    );
                }
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
        // IsEnemyBlood に基づいて使用するタグを決定
        string tagToUse = IsEnemyBlood ? enemydecalTag : decalTag;
        string tagToDestroy = IsEnemyBlood ? decalTag : enemydecalTag;

        // 近くのデカールを全てチェック
        Collider[] hitColliders = Physics.OverlapSphere(spawnPos, decalMergeRadius);
        GameObject closestDecal = null;
        float closestDistance = Mathf.Infinity;

        // デカールをチェックして削除
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag(tagToDestroy)) // 削除対象のタグを持つデカールがあれば削除
            {
                Destroy(collider.gameObject); // デカールを削除
            }
            else if (collider.CompareTag(tagToUse)) // 自身のタグと同じタグを持つデカールについてチェック
            {
                // 近くにあるデカールの中で一番近いものを探す
                float distance = Vector3.Distance(spawnPos, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDecal = collider.gameObject;
                }
            }
        }

        // 既存のデカールが近くにあった場合
        if (closestDecal != null)
        {
            float currentSize = closestDecal.transform.localScale.x;
            float newSize = Mathf.Min(currentSize + sizeIncrement, maxSize);
            closestDecal.transform.localScale = new Vector3(newSize, newSize, newSize);
        }
        else
        {
            // 近くにデカールがなかった場合は新しく作成
            GameObject newDecal = Instantiate(decalPrefab, spawnPos, rotation);
            newDecal.transform.localScale = Vector3.one;
            newDecal.tag = tagToUse; // 新しいデカールに決定したタグを付ける
        }
    }

}
