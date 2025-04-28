using UnityEngine;
using System.Collections.Generic;

public class ParticleManager : MonoBehaviour
{
    public enum ParticleType
    {
        BloodEnemy_Low,
        BloodEnemy_Mid,
        BloodEnemy_High,
        BloodEnemy_EX,
        Blood_Low,
        Blood_Mid,
        Blood_High,
        Blood_EX,
        Explosion
    }

    [System.Serializable]
    public struct ParticleData
    {
        public ParticleType type;
        public ParticleSystem particle; // ここは「Prefab」じゃなく「ParticleSystem」そのものを入れる
    }

    [Header("パーティクル一覧")]
    public ParticleData[] particleList;

    private Dictionary<ParticleType, ParticleSystem> particles;

    public static ParticleManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeParticles();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeParticles()
    {
        particles = new Dictionary<ParticleType, ParticleSystem>();

        foreach (var data in particleList)
        {
            if (data.particle != null)
            {
                particles[data.type] = data.particle;
            }
            else
            {
                Debug.LogWarning($"{data.type} のパーティクルが設定されていない");
            }
        }
    }

    /// <summary>
    /// 指定パーティクルを指定位置で再生する
    /// </summary>
    public void PlayParticle(ParticleType type, Vector3 position, Quaternion rotation)
    {
        if (!particles.ContainsKey(type))
        {
            Debug.LogWarning($"{type} のパーティクルが存在しない");
            return;
        }

        ParticleSystem ps = particles[type];
        ps.transform.position = position;
        ps.transform.rotation = rotation;
        ps.Play();
    }
}
