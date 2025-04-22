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
    public Camera cam;
    public Animator animator;
    public int armLayerIndex = 1;
    public bool isIKActive = false;
    private bool previousRightClick = false;

    [Header("動きの速度調整")]
    public float moveSpeed = 10f;
    public float resetSpeed = 5f;

    [Header("見た目用Flip対象")]
    public bool isRight = true;
    public Transform flipTarget;

    [SerializeField] private float flipSpeed = 5f; // 補間スピード
    [SerializeField] private float snapThreshold = 0.3f; // この範囲内は一気に反転

    [Header("手の移動制御")]
    public Vector3 mouseWorld;
    public Transform centerPoint;
    public float maxArmDistance = 2.5f;

    [Header("移動速度")]
    [SerializeField] private float moveSpeedX = 10f;     // 横移動の加速力（Forceの大きさ）
    [SerializeField] private float maxSpeed = 5f;        // 最大移動速度

    private Vector3 originalScale;

    // ラグドール化対象の Rigidbody2D のリスト
    public IKManager2D ikManager;
    public Rigidbody2D[] rigidbodies;
    public SpringJoint2D[] springJoint2Ds;
    public HingeJoint2D[] hingeJoint2Ds;
    public Rigidbody2D parentRb;      // 親のRigidbody2D
    public Collider2D parent_collider2D;
    private bool isRagdollActive = false;

    void Start()
    {
        // 起動時に元のスケールを保存
        if (flipTarget != null)
        {
            originalScale = flipTarget.localScale;
        }
    }

    void Update()
    {
        // -------------------------------------
        // Rキーでラグドール有効化（トグル）
        // -------------------------------------
        if (!isRagdollActive)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ActivateRagdoll(); // ラグドール有効化
            }
        }
        else
        {
            return; // ラグドール中は一切の処理を停止（Updateの無駄処理防止）
        }

        // -------------------------------------
        // マウス座標の取得（Z軸を0に固定）
        // -------------------------------------
        // カメラからのマウス位置でレイを飛ばし、キャラクターのZ平面と交差させる
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // このキャラのZ平面に当たる点を取得（たとえばキャラの中心Z位置）
        float zPlane = transform.position.z;

        // t = (目標Z - レイの原点Z) / レイの方向Z成分
        float t = (zPlane - ray.origin.z) / ray.direction.z;
        mouseWorld = ray.origin + ray.direction * t;

        // -------------------------------------
        // 右クリックでIK有効／無効を切り替え（トグル方式）
        // -------------------------------------
        bool currentRightClick = Input.GetMouseButton(1);
        if (currentRightClick && !previousRightClick)
        {
            isIKActive = !isIKActive;
            // IKのON/OFFに応じてアニメーションレイヤーの重みを変更
            animator.SetLayerWeight(armLayerIndex, isIKActive ? 0f : 1f);
        }
        previousRightClick = currentRightClick;

        // -------------------------------------
        // キャラの向き判定（マウスが右側か左側か）
        // -------------------------------------
        isRight = mouseWorld.x > transform.position.x;

        if (flipTarget == null) return;

        float targetSign = isRight ? 1f : -1f;
        Vector3 targetScale = originalScale;
        targetScale.x = Mathf.Abs(targetScale.x) * targetSign;

        if (Mathf.Abs(flipTarget.localScale.x - targetScale.x) < snapThreshold)
        {
            // 十分近ければスナップして即反転
            flipTarget.localScale = targetScale;
        }
        else
        {
            // スムーズに反転するよう補間
            flipTarget.localScale = Vector3.Lerp(flipTarget.localScale, targetScale, Time.deltaTime * flipSpeed);
        }

        // -------------------------------------
        // 移動処理：キー入力に応じて移動＋アニメーション切り替え
        // -------------------------------------
        float inputX = Input.GetAxis("Horizontal");
        bool isWalking = Mathf.Abs(inputX) > 0;

        if (isWalking)
        {
            animator.SetBool("IsWalk", true); // 歩行フラグをON

            // 逆向きに歩いているかどうか（右向きで左に歩く、またはその逆）
            bool oppositeDirection = (isRight && inputX < 0) || (!isRight && inputX > 0);

            // 横方向に力を加える（ForceMode2D.Force で加速度的に加える）
            parentRb.AddForce(new Vector2(inputX * moveSpeedX, 0f), ForceMode2D.Force);

            // 現在の速度を取得
            Vector2 currentVelocity = parentRb.linearVelocity;

            // 【ここがポイント】
            // 現在の速度を最大速度で割って
            float speedRatio = Mathf.Abs(currentVelocity.x) / maxSpeed;


            // 最大速度制限（オーバーしていたら制限する）
            if (Mathf.Abs(currentVelocity.x) > maxSpeed)
            {
                currentVelocity.x = Mathf.Sign(currentVelocity.x) * maxSpeed;
                parentRb.linearVelocity = new Vector2(currentVelocity.x, currentVelocity.y);
            }

            // 向きが逆ならマイナス値にして、反転再生
            float animSpeed = speedRatio * (oppositeDirection ? -1f : 1f);
            animator.SetFloat("Speed", animSpeed);
        }
        else
        {
            animator.SetBool("IsWalk", false); // 静止時
            animator.SetFloat("Speed", 0f);    // Speed値を初期状態に戻す
        }

        // -------------------------------------
        // IK処理（腕の追従制御）
        // -------------------------------------
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
    void ActivateRagdoll()
    {
        if (isRagdollActive) return; // 二度実行防止
        isRagdollActive = true;

        foreach (var rb in rigidbodies)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;

            if(parent_collider2D != null)
            {
                parent_collider2D.enabled = false;
            }
            // 親の速度を各子パーツに渡す（慣性引き継ぎ）
            if (parentRb != null)
            {
                rb.linearVelocity = parentRb.linearVelocity * 2;
                rb.angularVelocity = parentRb.angularVelocity * 2;
            }
        }

        if (ikManager != null) ikManager.enabled = false;
        if (animator != null) animator.enabled = false; // アニメーション停止

        StartCoroutine(FreezeAfterSeconds(3f)); // ラグドール3秒後に硬直
    }
    IEnumerator FreezeAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        foreach (var sp in springJoint2Ds)
        {
            if (!sp) continue;
            Destroy(sp);
        }

        foreach (var h in hingeJoint2Ds)
        {
            if (!h) continue;
            Destroy(h);
        }

        foreach (var rb in rigidbodies)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            Destroy(rb);
        }

        if (parentRb != null)
        {
            parentRb.bodyType = RigidbodyType2D.Dynamic;
        }
    }
}
