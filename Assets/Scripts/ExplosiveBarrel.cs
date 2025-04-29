using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("�������邽�߂ɕK�v�ȍŏ��Ռ���")]
    public float explosionThreshold = 10f;  // �Ռ��͂̂������l

    [Header("�������ɍ폜����܂ł̎���")]
    public float destroyDelay = 2f;          // �����G�t�F�N�g��ɉ��b�ŏ�����

    [Header("�_�Őݒ�")]
    public SpriteRenderer targetRenderer;    // �F��ς�����SpriteRenderer
    public Color blinkColor = Color.red;      // �_�Ŏ��̐F
    public float blinkInterval = 0.2f;        // �_�ł���Ԋu�i�b�j

    [Header("�����ݒ�")]
    public GameObject explosionTriggerObject;
    public float explosionRadius = 5f;        // �����̓͂��͈� �y���ǉ��z
    public float explosionForce = 700f;       // �����̋��� �y���ǉ��z
    public float upwardsModifier = 1f;        // ������ւ̗͕␳ �y���ǉ��z

    private bool hasExploded = false;         // ���łɔ���������
    private bool isBlinking = false;          // �_�Œ����ǂ���
    private int groundLayer;                  // Ground���C���[�̔ԍ�
    private Color originalColor;              // �ŏ��̐F
    private float blinkTimer = 0f;             // �_�ŗp�^�C�}�[
    private bool isOriginalColor = true;       // ���݁A�I���W�i���F���ǂ���

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

        // �����Ŕ����p�[�e�B�N���o��
        Vector3 hitPos = transform.position;
        Vector3 hitNormal = Vector3.up;
        Quaternion finalRot = Quaternion.LookRotation(hitNormal);

        GameReferences.Instance.particleManager.PlayParticle(
            ParticleManager.ParticleType.Explosion,
            hitPos,
            finalRot
        );

        // ������ �������甚�������ǉ� ������

        // �������ɃR���W�����̃A�N�e�B�u��Ԃ��I���ɂ���
        if (explosionTriggerObject != null)
        {
            explosionTriggerObject.SetActive(true); // �R���W�����I�u�W�F�N�g��L����
        }

        // ���a���̑S�R���C�_�[���擾
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.attachedRigidbody; // Rigidbody���t���Ă��邩�`�F�b�N
            if (rb != null && rb != GetComponent<Rigidbody>())
            {
                // �����͂�^����i������␳��������j
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsModifier, ForceMode.Impulse);
            }
        }

        // ������ �������������܂� ������
        // �������Ă���R���W�����I�u�W�F�N�g�̃A�N�e�B�u���I�t�ɂ���
        Invoke("DisableExplosionTrigger", 0.1f);
    }

    private void DisableExplosionTrigger()
    {
        if (explosionTriggerObject != null)
        {
            explosionTriggerObject.SetActive(false); // �R���W�����I�u�W�F�N�g�𖳌���
        }
        Destroy(gameObject); // �������g���폜
    }

    // �����͈͂�Gizmos�ŕ\��
    private void OnDrawGizmos()
    {
        // �F��ύX���Ĕ͈͂�����
        Gizmos.color = Color.red;
        // �͈͂�\���i���́j
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
