using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Infrastructure.Testing
{
    [ExecuteInEditMode]
    public class TestPlayableGraph : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip clip;
        
        private PlayableGraph _graph;

        [Button]
        private void PlayClip()
        {
            DestroyGraph();
            
            _graph = PlayableGraph.Create("Animation");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            
            var clipPlayable = AnimationClipPlayable.Create(_graph, clip);
            
            var output = AnimationPlayableOutput.Create(_graph, "Animation Output", animator);
            output.SetSourcePlayable(clipPlayable);
            
            _graph.Play();
        }

        private void DestroyGraph()
        {
            if (_graph.IsValid())
            {
                _graph.Destroy();
            }
        }

        private void OnDestroy()
        {
            DestroyGraph();
        }
    }
}