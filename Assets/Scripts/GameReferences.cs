using UnityEngine;

// �S�̂̋��ʎQ�Ƃ����V���O���g��
public class GameReferences : MonoBehaviour
{
    public static GameReferences Instance { get; private set; }

    // �����ɌŒ�Q�Ƃ���X�N���v�g����ׂ�

    [Header("�Q�ƃ��X�g")]
    public ParticleManager particleManager;

    // �K�v�ɉ����Ăǂ�ǂ�ǉ����Ă���

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // �V�[���܂��������Ȃ炱�����t����
        // DontDestroyOnLoad(gameObject);
    }
}
