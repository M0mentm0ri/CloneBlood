using UnityEngine;

public class IKControl : MonoBehaviour
{
    public Animator animator;        // Animator
    public string armAnimationState = "ArmAnimation";  // IK�p�̘r�A�j���[�V�����̏�Ԗ�

    void Update()
    {
        // �E�N���b�N��IK���C���[��؂�ւ�
        if (Input.GetMouseButton(1))
        {
            // IK���C���[�̃E�F�C�g��1�ɂ��āA�ʏ�A�j���[�V�������C���[�̃E�F�C�g��0�ɐݒ�
            animator.SetLayerWeight(1, 1f); // IK���C���[�i�r�Ȃǁj
            animator.SetLayerWeight(0, 0f); // �ʏ�A�j���[�V�������C���[�i�́A���Ȃǁj
        }
        else
        {
            // �ʏ�A�j���[�V�������C���[�̃E�F�C�g��1�ɁAIK���C���[�̃E�F�C�g��0�ɖ߂�
            animator.SetLayerWeight(1, 0f); // IK���C���[
            animator.SetLayerWeight(0, 1f); // �ʏ�A�j���[�V�������C���[
        }
    }
}