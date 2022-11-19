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

     private Camera _cam;
    
    // Start is called before the first frame update
    void Start()
    {
        _kernel = computeShader.FindKernel("CSMain");
        
        _cam = Camera.main;

        texture = new RenderTexture(_cam.pixelWidth, _cam.pixelHeight, 24);
        texture.enableRandomWrite = true;
        texture.Create();
        
        computeShader.SetTexture(_kernel, "Result", texture);
        
        _world = World.DefaultGameObjectInjectionWorld;
        _swarmSystem = _world.GetExistingSystemManaged<SwarmSystem>();
        
        _slimePos = new ComputeBuffer(1, 3 * 4);
        
        
    }

    // Update is called once per frame
    void Update()
    {

        var pos = _swarmSystem.entitiesPos;
        if (pos.Length != _slimePos.count)
        {
            _slimePos = new ComputeBuffer(pos.Length, 3 * 4);
        }
        
        
        _slimePos.SetData(pos);
        
        computeShader.SetMatrix("CamVP", _cam.previousViewProjectionMatrix);
        computeShader.SetBuffer(_kernel, "slimePos", _slimePos);
        computeShader.Dispatch(_kernel, texture.width/16,texture.height/16,pos.Length/4);
    }

    private void OnDisable()
    {
        _slimePos.Dispose();
    }
}
