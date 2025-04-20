using UnityEngine;
using UnityEngine.U2D.IK;

public class ArmIKLook : MonoBehaviour
{
    public Transform parent_LeftArm;     // 左手のIKターゲットの親
    public Transform target_LeftArm;     // 実際のIKターゲット（アニメ制御）
    public LimbSolver2D leftSolver;   // 左手のLimbSolver2D

    public Transform parent_RightArm;
    public Transform target_RightArm;
    public LimbSolver2D rightSolver;  // 右手のLimbSolver2D

    public Camera cam;
    public Animator animator;
    public int armLayerIndex = 1;

    private bool isIKActive = false;
    private bool previousRightClick = false;

    // スムーズに動く速度
    public float moveSpeed = 10f;   // IK追従速度
    public float resetSpeed = 5f;   // 戻り速度

    // Flip を適用するターゲット（SpriteRenderer や Scale反転方式に対応）
    public Transform flipTarget;  // ← ここにキャラの見た目部分をアサインする（例：SpriteRenderer付きの子オブジェクト）

    void LateUpdate()
    {
        // マウスのワールド座標を取得
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        
        // トグル式右クリック検知（押した瞬間だけ切り替え）
        bool currentRightClick = Input.GetMouseButton(1);
        if (currentRightClick && !previousRightClick)
        {
            isIKActive = !isIKActive;
            animator.SetLayerWeight(armLayerIndex, isIKActive ? 0f : 1f);
        }
        previousRightClick = currentRightClick;

        if (isIKActive)
        {
            // キャラの向きを判定（マウスが右側か左側か）
            bool isRight = mouseWorld.x > transform.position.x;

            // IKのFlip設定（右向きならflip = true、左向きならflip = false）
            leftSolver.flip = isRight;
            rightSolver.flip = isRight;

            // IKターゲット位置更新（追従）
            Vector3 offset_Left = target_LeftArm.position - parent_LeftArm.position;
            Vector3 offset_Right = target_RightArm.position - parent_RightArm.position;

            Vector3 targetPos_Left = mouseWorld - offset_Left;
            Vector3 targetPos_Right = mouseWorld - offset_Right;

            parent_LeftArm.position = Vector3.Lerp(parent_LeftArm.position, targetPos_Left, Time.deltaTime * moveSpeed);
            parent_RightArm.position = Vector3.Lerp(parent_RightArm.position, targetPos_Right, Time.deltaTime * moveSpeed);
        }
        else
        {
            // 非アクティブ時は元の位置に戻す
            parent_LeftArm.localPosition = Vector3.Lerp(parent_LeftArm.localPosition, Vector3.zero, Time.deltaTime * resetSpeed);
            parent_RightArm.localPosition = Vector3.Lerp(parent_RightArm.localPosition, Vector3.zero, Time.deltaTime * resetSpeed);
        }
    }
}
