using System.Collections.Generic;
using System.Linq;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.ApplyDamage;
using Assets._Project.Develop.Runtime.Utilities.Conditions;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class MinHealthTargetSelector : ITargetSelector
    {
        private Entity _source;

        public MinHealthTargetSelector(Entity entity)
        {
            _source = entity;
        }

        public Entity SelectTargetFrom(IEnumerable<Entity> targets)
        {
            IEnumerable<Entity> selectedTargets = targets.Where(target =>
            {
                bool result = target.HasComponent<TakeDamageRequest>();

                if(target.TryGetCanApplyDamage(out ICompositeCondition canApplyDamage))
                {
                    result = result && canApplyDamage.Evaluate();
                }

                result = result && (target != _source);

                result = result && (target.CurrentHealth.Value > 0);

                return result;
            });            
            
            if (selectedTargets.Any() == false)
                return null;
            
            Entity minHPtarget = selectedTargets.First();
            float minHP = minHPtarget.CurrentHealth.Value;

            foreach (Entity target in selectedTargets)
            {
                float targetHP = target.CurrentHealth.Value;

                if(targetHP < minHP)
                {
                    minHP = targetHP;
                    minHPtarget = target;
                }
            }
            
            return minHPtarget;
        }
    }
}