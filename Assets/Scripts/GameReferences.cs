using UnityEngine;

// �S�̂̋��ʎQ�Ƃ����V���O���g��
public class GameReferences : MonoBehaviour
{
    public static GameReferences Instance { get; private set; }


    // �����ɌŒ�Q�Ƃ���X�N���v�g����ׂ�

    [Header("�Q�ƃ��X�g")]
    public Shake shake; // Shake�N���X�̃C���X�^���X��ێ�
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

    // �_���[�W�v�Z
    public int GetDamageFromTag(string tag, string targetType)
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
}
