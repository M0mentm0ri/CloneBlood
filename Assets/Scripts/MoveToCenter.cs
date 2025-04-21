using System.Collections.Generic;
using UnityEngine;

public class MoveToCenter : MonoBehaviour
{
    [Header("���S�����Ώۃ��X�g")]
    public List<Transform> targetObjects;

    void Update()
    {
        if (targetObjects == null || targetObjects.Count == 0) return;

        Vector3 center = Vector3.zero;

        // �eTransform�̈ʒu�����v
        foreach (Transform t in targetObjects)
        {
            center += t.position;
        }

        // ���ς��Ƃ��Ē��S�_���o��
        center /= targetObjects.Count;

        // ���g�����̒��S�Ɉړ�
        transform.position = center;
    }
}
