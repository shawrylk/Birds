using Assets.Contracts;
using System;
using System.Collections.Concurrent;
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

        public static Vector3 ToWorldCoord(this Vector3 from)
        {
            var worldCoordiantes = Camera.main.ScreenToWorldPoint(from);
            return worldCoordiantes;
        }

        public const float GameRatio = 640 / 360f;
        public const float GameWidthPixel = 640f;
        public static readonly float GameHeightPixel = GameWidthPixel / Screen.width * Screen.height;
        public static readonly float GameWidthUnit = GameWidthPixel / 100f;
        public static readonly float GameHeightUnit = GameWidthUnit / GameRatio;

        /// <summary>
        /// Base offset is center point (0, 0)
        /// Landscape colliders have base offset from bottom point (0, yBot)
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static Vector2 ToUnit(this Vector2 from)
        {
            // 1. Convert world pixel to game pixel
            // 2. Convert game pixel to game unit
            return new Vector2(
                x: from.x / (GameWidthPixel / 2) * (GameWidthUnit / 2),
                y: (from.y - (GameHeightPixel / 2)) / (GameHeightPixel / 2) * (GameHeightUnit / 2));
        }
        //public static Vector2 ToScreenCoord(this Vector2 from)
        //{
        //    var worldCoordiantes = new Vector3(from.x, from.y, Camera.main.nearClipPlane);
        //    var screenCoordiantes = Camera.main.WorldToScreenPoint(worldCoordiantes);
        //    return new Vector2(screenCoordiantes.x, screenCoordiantes.y);
        //}

        public static Func<float, float> GetSToHzHandler(this float timeStepSeconds)
        {
            return (seconds) => 1 / timeStepSeconds * seconds;
        }

        public static Func<IEnumerator<int>> GetHzWaiter(this float hzPerSecond)
        {
            IEnumerator<int> wait()
            {
                var i = 0;
                while (i < hzPerSecond)
                {
                    yield return i++;
                }
            }

            return wait;
        }

        public static Func<T> SampleAt<T>(this Func<T> action, float hzedOut)
        {
            var wait = hzedOut.GetHzWaiter();
            var ret = default(T);
            var it = wait();
            return () =>
            {
                if (!it.MoveNext())
                {
                    it = wait();
                    ret = action.Invoke();
                }
                return ret;
            };
        }

        public static (Action<float> regen, Func<bool> isRunOut) GetEnergyHanlder(this float energyConsumePerSecond, float energyHzedOut, Func<float, float> sToHz)
        {
            var hz = 0;

            Action<float> regen = (energy) =>
            {
                var time = energy / energyConsumePerSecond;
                energyHzedOut = sToHz(time);
                hz = 0;
            };

            Func<bool> isRunOut = () => hz++ > energyHzedOut;

            return (regen, isRunOut);
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

        public static Func<ITargetFinder, Vector3> GetPositionResolver(this Transform transform)
        {
            return (targetFinder) =>
            {
                var targetPosition = targetFinder
                    .GetHighestPriorityTarget(transform)
                    .position;

                return targetPosition;
            };
        }

        public static void Add(this ConcurrentDictionary<string, List<GameObject>> dict, GameObject gameObject)
        {
            dict.AddOrUpdate(gameObject.transform.name,
                key =>
                {
                    var list = new List<GameObject>();
                    list.Add(gameObject);
                    return list;
                },
                (key, list) =>
                {
                    list.Add(gameObject);
                    return list;
                });
        }

        public static bool Remove(this ConcurrentDictionary<string, List<GameObject>> dict, GameObject gameObject)
        {
            var list = default(List<GameObject>);
            var ret = dict.TryGetValue(gameObject.name, out list);
            ret = ret && list.Remove(gameObject);
            return ret;
        }
    }
}
