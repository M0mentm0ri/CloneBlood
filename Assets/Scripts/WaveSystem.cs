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
    public GameObject enemyPrefab;
    public int spawnCount;
    public SpawnPointType spawnPointType; // 追加：どの方向から出すか
    public Target target;
    public float waitAfterSpawn;
}

public enum SpawnPointType
{
    RightGround,
    LeftGround,
    RightAir,
    LeftAir,
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
[System.Serializable]
public class SpawnPointEntry
{
    public SpawnPointType type;
    public Transform transform;
}


public class WaveSystem : MonoBehaviour
{
    public List<WaveData> waves = new List<WaveData>();
    public List<SpawnPointEntry> spawnPoints = new List<SpawnPointEntry>();
    public float startDelay = 2f;
    private int currentWaveIndex = 0;

    [Header("参照")]
    public CloneSpawner cloneSpawner;
    public TMP_Text waveCompleteText; // Wave完了テキスト用（UnityのUI Text）

    private int aliveEnemyCount = 0; // 生きている敵の数

    [Header("方向別の矢印表示UI")]
    public TMP_Text rightGroundArrow;
    public TMP_Text leftGroundArrow;
    public TMP_Text rightAirArrow;
    public TMP_Text leftAirArrow;


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

            yield return StartCoroutine(ShowDirectionIndicators(wave.instructions));

            aliveEnemyCount = 0;

            foreach (var instruction in wave.instructions)
            {
                for (int i = 0; i < instruction.spawnCount; i++)
                {
                    Transform spawnTransform = GetSpawnTransform(instruction.spawnPointType);
                    GameObject enemy = Instantiate(instruction.enemyPrefab, spawnTransform.position, Quaternion.identity);



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

    private IEnumerator ShowDirectionIndicators(List<SpawnInstruction> instructions)
    {
        // カウント初期化
        Dictionary<SpawnPointType, int> spawnCounts = new();
        foreach (SpawnPointType type in System.Enum.GetValues(typeof(SpawnPointType)))
        {
            spawnCounts[type] = 0;
        }

        // 指示ごとに数を集計
        foreach (var instr in instructions)
        {
            spawnCounts[instr.spawnPointType] += instr.spawnCount;
        }

        // 表示をセット（1体なら＜、2体以上なら＜＜）
        rightGroundArrow.text = spawnCounts[SpawnPointType.RightGround] >= 2 ? "<<" : spawnCounts[SpawnPointType.RightGround] == 1 ? "<" : "";
        leftGroundArrow.text = spawnCounts[SpawnPointType.LeftGround] >= 2 ? "<<" : spawnCounts[SpawnPointType.LeftGround] == 1 ? "<" : "";
        rightAirArrow.text = spawnCounts[SpawnPointType.RightAir] >= 2 ? "<<" : spawnCounts[SpawnPointType.RightAir] == 1 ? "<" : "";
        leftAirArrow.text = spawnCounts[SpawnPointType.LeftAir] >= 2 ? "<<" : spawnCounts[SpawnPointType.LeftAir] == 1 ? "<" : "";

        // 点滅処理
        float blinkTime = 2f;
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < blinkTime)
        {
            visible = !visible;

            rightGroundArrow.enabled = visible;
            leftGroundArrow.enabled = visible;
            rightAirArrow.enabled = visible;
            leftAirArrow.enabled = visible;

            yield return new WaitForSeconds(0.3f);
            elapsed += 0.3f;
        }

        // 全部非表示
        rightGroundArrow.text = "";
        leftGroundArrow.text = "";
        rightAirArrow.text = "";
        leftAirArrow.text = "";
    }

    private Transform GetSpawnTransform(SpawnPointType type)
    {
        foreach (var entry in spawnPoints)
        {
            if (entry.type == type)
                return entry.transform;
        }

        Debug.LogError($"SpawnPointType {type} に対応するTransformがありません");
        return null;
    }
}