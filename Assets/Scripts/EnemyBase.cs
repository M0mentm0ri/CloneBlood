// �G���ʂ̊�ՃN���X
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    // === �X�e�[�^�X ===
    [Header("Enemy Parameters")]
    public float health;
    public float attackPower;
    public float moveSpeed;
    public float baseMoveSpeed = 5f; // �Ⴆ�Ε����5

    // === �Q�� ===
    public Transform targetPosition;
    public Animator animator;

    // === ��ԊǗ� ===
    protected bool isDead = false;
    protected bool isAttacking = false;

    // ������

    // �X�V����
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

    // �A�j���[�V����Speed����
    protected void UpdateAnimationSpeed()
    {
        float speedParam = moveSpeed / baseMoveSpeed;
        animator.SetFloat("Speed", speedParam);
    }

    // �ړ�����
    // �ڕW�n�_

    protected virtual void Move()
    {
        // �܂��A�j�����U������
        animator.SetBool("IsAttack", false);

        // �A�j��Speed����
        UpdateAnimationSpeed();

        // ����
        Vector3 flatCurrent = new Vector3(transform.position.x, 0, transform.position.z); // Y���𖳎�����2D�ړ�
        Vector3 flatTarget = new Vector3(targetPosition.position.x, 0, targetPosition.position.z); // Y���𖳎�
        Vector3 direction = (flatTarget - flatCurrent).normalized;

        // �ڕW�n�_�ɋ߂Â��������肷�邽�߂̋���
        float distanceToTarget = Vector3.Distance(flatCurrent, flatTarget);
        float stoppingDistance = 5f; // ���̋������ɓ�������ړ����~

        // ���ۂɈړ�
        if (distanceToTarget > stoppingDistance)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;

            // �i�s�����������iX���̃X�P�[���Ŕ��]������j
            if (direction != Vector3.zero)
            {
                // �i�s�����Ɋ�Â��ăX�v���C�g�̌����𔽓]
                if (direction.x > 0) // �E�Ɍ������Ă���
                {
                    transform.localScale = new Vector3(1, 1, 1); // �E����
                }
                else if (direction.x < 0) // ���Ɍ������Ă���
                {
                    transform.localScale = new Vector3(-1, 1, 1); // ������
                }
            }
        }
        else
        {
            // �ڕW�n�_�ɓ��B������U���J�n
            Attack();
        }
    }

    // �U������
    protected virtual void Attack()
    {
        animator.SetBool("IsAttack", true);
        // �U�����W�b�N�������i��F�_���[�W����j
    }

    // �^�[�Q�b�g���m
    protected virtual bool DetectTarget()
    {
        // ������Raycast��SphereCast�Ȃǂ������\��
        return false;
    }

    // �_���[�W���󂯂�
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    // ���S����
    protected virtual void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");

        // �R���C�_�[�╨����~����
        // ��FDestroy(gameObject, 3f);
    }
}
