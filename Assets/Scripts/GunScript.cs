using UnityEngine;

public class GunScript : MonoBehaviour
{
    public Transform gunFront;        // 銃口のTransform
    public Rigidbody rigidbody;
    public float useblood = 5f;
    public float cooldownTime = 0.5f;
    public float range = 10f;         // Rayの射程距離

    public GameObject flashEffect;    // フラッシュエフェクト用のオブジェクト
    public ParticleSystem Blood_Particle;

    public bool isHummer = false; // ハンマーかどうか


    // 銃を撃つ
    public void Shoot()
    {
        // ==========================
        // 1. 2D処理（ゲームロジック）
        // ==========================

        if (Blood_Particle != null)
        {
            Blood_Particle.Play();
        }

        // ==========================
        // 2. フラッシュエフェクトの表示
        // ==========================
        if (flashEffect != null)
        {
            flashEffect.SetActive(true);  // フラッシュエフェクトを有効化

            // 一定時間後にフラッシュエフェクトを無効化する（例えば0.1秒後）
            Invoke("DeactivateFlash", 0.1f);
        }
    }

    // フラッシュエフェクトを無効化
    private void DeactivateFlash()
    {
        if (flashEffect != null)
        {
            flashEffect.SetActive(false);  // フラッシュエフェクトを無効化
        }
    }
}
