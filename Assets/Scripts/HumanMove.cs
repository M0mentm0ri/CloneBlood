using UnityEngine;
using UnityEngine.U2D.IK;
using System.Collections;

public class Human : MonoBehaviour
{
    [Header("左腕 IK 設定")]
    public Transform parent_LeftArm;
    public Transform target_LeftArm;
    public LimbSolver2D leftSolver;

    [Header("右腕 IK 設定")]
    public Transform parent_RightArm;
    public Transform target_RightArm;
    public LimbSolver2D rightSolver;

    [Header("共通設定")]
    public Animator animator;
    public int armLayerIndex = 1;
    public bool isIKActive = false;
    private bool previousRightClick = false;

    public bool isDead = false; // 死亡フラグ

    [Header("動きの速度調整")]
    public float moveSpeed = 10f;
    public float resetSpeed = 5f;

    [Header("見た目用Flip対象")]
    public bool isRight = true;
    public Transform flipTarget;

    [SerializeField] private float flipSpeed = 5f; // 補間スピード
    [SerializeField] private float snapThreshold = 0.3f; // この範囲内は一気に反転
    private Vector2 force = Vector2.zero;  // 毎回使い回す
    private Vector2 limitedVelocity = Vector2.zero;

    [Header("手の移動制御")]
    public Vector3 mouseWorld;
    public Transform centerPoint;
    public float maxArmDistance = 2.5f;

    [Header("移動速度")]
    [SerializeField] private float moveSpeedX = 10f;         // 横移動の加速力
    [SerializeField] private float maxSpeed = 5f;            // 最大移動速度

    [Header("アニメーション基準速度")]
    [SerializeField] private float animBaseSpeed = 3f;       // この速度でアニメーションSpeed=1fになる

    [Header("参照")]
    public WeaponPickup weaponPickup; // 武器を持つスクリプト
    public HumanStats humanStats; // 武器を持つスクリプト

    // ラグドール化対象の Rigidbody2D のリスト
    public IKManager2D ikManager;
    public Rigidbody[] rigidbodies;
    public SpringJoint[] springJoints;
    public HingeJoint[] hingeJoints;
    public Rigidbody parentRb;      // 親のRigidbody2D
    public Collider parent_collider;
    private bool isRagdollActive = false;


    void Update()
    {

        if(isDead || humanStats == null || !humanStats.IsInitiative)
        {
            return; // 死亡時は何もしない
        }

        // -------------------------------------
        // Rキーでラグドール有効化（トグル）
        // -------------------------------------
        if (!isRagdollActive)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                humanStats.Die(); // 死亡処理を呼び出す
            }
        }
        else
        {
            return; // ラグドール中は一切の処理を停止（Updateの無駄処理防止）
        }

        UpdateMouse();

        ToggleIK();

        UpdateFlip();

        Move();

        UpdateIK();
    }

    // -------------------------------------
    // マウス座標の取得（Z軸を0に固定）
    // -------------------------------------
    void UpdateMouse()
    {


        // カメラからのマウス位置でレイを飛ばし、キャラクターのZ平面と交差させる
        Ray ray = humanStats.cam.ScreenPointToRay(Input.mousePosition);

        // このキャラのZ平面に当たる点を取得（たとえばキャラの中心Z位置）
        float zPlane = transform.position.z;

        // t = (目標Z - レイの原点Z) / レイの方向Z成分
        float t = (zPlane - ray.origin.z) / ray.direction.z;
        mouseWorld = ray.origin + ray.direction * t;
    }

    // -------------------------------------
    // 右クリックでIK有効／無効を切り替え（トグル方式）//持つ武器がない場合は無効
    // -------------------------------------
    void ToggleIK()
    {


        if (weaponPickup == null)
        {
            return;
        }

        if (weaponPickup.HasGun)
        {
            bool currentRightClick = Input.GetMouseButton(1);
            if (currentRightClick && !previousRightClick)
            {
                isIKActive = !isIKActive;
                // IKのON/OFFに応じてアニメーションレイヤーの重みを変更
                animator.SetLayerWeight(armLayerIndex, isIKActive ? 0f : 1f);
            }
            previousRightClick = currentRightClick;
        }
        else
        {
            if (isIKActive)
            {
                isIKActive = false;
                // IKのON/OFFに応じてアニメーションレイヤーの重みを変更
                animator.SetLayerWeight(armLayerIndex, isIKActive ? 0f : 1f);
            }

        }
    }

    // -------------------------------------
    // キャラの向き判定（マウスが右側か左側か）
    // -------------------------------------
    void UpdateFlip()
    {


        isRight = mouseWorld.x > transform.position.x;

        if (flipTarget == null) return;

        // 右向き: Y = 0, 左向き: Y = 180
        float targetYRotation = isRight ? 0f : 180f;
        Quaternion targetRotation = Quaternion.Euler(0f, targetYRotation, 0f);

        if (Quaternion.Angle(flipTarget.rotation, targetRotation) < snapThreshold)
        {
            // 十分近ければスナップして即反転
            flipTarget.rotation = targetRotation;
        }
        else
        {
            // スムーズに回転補間
            flipTarget.rotation = Quaternion.Lerp(flipTarget.rotation, targetRotation, Time.deltaTime * flipSpeed);
        }
    }

    // -------------------------------------
    // 移動処理：キー入力に応じて移動＋アニメーション切り替え
    // -------------------------------------
    void Move()
    {

        // 入力取得


        float inputX = Input.GetAxis("Horizontal");
        bool isWalking = Mathf.Abs(inputX) > 0;

        if (isWalking)
        {
            animator.SetBool("IsWalk", true);

            // 向きの反転判定
            bool oppositeDirection = (isRight && inputX < 0) || (!isRight && inputX > 0);

            // 移動処理
            force.x = inputX * moveSpeedX;
            force.y = 0f;
            parentRb.AddForce(force, ForceMode.Force);

            // 現在の速度を取得
            limitedVelocity = parentRb.linearVelocity;

            // 最大速度制限
            if (Mathf.Abs(limitedVelocity.x) > maxSpeed)
            {
                limitedVelocity.x = Mathf.Sign(limitedVelocity.x) * maxSpeed;
                parentRb.linearVelocity = limitedVelocity;
            }

            // アニメーション再生速度を移動速度に応じて正規化
            float animSpeedRatio = Mathf.Abs(limitedVelocity.x) / animBaseSpeed;

            // 向きに応じて符号反転（反対方向なら負にする）
            float animSpeed = animSpeedRatio * (oppositeDirection ? -1f : 1f);

            animator.SetFloat("Speed", animSpeed);
        }
        else
        {
            // 止まっているとき
            animator.SetBool("IsWalk", false);
            animator.SetFloat("Speed", 0f);
        }
    }

    // -------------------------------------
    // IK処理（腕の追従制御）
    // -------------------------------------
    void UpdateIK()
    {

        if (isIKActive)
        {
            // ArmSolverの左右反転を有効に
            leftSolver.flip = true;
            rightSolver.flip = true;

            // 各腕の現在のオフセットを保持
            Vector3 offset_Left = target_LeftArm.position - parent_LeftArm.position;
            Vector3 offset_Right = target_RightArm.position - parent_RightArm.position;

            // 両手が目指す共通ターゲット（マウス座標）
            Vector3 targetPos = mouseWorld;

            // 腕の最大移動距離を制限
            Vector3 direction = targetPos - centerPoint.position;
            float distance = direction.magnitude;
            if (distance > maxArmDistance)
            {
                direction = direction.normalized * maxArmDistance;
                targetPos = centerPoint.position + direction;
            }

            // 両手をターゲット位置に滑らかに移動させる（オフセット補正あり）
            parent_LeftArm.position = Vector3.Lerp(parent_LeftArm.position, targetPos - offset_Left, Time.deltaTime * moveSpeed);
            parent_RightArm.position = Vector3.Lerp(parent_RightArm.position, targetPos - offset_Right, Time.deltaTime * moveSpeed);
        }
        else
        {
            // IKが無効なときは元の位置（ローカル）に戻す
            parent_LeftArm.localPosition = Vector3.Lerp(parent_LeftArm.localPosition, Vector3.zero, Time.deltaTime * resetSpeed);
            parent_RightArm.localPosition = Vector3.Lerp(parent_RightArm.localPosition, Vector3.zero, Time.deltaTime * resetSpeed);
        }
    }


    // ラグドール化の切り替え
    public void ActivateRagdoll()
    {
        if (isRagdollActive) return; // 二度実行防止
        isRagdollActive = true;

        foreach (var rb in rigidbodies)
        {
            rb.isKinematic = false;

            if(parent_collider != null)
            {
                parent_collider.enabled = false;
            }
            // 親の速度を各子パーツに渡す（慣性引き継ぎ）
            if (parentRb != null)
            {
                rb.linearVelocity = parentRb.linearVelocity * 2;
                rb.angularVelocity = parentRb.angularVelocity * 2;
            }
        }

        parentRb.isKinematic = true;

        if (ikManager != null) ikManager.enabled = false;
        if (animator != null) animator.enabled = false; // アニメーション停止

        StartCoroutine(FreezeAfterSeconds(3f)); // ラグドール3秒後に硬直
    }
    IEnumerator FreezeAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        foreach (var sp in springJoints)
        {
            if (!sp) continue;
            Destroy(sp);
        }

        foreach (var h in hingeJoints)
        {
            if (!h) continue;
            Destroy(h);
        }

        foreach (var rb in rigidbodies)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = Vector3.zero;
            Destroy(rb);
        }

        if (parentRb != null)
        {
            parentRb.isKinematic = false;
        }
    }

}
