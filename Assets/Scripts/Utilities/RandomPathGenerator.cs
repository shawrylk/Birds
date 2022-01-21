using Assets.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public class RandomPathGenerator : ITargetFinder
    {
        private const float unit = 3;

        public (Transform transform, Vector3 position) GetHighestPriorityTarget(Transform currentPosition)
        {
            return (null, currentPosition.position + new Vector3(-unit, -unit).Range(new Vector3(unit, unit)));
        }

        public void UpdateTargets(IEnumerable<Transform> targets)
        {
        }
    }
}
