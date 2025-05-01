using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// �G�̃X�|�[���w����\���N���X
/// </summary>
[System.Serializable]
public class SpawnInstruction
{
    public GameObject enemyPrefab;
    public int spawnCount;
    public SpawnPointType spawnPointType; // �ǉ��F�ǂ̕�������o����
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
    Player, // �v���C���[��_��
    Clone,  // Clone��_��
}


/// <summary>
/// 1�E�F�[�u�̑S�X�|�[���w�����܂Ƃ߂�N���X
/// </summary>
[System.Serializable]
public class WaveData
{
    public List<SpawnInstruction> instructions = new List<SpawnInstruction>();
}

/// <summary>
/// Wave�S�̂��Ǘ����A���ɓG���o���N���X
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

    [Header("�Q��")]
    public CloneSpawner cloneSpawner;
    public TMP_Text waveCompleteText; // Wave�����e�L�X�g�p�iUnity��UI Text�j

    private int aliveEnemyCount = 0; // �����Ă���G�̐�

    [Header("�����ʂ̖��\��UI")]
    public TMP_Text rightGroundArrow;
    public TMP_Text leftGroundArrow;
    public TMP_Text rightAirArrow;
    public TMP_Text leftAirArrow;


    private void Start()
    {
        waveCompleteText.text = ""; // �����͔�\��
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

    // �ʏ� or �ŏIWave�����̕\���ɑΉ������֐�
    private IEnumerator ShowWaveCompleteText(bool isLastWave = false)
    {
        if (isLastWave)
        {
            waveCompleteText.text = $"�ŏIWAVE\n�h�q����!!!";
            yield return new WaitForSeconds(4f);
        }
        else
        {
            waveCompleteText.text = $"WAVE {currentWaveIndex + 1}\n�h�q����";
            yield return new WaitForSeconds(3f);
        }

        waveCompleteText.text = "";
    }

    private IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(startDelay);

        while (currentWaveIndex < waves.Count)
        {

            // �� WAVE�J�n�e�L�X�g�\���i�ʏ� or �ŏI�j
            if (currentWaveIndex == waves.Count - 1)
            {
                waveCompleteText.text = "�ŏIWAVE\n�J�n!!!";
            }
            else
            {
                waveCompleteText.text = $"WAVE {currentWaveIndex + 1} \n�J�n";
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
                        Debug.LogError("EnemyBase��������܂���B");
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

            // �G�S�ł܂őҋ@
            while (aliveEnemyCount > 0)
            {
                yield return null;
            }

            // �� �ŏIWave�Ȃ���ʂȉ��o�ɂ���
            bool isLast = currentWaveIndex == waves.Count - 1;
            yield return ShowWaveCompleteText(isLast);

            currentWaveIndex++;
        }

        Debug.Log("�S�E�F�[�u�����I");
    }

    private IEnumerator ShowDirectionIndicators(List<SpawnInstruction> instructions)
    {
        // �J�E���g������
        Dictionary<SpawnPointType, int> spawnCounts = new();
        foreach (SpawnPointType type in System.Enum.GetValues(typeof(SpawnPointType)))
        {
            spawnCounts[type] = 0;
        }

        // �w�����Ƃɐ����W�v
        foreach (var instr in instructions)
        {
            spawnCounts[instr.spawnPointType] += instr.spawnCount;
        }

        // �\�����Z�b�g�i1�̂Ȃ灃�A2�̈ȏ�Ȃ灃���j
        rightGroundArrow.text = spawnCounts[SpawnPointType.RightGround] >= 2 ? "<<" : spawnCounts[SpawnPointType.RightGround] == 1 ? "<" : "";
        leftGroundArrow.text = spawnCounts[SpawnPointType.LeftGround] >= 2 ? "<<" : spawnCounts[SpawnPointType.LeftGround] == 1 ? "<" : "";
        rightAirArrow.text = spawnCounts[SpawnPointType.RightAir] >= 2 ? "<<" : spawnCounts[SpawnPointType.RightAir] == 1 ? "<" : "";
        leftAirArrow.text = spawnCounts[SpawnPointType.LeftAir] >= 2 ? "<<" : spawnCounts[SpawnPointType.LeftAir] == 1 ? "<" : "";

        // �_�ŏ���
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

        // �S����\��
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

        Debug.LogError($"SpawnPointType {type} �ɑΉ�����Transform������܂���");
        return null;
    }
}