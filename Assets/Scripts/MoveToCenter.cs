using System.Collections.Generic;
using UnityEngine;

public class MoveToCenter : MonoBehaviour
{
    [Header("中心を取る対象リスト")]
    public List<Transform> targetObjects;

    void Update()
    {
        if (targetObjects == null || targetObjects.Count == 0) return;

        Vector3 center = Vector3.zero;

        // 各Transformの位置を合計
        foreach (Transform t in targetObjects)
        {
            center += t.position;
        }

        // 平均をとって中心点を出す
        center /= targetObjects.Count;

        // 自身をその中心に移動
        transform.position = center;
    }
}
