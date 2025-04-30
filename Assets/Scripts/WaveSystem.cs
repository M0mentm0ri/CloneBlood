using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    public List<WaveData> waves = new List<WaveData>();
    public float startDelay = 2f;
    private int currentWaveIndex = 0;

    [Header("参照")]
    public CloneSpawner cloneSpawner;
    public TMP_Text waveCompleteText; // Wave完了テキスト用（UnityのUI Text）

    private int aliveEnemyCount = 0; // 生きている敵の数

    private void Start()
    {
        waveCompleteText.text = ""; // 初期は非表示
        StartCoroutine(RunWaves());
    }

    public void OnEnemyDied()
    {
        aliveEnemyCount--;

        if (aliveEnemyCount <= 0)
        {
            StartCoroutine(ShowWaveCompleteText());
        }
    }

    // 通常 or 最終Wave完了の表示に対応した関数
    private IEnumerator ShowWaveCompleteText(bool isLastWave = false)
    {
        if (isLastWave)
        {
            waveCompleteText.text = $"最終WAVE\n防衛成功!!!";
            yield return new WaitForSeconds(4f);
        }
        else
        {
            waveCompleteText.text = $"WAVE {currentWaveIndex + 1}\n防衛成功";
            yield return new WaitForSeconds(3f);
        }

        waveCompleteText.text = "";
    }

    private IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(startDelay);

        while (currentWaveIndex < waves.Count)
        {
            // ▼ WAVE開始テキスト表示（通常 or 最終）
            if (currentWaveIndex == waves.Count - 1)
            {
                waveCompleteText.text = "最終WAVE\n開始!!!";
            }
            else
            {
                waveCompleteText.text = $"WAVE {currentWaveIndex + 1} \n開始";
            }

            yield return new WaitForSeconds(2f);
            waveCompleteText.text = "";

            WaveData wave = waves[currentWaveIndex];
            aliveEnemyCount = 0;

            foreach (var instruction in wave.instructions)
            {
                for (int i = 0; i < instruction.spawnCount; i++)
                {
                    GameObject enemy = Instantiate(instruction.enemyPrefab, instruction.spawnPoint.position, Quaternion.identity);

                    EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
                    if (enemyBase == null)
                    {
                        Debug.LogError("EnemyBaseが見つかりません。");
                        yield break;
                    }

                    enemyBase.SetWaveSystem(this);

                    if (instruction.target == Target.Player)
                        enemyBase.targetPosition = cloneSpawner.clonePrefab.transform;
                    else if (instruction.target == Target.Clone)
                        enemyBase.targetPosition = cloneSpawner.spawnPoint.transform;

                    aliveEnemyCount++;
                }

                yield return new WaitForSeconds(instruction.waitAfterSpawn);
            }

            // 敵全滅まで待機
            while (aliveEnemyCount > 0)
            {
                yield return null;
            }

            // ▼ 最終Waveなら特別な演出にする
            bool isLast = currentWaveIndex == waves.Count - 1;
            yield return ShowWaveCompleteText(isLast);

            currentWaveIndex++;
        }

        Debug.Log("全ウェーブ完了！");
    }
}