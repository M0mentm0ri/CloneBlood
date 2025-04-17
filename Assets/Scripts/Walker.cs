using UnityEngine;

public class Walker : MonoBehaviour
{
    [Header("体幹")]
    public Rigidbody2D core;

    [Header("頭")]
    [SerializeField] Rigidbody2D head;

    [Header("足")]
    public Rigidbody2D leftLeg;
    public Rigidbody2D rightLeg;

    [Header("足の目標位置")]
    public Transform footTargetL;
    public Transform footTargetR;

    public Transform targetMarker; // ターゲット位置を視覚化用に置けるなら（任意）

    [Header("歩行設定")]
    public float moveSpeed = 10f;
    public float stepDistance = 1.0f;
    public float stepSpeed = 4.0f;
    public float footFollowForce = 100f;
    public float uprightForce = 50f;

    [Header("足の姿勢制御")]
    public Rigidbody2D leftFoot;
    public Rigidbody2D rightFoot;
    public float targetFootAngle = 0f; // 地面と水平 = 0度
    public float footTorqueStiffness = 100f;
    public float footTorqueDamping = 10f;

    [SerializeField] float hangHeight = 1.5f;
    [SerializeField] float hangForce = 100f;
    [SerializeField] float dampForce = 10f;

    public float maxHorizontalOffset = 1.0f;

    private float inputX;
    private float currentOffsetX = 0f;

    // 足の接地チェック用変数
    public Transform footCheckL;
    public Transform footCheckR;
    public float groundCheckRadius = 0.05f;
    public LayerMask groundLayer;

    void FixedUpdate()
    {
        // 入力取得（←→キー）
        inputX = Input.GetAxis("Horizontal");

        ApplyFootTorque(leftFoot);
        ApplyFootTorque(rightFoot);

        // ターゲットXの位置を少しずつ移動（滑らかに）
        currentOffsetX = Mathf.MoveTowards(currentOffsetX, inputX * maxHorizontalOffset, moveSpeed * Time.fixedDeltaTime);

        // ターゲット位置（腰＋足の平均から真上へ）＋Xオフセット
        Vector2 basePos = (leftLeg.position + rightLeg.position + core.position) / 3;
        Vector2 targetPos = basePos + Vector2.up * hangHeight + Vector2.right * currentOffsetX;

        // ターゲットマーカーの表示（任意）
        if (targetMarker != null)
            targetMarker.position = targetPos;

        // 足が接地しているか判定
        bool isLeftGrounded = Physics2D.OverlapCircle(footCheckL.position, groundCheckRadius, groundLayer);
        bool isRightGrounded = Physics2D.OverlapCircle(footCheckR.position, groundCheckRadius, groundLayer);
        bool isAnyFootGrounded = isLeftGrounded || isRightGrounded;

        // 接地している場合のみ、頭を引っ張る
        if (isAnyFootGrounded)
        {
            Vector2 toTarget = targetPos - head.position;
            Vector2 force = toTarget * hangForce - head.velocity * dampForce;
            head.AddForce(force);
        }
    }

    void ApplyFootTorque(Rigidbody2D footRb)
    {
        float currentAngle = footRb.rotation; // 現在のZ軸回転角
        float angleDiff = Mathf.DeltaAngle(currentAngle, targetFootAngle);

        // バネ力（角度差 × 剛性） - 角速度 × 減衰
        float torque = angleDiff * footTorqueStiffness - footRb.angularVelocity * footTorqueDamping;

        footRb.AddTorque(torque);
    }
}
