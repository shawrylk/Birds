using Assets.Contracts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public class PidController : IPidController
    {
        private float _kp = 0.0f;
        private float _ki = 0.0f;
        private float _kd = 0.0f;
        private float _p = 0.0f;
        private float _i = 0.0f;
        private float _d = 0.0f;
        private float _prev_error = 0.0f;
        public void Ready(float kp, float ki, float kd)
        {
            _kp = kp;
            _ki = ki;
            _kd = kd;
            _p = 0.0f;
            _i = 0.0f;
            _d = 0.0f;
            _prev_error = 0.0f;
        }

        public float GetOutputValue(float error, float deltaTime)
        {
            _p = error;
            _i = _p * deltaTime;
            _d = (_p - _prev_error) / deltaTime;
            _prev_error = error;
            return _kp * _p + _ki * _i + _kd * _d;
        }
    }
}
