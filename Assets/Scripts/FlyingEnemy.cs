using System;
using UnityEngine;

public class FlyingEnemy : EnemyBase
{
    [Header("飛行用設定")]
    public float hoverHeight = 5f;
    public bool keepFixedHeight = true;

    [Header("攻撃用設定")]
    public float attackRange = 10f; // ターゲットに近づいたとき攻撃する距離
    public float offsetAngle = 10f;  // インスペクタで設定可能なオフセット角度（Y軸方向）
    public ParticleSystem currentParticle;
    public Vector3 attackDirection; // 攻撃方向


    private bool isAttacking = false; // 攻撃中かどうかを判定するフラグ

    protected override void Update()
    {
        // 毎フレーム、攻撃判定を先に行う
        CheckAttack();

        // 攻撃していないときのみ移動
        if (!isAttacking)
        {
            Move();
        }
    }

    protected override void Move()
    {
        if (targetPosition == null) return;

        // 横移動だけを考慮（Y方向は固定）
        Vector3 direction = targetPosition.position - transform.position;
        direction.y = 0;
        direction = direction.normalized;

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition.position);

        if (distanceToTarget > stoppingDistance)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;

            // 進行方向を向く（X軸だけ反転させる）
            if (direction != Vector3.zero)
            {
                Vector3 currentScale = transform.localScale;
                if (direction.x > 0)
                    transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z); // 左向き
                else if (direction.x < 0)
                    transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z); // 右向き
            }

            // 高さを固定
            if (keepFixedHeight)
            {
                Vector3 pos = transform.position;
                pos.y = hoverHeight;
                transform.position = pos;
            }
        }
    }

    void CheckAttack()
    {
        isAttacking = false; // 毎フレーム初期化

        // 攻撃方向はとりあえずターゲット方向にしておく（見失っても攻撃できるように）
        if (targetPosition != null)
        {
            attackDirection = (targetPosition.position - transform.position).normalized;
        }

        // ↓↓↓ 真下にオフセットした位置を中心に範囲検知
        Vector3 detectCenter = transform.position + Vector3.down * 1f; // 足元から1m下
        LayerMask playerMask = LayerMask.GetMask("Player");

        Collider[] hits = Physics.OverlapSphere(detectCenter, sphereRadius, playerMask);
        if (hits.Length > 0)
        {
            isAttacking = true;
            animator.SetBool("IsAttack", true);

            // プレイヤー方向を攻撃方向に設定
            attackDirection = (hits[0].transform.position - transform.position).normalized;

            // パーティクルを回転させる
            if (currentParticle != null)
            {
                // X軸方向にオフセットを加える
                Vector3 offsetDirection = new Vector3(offsetAngle, 0, 0); // X軸に対するオフセットを作成

                // attackDirection にオフセットを足す
                attackDirection += offsetDirection;  // X軸方向にオフセットを足す


                currentParticle.transform.rotation = Quaternion.LookRotation(attackDirection);
            }

            return; // 攻撃完了
        }

        // ターゲット地点が近ければそちらに攻撃
        if (targetPosition != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition.position);
            if (distanceToTarget <= attackRange)
            {
                isAttacking = true;
                animator.SetBool("IsAttack", true);

                attackDirection = (targetPosition.position - transform.position).normalized;

                // X軸方向にオフセットを加える
                Vector3 offsetDirection = new Vector3(offsetAngle, 0, 0); // X軸に対するオフセットを作成

                // attackDirection にオフセットを足す
                attackDirection += offsetDirection;  // X軸方向にオフセットを足す

                if (currentParticle != null)
                {
                    currentParticle.transform.rotation = Quaternion.LookRotation(attackDirection);
                }

                return;
            }
        }

        animator.SetBool("IsAttack", false);  // 攻撃解除
    }

    public void Fire()
    {
        if (currentParticle != null)
        {
            // 停止と再生（ここで初めてリセットされる）
            currentParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            currentParticle.Play();
        }
    }

    protected override bool DetectTarget()
    {
        if (targetPosition == null) return false;

        // プレイヤーとの距離を計算
        float distance = Vector3.Distance(transform.position, targetPosition.position);

        // 指定範囲内かつ「Player」レイヤーのみ検知
        if (distance < detectionRange && targetPosition.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            return true;
        }

        return false;
    }

    // Editor上でSphereCastの検知範囲を可視化
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 detectCenter = transform.position + Vector3.down * 1f;
        Gizmos.DrawWireSphere(detectCenter, sphereRadius);
    }
}
