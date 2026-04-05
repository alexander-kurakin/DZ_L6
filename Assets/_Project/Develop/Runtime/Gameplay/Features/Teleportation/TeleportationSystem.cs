using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Teleportation
{
    public class TeleportationSystem : IInitializableSystem, IDisposableSystem
    {
        private Transform _transform;
        private ReactiveVariable<float> _teleportationRadius;
        private ReactiveVariable<TeleportMode> _teleportMode;
        private ReactiveVariable<Entity> _currentTarget;
        private ReactiveVariable<float> _teleportRepulseDistance;

        private ReactiveEvent _teleportDelayEndEvent;
        private ReactiveEvent _teleportImpactDamageRequest;

        private IDisposable _teleportDelayEndDisposable;

        public void OnInit(Entity entity)
        {
            _transform = entity.Transform;
            _currentTarget = entity.CurrentTarget;
            _teleportationRadius = entity.TeleportationRadius;
            _teleportRepulseDistance =  entity.TeleportRepulseDistance;
            _teleportDelayEndEvent = entity.TeleportDelayEndEvent;
            _teleportImpactDamageRequest = entity.DealAreaImpactDamageRequest;
            _teleportMode = entity.TeleporterMode;
            _teleportDelayEndDisposable = _teleportDelayEndEvent.Subscribe(OnTeleportDelayEnd);
        }

        private void OnTeleportDelayEnd()
        {
            switch (_teleportMode.Value)
            {
                case TeleportMode.RandomInCircle:

                    OffsetByRandomPoint();
                    break;

                case TeleportMode.TowardsCurrentTarget:
                    OffsetByDirectionTowardsCurrentTarget();
                    break;
            }

            _teleportImpactDamageRequest.Invoke();
        }

        private void OffsetByRandomPoint()
        {
            Vector2 random2DPoint = _teleportationRadius.Value * UnityEngine.Random.insideUnitCircle;
            Vector3 randomOffset = new Vector3(random2DPoint.x, 0f, random2DPoint.y);

            _transform.position += randomOffset;
        }

        private void OffsetByDirectionTowardsCurrentTarget()
        {
            if (_currentTarget.Value == null)
                return;
            
            Vector3 currentPosition = _transform.position;
            Vector3 targetPosition = _currentTarget.Value.Transform.position;

            Vector3 horizontalOffset =
                new Vector3(
                    targetPosition.x - currentPosition.x,
                    0f,
                    targetPosition.z - currentPosition.z);
            
            float distanceToTarget = horizontalOffset.magnitude;

            if (distanceToTarget <= _teleportRepulseDistance.Value)
                return;
            
            Vector3 normalizedDirection = horizontalOffset.normalized;

            float teleportDistance =
                MathF.Min(_teleportationRadius.Value, distanceToTarget - _teleportRepulseDistance.Value);
            
            _transform.position += normalizedDirection * teleportDistance;
        }

        public void OnDispose()
        {
            _teleportDelayEndDisposable.Dispose();
        }
    }
}