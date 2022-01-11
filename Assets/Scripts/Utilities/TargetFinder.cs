﻿using Assets.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public class TargetFinder : ITargetFinder
    {
        private IList<Transform> _targets;
        private ITargetFinder _randomPath = new RandomPathGenerator();
        public void UpdateTargets(IList<Transform> targets)
        {
            _targets = targets;
        }

        public (Transform, Vector3) GetHighestPriorityTarget(Vector3 currentPosition)
        {
            if (_targets == null
                || _targets.Count == 0)
            {
                return _randomPath.GetHighestPriorityTarget(currentPosition);
            }

            var minDistance = float.MaxValue;
            Transform nearestTransform = null;
            foreach (var target in _targets)
            {
                if (target == null) continue;
                var distance = (target.position - currentPosition).sqrMagnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestTransform = target;
                }
            }
            return (nearestTransform, nearestTransform?.position ?? default);
        }
    }
}
