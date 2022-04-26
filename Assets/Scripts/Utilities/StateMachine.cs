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
        public MonoBehaviour Owner { get; set; }
        public StateConductor Conductor { get; set; }
        public Func<IEnumerator> Coroutine { get; set; }
        public Func<(State oldState, State newState), bool> OnStateChanged;
    }
    public abstract class StateConductor
    {
        protected Coroutine _coroutine;
        public Func<(State oldState, State newState), bool> OnStateChanged;
        protected State State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    var oldState = _state;
                    if (oldState?.OnStateChanged?.Invoke((oldState, value)) != true)
                    {
                        _state = value;
                        _state.Conductor = this;
                        _state.Owner = oldState?.Owner ?? owner;
                        _state?.OnStateChanged?.Invoke((oldState, value));
                    }
                }
            }
        }
        private State _state;
        protected MonoBehaviour owner;
        public StateConductor(MonoBehaviour owner)
        {
            this.owner = owner;
        }
        protected void Run(State state)
        {
            State = state;
            State.Owner = owner;
            _coroutine = owner.StartCoroutine(Coroutine());
        }
        protected virtual IEnumerator Coroutine()
        {
            while (true)
            {
                yield return State.Coroutine?.Invoke();
            }
        }
        public virtual void ChangeState(State state, bool isForce = false)
        {
            if (state != null)
            {
                State = state;
                if (isForce)
                {
                    owner.StopCoroutine(_coroutine);
                    _coroutine = owner.StartCoroutine(Coroutine());
                }
            }
        }
    }

}
