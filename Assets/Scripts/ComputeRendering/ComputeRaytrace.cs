using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Unity.Entities;

public class ComputeRaytrace : MonoBehaviour
{
     public RenderTexture texture;
     public ComputeShader computeShader;


     private World _world;
     private SwarmSystem _swarmSystem;

     private int _kernel;

     private ComputeBuffer _slimePos;
    
    // Start is called before the first frame update
    void Start()
    {
        _kernel = computeShader.FindKernel("CSMain");

        texture = new RenderTexture(512, 512, 24);
        texture.enableRandomWrite = true;
        texture.Create();
        
        computeShader.SetTexture(_kernel, "Result", texture);
        
        _world = World.DefaultGameObjectInjectionWorld;
        _swarmSystem = _world.GetExistingSystemManaged<SwarmSystem>();
        
        
    }

    // Update is called once per frame
    void Update()
    {

        var pos = _swarmSystem.entitiesPos;
        _slimePos = new ComputeBuffer(pos.Length, 3 * 4);
        _slimePos.SetData(pos);
        
        computeShader.SetBuffer(_kernel, "slimePos", _slimePos);
        computeShader.Dispatch(_kernel, texture.width/16,texture.height/16,pos.Length/4);
    }

    private void OnDisable()
    {
        _slimePos.Dispose();
    }
}
