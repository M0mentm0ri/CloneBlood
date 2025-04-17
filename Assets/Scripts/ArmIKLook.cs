using UnityEngine;

public class IKControl : MonoBehaviour
{
    public Animator animator;        // Animator
    public string armAnimationState = "ArmAnimation";  // IK用の腕アニメーションの状態名

    void Update()
    {
        // 右クリックでIKレイヤーを切り替え
        if (Input.GetMouseButton(1))
        {
            // IKレイヤーのウェイトを1にして、通常アニメーションレイヤーのウェイトを0に設定
            animator.SetLayerWeight(1, 1f); // IKレイヤー（腕など）
            animator.SetLayerWeight(0, 0f); // 通常アニメーションレイヤー（体、足など）
        }
        else
        {
            // 通常アニメーションレイヤーのウェイトを1に、IKレイヤーのウェイトを0に戻す
            animator.SetLayerWeight(1, 0f); // IKレイヤー
            animator.SetLayerWeight(0, 1f); // 通常アニメーションレイヤー
        }
    }
}