using UnityEngine;

public class DecalFade : MonoBehaviour
{
    [Header("時間設定")]
    public float lifeTime = 5f;       // 縮小が始まるまでの時間
    public float shrinkDuration = 2f; // 縮小にかける時間

    private float elapsed = 0f;
    private Vector3 initialScale;

    private void Start()
    {
        // 初期サイズ記録
        initialScale = transform.localScale;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        if (elapsed > lifeTime)
        {
            float shrinkProgress = (elapsed - lifeTime) / shrinkDuration;
            shrinkProgress = Mathf.Clamp01(shrinkProgress);

            // 徐々に小さくする
            float scale = Mathf.Lerp(1f, 0f, shrinkProgress);
            transform.localScale = initialScale * scale;

            if (shrinkProgress >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}