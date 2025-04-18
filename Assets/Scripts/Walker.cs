using UnityEngine;

public class Walker : MonoBehaviour
{
    [Header("�̊�")]
    public Rigidbody2D core;

    [Header("��")]
    [SerializeField] Rigidbody2D head;

    [Header("��")]
    public Rigidbody2D leftLeg;
    public Rigidbody2D rightLeg;

    [Header("���̖ڕW�ʒu")]
    public Transform footTargetL;
    public Transform footTargetR;

    public Transform targetMarker; // �^�[�Q�b�g�ʒu�����o���p�ɒu����Ȃ�i�C�Ӂj

    [Header("���s�ݒ�")]
    public float moveSpeed = 10f;
    public float stepDistance = 1.0f;
    public float stepSpeed = 4.0f;
    public float footFollowForce = 100f;
    public float uprightForce = 50f;

    [Header("���̎p������")]
    public Rigidbody2D leftFoot;
    public Rigidbody2D rightFoot;
    public float targetFootAngle = 0f; // �n�ʂƐ��� = 0�x
    public float footTorqueStiffness = 100f;
    public float footTorqueDamping = 10f;

    [SerializeField] float hangHeight = 1.5f;
    [SerializeField] float hangForce = 100f;
    [SerializeField] float dampForce = 10f;

    public float maxHorizontalOffset = 1.0f;

    private float inputX;
    private float currentOffsetX = 0f;

    // ���̐ڒn�`�F�b�N�p�ϐ�
    public Transform footCheckL;
    public Transform footCheckR;
    public float groundCheckRadius = 0.05f;
    public LayerMask groundLayer;

    void FixedUpdate()
    {
        // ���͎擾�i�����L�[�j
        inputX = Input.GetAxis("Horizontal");

        ApplyFootTorque(leftFoot);
        ApplyFootTorque(rightFoot);

        // �^�[�Q�b�gX�̈ʒu���������ړ��i���炩�Ɂj
        currentOffsetX = Mathf.MoveTowards(currentOffsetX, inputX * maxHorizontalOffset, moveSpeed * Time.fixedDeltaTime);

        // �^�[�Q�b�g�ʒu�i���{���̕��ς���^��ցj�{X�I�t�Z�b�g
        Vector2 basePos = (leftLeg.position + rightLeg.position + core.position) / 3;
        Vector2 targetPos = basePos + Vector2.up * hangHeight + Vector2.right * currentOffsetX;

        // �^�[�Q�b�g�}�[�J�[�̕\���i�C�Ӂj
        if (targetMarker != null)
            targetMarker.position = targetPos;

        // �����ڒn���Ă��邩����
        bool isLeftGrounded = Physics2D.OverlapCircle(footCheckL.position, groundCheckRadius, groundLayer);
        bool isRightGrounded = Physics2D.OverlapCircle(footCheckR.position, groundCheckRadius, groundLayer);
        bool isAnyFootGrounded = isLeftGrounded || isRightGrounded;

        // �ڒn���Ă���ꍇ�̂݁A������������
        if (isAnyFootGrounded)
        {
            Vector2 toTarget = targetPos - head.position;
            Vector2 force = toTarget * hangForce - head.linearVelocity * dampForce;
            head.AddForce(force);
        }
    }

    void ApplyFootTorque(Rigidbody2D footRb)
    {
        float currentAngle = footRb.rotation; // ���݂�Z����]�p
        float angleDiff = Mathf.DeltaAngle(currentAngle, targetFootAngle);

        // �o�l�́i�p�x�� �~ �����j - �p���x �~ ����
        float torque = angleDiff * footTorqueStiffness - footRb.angularVelocity * footTorqueDamping;

        footRb.AddTorque(torque);
    }
}
