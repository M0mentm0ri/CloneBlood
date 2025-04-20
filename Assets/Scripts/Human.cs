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
    private bool isIKActive = false;
    private bool previousRightClick = false;

    [Header("動きの速度調整")]
    public float moveSpeed = 10f;
    public float resetSpeed = 5f;

    [Header("見た目用Flip対象")]
    public Transform flipTarget;

    [SerializeField] private float flipSpeed = 5f; // 補間スピード
    [SerializeField] private float snapThreshold = 0.3f; // この範囲内は一気に反転

    private float targetScaleX = 1f;
    private float currentScaleX = 1f;

    [Header("手の移動制御")]
    public Transform centerPoint;
    public float maxArmDistance = 2.5f;

    [Header("移動速度")]
    public float moveSpeedX = 5f; // 横移動速度
    public float moveSpeedY = 5f; // 縦移動速度

    private Vector3 originalScale;

    // ラグドール化対象の Rigidbody2D のリスト
    public IKManager2D ikManager;
    public Rigidbody2D[] rigidbodies;
    public SpringJoint2D[] springJoint2Ds;
    public HingeJoint2D[] hingeJoint2Ds;
    public Rigidbody2D parentRb;      // 親のRigidbody2D
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

        // Rキーでラグドール化のトグル
        if (!isRagdollActive)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ActivateRagdoll();
            }
            
        }
        else
        {
            return; // それ以外の処理は全部スキップ
        }

        // マウスのワールド座標を取得
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        // 右クリックトグル検知（押した瞬間だけ切り替え）
        bool currentRightClick = Input.GetMouseButton(1);
        if (currentRightClick && !previousRightClick)
        {
            isIKActive = !isIKActive;

            // アニメーションレイヤーの重みを切り替え（0 = 無効、1 = 有効）
            animator.SetLayerWeight(armLayerIndex, isIKActive ? 0f : 1f);
        }
        previousRightClick = currentRightClick;


        // キャラの向きを判定（マウスが右側か左側か）
        bool isRight = mouseWorld.x > transform.position.x;

        if (flipTarget == null) return;

        float targetSign = isRight ? 1f : -1f;

        // 反転方向が変わる際、スムーズに補間
        Vector3 targetScale = originalScale;
        targetScale.x = Mathf.Abs(targetScale.x) * targetSign;

        if (Mathf.Abs(flipTarget.localScale.x - targetScale.x) < snapThreshold)
        {
            flipTarget.localScale = targetScale;
        }
        else
        {
            // 補間でスムーズに反転
            flipTarget.localScale = Vector3.Lerp(flipTarget.localScale, targetScale, Time.deltaTime * flipSpeed);
        }

        // アニメーションコントローラーで歩行中かどうか
        bool isWalking = Mathf.Abs(Input.GetAxis("Horizontal")) > 0 || Mathf.Abs(Input.GetAxis("Vertical")) > 0;

        if (isWalking)
        {
            animator.SetBool("IsWalk", true);  // 歩行中は歩行アニメーションをTrueに

            // 移動方向に応じてアニメーションのSpeedパラメータを設定
            if ((isRight && Input.GetAxis("Horizontal") < 0) || (!isRight && Input.GetAxis("Horizontal") > 0))
            {
                animator.SetFloat("Speed", -1f); // 右向きの時に左に歩く → Speedを-1に設定
            }
            else
            {
                animator.SetFloat("Speed", 1f);  // それ以外の移動方向 → Speedを1に設定
            }

            // 移動方向を設定
            float moveX = Input.GetAxis("Horizontal") * moveSpeedX * Time.deltaTime;
            float moveY = Input.GetAxis("Vertical") * moveSpeedY * Time.deltaTime;

            // 移動する
            transform.Translate(new Vector3(moveX, moveY, 0f));  // 移動方向に基づいてキャラクターを動かす
        }
        else
        {
            animator.SetBool("IsWalk", false);  // 歩行していないときは歩行アニメーションをFalseに
            animator.SetFloat("Speed", 1f);     // 歩行していない時はSpeedを1に戻す
        }

        if (isIKActive)
        {
            leftSolver.flip = true;
            rightSolver.flip = true;

            // 両手のオフセットを取得
            Vector3 offset_Left = target_LeftArm.position - parent_LeftArm.position;
            Vector3 offset_Right = target_RightArm.position - parent_RightArm.position;

            // 共通ターゲット位置を計算（左右両方の手が同じ位置に向かう）
            Vector3 targetPos = mouseWorld;

            // centerPointから一定の距離を超えないように制限
            Vector3 direction = targetPos - centerPoint.position;
            float distance = direction.magnitude;

            if (distance > maxArmDistance)
            {
                direction = direction.normalized * maxArmDistance;
                targetPos = centerPoint.position + direction;
            }

            // 両手を同じ位置にスムーズに移動
            parent_LeftArm.position = Vector3.Lerp(parent_LeftArm.position, targetPos - offset_Left, Time.deltaTime * moveSpeed);
            parent_RightArm.position = Vector3.Lerp(parent_RightArm.position, targetPos - offset_Right, Time.deltaTime * moveSpeed);
        }
        else
        {
            // IK無効時は元の位置（ローカル座標）に戻す
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
