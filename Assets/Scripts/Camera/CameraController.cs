using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraController : MonoBehaviour
{
    public Vector3 offsetToPlayer = new Vector3(0, 2f, 0);
    public Vector3 offsetToCenter = new Vector3(0, 0, -0.5f);
    public Transform target;
    [HideInInspector]
    public Player player;
    private CameraFollow follow; 
    private void Awake() {
        player = target.GetComponent<Player>();
        if(GameConfig.Instance.GameMode == GameMode.Normal){
            follow = new CameraLockFollow();
        }else{
            follow = new CameraFreeFollow();
        }
    }

    private void Start() {
        follow.Init(this);
    }

    /// <summary>
    /// Update中会有重影
    /// </summary>
    private void LateUpdate() {
        follow.LateUpdate();
    }
}
