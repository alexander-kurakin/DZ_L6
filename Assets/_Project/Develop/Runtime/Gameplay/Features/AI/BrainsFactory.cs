using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI.States;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.Timer;
using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Gameplay.Features.Teleportation;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI
{
    public class BrainsFactory
    {
        private readonly DIContainer _container;
        private readonly TimerServiceFactory _timerServiceFactory;
        private readonly AIBrainsContext _brainsContext;
        private readonly IInputService _inputService;
        private readonly IMouseInputService _mouseInputService;
        private readonly EntitiesLifeContext _entitiesLifeContext;

        public BrainsFactory(DIContainer container)
        {
            _container = container;
            _timerServiceFactory = _container.Resolve<TimerServiceFactory>();
            _brainsContext = _container.Resolve<AIBrainsContext>();
            _inputService = _container.Resolve<IInputService>();
            _mouseInputService = _container.Resolve<IMouseInputService>();
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
        }

        public StateMachineBrain CreateMainHeroManualBrain(Entity entity)
        {
            PlayerInputMovementState movementState = new PlayerInputMovementState(entity, _inputService);
            StandingAimState standingAimingState = new StandingAimState(entity, _mouseInputService);
            AttackTriggerState attackingState = new AttackTriggerState(entity);
            
            ReactiveVariable<bool> inAttackProcess = entity.InAttackProcess;
            
            ICondition moveToStand = new FuncCondition(() => _inputService.Direction == Vector3.zero);
            ICondition standToMove = new FuncCondition(() => _inputService.Direction != Vector3.zero);
            ICondition standToAttack = new CompositeCondition()
                .Add(entity.CanStartAttack)
                .Add(new FuncCondition(() => _mouseInputService.FireButtonPressed));
            ICondition attackToStand = new CompositeCondition()
                .Add(new FuncCondition(() => inAttackProcess.Value == false))
                .Add(new FuncCondition(() => _inputService.Direction == Vector3.zero));
            ICondition attackToMove = new FuncCondition(() => _inputService.Direction != Vector3.zero);
            
            AIStateMachine behaviour = new AIStateMachine();
            
            behaviour.AddState(movementState);
            behaviour.AddState(standingAimingState);
            behaviour.AddState(attackingState);
            
            behaviour.AddTransition(movementState, standingAimingState, moveToStand);
            behaviour.AddTransition(standingAimingState, movementState, standToMove);
            behaviour.AddTransition(standingAimingState, attackingState, standToAttack);
            behaviour.AddTransition(attackingState, standingAimingState, attackToStand);
            behaviour.AddTransition(attackingState, movementState, attackToMove);
            
            StateMachineBrain brain = new StateMachineBrain(behaviour);
            
            _brainsContext.SetFor(entity, brain);
            return brain;
        }

        public StateMachineBrain CreateGhostBrain(Entity entity)
        {
            AIStateMachine stateMachine = CreateRandomMovementStateMachine(entity);
            StateMachineBrain brain = new StateMachineBrain(stateMachine);

            _brainsContext.SetFor(entity, brain);

            return brain;
        }
        
        public StateMachineBrain CreateSimpleTeleporterBrain(Entity entity)
        {
            entity.TeleporterMode.Value = TeleportMode.RandomInCircle;
            
            AIStateMachine stateMachine = CreateContinuousEventStateMachine(entity);
            StateMachineBrain brain = new StateMachineBrain(stateMachine);
            
            _brainsContext.SetFor(entity, brain);
            
            return brain;
        }

        public StateMachineBrain CreateComplexTeleporterBrain(Entity entity, ITargetSelector targetSelector)
        {
            entity.TeleporterMode.Value = TeleportMode.TowardsCurrentTarget;
            
            EmptyState regenerationState = new EmptyState();
            
            AIStateMachine teleportationState = CreateContinuousEventStateMachine(entity);

            ReactiveVariable<Entity> currentTarget = entity.CurrentTarget;

            ICompositeCondition fromRegenToTeleportStateCondition = new CompositeCondition()
                .Add(new FuncCondition(() => currentTarget.Value != null))
                .Add(new FuncCondition(() => entity.CurrentEnergy.Value >= 0.4f * entity.MaxEnergy.Value));

            ICompositeCondition fromTeleportToRegenStateCondition = new CompositeCondition(LogicOperations.Or)
                .Add(new FuncCondition(() => currentTarget.Value == null))
                .Add(new FuncCondition(() => entity.CurrentEnergy.Value < 0.4f * entity.MaxEnergy.Value));

            AIStateMachine behaviour = new AIStateMachine();

            behaviour.AddState(regenerationState);
            behaviour.AddState(teleportationState);

            behaviour.AddTransition(regenerationState, teleportationState, fromRegenToTeleportStateCondition);
            behaviour.AddTransition(teleportationState, regenerationState, fromTeleportToRegenStateCondition);

            FindTargetState findTargetState = new FindTargetState(targetSelector, _entitiesLifeContext, entity);
            AIParallelState parallelState = new AIParallelState(findTargetState, behaviour);

            AIStateMachine rootStateMachine = new AIStateMachine();
            rootStateMachine.AddState(parallelState);

            StateMachineBrain brain = new StateMachineBrain(rootStateMachine);
            _brainsContext.SetFor(entity, brain);

            return brain;
        }
        

        private AIStateMachine CreateContinuousEventStateMachine(Entity entity)
        {
            ContinuousEventSpamState continuousEventSpamState = new ContinuousEventSpamState(entity.StartTeleportationRequest, 3f);

            AIStateMachine stateMachine = new AIStateMachine();

            stateMachine.AddState(continuousEventSpamState);
            
            return stateMachine;
        }

        private AIStateMachine CreateRandomMovementStateMachine(Entity entity)
        {
            List<IDisposable> disposables = new List<IDisposable>();

            RandomMovementState randomMovementState = new RandomMovementState(entity, 0.5f);

            EmptyState emptyState = new EmptyState();

            TimerService movementTimer = _timerServiceFactory.Create(2f);
            disposables.Add(movementTimer);
            disposables.Add(randomMovementState.Entered.Subscribe(movementTimer.Restart));

            TimerService idleTimer = _timerServiceFactory.Create(3f);
            disposables.Add(idleTimer);
            disposables.Add(emptyState.Entered.Subscribe(idleTimer.Restart));

            FuncCondition movementTimerEndedCondition = new FuncCondition(() => movementTimer.IsOver);
            FuncCondition idleTimerEndedCondition = new FuncCondition(() => idleTimer.IsOver);

            AIStateMachine stateMachine = new AIStateMachine(disposables);

            stateMachine.AddState(randomMovementState);
            stateMachine.AddState(emptyState);

            stateMachine.AddTransition(randomMovementState, emptyState, movementTimerEndedCondition);
            stateMachine.AddTransition(emptyState, randomMovementState, idleTimerEndedCondition);

            return stateMachine;
        }
    }
}
