using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public static class ProjectileMotion
    {
        public static Func<IEnumerator<(float x, float y)>> Ready(float initialVelocity, float angle, float gravity)
        {
            var radiantAngle = Mathf.Deg2Rad * angle;
            var time = 0.0f;
            IEnumerator<(float x, float y)> getOutput()
            {
                while (true)
                {
                    var x = initialVelocity * Mathf.Cos(angle) * time;
                    var y = initialVelocity * Mathf.Sin(angle) * time - 0.5 * gravity * time;
                    yield return ((float)x, (float)y);
                    time += Time.fixedDeltaTime;
                }
            };
            return getOutput;
        }
    }
}
