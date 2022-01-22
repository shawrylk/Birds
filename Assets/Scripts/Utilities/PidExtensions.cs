using Assets.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public class PIDOptions
    {
        public (float kp, float ki, float kd) X;
        public (float kp, float ki, float kd) Y;
        public (float min, float max) ClampX;
        public (float min, float max) ClampY;
        public Transform Transform;
        public Rigidbody2D Rigidbody2D;
    }
    public static class PidExtensions
    {
        public static
            (Action<Vector3, float> pid,
            Action reset)
            GetPidHandler(Action<PIDOptions> optionsCallback)
        {
            var options = new PIDOptions();
            optionsCallback?.Invoke(options);

            IPidController pidX = new PidController();
            IPidController pidY = new PidController();

            pidX.Ready(options.X.kp, options.X.ki, options.X.kd);
            pidY.Ready(options.Y.kp, options.Y.ki, options.Y.kd);

            return ((targetPosition, timeStep) =>
            {
                var distanceVector = targetPosition - options.Transform.position;

                var magnitudeX = pidX.GetOutputValue(distanceVector.x, timeStep);
                var magnitudeY = pidY.GetOutputValue(distanceVector.y, timeStep);

                magnitudeX = Mathf.Clamp(magnitudeX, options.ClampX.min, options.ClampX.max);
                magnitudeY = Mathf.Clamp(magnitudeY, options.ClampY.min, options.ClampY.max);

                options.Rigidbody2D.AddRelativeForce(magnitudeX * Vector2.right);
                options.Rigidbody2D.AddRelativeForce(magnitudeY * Vector2.up);
            },
            () =>
            {
                pidX.Ready(options.X.kp, options.X.ki, options.X.kd);
                pidY.Ready(options.Y.kp, options.Y.ki, options.Y.kd);
            }
            );
        }
    }
}
