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

    // === �Q�� ===
    public Transform targetPosition;
    public Animator animator;

    //���S�ʒu
    public Transform flontposition; // ���S�ʒu��Transform 
    // === �X�v���C�g ===
    public SpriteRenderer[] spriteRenderers;  // ������SpriteRenderer���i�[
    public Color maxHealthColor = Color.white; // �ő�w���X���̐F
    public Color minHealthColor = Color.red;  // �Œ�w���X���̐F

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

    private void OnParticleCollision(GameObject other)
    {
        if (isDead) return;

        // �_���[�W����
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

    // �_���[�W�v�Z
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
}
