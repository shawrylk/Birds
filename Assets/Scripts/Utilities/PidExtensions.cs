using Assets.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public static class PidExtensions
    {
        public static
            (Action<Vector3, float> pid,
            Action reset)
            GetPidHandler(
            this Transform transform,
            float p,
            float i,
            float d,
            float minClamp = 3f,
            float maxClamp = 3f,
            Rigidbody2D rigidbody = null)
        {
            rigidbody = rigidbody ?? transform.GetComponent<Rigidbody2D>();

            IPidController pidX = new PidController();
            IPidController pidY = new PidController();

            pidX.Ready(p, i, d);
            pidY.Ready(p, i, d);

            return ((targetPosition, timeStep) =>
            {
                var distanceVector = targetPosition - transform.position;

                var magnitudeX = pidX.GetOutputValue(distanceVector.x, timeStep);
                var magnitudeY = pidY.GetOutputValue(distanceVector.y, timeStep);

                magnitudeX = Mathf.Clamp(magnitudeX, minClamp, maxClamp);
                magnitudeY = Mathf.Clamp(magnitudeY, minClamp, maxClamp);

                rigidbody.AddRelativeForce(magnitudeX * Vector2.right);
                rigidbody.AddRelativeForce(magnitudeY * Vector2.up);
            },
            () =>
            {
                pidX.Ready(p, i, d);
                pidY.Ready(p, i, d);
            }
            );
        }
    }
}
