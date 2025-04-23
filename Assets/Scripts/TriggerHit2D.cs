using UnityEngine;

public class TriggerHit2D : MonoBehaviour
{
    // ������΂��͂̑傫���i�����j
    public float knockbackForce = 10f;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("2D Trigger hit: " + other.name);

        Rigidbody2D rb = other.attachedRigidbody;

        if (rb != null)
        {
            // ������΂����� = ����̈ʒu - �����̈ʒu
            Vector2 direction = (other.transform.position - transform.position).normalized;

            // AddForce �ňꎞ�I�ȏՌ���������iImpulse���[�h�j
            rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        }
    }
}
