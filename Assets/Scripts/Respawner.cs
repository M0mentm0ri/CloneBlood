using UnityEngine;

public class Respawner : MonoBehaviour
{
    [Header("復活させたいPrefab")]
    public GameObject prefabToRespawn; // 消えたら置き直したいプレハブ

    [Header("復活させる位置")]
    public Vector3 respawnPosition;    // 再生成する場所

    private GameObject currentInstance; // 今存在しているインスタンス

    private void Start()
    {
        // 最初に一回生成しておく
        Spawn();
    }

    private void Update()
    {
        // 今のインスタンスがなくなったら再生成する
        if (currentInstance == null)
        {
            Spawn();
        }
    }

    private void Spawn()
    {
        // インスタンスを作成して記録する
        currentInstance = Instantiate(prefabToRespawn, respawnPosition, Quaternion.identity);
    }
}
