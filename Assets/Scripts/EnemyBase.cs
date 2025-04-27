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

    // === スプライト ===
    public SpriteRenderer[] spriteRenderers;  // 複数のSpriteRendererを格納
    public Color maxHealthColor = Color.white; // 最大ヘルス時の色
    public Color minHealthColor = Color.red;  // 最低ヘルス時の色

    // === 状態管理 ===
    protected bool isDead = false;

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
        Color currentColor = Color.Lerp(minHealthColor, maxHealthColor, adjustedHealthRatio);

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

            // 進行方向を向く（X軸のスケールで反転させる）
            if (direction != Vector3.zero)
            {
                // 進行方向が右ならそのまま、左なら反転
                if (direction.x > 0 && transform.localScale.x < 0)
                {
                    // 左向きから右向きに反転
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                }
                else if (direction.x < 0 && transform.localScale.x > 0)
                {
                    // 右向きから左向きに反転
                    transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
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
    }
}
