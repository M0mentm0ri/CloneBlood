using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GunScript : MonoBehaviour
{

    public Human human;          // Humanスクリプトへの参照
    public Transform gunWrist;        // 手首ボーン（これを回転させる）
    public Transform gunFront;        // 銃口のTransform
    public Transform gunDirection;    // 向きの基準Transform
    public float shootForce = 500f;   // 吹っ飛ばす力
    public float angle_clamp = 90f;         // 銃の角度制限
    public float range = 10f;         // Rayの射程距離

    // 最後のローカル角度を記憶する
    private float lastLocalAngle = 0f;

    // 最小距離：この距離未満なら固定
    public float minAimDistance = 0.5f;


    public LineRenderer lineRenderer; // 線を描画するためのLineRenderer

    void Update()
    {
        // 🔥 マウス方向に手首（＝銃）を向ける
        AimAtMouse();

        // 毎フレーム線を描画
        UpdateLaserLine();

        // 発射処理
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }


    void AimAtMouse()
    {
        if (human == null || gunWrist == null)
            return;

        // マウスのワールド位置
        Vector3 mouseWorldPos = human.mouseWorld;

        // 手首からマウスへのベクトル
        Vector2 aimDir = mouseWorldPos - gunWrist.position;

        // 距離が近すぎるなら回転しない
        if (aimDir.magnitude < minAimDistance)
        {
            Vector3 localEuler = gunWrist.localEulerAngles;
            localEuler.z = lastLocalAngle;
            gunWrist.localEulerAngles = localEuler;
            return;
        }

        // マウス方向の角度（ワールド）
        float rawAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        // 親の回転を差し引いてローカル角度にする
        float parentZ = gunWrist.parent != null ? gunWrist.parent.eulerAngles.z : 0f;
        float localTargetAngle;

        if (!human.isRight) // 左向き（親が反転している）
        {
            // 左向きの場合は角度計算を反転
            localTargetAngle = Mathf.DeltaAngle(0, 180f - (rawAngle - parentZ));
        }
        else // 右向き（通常）
        {
            // 通常の計算
            localTargetAngle = Mathf.DeltaAngle(0, rawAngle - parentZ);
        }

        // 緩やかに角度制限（Cosで丸くなる）
        float t = Mathf.Abs(localTargetAngle) / angle_clamp;
        t = Mathf.Clamp01(t);
        float weight = Mathf.Cos(t * Mathf.PI / 2);
        float finalLocalAngle = Mathf.Clamp(localTargetAngle * weight, -angle_clamp, angle_clamp);

        // ローカル回転を適用
        Vector3 newEuler = gunWrist.localEulerAngles;
        newEuler.z = finalLocalAngle;
        gunWrist.localEulerAngles = newEuler;
        lastLocalAngle = finalLocalAngle;
    }

    void UpdateLaserLine()
    {
        Vector2 direction = (gunDirection.position - gunFront.position).normalized;
        Vector3 endPosition = gunFront.position + (Vector3)(direction * range);

        lineRenderer.SetPosition(0, gunFront.position);
        lineRenderer.SetPosition(1, endPosition);
    }

    void Shoot()
    {
        Vector2 direction = (gunDirection.position - gunFront.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(gunFront.position, direction, range);

        if (hit.collider != null)
        {
            Debug.Log("命中: " + hit.collider.name);

            Rigidbody2D rb = hit.collider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(direction * shootForce);
            }
        }
    }
}
