using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveController
{
    protected Player player;
    protected Transform transform;
    protected Rigidbody rigidbody;
    public virtual void Init(Player player)
    {
        this.player = player;
        transform = player.transform;
        rigidbody = player.GetComponent<Rigidbody>();
        player.inputController.Player.Move.performed += OnMove;
        player.inputController.Player.Move.canceled += OnMove;
        player.inputController.Player.Jump.performed += OnJump;
    }
    public virtual void ResetPos(){

    }
    public virtual void Update()
    {

    }
    public virtual void FixedUpdate(){
        
    }
    protected virtual void OnMove(InputAction.CallbackContext ctx)
    {
        
    }

    protected virtual void OnJump(InputAction.CallbackContext ctx)
    {
        
    }
    public virtual void OnCollisionEnter(Collision other)
    {

    }
    public virtual void OnCollisionStay(Collision other)
    {

    }
    public virtual void OnCollisionExit(Collision other)
    {
        
    }
    public virtual void OnDie(){
        rigidbody.velocity = Vector3.zero;
    }
    /// <summary>
    /// 获取垂直方向上的视线角度
    /// </summary>
    public virtual float GetVerticalViewAngle(){
        return 0;
    }
}
