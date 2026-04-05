using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Teleportation;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class FindTargetState : State, IUpdatableState
    {
        private ITargetSelector _targetSelector;
        private EntitiesLifeContext _entitiesLifeContext;
        private ReactiveVariable<Entity> _currentTarget;
        private Entity _stateUser;

        private IDisposable changeTargetNotifier;

        public FindTargetState(
            ITargetSelector targetSelector,
            EntitiesLifeContext entitiesLifeContext,
            Entity entity)
        {
            _targetSelector = targetSelector;
            _entitiesLifeContext = entitiesLifeContext;
            _currentTarget = entity.CurrentTarget;
            _stateUser = entity;
        }
        
        public override void Enter()
        {
            base.Enter();
            changeTargetNotifier = _currentTarget.Subscribe(OnCurrentTargetChanged);
        }

        private void OnCurrentTargetChanged(Entity oldTarget, Entity newTarget)
        {
            if (_stateUser.TryGetTeleporterMode(out ReactiveVariable<TeleportMode> mode))
            {
                if (mode.Value == TeleportMode.TowardsCurrentTarget)
                    Debug.Log($"Target changed to {newTarget} with {newTarget.CurrentHealth.Value} HP");
            }
        }

        public override void Exit()
        {
            base.Exit();
            changeTargetNotifier?.Dispose();
        }

        public void Update(float deltaTime)
        {
            _currentTarget.Value = _targetSelector.SelectTargetFrom(_entitiesLifeContext.Entities);
        }
    }
}
