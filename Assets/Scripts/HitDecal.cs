using UnityEngine;
using System.Collections.Generic;

public class HitDecal : MonoBehaviour
{
    public GameObject decalPrefab;
    public ParticleSystem particleSystem;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    public float decalMergeRadius = 0.5f; // �������锼�a�i�����j���w��
    public string decalTag = "Decal"; // �����Ώۂ̃^�O�i�f�J�[���̃^�O���w��j
    public float sizeIncrement = 0.2f; // �ǉ�����T�C�Y�̑����ʁi0.2�{�����Z�j
    public float maxSize = 3f; // �ő�T�C�Y�̐���

    void OnParticleCollision(GameObject other)
    {
        int numEvents = particleSystem.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < numEvents; i++)
        {
            Vector3 hitPos = collisionEvents[i].intersection;    // �Փˈʒu
            Vector3 hitNormal = collisionEvents[i].normal;        // �Փ˖ʂ̖@��

            // ��ɕ\���J�������� or ���[���h������Ɍ����������߁A�@���̋t��������������
            // �ʏ��Z+�����f�J�[���Ȃ�AZ-������Look����i�@���̋t�������j
            Quaternion finalRot = Quaternion.LookRotation(-hitNormal);

            MergeOrCreateDecal(hitPos, finalRot);
        }
    }

    // �߂��̃f�J�[�����������A�����܂��͐V�K�쐬
    void MergeOrCreateDecal(Vector3 spawnPos, Quaternion rotation)
    {
        // �߂��̃f�J�[��������
        Collider[] hitColliders = Physics.OverlapSphere(spawnPos, decalMergeRadius);

        GameObject closestDecal = null;
        float closestDistance = Mathf.Infinity;

        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag(decalTag))  // �^�O����v����f�J�[��������
            {
                float distance = Vector3.Distance(spawnPos, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDecal = collider.gameObject;
                }
            }
        }

        // �߂��Ƀf�J�[�����������ꍇ�A����
        if (closestDecal != null)
        {
            // �����̃f�J�[���̃T�C�Y���擾�i�����T�C�Y����ɉ��Z���Ă����j
            float currentSize = closestDecal.transform.localScale.x; // ���ɑS���������T�C�Y���Ɖ���
            float newSize = Mathf.Min(currentSize + sizeIncrement, maxSize); // �T�C�Y�����ʁi0.2�{���Z�j�A�ő�T�C�Y����

            // �����̃f�J�[���̃T�C�Y��ύX
            closestDecal.transform.localScale = new Vector3(newSize, newSize, newSize);  // �V�����T�C�Y��ݒ�
        }
        else
        {
            // �߂��Ƀf�J�[�����Ȃ���ΐV�����f�J�[���𐶐�
            GameObject newDecal = Instantiate(decalPrefab, spawnPos, rotation);
            newDecal.transform.localScale = new Vector3(1f, 1f, 1f); // �����T�C�Y
            newDecal.tag = decalTag;  // �^�O��ݒ�
        }
    }
}
