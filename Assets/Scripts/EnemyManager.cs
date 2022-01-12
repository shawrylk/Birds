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
        private int _enemy_count_max;
        private Dictionary<string, Transform> _boundaries;

        private void Awake()
        {
            _boundaries = Global.Items[Global.BOUNDARIES_TAG] as Dictionary<string, Transform>;
            _enemy_count_max = EnemyPrefabs.Length;
            //StartAutoSpawnEnemy();
        }
        private void StartAutoSpawnEnemy()
        {
            StartCoroutine(SpawnEnemy());
        }
        private IEnumerator SpawnEnemy()
        {
            var offset = 0.6f;
            var heightInUnit = Screen.height * Global.UnitsPerPixel;
            var horizontalMargin = new Vector3(offset, 0, 0);
            var verticalMargin = new Vector3(0, offset, 0);
            var left = _boundaries[Global.LEFT_BOUNDARY].position + horizontalMargin;
            var right = _boundaries[Global.RIGHT_BOUNDARY].position - horizontalMargin;
            var top = _boundaries[Global.TOP_BOUNDARY].position - verticalMargin - new Vector3(0, heightInUnit / 2, 0);
            var bottom = _boundaries[Global.BOTTOM_BOUNDARY].position + verticalMargin;

            var time = 120.0f;
            while (true)
            {
                yield return new WaitForSeconds(time);
                var position = new Vector2(left.Range(right).x, top.Range(bottom).y);
                var enemy =
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
