using UnityEngine;
using Unity.Cinemachine;

public class Shake : MonoBehaviour
{
    // �e��h��p��ImpulseSource�𕪂��ēo�^
    [Header("���R�C���p (�n���h�K��)")]
    public CinemachineImpulseSource handGunImpulse;

    [Header("���R�C���p (�V���b�g�K��)")]
    public CinemachineImpulseSource shotgunImpulse;

    [Header("�����p")]
    public CinemachineImpulseSource explosionImpulse;

    // �h��̃^�C�v���w�肷��enum
    public enum ShakeType
    {
        HandGun,
        ShotGun,
        Explosion
    }

    // �h��𔭐�������֐�
    public void ShakeScreen(ShakeType type)
    {
        switch (type)
        {
            case ShakeType.HandGun:
                if (handGunImpulse != null)
                    handGunImpulse.GenerateImpulse();
                break;

            case ShakeType.ShotGun:
                if (shotgunImpulse != null)
                    shotgunImpulse.GenerateImpulse();
                break;

            case ShakeType.Explosion:
                if (explosionImpulse != null)
                    explosionImpulse.GenerateImpulse();
                break;
        }
    }
}
