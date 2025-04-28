using UnityEngine;

// 全体の共通参照を持つシングルトン
public class GameReferences : MonoBehaviour
{
    public static GameReferences Instance { get; private set; }

    // ここに固定参照するスクリプトを並べる

    [Header("参照リスト")]
    public ParticleManager particleManager;

    // 必要に応じてどんどん追加していく

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // シーンまたぎたいならこいつも付ける
        // DontDestroyOnLoad(gameObject);
    }
}
