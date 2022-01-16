using Assets.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public class TreeFinder : ITargetFinder
    {
        private class TreeSlot
        {
            public int Taken { get; set; }
            public Transform Tree { get; set; }
            public Vector2 Slot { get; set; }
        }
        private sealed class TreeSlotFinder
        {
            private static List<TreeSlot> _slots;
            private static int _minTaken = 0;
            public static TreeSlotFinder Instance { get; } = new TreeSlotFinder();
            static TreeSlotFinder() { }
            private TreeSlotFinder() { }
            public void UpdateTargets(IEnumerable<Transform> trees)
            {
                _slots = _slots ?? trees
                    .Select(t => t.GetComponent<EdgeCollider2D>().points
                        .Select(p => new TreeSlot
                        {
                            Tree = t.transform,
                            Slot = p
                        }))
                    .SelectMany(s => s)
                    .ToList();
            }
            public Func<TreeSlot> GetRandomSlotGenerator()
            {
                return () =>
                {
                    _minTaken = _slots.Min(s => s.Taken);
                    var rets = _slots.Where(s => s.Taken <= _minTaken).ToList();
                    var index = UnityEngine.Random.Range(0, rets.Count);
                    return rets[index];
                };
            }
        }
        private Func<TreeSlot> _slotFinder;
        public (Transform transform, Vector3 position) GetHighestPriorityTarget(Transform currentPosition)
        {
            var randomSlot = _slotFinder();
            return (randomSlot.Tree, randomSlot.Slot * randomSlot.Tree.localScale
                + (Vector2)randomSlot.Tree.position);
        }

        public void UpdateTargets(IEnumerable<Transform> targets)
        {
            TreeSlotFinder.Instance.UpdateTargets(targets);
            _slotFinder = TreeSlotFinder.Instance.GetRandomSlotGenerator();
        }
    }
}
