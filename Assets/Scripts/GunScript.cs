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
    public GameObject decalPrefab; // Inspectorで設定

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
        // ==========================
        // 1. 2D処理（ゲームロジック）
        // ==========================

        // 方向計算（2DだけどZ=0で問題なし）
        Vector2 direction2D = (gunDirection.position - gunFront.position).normalized;

        // 2D Raycast（プレイヤーや敵などの2Dオブジェクト用）
        RaycastHit2D hit2D = Physics2D.Raycast(gunFront.position, direction2D, range);

        if (hit2D.collider != null)
        {
            Debug.Log("【2D命中】: " + hit2D.collider.name);

            // Rigidbody2D を持ってたら吹っ飛ばす
            Rigidbody2D rb2D = hit2D.collider.GetComponent<Rigidbody2D>();
            if (rb2D != null)
            {
                rb2D.AddForce(direction2D * shootForce);
            }
        }

        // ==========================
        // 2. 3D処理（演出用デカール）
        // ==========================

        // 3D用にVector3の方向を用意（Z方向も含む）
        Vector3 direction3D = (gunDirection.position - gunFront.position).normalized;

        // 3D Raycast（壁や床など3Dオブジェクトへの命中確認）
        if (Physics.Raycast(gunFront.position, direction3D, out RaycastHit hit3D, range))
        {
            Debug.Log("【3D命中】: " + hit3D.collider.name);

            if (decalPrefab != null)
            {
                Vector3 spawnPos = hit3D.point + hit3D.normal * 0.01f;

                // まず通常の回転
                Quaternion lookRot = Quaternion.LookRotation(hit3D.normal);

                // オイラー角に変換してX軸を反転
                Vector3 euler = lookRot.eulerAngles;

                // ↓↓↓ ここを +90°に強制する
                euler.x = (euler.x + 180f) % 360f;

                Quaternion finalRot = Quaternion.Euler(euler);

                GameObject decal = Instantiate(decalPrefab, spawnPos, finalRot);
                decal.transform.SetParent(hit3D.collider.transform);

                // ランダム回転
                decal.transform.Rotate(Vector3.forward, Random.Range(0f, 360f));
            }
        }
    }

}
