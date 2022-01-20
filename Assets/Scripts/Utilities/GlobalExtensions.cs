using Assets.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public static class GlobalExtensions
    {
        public static int ToPixel(this int unit)
        {
            return unit * Global.PIXEL_PER_UNIT;
        }
        public static float ToUnit(this int pixel)
        {
            return pixel / (1.0f * Global.PIXEL_PER_UNIT);
        }
        public static bool TryAdd<K, V>(this Dictionary<K, V> dictionary, K key, V value)
        {
            if (dictionary.ContainsKey(key))
            {
                return false;
            }
            if (value is null)
            {
                return false;
            }
            dictionary.Add(key, value);
            return true;
        }

        public static V TryGet<K, V>(this Dictionary<K, V> dictionary, K key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            return default;
        }

        public static async Task<V> TryGetUntilNotNull<K, V>(this Dictionary<K, V> dictionary, K key) where V : class
        {
            V ret = default;
            while (ret == default)
            {
                ret = dictionary.TryGet(key);
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
            return ret;
        }

        public static Vector3 Range(this Vector3 from, Vector3 to)
        {
            return new Vector3(
                x: UnityEngine.Random.Range(from.x, to.x),
                y: UnityEngine.Random.Range(from.y, to.y),
                z: UnityEngine.Random.Range(from.z, to.z));
        }

        public static Vector2 ToWorldCoord(this Vector2 from)
        {
            var screenCoordiantes = new Vector3(from.x, from.y, Camera.main.nearClipPlane);
            var worldCoordiantes = Camera.main.ScreenToWorldPoint(screenCoordiantes);
            return new Vector2(worldCoordiantes.x, worldCoordiantes.y);
        }

        public static Func<float, float> GetSToHzHandler(this float timeStepSeconds)
        {
            return (seconds) => 1 / timeStepSeconds * seconds;
        }

        public static GameObject GetGameObject(this Dictionary<string, GameObject> dictionary, string keyTag)
        {
            GameObject go = null;

            if (!Global.GameObjects.TryGetValue(keyTag, out go))
            {
                go = GameObject
                    .FindGameObjectWithTag(keyTag);
                Global.GameObjects.TryAdd(keyTag, go);
            }

            return go;
        }

        public static Func<ITargetFinder, Vector3> GetPositionResolverHandler(this Transform transform)
        {
            return (targetFinder) =>
            {
                var targetPosition = targetFinder
                    .GetHighestPriorityTarget(transform)
                    .position;

                return targetPosition;
            };
        }
    }
}
