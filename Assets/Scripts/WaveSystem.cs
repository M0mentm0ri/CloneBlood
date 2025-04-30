using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵のスポーン指示を表すクラス
/// </summary>
[System.Serializable]
public class SpawnInstruction
{
    public GameObject enemyPrefab; // 出す敵の種類（Prefab）
    public int spawnCount;         // 何体出すか
    public Transform spawnPoint;   // どの地点から出すか
    public Target target;        // どのターゲットを狙うか
    public float waitAfterSpawn;   // この指示後、何秒待機するか
}

public enum Target
{
    Player, // プレイヤーを狙う
    Clone,  // Cloneを狙う
}


/// <summary>
/// 1ウェーブの全スポーン指示をまとめるクラス
/// </summary>
[System.Serializable]
public class WaveData
{
    public List<SpawnInstruction> instructions = new List<SpawnInstruction>();
}

/// <summary>
/// Wave全体を管理し、順に敵を出すクラス
/// </summary>
public class WaveSystem : MonoBehaviour
{
    public List<WaveData> waves = new List<WaveData>(); // 全ウェーブリスト
    public float startDelay = 2f;  // 最初のウェーブ開始前の遅延時間

    private int currentWaveIndex = 0;


    [Header("参照")]
    public CloneSpawner cloneSpawner; // CloneSpawnerの参照


    private void Start()
    {
        StartCoroutine(RunWaves());
    }

    /// <summary>
    /// 全ウェーブを順に再生するコルーチン
    /// </summary>
    private IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(startDelay);

        while (currentWaveIndex < waves.Count)
        {
            WaveData wave = waves[currentWaveIndex];
            Debug.Log($"Wave {currentWaveIndex + 1} 開始");

            // 指示ごとに処理
            foreach (var instruction in wave.instructions)
            {
                for (int i = 0; i < instruction.spawnCount; i++)
                {
                    // 敵を生成
                    GameObject enemy = Instantiate(instruction.enemyPrefab, instruction.spawnPoint.position, Quaternion.identity);

                    EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();

                    if(enemyBase == null)
                    {
                        Debug.LogError("EnemyBaseが見つかりません。");
                        yield break;
                    }

                    if (instruction.target == Target.Player)
                    {
                        enemyBase.targetPosition = cloneSpawner.clonePrefab.transform;
                    }
                    else if(instruction.target == Target.Clone)
                    {
                        enemyBase.targetPosition = cloneSpawner.spawnPoint.transform;
                    }
                }

                // 指定された待機時間
                yield return new WaitForSeconds(instruction.waitAfterSpawn);
            }

            currentWaveIndex++;
        }

        Debug.Log("全ウェーブ完了！");
    }
}
