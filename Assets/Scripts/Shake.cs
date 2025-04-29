using UnityEngine;
using Unity.Cinemachine;

public class Shake : MonoBehaviour
{
    // 各種揺れ用のImpulseSourceを分けて登録
    [Header("リコイル用 (ハンドガン)")]
    public CinemachineImpulseSource handGunImpulse;

    [Header("リコイル用 (ショットガン)")]
    public CinemachineImpulseSource shotgunImpulse;

    [Header("爆発用")]
    public CinemachineImpulseSource explosionImpulse;

    // 揺れのタイプを指定するenum
    public enum ShakeType
    {
        HandGun,
        ShotGun,
        Explosion
    }

    // 揺れを発生させる関数
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
