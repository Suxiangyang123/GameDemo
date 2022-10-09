using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// 正常游戏模式,相机锁定,玩家运动方向受移动键控制
/// </summary>
public class PlayerLockMoveController : PlayerMoveController
{
    //地面//
    private float maxHorizonSpeed = 13.5f;
    private float horizontalAcc = 40;
    // 超出最大速度后的减速度(按键与速度方向同向),反向则是-horizontalAcc
    private float horizonOverSpeedAcc = 16;

    //滞空//
    private float horizontalAccInAir = 26;
    // 滞空时横向超出最大速度后的减速度(按键与速度方向同向),反向则是-horizontalAccInAir
    private float horizonOverSpeedAccInAir = 10.4f;
    private float verticalAcc = -12f;
    /// <summary>
    /// 最大下落速度,向下所以取负
    /// </summary>
    private float minVerticalSpeed = -8;

    // private float jumpStartHorSpeed = 2;
    private float jumpStartVerticalSpeed = 6f;

    /// <summary>
    /// 与玩家碰撞接触点的法线与Vector3.up角度小于该变量,视为玩家着地
    /// </summary>
    private float maxGroundedAngle = 30;

    private Vector3 velocity;
    private bool isGrounded = true;
    private Vector2 force;

    public override void Init(Player player){
        base.Init(player);
        ResetPos();
    }
    public override void ResetPos(){
        transform.position = BuildManager.Instance.GetPlayerStartPos();
        velocity.y = -1;
    }
    public override void Update(){
        //只是更改速度而不是移动,而且也不需要太高精度,在Update中意思一下就行
        SpeedDamp();
        RotateAlongSpeed();
    }
    /**
     * @description: XZ平面模型朝向转到速度方向
     * @param {*}
     * @return {*}
     */
    private void RotateAlongSpeed(){
        if (velocity.x != 0 || velocity.z != 0){
            Quaternion toward = Quaternion.LookRotation(new Vector3(velocity.x, 0, velocity.z), Vector3.up);
            rigidbody.rotation = Quaternion.Lerp(rigidbody.rotation, toward, 0.1f);
        }
    }
    /**
     * @description: 按指定加速度进行变速
     * @param {*}
     * @return {*}
     */
    private void SpeedDamp(){
        float acc = isGrounded ? horizontalAcc : horizontalAccInAir;
        float overSpeedAcc = isGrounded ? horizonOverSpeedAcc : horizonOverSpeedAccInAir;
        ChangeHorizonSpeed(ref velocity.x, force.x, acc, overSpeedAcc);
        ChangeHorizonSpeed(ref velocity.z, force.y, acc, overSpeedAcc);
        if (!isGrounded){
            ChangeVerticalSpeed(ref velocity.y);
        }
        float speedXY = new Vector3(velocity.x, 0, velocity.z).magnitude;
        player.playerAnimController.SetFloat(AnimatorHashes.honSpeed, speedXY);
        rigidbody.velocity = velocity;
    }

    private void ChangeVerticalSpeed(ref float speed){
        if (speed == minVerticalSpeed){
            return;
        }
        speed += verticalAcc * Time.deltaTime;
        speed = speed < minVerticalSpeed ? minVerticalSpeed : speed;
    }
    private void ChangeHorizonSpeed(ref float speed, float force, float acc, float overAcc){
        if(speed == 0){
            speed += force * acc * Time.deltaTime;
        }
        else{
            if(force == 0){
                //减速到0
                int dir = speed > 0 ? -1 : 1;
                float change = dir * acc * Time.deltaTime;
                speed = Mathf.Abs(speed) > Mathf.Abs(change) ? speed + change : 0;
            }else{
                if(force < 0 == speed < 0){
                    //力和速度同向
                    if(Mathf.Abs(speed) == maxHorizonSpeed){
                        //同向,且速度等于最大值,不处理
                        return;
                    }
                    if(Mathf.Abs(speed) > maxHorizonSpeed){
                        //同向,且速度超过最大值 => 减速到最大值
                        int dir = speed > 0 ? -1 : 1;
                        float change = dir * overAcc * Time.deltaTime;
                        speed = Mathf.Abs(speed) - maxHorizonSpeed > Mathf.Abs(change) ? speed + change : -maxHorizonSpeed * dir;
                        return;
                    }
                }
                //其他情况正常加减速
                speed += force * acc * Time.deltaTime;
            }
        }
    }

    protected override void OnMove(InputAction.CallbackContext ctx){
        if(ctx.performed){
            force = ctx.ReadValue<Vector2>();
        }
        if(ctx.canceled){
            force = Vector2.zero;
        }
    }

    #region 跳跃与着地相关
    protected override void OnJump(InputAction.CallbackContext ctx){
        if(isGrounded){
            //垂直方向和水平方向处理不同,垂直方向起跳时给初始速度 下落通过施加向下的力实现
            velocity.y = jumpStartVerticalSpeed;
            SetIsGround(false, true);
        }
    }
    public override void OnCollisionEnter(Collision other){
        //判断是否是下落
        if(!isGrounded){
            if(CheckIsGroundedByCollision(other)){
                SetIsGround(true, true);
                velocity = new Vector3(0, -1, 0);
                // velocity.y = -1f;//留一点点保持一个下落的趋势
            }
        }
    }
    /// <summary>
    /// 目前平地走抬脚就会触发..
    /// </summary>
    public override void OnCollisionExit(Collision other){
        //用于检测从平台上走出去的情况,只做下落处理,不修改动画
        if(isGrounded){
            SetIsGround(CheckIsGroundedByRaycast(), false);
        }
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
        //刚触发时实际上并没有离开,需要加个速度偏移(数值随便取的,差不多就行)
        Vector3 offset = rigidbody.velocity.normalized * 0.1f;
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
        if(setAnim){
            player.playerAnimController.SetBool(AnimatorHashes.isGrounded, isGrounded);
        }
    }
    public override void OnDie(){
        velocity = new Vector3(0, -1, 0);
        rigidbody.velocity = Vector3.zero;
    }
    #endregion
}
