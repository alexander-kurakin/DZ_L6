using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class StandingAimState : State, IUpdatableState
    {
        private const float MouseYawDegreesPerUnit = 10f;
        private readonly Entity _entity;
        private readonly IMouseInputService _mouseInput;
        private readonly ReactiveVariable<Vector3> _rotationDirection;
        
        public StandingAimState(Entity entity, IMouseInputService mouseInput)
        {
            _entity = entity;
            _mouseInput = mouseInput;
            _rotationDirection = entity.RotationDirection;
        }
        public void Update(float deltaTime)
        {
            float yaw = _mouseInput.HorizontalDelta * MouseYawDegreesPerUnit;
            
            if (Mathf.Abs(yaw) < 0.05f)
                return;
            
            Vector3 horizontalRotation  = Quaternion.Euler(0f, yaw, 0f) * _entity.Transform.forward;
            
            if (horizontalRotation.sqrMagnitude > 0.05f)
                _rotationDirection.Value = horizontalRotation.normalized;
        }
    }
}