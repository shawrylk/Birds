using Assets.Contracts;
using Assets.Inputs;
using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    [DefaultExecutionOrder(Global.ENEMY_MANAGER_ORDER)]
    public class EnemyManager : BaseScript
    {
        public GameObject[] EnemyPrefabs;
        public GameObject DangerSign;
        [SerializeField]
        private ParticleSystem _touchFx = null;
        private List<ParticleSystem> _touchFxPool = new List<ParticleSystem>();

        private int _enemy_count_max;
        private Dictionary<string, Transform> _boundaries;
        private IInput _input;

        private event Func<Vector2, Task<bool>> _touchEvent;
        public event Func<Vector2, Task<bool>> TouchEvent
        {
            add => _touchEvent += value;
            remove => _touchEvent -= value;
        }

        private void Awake()
        {
            _boundaries = Global.Items[Global.BOUNDARIES_TAG] as Dictionary<string, Transform>;
            _enemy_count_max = EnemyPrefabs.Length;
            _input = Global.Items[Global.INPUT] as IInput;
            _input.TapHandler += TouchHandler;

            StartAutoSpawnEnemy();
        }

        private void OnDisable()
        {
            _input.TapHandler -= TouchHandler;
        }

        private int index = 0;
        private async Task TouchHandler(Vector2 position)
        {
            var handled = await (_touchEvent?.Invoke(position) ?? Task.FromResult(false));
            if (handled)
            {
                if (_touchFxPool.Count <= index)
                {
                    _touchFxPool.Add(Instantiate(
                        _touchFx,
                        position,
                        Quaternion.identity));
                }
                _touchFxPool[index].transform.position = position;
                _touchFxPool[index].Play();
                index = ++index % 10;
            }
        }

        private void StartAutoSpawnEnemy()
        {
            StartCoroutine(SpawnEnemy());
        }
        private IEnumerator SpawnEnemy()
        {
            var offset = 2f;
            var heightInUnit = Screen.height * Global.UnitsPerPixel;
            var horizontalMargin = new Vector3(offset, 0, 0);
            var verticalMargin = new Vector3(0, offset, 0);
            var left = _boundaries[Global.LEFT_BOUNDARY].position + horizontalMargin;
            var right = _boundaries[Global.RIGHT_BOUNDARY].position - horizontalMargin;
            var top = _boundaries[Global.TOP_BOUNDARY].position - verticalMargin - new Vector3(0, heightInUnit / 2, 0);
            var bottom = _boundaries[Global.BOTTOM_BOUNDARY].position + verticalMargin;

            var timeStep1 = UnityEngine.Random.Range(5f, 7f);
            var timeStep2 = 2.0f;
            while (true)
            {
                yield return new WaitForSeconds(timeStep1);

                var position = new Vector2(left.Range(right).x, top.Range(bottom).y);

                var danger = Instantiate(original: DangerSign,
                    position: position,
                    rotation: Quaternion.identity,
                    parent: transform);

                danger.GetComponent<DangerSign.DangerSign>()
                    .StartWarningCoroutine(timeStep2);

                yield return new WaitForSeconds(timeStep2);

                Instantiate(original: GetEnemyAtIndex(0),
                    position: position,
                    rotation: Quaternion.identity,
                    parent: transform);
            }
        }

        private GameObject GetEnemyAtIndex(int index)
        {
            if (index <= _enemy_count_max)
            {
                return EnemyPrefabs[index];
            }
            return EnemyPrefabs[_enemy_count_max];
        }
    }
}
