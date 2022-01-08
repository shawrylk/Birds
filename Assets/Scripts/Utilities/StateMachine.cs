using Assets.Contracts;
using Assets.Scripts.Birds;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public abstract class State
    {
        protected Enumeration ID;
        public Func<Context, IEnumerator> Couroutine { get; set; }
    }
    public abstract class Context
    {
        public State State { get; set; }
        private MonoBehaviour _owner;
        public Context(MonoBehaviour owner)
        {
            _owner = owner;
        }
        protected void Run(State state)
        {
            State = state;
            _owner.StartCoroutine(Coroutine());
        }
        private IEnumerator Coroutine()
        {
            while (true)
            {
                yield return State.Couroutine?.Invoke(this);
            }
        }
    }

}
