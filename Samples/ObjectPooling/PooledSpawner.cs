using UnityEngine;
using UnityEngine.Pool;

namespace UnityBestPractices.Samples
{
    /// <summary>
    /// Spawns pooled projectiles on a timer using Unity's built-in ObjectPool.
    /// </summary>
    public class PooledSpawner : MonoBehaviour
    {
        #region Fields

        [SerializeField] private PooledProjectile m_ProjectilePrefab;
        [SerializeField] private float m_SpawnInterval = 1f;
        [SerializeField] private int m_DefaultCapacity = 10;
        [SerializeField] private int m_MaxSize = 50;

        private ObjectPool<PooledProjectile> m_Pool;
        private float m_Timer;

        #endregion

        #region Properties

        public ObjectPool<PooledProjectile> Pool => m_Pool;

        #endregion

        #region MonoBehaviour Methods

        private void Awake()
        {
            m_Pool = new ObjectPool<PooledProjectile>(
                createFunc: CreateProjectile,
                actionOnGet: OnTakeFromPool,
                actionOnRelease: OnReturnedToPool,
                actionOnDestroy: OnDestroyProjectile,
                collectionCheck: false,
                defaultCapacity: m_DefaultCapacity,
                maxSize: m_MaxSize);
        }

        private void Update()
        {
            m_Timer += Time.deltaTime;

            if (m_Timer >= m_SpawnInterval)
            {
                m_Timer = 0f;
                SpawnProjectile();
            }
        }

        #endregion

        #region Public Methods

        public void ReturnToPool(PooledProjectile projectile)
        {
            m_Pool.Release(projectile);
        }

        #endregion

        #region Private Methods

        private void SpawnProjectile()
        {
            var projectile = m_Pool.Get();
            projectile.transform.SetPositionAndRotation(transform.position, transform.rotation);
            projectile.Initialize(this);
        }

        private PooledProjectile CreateProjectile()
        {
            return Instantiate(m_ProjectilePrefab);
        }

        private void OnTakeFromPool(PooledProjectile projectile)
        {
            projectile.gameObject.SetActive(true);
        }

        private void OnReturnedToPool(PooledProjectile projectile)
        {
            projectile.gameObject.SetActive(false);
        }

        private void OnDestroyProjectile(PooledProjectile projectile)
        {
            Destroy(projectile.gameObject);
        }

        #endregion
    }
}
