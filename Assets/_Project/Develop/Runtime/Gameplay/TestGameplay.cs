using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI.States;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay
{
    public class TestGameplay : MonoBehaviour
    {
        private DIContainer _container;
        private EntitiesFactory _entitiesFactory;
        private BrainsFactory _brainsFactory;

        private Entity _teleportator;
        private Entity _ghost;
        private Entity _ghostTwo;
        private Entity _ghostThree;
        private Entity _mainHero;

        private bool _isRunning;

        public void Initialize(DIContainer container)
        {
            _container = container;
            _entitiesFactory = _container.Resolve<EntitiesFactory>();
            _brainsFactory = _container.Resolve<BrainsFactory>();
        }

        public void Run()
        {
            _mainHero = _entitiesFactory.CreateHero(Vector3.zero);
            _mainHero.AddCurrentTarget();
            _brainsFactory.CreateMainHeroBrain(_mainHero, new NearestDamageableTargetSelector(_mainHero));
            
            _teleportator = _entitiesFactory.CreateTeleportator(Vector3.zero + Vector3.right * 5);
            _brainsFactory.CreateSimpleTeleporterBrain(_teleportator);
            
            _ghost = _entitiesFactory.CreateGhost(Vector3.zero + Vector3.forward * 5);
            _brainsFactory.CreateGhostBrain(_ghost);
            
            _ghostTwo = _entitiesFactory.CreateGhost(Vector3.zero + Vector3.back * 5);
            _brainsFactory.CreateGhostBrain(_ghostTwo);
            
            _ghostThree = _entitiesFactory.CreateGhost(Vector3.zero + Vector3.left * 5);
            _brainsFactory.CreateGhostBrain(_ghostThree);

            _isRunning = true;
        }

        private void Update()
        {
            if (_isRunning == false)
                return;

            //if (Input.GetKeyDown(KeyCode.Space))
            //    _teleportator.TakeDamageRequest.Invoke(50);

            //if (Input.GetKeyDown(KeyCode.T))
            //    _teleportator.StartTeleportationRequest.Invoke();

            //Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

            //_entity.MoveDirection.Value = input;
            //_entity.RotationDirection.Value = input;
        }
    }
}
