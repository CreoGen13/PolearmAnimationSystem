using System.Collections.Generic;
using AnimationSystem.Character;
using Infrastructure.Interfaces;
using UnityEngine;

namespace AnimationSystem
{
    public class MainAnimationController : MonoBehaviour
    {
        [SerializeField] private CharacterAnimationController characterAnimationController;
        [SerializeField] private List<MonoBehaviour> animators;
        
        private readonly List<IUpdatable> _updatables = new();
        private readonly List<ILateUpdatable> _lateUpdatables = new();
        private readonly List<IDestroyable> _destroyables = new();

        private void Awake()
        {
            _updatables.Clear();
            
            foreach (var animator in animators)
            {
                if (animator is IInitializable initializable)
                {
                    initializable.Initialize();
                }

                if (animator is IUpdatable updatable)
                {
                    _updatables.Add(updatable);
                }
                
                if (animator is ILateUpdatable lateUpdatable)
                {
                    _lateUpdatables.Add(lateUpdatable);
                }
                
                if (animator is IDestroyable destroyable)
                {
                    _destroyables.Add(destroyable);
                }
            }
        }

        private void Update()
        {
            foreach (var updatable in _updatables)
            {
                updatable.ManualUpdate();
            }
        }

        private void LateUpdate()
        {
            foreach (var lateUpdatable in _lateUpdatables)
            {
                lateUpdatable.ManualLateUpdate();
            }
        }

        private void OnDestroy()
        {
            foreach (var destroyable in _destroyables)
            {
                destroyable.ManualDestroy();
            }
        }
    }
}