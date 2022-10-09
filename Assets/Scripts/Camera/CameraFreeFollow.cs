using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFreeFollow : CameraFollow
{
    /// <summary>
    /// 玩家脚底起点加上向上的偏移作为中心,相机相对中心旋转
    /// </summary>
    private Vector3 offsetToPlayer = new Vector3(0, 2f, 0);
    private Vector3 offsetToCenter = new Vector3(0, 0, -1.5f);

    public override void Init(CameraController controller)
    {
        base.Init(controller);
    }
    public override void LateUpdate()
    {
        Follow();
    }
    private void Follow(){
        Vector3 angles = target.eulerAngles;//取玩家y轴的转动角度
        angles.x = controller.player.playerMoveController.GetVerticalViewAngle();
        transform.eulerAngles = angles;
        transform.position = target.position + offsetToPlayer +  Quaternion.Euler(transform.eulerAngles) * offsetToCenter;
    }
}
