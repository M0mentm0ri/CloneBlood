using UnityEngine;
using Unity.Cinemachine;

public class Shake : MonoBehaviour
{
    // Chimachine Impulse Source �R���|�[�l���g���i�[
    public CinemachineImpulseSource impulseSource;

    // �h��̋��������肷��ϐ��i0����1�Őݒ�j
    public float shakeStrength = 1f;



    // �C���p���X�𔭐������ĉ�ʂ�h�炷�֐�
    public void ShakeScreen(float strength)
    {
        // �����𒲐�
        shakeStrength = Mathf.Clamp(strength, 0f, 1f); // ������0����1�̊Ԃɐ���

        impulseSource.GenerateImpulse();
    }

    // ������ς���֐��i�O������Ăяo���\�j
    public void SetShakeStrength(float strength)
    {
        shakeStrength = Mathf.Clamp(strength, 0f, 1f); // ������0����1�̊Ԃɐ���
    }
}
