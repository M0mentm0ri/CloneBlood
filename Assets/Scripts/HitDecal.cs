using UnityEngine;
using System.Collections.Generic;

public class HitDecal : MonoBehaviour
{
    public GameObject decalPrefab;
    public ParticleSystem particleSystem;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    public float decalMergeRadius = 0.5f; // 統合する半径（距離）を指定
    public string decalTag = "Decal"; // 統合対象のタグ（デカールのタグを指定）
    public float sizeIncrement = 0.2f; // 追加するサイズの増加量（0.2倍ずつ加算）
    public float maxSize = 3f; // 最大サイズの制限

    void OnParticleCollision(GameObject other)
    {
        int numEvents = particleSystem.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < numEvents; i++)
        {
            Vector3 hitPos = collisionEvents[i].intersection;    // 衝突位置
            Vector3 hitNormal = collisionEvents[i].normal;        // 衝突面の法線

            // 常に表をカメラ方向 or ワールド上方向に向けたいため、法線の逆方向を向かせる
            // 通常のZ+向きデカールなら、Z-方向へLookする（法線の逆を向く）
            Quaternion finalRot = Quaternion.LookRotation(-hitNormal);

            MergeOrCreateDecal(hitPos, finalRot);
        }
    }

    // 近くのデカールを検索し、統合または新規作成
    void MergeOrCreateDecal(Vector3 spawnPos, Quaternion rotation)
    {
        // 近くのデカールを検索
        Collider[] hitColliders = Physics.OverlapSphere(spawnPos, decalMergeRadius);

        GameObject closestDecal = null;
        float closestDistance = Mathf.Infinity;

        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag(decalTag))  // タグが一致するデカールを検索
            {
                float distance = Vector3.Distance(spawnPos, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDecal = collider.gameObject;
                }
            }
        }

        // 近くにデカールがあった場合、統合
        if (closestDecal != null)
        {
            // 既存のデカールのサイズを取得（初期サイズを基に加算していく）
            float currentSize = closestDecal.transform.localScale.x; // 仮に全軸が同じサイズだと仮定
            float newSize = Mathf.Min(currentSize + sizeIncrement, maxSize); // サイズ増加量（0.2倍加算）、最大サイズ制限

            // 既存のデカールのサイズを変更
            closestDecal.transform.localScale = new Vector3(newSize, newSize, newSize);  // 新しいサイズを設定
        }
        else
        {
            // 近くにデカールがなければ新しいデカールを生成
            GameObject newDecal = Instantiate(decalPrefab, spawnPos, rotation);
            newDecal.transform.localScale = new Vector3(1f, 1f, 1f); // 初期サイズ
            newDecal.tag = decalTag;  // タグを設定
        }
    }
}
