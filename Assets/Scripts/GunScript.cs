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
    public GameObject mouthObject; // マウスの位置に移動させるオブジェクト

    private Vector3 cachePosition;
    // 最大移動可能距離（例えば2ユニットまでに制限）
    public float maxDistance = 2.0f;
    public float maxUpDistance = 1.5f; // 上方向の最大移動距離（インスペクターから設定）
    private Vector3 initialLocalPosition;

    // 最後のローカル角度を記憶する
    private float lastLocalAngle = 0f;

    // 最小距離：この距離未満なら固定
    public float minAimDistance = 0.5f;

    public ParticleSystem Blood_Particle;

    public LineRenderer lineRenderer; // 線を描画するためのLineRenderer

    void Start()
    {
        initialLocalPosition = mouthObject.transform.localPosition;
    }

    void Update()
    {
        if (!human.isIKActive)
        {
            // Z軸を緩やかに0に戻す処理を追加
            Vector3 currentEuler = gunWrist.localEulerAngles;

            // Unityの角度は 0〜360 なので、範囲補正（例：359→-1）
            float z = currentEuler.z;
            if (z > 180f) z -= 360f;

            // 緩やかに0へ近づける（Lerp）
            float smoothedZ = Mathf.Lerp(z, 0f, Time.deltaTime * 5f); // 5fは速度。好みで調整可能
            currentEuler.z = smoothedZ;

            cachePosition.Set(currentEuler.x, currentEuler.y, (smoothedZ + 360f) % 360f);
            gunWrist.localEulerAngles = cachePosition;

            // マウスオブジェクトを緩やかに元の位置に戻す

            cachePosition.x = gunWrist.position.x;
            cachePosition.y = gunWrist.position.y;
            cachePosition.z = mouthObject.transform.position.z;

            mouthObject.transform.localPosition = Vector3.Lerp(
                mouthObject.transform.localPosition,
                initialLocalPosition,
                Time.deltaTime * 5f // ← この数値が「戻る速度」
            );

            return;
        }

        // マウス位置（ターゲット位置）
        Vector3 targetPos = human.mouseWorld;

        // 中心点（制限の起点）
        Vector3 center = human.centerPoint.position;

        // 中心からマウスまでの距離ベクトル
        Vector3 offset = targetPos - center;

        // ---- 距離制限（XとYを個別に制限） ----

        // X方向制限（左右）：maxDistance にクランプ
        offset.x = Mathf.Clamp(offset.x, -maxDistance, maxDistance);

        // Y方向制限（上下）：下は0、上は maxUpDistance にクランプ
        offset.y = Mathf.Clamp(offset.y, 0f, maxUpDistance);

        // 実際に mouthObject を更新する位置
        mouthObject.transform.position = center + offset;


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

        if (Blood_Particle != null)
        {
            Blood_Particle.Play();
        }

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
    }

}
