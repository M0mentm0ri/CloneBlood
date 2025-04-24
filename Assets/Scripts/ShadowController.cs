using UnityEngine;

public class ShadowController : MonoBehaviour
{
    [Header("設定")]
    public Transform decalShadow; // 子にある影のTransform
    public float rayLength = 2.0f; // Rayの長さ（地面までの最大想定距離）
    public float maxShadowScale = 1.0f; // 最大スケール（足元ピッタリ）
    public float minShadowScale = 0.2f; // 最小スケール（浮いてるとき）

    public LayerMask groundLayer; // 地面判定用レイヤー

    void Update()
    {
        Vector2 origin = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, groundLayer);

        if (hit.collider != null)
        {
            Vector3 shadowPos = new Vector3(origin.x, hit.point.y + 0.1f, 0f);
            decalShadow.position = shadowPos;

            // ← ここで回転を固定（ワールドでの角度を0にする）
            decalShadow.rotation = Quaternion.Euler(90f, 0f, 0f);

            float distanceRatio = hit.distance / rayLength;
            float scale = Mathf.Lerp(maxShadowScale, minShadowScale, distanceRatio);
            decalShadow.localScale = new Vector3(scale, scale, 1f);
        }
        else
        {
            decalShadow.localScale = Vector3.zero;
        }
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.identity; // 親の回転を相殺して水平維持
    }
}
