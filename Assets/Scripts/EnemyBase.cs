// 敵共通の基盤クラス
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    // === ステータス ===
    [Header("Enemy Parameters")]
    public float health;
    public float attackPower;
    public float moveSpeed;
    public float baseMoveSpeed = 5f; // 例えば歩き基準5

    // === 参照 ===
    public Transform targetPosition;
    public Animator animator;

    // === 状態管理 ===
    protected bool isDead = false;
    protected bool isAttacking = false;

    // 初期化

    // 更新処理
    protected virtual void Update()
    {
        if (isDead) return;

        UpdateAnimationSpeed();

        if (DetectTarget())
        {
            Attack();
        }
        else
        {
            Move();
        }
    }

    // アニメーションSpeed調整
    protected void UpdateAnimationSpeed()
    {
        float speedParam = moveSpeed / baseMoveSpeed;
        animator.SetFloat("Speed", speedParam);
    }

    // 移動処理
    // 目標地点

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
                // 進行方向に基づいてスプライトの向きを反転
                if (direction.x > 0) // 右に向かっている
                {
                    transform.localScale = new Vector3(1, 1, 1); // 右向き
                }
                else if (direction.x < 0) // 左に向かっている
                {
                    transform.localScale = new Vector3(-1, 1, 1); // 左向き
                }
            }
        }
        else
        {
            // 目標地点に到達したら攻撃開始
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

    // ダメージを受ける
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    // 死亡処理
    protected virtual void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");

        // コライダーや物理停止処理
        // 例：Destroy(gameObject, 3f);
    }
}
