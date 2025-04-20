using UnityEngine;
using UnityEngine.U2D.IK;

public class ArmIKLook : MonoBehaviour
{
    public Transform parent_LeftArm;     // �����IK�^�[�Q�b�g�̐e
    public Transform target_LeftArm;     // ���ۂ�IK�^�[�Q�b�g�i�A�j������j
    public LimbSolver2D leftSolver;   // �����LimbSolver2D

    public Transform parent_RightArm;
    public Transform target_RightArm;
    public LimbSolver2D rightSolver;  // �E���LimbSolver2D

    public Camera cam;
    public Animator animator;
    public int armLayerIndex = 1;

    private bool isIKActive = false;
    private bool previousRightClick = false;

    // �X���[�Y�ɓ������x
    public float moveSpeed = 10f;   // IK�Ǐ]���x
    public float resetSpeed = 5f;   // �߂葬�x

    // Flip ��K�p����^�[�Q�b�g�iSpriteRenderer �� Scale���]�����ɑΉ��j
    public Transform flipTarget;  // �� �����ɃL�����̌����ڕ������A�T�C������i��FSpriteRenderer�t���̎q�I�u�W�F�N�g�j

    void LateUpdate()
    {
        // �}�E�X�̃��[���h���W���擾
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        
        // �g�O�����E�N���b�N���m�i�������u�Ԃ����؂�ւ��j
        bool currentRightClick = Input.GetMouseButton(1);
        if (currentRightClick && !previousRightClick)
        {
            isIKActive = !isIKActive;
            animator.SetLayerWeight(armLayerIndex, isIKActive ? 0f : 1f);
        }
        previousRightClick = currentRightClick;

        if (isIKActive)
        {
            // �L�����̌����𔻒�i�}�E�X���E�����������j
            bool isRight = mouseWorld.x > transform.position.x;

            // IK��Flip�ݒ�i�E�����Ȃ�flip = true�A�������Ȃ�flip = false�j
            leftSolver.flip = isRight;
            rightSolver.flip = isRight;

            // IK�^�[�Q�b�g�ʒu�X�V�i�Ǐ]�j
            Vector3 offset_Left = target_LeftArm.position - parent_LeftArm.position;
            Vector3 offset_Right = target_RightArm.position - parent_RightArm.position;

            Vector3 targetPos_Left = mouseWorld - offset_Left;
            Vector3 targetPos_Right = mouseWorld - offset_Right;

            parent_LeftArm.position = Vector3.Lerp(parent_LeftArm.position, targetPos_Left, Time.deltaTime * moveSpeed);
            parent_RightArm.position = Vector3.Lerp(parent_RightArm.position, targetPos_Right, Time.deltaTime * moveSpeed);
        }
        else
        {
            // ��A�N�e�B�u���͌��̈ʒu�ɖ߂�
            parent_LeftArm.localPosition = Vector3.Lerp(parent_LeftArm.localPosition, Vector3.zero, Time.deltaTime * resetSpeed);
            parent_RightArm.localPosition = Vector3.Lerp(parent_RightArm.localPosition, Vector3.zero, Time.deltaTime * resetSpeed);
        }
    }
}
