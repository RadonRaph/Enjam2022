using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;





//Attention c'est une struct
public struct SwarmAgentComponent : IComponentData
{
    public float3 Color;
    public float3 Velocity;

    public float3 debug;

}



//Qui grace a ça va généré le composant ECS equivalent
public class SwarmAgentBaker : Baker<SwarmAgentAuthoring>
{
    public override void Bake(SwarmAgentAuthoring authoring)
    {
        AddComponent(new SwarmAgentComponent{Color = new float3(authoring.color.r, authoring.color.g, authoring.color.b)});
    }
}