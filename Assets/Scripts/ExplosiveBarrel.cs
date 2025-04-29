using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("爆発するために必要な最小衝撃力")]
    public float explosionThreshold = 10f;  // 衝撃力のしきい値

    [Header("爆発時に削除するまでの時間")]
    public float destroyDelay = 2f;          // 爆発エフェクト後に何秒で消すか

    [Header("点滅設定")]
    public SpriteRenderer targetRenderer;    // 色を変えたいSpriteRenderer
    public Color blinkColor = Color.red;      // 点滅時の色
    public float blinkInterval = 0.2f;        // 点滅する間隔（秒）

    [Header("爆発設定")]
    public GameObject explosionTriggerObject;
    public float explosionRadius = 5f;        // 爆風の届く範囲 【★追加】
    public float explosionForce = 700f;       // 爆風の強さ 【★追加】
    public float upwardsModifier = 1f;        // 上方向への力補正 【★追加】

    private bool hasExploded = false;         // すでに爆発したか
    private bool isBlinking = false;          // 点滅中かどうか
    private int groundLayer;                  // Groundレイヤーの番号
    private Color originalColor;              // 最初の色
    private float blinkTimer = 0f;             // 点滅用タイマー
    private bool isOriginalColor = true;       // 現在、オリジナル色かどうか

    private void Awake()
    {
        groundLayer = LayerMask.NameToLayer("Ground");

        if (targetRenderer != null)
        {
            originalColor = targetRenderer.color;
        }
    }

    private void Update()
    {
        if (isBlinking && targetRenderer != null)
        {
            blinkTimer += Time.deltaTime;

            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0f;

                if (isOriginalColor)
                {
                    targetRenderer.color = blinkColor;
                }
                else
                {
                    targetRenderer.color = originalColor;
                }

                isOriginalColor = !isOriginalColor;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

        if (collision.gameObject.layer == groundLayer)
        {
            return;
        }

        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce >= explosionThreshold)
        {
            StartBlinking();
            Invoke(nameof(ExplodeDelayed), destroyDelay);
        }
    }

    private void StartBlinking()
    {
        if (isBlinking) return;

        isBlinking = true;
        blinkTimer = 0f;
        isOriginalColor = true;
    }

    private void ExplodeDelayed()
    {
        Explode();
    }

    private void Explode()
    {
        hasExploded = true;
        isBlinking = false;

        if (targetRenderer != null)
        {
            targetRenderer.color = originalColor;
        }

        GameReferences.Instance.shake.ShakeScreen(Shake.ShakeType.Explosion);

        // ここで爆発パーティクル出す
        Vector3 hitPos = transform.position;
        Vector3 hitNormal = Vector3.up;
        Quaternion finalRot = Quaternion.LookRotation(hitNormal);

        GameReferences.Instance.particleManager.PlayParticle(
            ParticleManager.ParticleType.Explosion,
            hitPos,
            finalRot
        );

        // ★★★ ここから爆風処理追加 ★★★

        // 爆発時にコリジョンのアクティブ状態をオンにする
        if (explosionTriggerObject != null)
        {
            explosionTriggerObject.SetActive(true); // コリジョンオブジェクトを有効化
        }

        // 半径内の全コライダーを取得
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.attachedRigidbody; // Rigidbodyが付いているかチェック
            if (rb != null && rb != GetComponent<Rigidbody>())
            {
                // 爆発力を与える（上方向補正もかける）
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsModifier, ForceMode.Impulse);
            }
        }

        // ★★★ 爆風処理ここまで ★★★
        // 少ししてからコリジョンオブジェクトのアクティブをオフにする
        Invoke("DisableExplosionTrigger", 0.1f);
    }

    private void DisableExplosionTrigger()
    {
        if (explosionTriggerObject != null)
        {
            explosionTriggerObject.SetActive(false); // コリジョンオブジェクトを無効化
        }
        Destroy(gameObject); // 自分自身を削除
    }

    // 爆発範囲をGizmosで表示
    private void OnDrawGizmos()
    {
        // 色を変更して範囲を示す
        Gizmos.color = Color.red;
        // 範囲を表示（球体）
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
