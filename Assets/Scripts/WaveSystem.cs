using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public List<WaveData> waves = new List<WaveData>(); // �S�E�F�[�u���X�g
    public float startDelay = 2f;  // �ŏ��̃E�F�[�u�J�n�O�̒x������

    private int currentWaveIndex = 0;


    [Header("�Q��")]
    public CloneSpawner cloneSpawner; // CloneSpawner�̎Q��


    private void Start()
    {
        StartCoroutine(RunWaves());
    }

    /// <summary>
    /// �S�E�F�[�u�����ɍĐ�����R���[�`��
    /// </summary>
    private IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(startDelay);

        while (currentWaveIndex < waves.Count)
        {
            WaveData wave = waves[currentWaveIndex];
            Debug.Log($"Wave {currentWaveIndex + 1} �J�n");

            // �w�����Ƃɏ���
            foreach (var instruction in wave.instructions)
            {
                for (int i = 0; i < instruction.spawnCount; i++)
                {
                    // �G�𐶐�
                    GameObject enemy = Instantiate(instruction.enemyPrefab, instruction.spawnPoint.position, Quaternion.identity);

                    EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();

                    if(enemyBase == null)
                    {
                        Debug.LogError("EnemyBase��������܂���B");
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

                // �w�肳�ꂽ�ҋ@����
                yield return new WaitForSeconds(instruction.waitAfterSpawn);
            }

            currentWaveIndex++;
        }

        Debug.Log("�S�E�F�[�u�����I");
    }
}
