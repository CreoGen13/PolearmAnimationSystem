using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    /// <summary>
    /// The ChainIK constraint job.
    /// </summary>
    [Unity.Burst.BurstCompile]
    public struct ChainTrianglesIKConstraintJob : IWeightedAnimationJob
    {
        /// <summary>An array of Transform handles that represents the Transform chain.</summary>
        public NativeArray<ReadWriteTransformHandle> chain;
        /// <summary>The Transform handle for the target Transform.</summary>
        public ReadOnlyTransformHandle target;

        /// <summary>The offset applied to the target transform if maintainTargetPositionOffset or maintainTargetRotationOffset is enabled.</summary>
        public AffineTransform targetOffset;

        /// <summary>An array of length in between Transforms in the chain.</summary>
        public NativeArray<float> linkLengths;

        /// <summary>An array of positions for Transforms in the chain.</summary>
        public NativeArray<Vector3> linkPositions;

        /// <summary>The weight for which ChainIK target has an effect on chain (up to tip Transform). This is a value in between 0 and 1.</summary>
        public FloatProperty chainRotationWeight;
        /// <summary>The weight for which ChainIK target has and effect on tip Transform. This is a value in between 0 and 1.</summary>
        public FloatProperty tipRotationWeight;

        /// <summary>CacheIndex to ChainIK tolerance value.</summary>
        /// <seealso cref="AnimationJobCache"/>
        public CacheIndex toleranceIdx;
        /// <summary>CacheIndex to ChainIK maxIterations value.</summary>
        /// <seealso cref="AnimationJobCache"/>
        public CacheIndex maxIterationsIdx;
        /// <summary>Cache for static properties in the job.</summary>
        public AnimationJobCache cache;

        /// <summary>The maximum distance the Transform chain can reach.</summary>
        public float maxReach;

        /// <inheritdoc />
        public FloatProperty jobWeight { get; set; }

        /// <summary>
        /// Defines what to do when processing the root motion.
        /// </summary>
        /// <param name="stream">The animation stream to work on.</param>
        public void ProcessRootMotion(AnimationStream stream) { }

        /// <summary>
        /// Defines what to do when processing the animation.
        /// </summary>
        /// <param name="stream">The animation stream to work on.</param>
        public void ProcessAnimation(AnimationStream stream)
        {
            var w = jobWeight.Get(stream);
            if (w > 0f)
            {
                for (int i = 0; i < chain.Length; ++i)
                {
                    var handle = chain[i];
                    linkPositions[i] = handle.GetPosition(stream);
                    chain[i] = handle;
                }

                var tipIndex = chain.Length - 1;
                if (ChainTrianglesUtils.SolveTriangles(ref linkPositions, ref linkLengths, target.GetPosition(stream) + targetOffset.translation,
                    cache.GetRaw(toleranceIdx), maxReach))
                {
                    var chainRWeight = chainRotationWeight.Get(stream) * w;
                    for (var i = 0; i < tipIndex; ++i)
                    {
                        var prevDir = chain[i + 1].GetPosition(stream) - chain[i].GetPosition(stream);
                        var newDir = linkPositions[i + 1] - linkPositions[i];
                        var rot = chain[i].GetRotation(stream);
                        chain[i].SetRotation(stream, Quaternion.Lerp(rot, QuaternionExt.FromToRotation(prevDir, newDir) * rot, chainRWeight));
                    }
                }

                chain[tipIndex].SetRotation(
                    stream,
                    Quaternion.Lerp(
                        chain[tipIndex].GetRotation(stream),
                        target.GetRotation(stream) * targetOffset.rotation,
                        tipRotationWeight.Get(stream) * w
                        )
                    );
            }
            else
            {
                for (var i = 0; i < chain.Length; ++i)
                {
                    AnimationRuntimeUtils.PassThrough(stream, chain[i]);
                }
            }
        }
        
    }
    

    /// <summary>
    /// This interface defines the data mapping for the ChainIK constraint.
    /// </summary>
    public interface IChainTrianglesIKConstraintData
    {
        /// <summary>The root Transform of the ChainIK hierarchy.</summary>
        Transform root { get; }
        /// <summary>The tip Transform of the ChainIK hierarchy. The tip needs to be a descendant/child of the root Transform.</summary>
        Transform tip { get; }
        /// <summary>The ChainIK target Transform.</summary>
        Transform target { get; }
        
        /// <summary>
        /// The allowed distance between the tip and target Transform positions.
        /// When the distance is smaller than the tolerance, the algorithm has converged on a solution and will stop.
        /// </summary>
        float tolerance { get; }
        /// <summary>This is used to maintain the current position offset from the tip Transform to target Transform.</summary>
        bool maintainTargetPositionOffset { get; }
        /// <summary>This is used to maintain the current rotation offset from the tip Transform to target Transform.</summary>
        bool maintainTargetRotationOffset { get; }

        /// <summary>The path to the chain rotation weight property in the constraint component.</summary>
        string chainRotationWeightFloatProperty { get; }
        /// <summary>The path to the tip rotation weight property in the constraint component.</summary>
        string tipRotationWeightFloatProperty { get; }
    }

    /// <summary>
    /// The ChainIK constraint job binder.
    /// </summary>
    /// <typeparam name="T">The constraint data type</typeparam>
    public class ChainTrianglesIKConstraintJobBinder<T> : AnimationJobBinder<ChainTrianglesIKConstraintJob, T>
        where T : struct, IAnimationJobData, IChainTrianglesIKConstraintData
    {
        /// <inheritdoc />
        public override ChainTrianglesIKConstraintJob Create(Animator animator, ref T data, Component component)
        {
            Transform[] chain = ConstraintsUtils.ExtractChain(data.root, data.tip);

            var job = new ChainTrianglesIKConstraintJob
            {
                chain = new NativeArray<ReadWriteTransformHandle>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
                linkLengths = new NativeArray<float>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
                linkPositions = new NativeArray<Vector3>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
                maxReach = 0f
            };

            var tipIndex = chain.Length - 1;
            for (var i = 0; i < chain.Length; ++i)
            {
                job.chain[i] = ReadWriteTransformHandle.Bind(animator, chain[i]);
                job.linkLengths[i] = (i != tipIndex) ? Vector3.Distance(chain[i].position, chain[i + 1].position) : 0f;
                job.maxReach += job.linkLengths[i];
            }

            job.target = ReadOnlyTransformHandle.Bind(animator, data.target);
            job.targetOffset = AffineTransform.identity;
            if (data.maintainTargetPositionOffset)
            {
                job.targetOffset.translation = data.tip.position - data.target.position;
            }

            if (data.maintainTargetRotationOffset)
            {
                job.targetOffset.rotation = Quaternion.Inverse(data.target.rotation) * data.tip.rotation;
            }

            job.chainRotationWeight = FloatProperty.Bind(animator, component, data.chainRotationWeightFloatProperty);
            job.tipRotationWeight = FloatProperty.Bind(animator, component, data.tipRotationWeightFloatProperty);

            var cacheBuilder = new AnimationJobCacheBuilder();
            job.toleranceIdx = cacheBuilder.Add(data.tolerance);
            job.cache = cacheBuilder.Build();

            return job;
        }

        /// <inheritdoc />
        public override void Destroy(ChainTrianglesIKConstraintJob job)
        {
            job.chain.Dispose();
            job.linkLengths.Dispose();
            job.linkPositions.Dispose();
            job.cache.Dispose();
        }

        /// <inheritdoc />
        public override void Update(ChainTrianglesIKConstraintJob job, ref T data)
        {
            job.cache.SetRaw(data.tolerance, job.toleranceIdx);
        }
    }
}
