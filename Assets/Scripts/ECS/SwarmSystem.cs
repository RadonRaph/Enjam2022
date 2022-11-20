using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial class SwarmSystem : SystemBase
{
    public EntityQuery Query;
    public Transform Target;

    public NativeArray<Vector3> entitiesPos;

    private float3[] _targetRays;
    private float3[] _repulsionRays;
    private float3[] _startRays;
    
    protected override void OnCreate()
    {
        base.OnCreate();

        Query = new EntityQueryBuilder(Allocator.Temp).WithAll<SwarmAgentComponent>().WithAll<LocalToWorldTransform>().Build(this);
    }

    protected override void OnUpdate()
    {

        var entitiesTransforms = Query.ToComponentDataArray<LocalToWorldTransform>(Allocator.TempJob);

        int count = Query.CalculateEntityCount();


        entitiesPos = new NativeArray<Vector3>(count, Allocator.Persistent);

        for (int i = 0; i < count; i++)
        {
            entitiesPos[i] = entitiesTransforms[i].Value.Position;
        }

        NativeArray<float3> targetRays = new NativeArray<float3>(count, Allocator.TempJob);
        NativeArray<float3> repulsionRays = new NativeArray<float3>(count, Allocator.TempJob);
        NativeArray<float3> startRays = new NativeArray<float3>(count, Allocator.TempJob);

        float3 target = float3.zero;
        
        if (Target != null)
        {
             target = Target.position;
        }



        Entities.WithReadOnly(entitiesTransforms).WithAll<SwarmAgentComponent>().ForEach(
            (int entityInQueryIndex, ref LocalToWorldTransform trans, ref SwarmAgentComponent swarm) =>
            {

                //Target vector
                float3 targetVector = math.normalize(target - trans.Value.Position);
                float targetDist = math.distance(target, trans.Value.Position);
                
                float3 repulsionVector = float3.zero;

                for (int i = entityInQueryIndex%5; i < entitiesTransforms.Length; i+=5)
                {
                    LocalToWorldTransform otherTrans = entitiesTransforms[i];

                    float3 antiNan = new float3(0.001f, 0.001f, 0.001f);
                    float3 dir = math.normalize(otherTrans.Value.Position - trans.Value.Position+antiNan);
                    float dist = math.distance(trans.Value.Position, otherTrans.Value.Position);
                    
                    
                    if (math.isnan(dir).x || math.isnan(dist))
                        continue;
                    
                    

                    float force = math.clamp(5 - dist, 0, 5)*0.1f;

                    repulsionVector += force * dir;

                }

                targetRays[entityInQueryIndex] = targetVector * targetDist; 
                repulsionRays[entityInQueryIndex] = repulsionVector; 
                startRays[entityInQueryIndex] = trans.Value.Position;

                float3 gravity = new float3(0, -9.81f, 0);
                
                float3 moveVector = (targetVector * targetDist * 0.1f) - repulsionVector * 0.1f + gravity*0.1f;

                moveVector = math.normalize(moveVector);

                swarm.debug = repulsionVector;
                swarm.Velocity *= 0.95f;
                swarm.Velocity += moveVector;

                trans.Value.Position += swarm.Velocity*0.1f;
                if (trans.Value.Position.y < 0)
                    trans.Value.Position.y = 0;

                //trans.Value.Position += targetVector*0.1f;

            }
        ).WithStoreEntityQueryInField(ref Query).ScheduleParallel();
        
        /*
        handle.Complete();

        for (int i = 0; i < count; i++)
        {
            Debug.DrawRay(startRays[i], targetRays[i], Color.green);
            Debug.DrawRay(startRays[i], repulsionRays[i], Color.red);
        }
*/
    }
}
