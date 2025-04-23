using UnityEngine;

public class TriggerHit2D : MonoBehaviour
{
    // 吹っ飛ばす力の大きさ（調整可）
    public float knockbackForce = 10f;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("2D Trigger hit: " + other.name);

        Rigidbody2D rb = other.attachedRigidbody;

        if (rb != null)
        {
            // 吹っ飛ばす方向 = 相手の位置 - 自分の位置
            Vector2 direction = (other.transform.position - transform.position).normalized;

            // AddForce で一時的な衝撃を加える（Impulseモード）
            rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        }
    }
}
