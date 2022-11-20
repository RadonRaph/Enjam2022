using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Unity.Entities;
using UnityEngine.UI;

public class ComputeRaytrace : MonoBehaviour
{
     public RenderTexture texture;
     public ComputeShader computeShader;


     private World _world;
     private SwarmSystem _swarmSystem;

     private int _kernel;
     private int _prepassKernel;

     private ComputeBuffer _slimePos;

     private Camera _cam;

     public RawImage rawImage;

     public Vector2 ratio;
     public Vector2 offset;

     public Material fullscreenMat;
    
    // Start is called before the first frame update
    void Start()
    {
        _kernel = computeShader.FindKernel("CSMain");
        _prepassKernel = computeShader.FindKernel("SlimePrepass");
        
        _cam = Camera.main;

        texture = new RenderTexture(_cam.pixelWidth, _cam.pixelHeight, 24);
        texture.enableRandomWrite = true;
        texture.Create();
        
        computeShader.SetTexture(_kernel, "Result", texture);
        computeShader.SetTexture(2, "Result", texture);
        
        _world = World.DefaultGameObjectInjectionWorld;
        _swarmSystem = _world.GetExistingSystemManaged<SwarmSystem>();
        
        _slimePos = new ComputeBuffer(1, 3 * 4);

        rawImage.texture = texture;

    }

    // Update is called once per frame
    void Update()
    {

        var pos = _swarmSystem.entitiesPos;
        int count = pos.Length;
        
        if (count <= 0)
            return;
        
        if (count != _slimePos.count)
        {
            if (_slimePos.IsValid())
                _slimePos.Dispose();
            
            _slimePos = new ComputeBuffer(pos.Length, 3 * 4);
        }

        Vector3[] debugPos = new Vector3[1];
        debugPos[0] = Vector3.zero;


        
        _slimePos.SetData(pos);
        
        fullscreenMat.SetInt("slimesCount", count);
        fullscreenMat.SetVector("camPos", _cam.transform.position);
        fullscreenMat.SetBuffer("slimes", _slimePos);

        /*
        computeShader.SetInt("slimesCount", count);
        computeShader.SetInt("width", texture.width);
        computeShader.SetInt("height", texture.height);
        computeShader.SetVector("ratio", ratio);
        computeShader.SetVector("offset", offset);
        
        computeShader.SetMatrix("CamVP", _cam.previousViewProjectionMatrix);
        computeShader.SetMatrix("worldToCam", _cam.worldToCameraMatrix);
        computeShader.SetMatrix("projection", _cam.projectionMatrix);

        computeShader.SetVector("camPos", _cam.transform.position);
        
        computeShader.SetVector("ray",Camera.main.gameObject.transform.position);
        
        computeShader.SetBuffer(_kernel, "slimePos", _slimePos);
        computeShader.SetBuffer(_prepassKernel, "slimePos", _slimePos);

       // computeShader.Dispatch(_prepassKernel, Mathf.CeilToInt(count),1,1);

        //computeShader.Dispatch(_kernel,  Mathf.CeilToInt(texture.width/32f),Mathf.CeilToInt(texture.height/32f),1);
        
        computeShader.Dispatch(2,texture.width/8,texture.height/8,1);

        Material m;

        /*
        
        Vector3[] result = new Vector3[2];
        _slimePos.GetData(result);
        Debug.Log(result[0] + " | " + result[1]);
        */
    }

    private void OnDisable()
    {
        _slimePos.Dispose();
    }
}
