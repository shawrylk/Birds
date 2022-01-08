﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Contracts
{
    public interface ITargetFinder
    {
        void UpdateTargets(IList<Transform> targets);
        (Transform transform, Vector3 position) GetHighestPriorityTarget(Vector3 currentPosition);
    }
}
