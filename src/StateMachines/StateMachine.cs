﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateMachines
{
    internal class StateMachine<TState, TData> : IStateMachine<TState, TData>
        where TState : struct
    {
        public StateMachine(IEnumerable<IStateMachineStep<TState, TData>> steps, TState state, TData data)
        {
            State = state;
            Data = data;

            _steps = steps.GetEnumerator();
        }

        public TState State { get; private set; }
        public TData Data { get; }

        private readonly IEnumerator<IStateMachineStep<TState, TData>> _steps;

        public async Task<bool> MoveNextAsync(CancellationToken cancellationToken = default)
        {
            if (TryMoveToState())
            {
                if (await _steps.Current.ExecuteAsync(Data, cancellationToken))
                {
                    if (_steps.MoveNext())
                    {
                        State = _steps.Current.State;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TryMoveToState()
        {
            if (_steps.Current == null)
            {
                if (!_steps.MoveNext())
                {
                    return false;
                }
            }

            do
            {
                if (_steps.Current.State.Equals(State))
                {
                    return true;
                }
            }
            while (_steps.MoveNext());

            return false;
        }
    }
}
