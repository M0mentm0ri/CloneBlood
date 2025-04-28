using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    // === ステータス ===
    [Header("Enemy Parameters")]
    public float health;
    public float maxHealth = 100f;
    public float attackPower;
    public float moveSpeed;
    public float baseMoveSpeed = 5f;

    // === 参照 ===
    public Transform targetPosition;
    public Animator animator;

    //中心位置
    public Transform flontposition; // 中心位置のTransform 
    // === スプライト ===
    public SpriteRenderer[] spriteRenderers;  // 複数のSpriteRendererを格納
    public Color maxHealthColor = Color.white; // 最大ヘルス時の色
    public Color minHealthColor = Color.red;  // 最低ヘルス時の色

    [Header("参照")]
    private ParticleManager particleManager; // パーティクルマネージャーの参照

    // === 状態管理 ===
    protected bool isDead = false;

    private void Start()
    {
        particleManager = GameReferences.Instance.particleManager;
    }
    // 更新処理
    protected virtual void Update()
    {
        if (isDead) return;

        UpdateAnimationSpeed();

        // ヘルスに応じてスプライトの色を変更
        UpdateSpriteColor();

        if (DetectTarget())
        {
            Attack();
        }
        else
        {
            Move();
        }
    }

    protected void UpdateSpriteColor()
    {
        // ヘルスの割合を計算
        float healthRatio = health / maxHealth;

        // ヘルスが高いときに色変化を遅く、低くなるほど急激に変化
        // Mathf.Powで非線形に補間の速度を調整
        float adjustedHealthRatio = Mathf.Pow(healthRatio, 0.5f); // 0.5f でゆっくりとした変化を得る

        // 色を補間
        Color currentColor = Color.Lerp(minHealthColor, maxHealthColor, healthRatio);

        // すべてのSpriteRendererに色を設定
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            renderer.color = currentColor;
        }
    }

    // アニメーションSpeed調整
    protected void UpdateAnimationSpeed()
    {
        float speedParam = moveSpeed / baseMoveSpeed;
        animator.SetFloat("Speed", speedParam);
    }

    // 移動処理
    protected virtual void Move()
    {
        // まずアニメを攻撃解除
        animator.SetBool("IsAttack", false);

        // アニメSpeed調整
        UpdateAnimationSpeed();

        // 方向
        Vector3 flatCurrent = new Vector3(transform.position.x, 0, transform.position.z); // Y軸を無視して2D移動
        Vector3 flatTarget = new Vector3(targetPosition.position.x, 0, targetPosition.position.z); // Y軸を無視
        Vector3 direction = (flatTarget - flatCurrent).normalized;

        // 目標地点に近づいたか判定するための距離
        float distanceToTarget = Vector3.Distance(flatCurrent, flatTarget);
        float stoppingDistance = 5f; // この距離内に入ったら移動を停止

        // 実際に移動
        if (distanceToTarget > stoppingDistance)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;

            // 進行方向を向く（X軸だけ反転させる）
            if (direction != Vector3.zero)
            {
                // 現在のスケールを保持したまま、X軸だけ反転
                Vector3 currentScale = transform.localScale;
                if (direction.x > 0)
                {
                    transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z); // 右向き
                }
                else if (direction.x < 0)
                {
                    transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z); // 左向き
                }
            }
        }
        else
        {
            Attack();
        }
    }

    // 攻撃処理
    protected virtual void Attack()
    {
        animator.SetBool("IsAttack", true);
        // 攻撃ロジックを書く（例：ダメージ判定）
    }

    // ターゲット検知
    protected virtual bool DetectTarget()
    {
        // ここにRaycastやSphereCastなどを書く予定
        return false;
    }

    private void OnParticleCollision(GameObject other)
    {
        if (isDead) return;

        // ダメージ処理
        int damage = GetDamageFromTag(other.tag, "Enemy");

        if (damage > 0)
        {
            health -= damage;
        }

        if (health <= 0)
        {
            Die();
        }
    }

    // ダメージ計算
    private int GetDamageFromTag(string tag, string targetType)
    {
        if (string.IsNullOrEmpty(tag) || tag[0] != '@') return 0;

        string[] parts = tag.Substring(1).Split('_');
        if (parts.Length == 2)
        {
            string tagType = parts[0];
            int dmg;
            if (int.TryParse(parts[1], out dmg))
            {
                if (tagType == "All" || tagType == targetType)
                {
                    return dmg;
                }
            }
        }
        return 0;
    }

    // 死亡処理
    protected virtual void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");

        // 血のパーティクルを再生（必要なパーティクルを指定）
        GameReferences.Instance.particleManager.PlayParticle(
            ParticleManager.ParticleType.BloodEnemy_Mid,  // 血のパーティクル（適切なものを選択）
            flontposition.position,                           // 衝突位置 (キャラクターの位置)
            flontposition.rotation                            // 衝突面に基づく回転 (キャラクターの回転)
        );

        // 死亡時にスケールを徐々に小さくする
        StartCoroutine(ShrinkAndDestroy());
    }

    // 死亡時にスケールを小さくして消滅させる
    private IEnumerator ShrinkAndDestroy()
    {
        float shrinkDuration = 2f; // 徐々に小さくする時間
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = Vector3.zero;

        float elapsedTime = 0f;

        // 徐々にスケールを小さくしていく
        while (elapsedTime < shrinkDuration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / shrinkDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 最終的にスケールを0にして消滅
        transform.localScale = targetScale;

        // オブジェクトを非アクティブ化して完全に消す
        gameObject.SetActive(false);
    }
}
