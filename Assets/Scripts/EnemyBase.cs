using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    // === �X�e�[�^�X ===
    [Header("Enemy Parameters")]
    public float health;
    public float maxHealth = 100f;
    public float attackPower;
    public float moveSpeed;
    public float baseMoveSpeed = 5f;

    public float detectionRange = 2f;
    public float sphereRadius = 0.5f;
    public float stoppingDistance = 3f; // ���̋������ɓ�������ړ����~

    // === �Q�� ===
    public Transform targetPosition;
    public Animator animator;

    //���S�ʒu
    public Transform flontposition; // ���S�ʒu��Transform 
    // === �X�v���C�g ===
    public SpriteRenderer[] spriteRenderers;  // ������SpriteRenderer���i�[
    public Color maxHealthColor = Color.white; // �ő�w���X���̐F
    public Color minHealthColor = Color.red;  // �Œ�w���X���̐F

    [Header("�U������p")]
    public GameObject attackCollider; // �ꎞ�I�ɃA�N�e�B�u�ɂ���U���p�̓����蔻��

    [Header("�Q��")]
    private ParticleManager particleManager; // �p�[�e�B�N���}�l�[�W���[�̎Q��

    // === ��ԊǗ� ===
    protected bool isDead = false;

    private void Start()
    {
        particleManager = GameReferences.Instance.particleManager;
    }
    // �X�V����
    protected virtual void Update()
    {
        if (isDead) return;

        UpdateAnimationSpeed();

        // �w���X�ɉ����ăX�v���C�g�̐F��ύX
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
        // �w���X�̊������v�Z
        float healthRatio = health / maxHealth;

        // �w���X�������Ƃ��ɐF�ω���x���A�Ⴍ�Ȃ�قǋ}���ɕω�
        // Mathf.Pow�Ŕ���`�ɕ�Ԃ̑��x�𒲐�
        float adjustedHealthRatio = Mathf.Pow(healthRatio, 0.5f); // 0.5f �ł������Ƃ����ω��𓾂�

        // �F����
        Color currentColor = Color.Lerp(minHealthColor, maxHealthColor, healthRatio);

        // ���ׂĂ�SpriteRenderer�ɐF��ݒ�
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            renderer.color = currentColor;
        }
    }

    // �A�j���[�V����Speed����
    protected void UpdateAnimationSpeed()
    {
        float speedParam = moveSpeed / baseMoveSpeed;
        animator.SetFloat("Speed", speedParam);
    }

    // �ړ�����
    protected virtual void Move()
    {
        // �܂��A�j�����U������
        animator.SetBool("IsAttack", false);

        // �A�j��Speed����
        UpdateAnimationSpeed();

        if(targetPosition == null) return;

        // ����
        Vector3 flatCurrent = new Vector3(transform.position.x, 0, transform.position.z); // Y���𖳎�����2D�ړ�
        Vector3 flatTarget = new Vector3(targetPosition.position.x, 0, targetPosition.position.z); // Y���𖳎�
        Vector3 direction = (flatTarget - flatCurrent).normalized;

        // �ڕW�n�_�ɋ߂Â��������肷�邽�߂̋���
        float distanceToTarget = Vector3.Distance(flatCurrent, flatTarget);

        // ���ۂɈړ�
        if (distanceToTarget > stoppingDistance)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;

            // �i�s�����������iX���������]������j
            if (direction != Vector3.zero)
            {
                // ���݂̃X�P�[����ێ������܂܁AX���������]
                Vector3 currentScale = transform.localScale;
                if (direction.x > 0)
                {
                    transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z); // �E����
                }
                else if (direction.x < 0)
                {
                    transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z); // ������
                }
            }
        }
        else
        {
            Attack();
        }
    }


    // �v���C���[�̌��o�����iX��������SphereCast�j
    protected bool DetectTarget()
    {
        Vector3 origin = flontposition.position;
        Vector3 baseDirection = transform.right;
        Vector3 direction = (baseDirection.x < 0) ? -baseDirection : baseDirection;

        RaycastHit hit;

        // X�������ɒZ����SphereCast
        if (Physics.SphereCast(origin, sphereRadius, direction, out hit, detectionRange))
        {
            // Layer����i��: "Player" ���C���[�j
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                return true; // �v���C���[���m
            }
        }

        return false; // ���m�Ȃ�
    }

    // �U������
    protected virtual void Attack()
    {
        animator.SetBool("IsAttack", true);
        // �U�����W�b�N�������i��F�_���[�W����j
    }

    private void OnParticleCollision(GameObject other)
    {
        if (isDead) return;

        // �_���[�W����
        int damage = GameReferences.Instance.GetDamageFromTag(other.tag, "Enemy");

        if (damage > 0)
        {
            health -= damage;
        }

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

        // ���̃p�[�e�B�N�����Đ��i�K�v�ȃp�[�e�B�N�����w��j
        GameReferences.Instance.particleManager.PlayParticle(
            ParticleManager.ParticleType.BloodEnemy_Mid,  // ���̃p�[�e�B�N���i�K�؂Ȃ��̂�I���j
            flontposition.position,                           // �Փˈʒu (�L�����N�^�[�̈ʒu)
            flontposition.rotation                            // �Փ˖ʂɊ�Â���] (�L�����N�^�[�̉�])
        );

        // ���S���ɃX�P�[�������X�ɏ���������
        StartCoroutine(ShrinkAndDestroy());
    }

    // ���S���ɃX�P�[�������������ď��ł�����
    private IEnumerator ShrinkAndDestroy()
    {
        float shrinkDuration = 2f; // ���X�ɏ��������鎞��
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = Vector3.zero;

        float elapsedTime = 0f;

        // ���X�ɃX�P�[�������������Ă���
        while (elapsedTime < shrinkDuration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / shrinkDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �ŏI�I�ɃX�P�[����0�ɂ��ď���
        transform.localScale = targetScale;

        // �I�u�W�F�N�g���A�N�e�B�u�����Ċ��S�ɏ���
        gameObject.SetActive(false);
    }

    // �A�j���[�V�����C�x���g�ŌĂяo�����U�����s����
    public void PerformAttack()
    {
        StartCoroutine(ActivateAttackCollider());
    }

    private IEnumerator ActivateAttackCollider()
    {
        attackCollider.SetActive(true);
        yield return new WaitForSeconds(0.1f); // ��u�����L���i�q�b�g���o�p�j
        attackCollider.SetActive(false);
    }


    private void OnDrawGizmos()
    {
        if (flontposition == null) return;

        Vector3 origin = flontposition.position;

        // ������ localScale.x �Ō����ڂɍ��킹�Ē���
        Vector3 direction = (transform.localScale.x < 0f) ? -transform.right : transform.right;

        // Ray�i���o���j�̉���
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(origin, origin + direction * detectionRange);

        // �I�_��Sphere�i���o�͈̖͂ڈ��j��`��
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        Gizmos.DrawWireSphere(origin + direction * detectionRange, sphereRadius);

        // �N�_�ɂ�Sphere�i�J�n�_�͈̔́j��`��
        Gizmos.color = new Color(0, 0.5f, 1f, 0.2f);
        Gizmos.DrawWireSphere(origin, sphereRadius);
    }
}
