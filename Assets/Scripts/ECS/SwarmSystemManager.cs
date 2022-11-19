using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Serialization;

public class SwarmSystemManager : MonoBehaviour
{
    private World _world;
    private SwarmSystem _swarmSystem;

    public Transform target;
    
    // Start is called before the first frame update
    void Start()
    {
        _world = World.DefaultGameObjectInjectionWorld;
        _swarmSystem = _world.GetExistingSystemManaged<SwarmSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        _swarmSystem.Target = target;
    }
}
