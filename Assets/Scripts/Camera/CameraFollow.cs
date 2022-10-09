using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow
{
    public CameraController controller;
    public Transform target;
    public Transform transform;
    public virtual void Init(CameraController controller){
        this.controller = controller;
        target = controller.target;
        transform = controller.transform;
    }
    public virtual void LateUpdate(){

    }
}
