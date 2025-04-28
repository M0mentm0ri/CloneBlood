using UnityEngine;
using Unity.Cinemachine;

public class Shake : MonoBehaviour
{
    // Chimachine Impulse Source コンポーネントを格納
    public CinemachineImpulseSource impulseSource;

    // 揺れの強さを決定する変数（0から1で設定）
    public float shakeStrength = 1f;



    // インパルスを発生させて画面を揺らす関数
    public void ShakeScreen(float strength)
    {
        // 強さを調整
        shakeStrength = Mathf.Clamp(strength, 0f, 1f); // 強さを0から1の間に制限

        impulseSource.GenerateImpulse();
    }

    // 強さを変える関数（外部から呼び出し可能）
    public void SetShakeStrength(float strength)
    {
        shakeStrength = Mathf.Clamp(strength, 0f, 1f); // 強さを0から1の間に制限
    }
}
