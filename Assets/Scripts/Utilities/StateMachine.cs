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
        public Func<(State oldState, State newState), bool> OnStateChanged;
        protected State State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    if (OnStateChanged?.Invoke((_state, value)) != true)
                    {
                        _state = value;
                    }
                }
            }
        }
        private State _state;
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
