using UnityEngine;

public class GunScript : MonoBehaviour
{

    public Human human;          // Humanスクリプトへの参照
    public Transform gunFront;        // 銃口のTransform
    public Rigidbody rigidbody;
    public Transform transform;       // 銃の回転対象となる手首（GunScriptから移籍）
    public float range = 10f;         // Rayの射程距離

    public ParticleSystem Blood_Particle;

    public void Shoot()
    {
        // ==========================
        // 1. 2D処理（ゲームロジック）
        // ==========================

        if (Blood_Particle != null)
        {
            Blood_Particle.Play();
        }
    }

}
