using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public class LifeTimeBase
    {
        public class SingletonScript<T> : MonoBehaviour where T : Component
        {
            private static T instance = default;
            public static T Instance => instance = instance
                ?? FindObjectOfType<T>()
                ?? (new GameObject()).AddComponent<T>();
        }
        public class Singleton<T> : MonoBehaviour where T : class, new()
        {
            private static T instance = default;
            public static T Instance => instance = instance ?? new T();
        }

    }
}
