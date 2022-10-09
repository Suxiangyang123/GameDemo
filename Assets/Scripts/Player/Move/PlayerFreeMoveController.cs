using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// 建造模式中,相机不锁定方向跟随玩家,前进方向受鼠标移动控制
/// </summary>
public class PlayerFreeMoveController : PlayerMoveController
{
    private float forwardSpeed = 10;
    private float rightSpeed = 5;
    private float curAccelarate = 1;
    private float accelerateRatio = 1.5f;
    /// <summary>
    /// 感觉跳跃时在空中的移动距离有点远,空中水平方向上的速度乘以一个系数
    /// </summary>
    private float speedDampInAir = 0.6f;
    //纵向
    private float jumpStartVerticalSpeed = 12;//8
    private float verticalAcc = -25;//18
    /// <summary>
    /// 与玩家碰撞接触点的法线与Vector3.up角度小于该变量,视为玩家着地
    /// </summary>
    private float maxGroundedAngle = 30;
    //角度
    private float rotateSpeed = 2;
    private float verticalViewAngle = 0;
    //纵向转动的角度限制
    private float maxAngle;
    private float minAngle;

    /// <summary>
    /// 读取输入获得的移动方向,取值为1,-1,0
    /// </summary>
    private Vector2 dir;
    private Vector3 velocity;
    private bool isGrounded = true;
    public override void Init(Player player){
        base.Init(player);
        player.inputController.Player.Accelerate.performed += OnAccelerate;
        player.inputController.Player.Accelerate.canceled += OnAccelerate;
        player.inputController.Player.Rotate.performed += OnRotate;

        maxAngle = GameConfig.Instance.MaxViewAngle;
        minAngle = GameConfig.Instance.MinViewAngle;

        //TODO
        verticalAcc *= Time.fixedDeltaTime;
    }

    public override void FixedUpdate()
    {
        //不使用gravity,因为莫名原因会导致水平方向逐渐减速
        if(!isGrounded){
            velocity.y += verticalAcc;//verticalAcc预先乘过fixedDeltaTime
            SetRigidBodyVelocity();
        }
    }

    protected override void OnMove(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            //velocity对应的是当前朝向下的速度,实际移动刚体的速度转化到世界坐标系
            dir = ctx.ReadValue<Vector2>();
            velocity.x = dir.x * rightSpeed * curAccelarate;
            velocity.z = dir.y * forwardSpeed * curAccelarate;
            SetRigidBodyVelocity();
            SetMoveAnimParam(curAccelarate);
        }
        if (ctx.canceled)
        {
            dir = Vector2.zero;
            velocity.x = 0;
            velocity.z = 0;
            SetRigidBodyVelocity();
            SetMoveAnimParam();
        }
    }

    /// <summary>
    /// 加速
    /// </summary>
    /// <param name="ctx"></param>
    private void OnAccelerate(InputAction.CallbackContext ctx){
        if (ctx.performed)
        {
            //始终从初始值开始,避免多次加速
            curAccelarate = accelerateRatio;
            velocity.x = dir.x * rightSpeed * curAccelarate;
            velocity.z = dir.y * forwardSpeed * curAccelarate;
            SetRigidBodyVelocity();
            SetMoveAnimParam(curAccelarate);
        }
        if (ctx.canceled)
        {
            curAccelarate = 1;
            velocity.x = dir.x * rightSpeed;
            velocity.z = dir.y * forwardSpeed;
            SetRigidBodyVelocity();
            SetMoveAnimParam();
        }
    }

    private void OnRotate(InputAction.CallbackContext ctx){
        Vector2 angles = ctx.ReadValue<Vector2>();
        if(Mathf.Abs(angles.x) > 0.1f){
            rigidbody.rotation *= Quaternion.AngleAxis(angles.x * rotateSpeed, Vector3.up);
            SetRigidBodyVelocity();
        }
        if(Mathf.Abs(angles.y) > 0.1f){//低于一定幅度不移动
            // if(angles.x > 180){//0以下会自动变成359..(之前赋值给移动组件后再取值出现的问题)
            //     angles.x -= 360;
            // }
            verticalViewAngle -= rotateSpeed * angles.y;
            verticalViewAngle = Mathf.Clamp(verticalViewAngle, minAngle, maxAngle);//限制范围
        }
    }
    public override float GetVerticalViewAngle()
    {
        return verticalViewAngle;
    }

    /// <summary>
    /// 设置水平方向的移动动画参数,根据参数确定播放走还是跑
    /// </summary>
    /// <param name="ratio">普通为1,加速为1.5</param>
    private void SetMoveAnimParam(float ratio = 1){
        player.playerAnimController.SetFloat(AnimatorHashes.rightSpeed, dir.x * ratio);
        player.playerAnimController.SetFloat(AnimatorHashes.forwardSpeed, dir.y * ratio);
    }

    private void SetRigidBodyVelocity(){
        if(isGrounded){
            rigidbody.velocity = rigidbody.rotation * velocity;
        }else{
            Vector3 fixedVelocity= new Vector3(velocity.x * speedDampInAir, velocity.y, velocity.z * speedDampInAir);
            rigidbody.velocity = rigidbody.rotation * fixedVelocity;
        }

    }

    #region 跳跃与着地相关
    protected override void OnJump(InputAction.CallbackContext ctx)
    {
        if (isGrounded)
        {
            //垂直方向和水平方向处理不同,垂直方向起跳时给初始速度 下落通过施加向下的力实现
            velocity.y = jumpStartVerticalSpeed;
            SetRigidBodyVelocity();
            SetIsGround(false, true);
        }
    }
    public override void OnCollisionEnter(Collision other)
    {
        //判断是否是下落
        if(!isGrounded){
            if(CheckIsGroundedByCollision(other)){
                SetIsGround(true, true);  
                velocity.y = -1f;//留一点点保持一个下落的趋势
                SetRigidBodyVelocity();
            }
        }
    }
    /// <summary>
    /// 目前平地走抬脚就会触发..
    /// </summary>
    public override void OnCollisionExit(Collision other)
    {
        //用于检测从平台上走出去的情况,只做下落处理,不修改动画
        SetIsGround(CheckIsGroundedByRaycast(), false);
    }
    private bool CheckIsGroundedByCollision(Collision other){
        foreach(ContactPoint point in other.contacts){
            if(Vector3.Angle(point.normal, Vector3.up) < maxGroundedAngle){
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 用于平台上走出去的时候能检测到悬空(Exit触发的时机有点奇怪)
    /// </summary>
    private bool CheckIsGroundedByRaycast(){
        //刚触发时实际上并没有离开,需要加个速度偏移(随便取的,差不多就行)
        Vector3 offset = Vector3.zero;//rigidbody.velocity.normalized * 0.3f;
        //坐标可能会在平面以下浮动,抬高一点开始检测
        offset.y = 0.5f;
        if(Physics.Raycast(transform.position + offset, Vector3.down, 0.8f)){
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// 设置属性的同时调整动画
    /// </summary>
    public void SetIsGround(bool isGrounded, bool setAnim){
        this.isGrounded = isGrounded;
        SetRigidBodyVelocity();
        if(setAnim){
            player.playerAnimController.SetBool(AnimatorHashes.isGrounded, isGrounded);
        }
    }
    #endregion
}
