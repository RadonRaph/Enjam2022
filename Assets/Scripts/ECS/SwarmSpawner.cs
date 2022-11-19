using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class SwarmSpawner : MonoBehaviour
{
    public int targetEntityCount = 100;
    public int currentEntityCount = 0;
    
    
    public Mesh mesh;
    public Material material;
    
    
    [GenerateTestsForBurstCompatibility]
    public struct SpawnJob : IJobParallelFor
    {
        public Entity Prototype;
        public int EntityCount;
        public EntityCommandBuffer.ParallelWriter Ecb;
        public NativeArray<float3> Pos;

        public void Execute(int index)
        {
            // Clone the Prototype entity to create a new entity.
            var e = Ecb.Instantiate(index, Prototype);

            LocalToWorldTransform trans = new LocalToWorldTransform();

            trans.Value.Position = Pos[index];
            trans.Value.Scale = 1f;
            
            // Prototype has all correct components up front, can use SetComponent to
            // set values unique to the newly created entity, such as the transform.
            Ecb.SetComponent(index, e, trans);
           // Ecb.SetComponent(index, e, new LocalToWorld {Value = ComputeTransform(index)});
        }
        
        public float4x4 ComputeTransform(int index)
        {
            return float4x4.Translate(new float3(index, 0, 0));
        }

    }


    private World _world;
    private EntityManager _manager;

    private EntityCommandBuffer _ecb;

    private RenderMeshDescription _desc = new RenderMeshDescription(ShadowCastingMode.On, receiveShadows: true);
    private RenderMeshArray _meshArray;

    private Entity _prototype;
    
    // Start is called before the first frame update
    void Start()
    {
        _world = World.DefaultGameObjectInjectionWorld;
        _manager = _world.EntityManager;

        _ecb = new EntityCommandBuffer(Allocator.TempJob);

        _meshArray = new RenderMeshArray(new List<Material> {material}.ToArray(), new List<Mesh> {mesh}.ToArray());

        _prototype = _manager.CreateEntity();

        _manager.AddComponent<SwarmAgentComponent>(_prototype);
        _manager.AddComponent<LocalToWorldTransform>(_prototype);
        
        RenderMeshUtility.AddComponents(
            _prototype,
            _manager,
            _desc,
            _meshArray,
            MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));



    }

    // Update is called once per frame
    void Update()
    {
        if (currentEntityCount < targetEntityCount)
        {
            int count = targetEntityCount - currentEntityCount;

            NativeArray<float3> pos = new NativeArray<float3>(count, Allocator.TempJob);

            for (int i = 0; i < pos.Length; i++)
            {
                pos[i] = new float3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
            }

            var spawnJob = new SpawnJob()
            {
                Prototype = _prototype,
                Ecb = _ecb.AsParallelWriter(),
                EntityCount = count,
                Pos = pos,
            };

            var spawnHandle = spawnJob.Schedule(count, 128);
            
            spawnHandle.Complete();
            _ecb.Playback(_manager);
            
            currentEntityCount += count;

        }
    }

    private void OnDestroy()
    {
        
       // _ecb.Dispose(); 
       // _manager.DestroyEntity(_prototype);
    }
}
