using UnityEngine;

// 全体の共通参照を持つシングルトン
public class GameReferences : MonoBehaviour
{
    public static GameReferences Instance { get; private set; }


    // ここに固定参照するスクリプトを並べる

    [Header("参照リスト")]
    public Shake shake; // Shakeクラスのインスタンスを保持
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

    // ダメージ計算
    public int GetDamageFromTag(string tag, string targetType)
    {
        if (string.IsNullOrEmpty(tag) || tag[0] != '@') return 0;

        string[] parts = tag.Substring(1).Split('_');
        if (parts.Length == 2)
        {
            string tagType = parts[0];
            int dmg;
            if (int.TryParse(parts[1], out dmg))
            {
                if (tagType == "All" || tagType == targetType)
                {
                    return dmg;
                }
            }
        }
        return 0;
    }
}
