using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class ContinuousEventSpamState : State, IUpdatableState
    {
        private ReactiveEvent _triggeredEvent;

        private float _cooldownBetweenEventTriggering;

        private float _time;

        public ContinuousEventSpamState(
            ReactiveEvent triggeredEvent,
            float cooldownBetweenEventTriggering
        )
        {
            _triggeredEvent = triggeredEvent;
            _cooldownBetweenEventTriggering = cooldownBetweenEventTriggering;
        }

        public void Update(float deltaTime)
        {
            _time += deltaTime;

            if (_time >= _cooldownBetweenEventTriggering)
            {
                _triggeredEvent.Invoke();
                _time = 0f;
            }
        }
    }
}