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
    public GameObject enemyPrefab; // �o���G�̎�ށiPrefab�j
    public int spawnCount;         // ���̏o����
    public Transform spawnPoint;   // �ǂ̒n�_����o����
    public Target target;        // �ǂ̃^�[�Q�b�g��_����
    public float waitAfterSpawn;   // ���̎w����A���b�ҋ@���邩
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

public class WaveSystem : MonoBehaviour
{
    public List<WaveData> waves = new List<WaveData>();
    public float startDelay = 2f;
    private int currentWaveIndex = 0;

    [Header("�Q��")]
    public CloneSpawner cloneSpawner;
    public TMP_Text waveCompleteText; // Wave�����e�L�X�g�p�iUnity��UI Text�j

    private int aliveEnemyCount = 0; // �����Ă���G�̐�

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
            aliveEnemyCount = 0;

            foreach (var instruction in wave.instructions)
            {
                for (int i = 0; i < instruction.spawnCount; i++)
                {
                    GameObject enemy = Instantiate(instruction.enemyPrefab, instruction.spawnPoint.position, Quaternion.identity);

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
}