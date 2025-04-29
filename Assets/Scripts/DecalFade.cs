using UnityEngine;

public class DecalFade : MonoBehaviour
{
    [Header("���Ԑݒ�")]
    public float lifeTime = 5f;       // �k�����n�܂�܂ł̎���
    public float shrinkDuration = 2f; // �k���ɂ����鎞��

    private float elapsed = 0f;
    private Vector3 initialScale;

    private void Start()
    {
        // �����T�C�Y�L�^
        initialScale = transform.localScale;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        if (elapsed > lifeTime)
        {
            float shrinkProgress = (elapsed - lifeTime) / shrinkDuration;
            shrinkProgress = Mathf.Clamp01(shrinkProgress);

            // ���X�ɏ���������
            float scale = Mathf.Lerp(1f, 0f, shrinkProgress);
            transform.localScale = initialScale * scale;

            if (shrinkProgress >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}