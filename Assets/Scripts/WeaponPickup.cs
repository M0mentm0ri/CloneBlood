using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("参照")]
    public Human human;                     // Humanスクリプト（マウス位置などを参照）
    public Transform handTransform;         // 武器を持つ手のTransform（ここに武器を装着）
    public LayerMask weaponLayer;           // 武器のLayer（レイヤーでフィルタリング）

    [Header("拾い条件")]
    public float pickupRange = 2f;          // 拾える距離

    [Header("照準補助")]
    public Transform mouthObject;           // マウス方向に移動するオブジェクト（照準点）
    public float maxDistance = 2.0f;        // 左右の制限距離
    public float maxUpDistance = 1.5f;      // 上方向の制限距離
    public float minAimDistance = 0.5f;     // 回転させる最小距離
    public float angle_clamp = 90f;         // 銃の角度制限

    [Header("装備中の武器")]
    public GunScript currentGun;            // 現在装備しているGunScript
    public Transform gunWrist;              // 銃の回転対象となる手首（GunScriptから移籍）
    public bool HasGun; // 武器を持っているかどうか

    private Vector3 initialLocalPosition;   // mouthObject の初期位置
    private float lastLocalAngle = 0f;      // 最後の手首角度（回転補正）
    private Vector3 cachePosition;

    void Start()
    {
        initialLocalPosition = mouthObject.transform.localPosition;
        //初期で武器を持っているかどうか
        HasGun = currentGun != null;
    }

    void Update()
    {
        if(human == null || human.isDead)
        {
            return;
        }

        // Fキーを押したら拾う
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryPickupWeapon();
        }

        if (!human.isIKActive)
        {
            AimReturn();
            return; // IKが無効な場合は何もしない
        }

        MouthClamp();

        // 🔥 マウス方向に手首（＝銃）を向ける
        AimAtMouse();

        // マウスクリックで発射（拾ってる武器があれば）
        if (Input.GetMouseButtonDown(0) && currentGun != null)
        {
            currentGun.Shoot(); // GunScriptの発射関数を呼ぶだけ
        }
    }

    void TryPickupWeapon()
    {
        if (currentGun != null)
        {
            DropCurrentWeapon();
        }
        else
        {
            // 3D空間内での球体範囲の当たり判定
            Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange, weaponLayer);
            foreach (Collider hit in hits)
            {
                GunScript gun = hit.GetComponent<GunScript>();
                if (gun != null)
                {
                    PickupWeapon(gun);
                    break;
                }
            }
        }

    }

    void PickupWeapon(GunScript gun)
    {
        // 武器のTransformを手の子にして、位置・回転をリセット
        Transform weapon = gun.transform;
        weapon.SetParent(handTransform);
        weapon.localPosition = Vector3.zero;
        weapon.localRotation = Quaternion.identity;

        currentGun = gun;

        currentGun.rigidbody.isKinematic = true; // Rigidbodyを無効にする
        // IKや手首補正はここでやるならこの位置で書いてOK
        // 例: gun.gunWrist.localEulerAngles = Vector3.zero;
        HasGun = true;
    }
    
    void AimReturn()
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

    }

    void MouthClamp()
    {
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
    public void DropCurrentWeapon()
    {
        currentGun.rigidbody.isKinematic = false; // Rigidbodyを有効にする
        // 親子関係を切る（地面に落とす）
        currentGun.transform.SetParent(null);
        currentGun = null;
        HasGun = false; // 武器を持っていない状態に戻す

    }

    // Gizmoで視覚確認
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
