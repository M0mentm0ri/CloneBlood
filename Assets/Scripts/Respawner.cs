using UnityEngine;

public class Respawner : MonoBehaviour
{
    [Header("������������Prefab")]
    public GameObject prefabToRespawn; // ��������u�����������v���n�u

    [Header("����������ʒu")]
    public Vector3 respawnPosition;    // �Đ�������ꏊ

    private GameObject currentInstance; // �����݂��Ă���C���X�^���X

    private void Start()
    {
        // �ŏ��Ɉ�񐶐����Ă���
        Spawn();
    }

    private void Update()
    {
        // ���̃C���X�^���X���Ȃ��Ȃ�����Đ�������
        if (currentInstance == null)
        {
            Spawn();
        }
    }

    private void Spawn()
    {
        // �C���X�^���X���쐬���ċL�^����
        currentInstance = Instantiate(prefabToRespawn, respawnPosition, Quaternion.identity);
    }
}
