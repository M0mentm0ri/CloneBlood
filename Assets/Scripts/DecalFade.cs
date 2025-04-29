using UnityEngine;

public class DecalFade : MonoBehaviour
{
    [Header("ŽžŠÔÝ’è")]
    public float lifeTime = 5f;       // k¬‚ªŽn‚Ü‚é‚Ü‚Å‚ÌŽžŠÔ
    public float shrinkDuration = 2f; // k¬‚É‚©‚¯‚éŽžŠÔ

    private float elapsed = 0f;
    private Vector3 initialScale;

    private void Start()
    {
        // ‰ŠúƒTƒCƒY‹L˜^
        initialScale = transform.localScale;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        if (elapsed > lifeTime)
        {
            float shrinkProgress = (elapsed - lifeTime) / shrinkDuration;
            shrinkProgress = Mathf.Clamp01(shrinkProgress);

            // ™X‚É¬‚³‚­‚·‚é
            float scale = Mathf.Lerp(1f, 0f, shrinkProgress);
            transform.localScale = initialScale * scale;

            if (shrinkProgress >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}