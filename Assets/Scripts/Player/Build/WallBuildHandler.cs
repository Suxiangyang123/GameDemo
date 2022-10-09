using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class WallBuildHandler : BuildHandlerBase
{
    private float criticalAngle = 40;
    public WallBuildHandler(){
        InitData(new Vector3(0, 0.5f ,0), 0.5f, 1.5f, 4, 2, 4);
        interactableBuildingPriority = new Dictionary<string, float>(){
        {"Celling", 0},{"Spike", 0},{"Wall", 0}};
    }

    /// <summary>
    /// 根据玩家位置和视角计算出虚拟坐标(周围没有可交互建筑,即没处理吸附等操作时的坐标)
    /// </summary>
    protected override Vector3 GetVirtualPos(){
        //水平方向midAngle时距离最远,最低和最高视角距离为0
        float angleDiff = viewAngle < midAngle ? midAngle - minViewAngle : maxViewAngle - midAngle;
        float ratio = Mathf.Abs(viewAngle - midAngle) / angleDiff;
        float offsetX = maxDistance * (1 - ratio);
        float offsetY = 0;
        if(viewAngle < criticalAngle){
            offsetY = maxHeight * (criticalAngle - viewAngle) / (criticalAngle - minViewAngle);
        }
        return trans_player.position + trans_player.forward * offsetX + trans_player.up * offsetY;
    }

    public override void UpdateViewPos(){
        viewAngle = buildController.player.playerMoveController.GetVerticalViewAngle();
        virtualPos = GetVirtualPos();
        Transform nearstBuilding = FindNearstBuilding();
        if(nearstBuilding != null){
            SetViewPos(nearstBuilding);
            buildController.SetBuildAble(true);
        }else{
            SetViewPos();
            buildController.SetBuildAble(false);
        }
    }
    
    private void SetViewPos(Transform nearstBuilding){
        Vector3 dir = virtualPos - nearstBuilding.position;
        dir.y = 0;//忽略y轴
        switch(nearstBuilding.tag){
            case "Spike":
            case "Celling":{ 
                //判断当前位置与天花板前后左右哪个方位最贴近
                Vector3 offset;
                Vector3 angles = nearstBuilding.eulerAngles;
                float angleOffset;
                //将世界坐标中两者的方位向量转换到天花板的坐标系中
                dir = Quaternion.AngleAxis(-nearstBuilding.eulerAngles.y, Vector3.up) * dir;
                if(Mathf.Abs(dir.z) >= Mathf.Abs(dir.x)){//比较方位向量的分量
                    //贴近z轴
                    if(dir.z >= 0){//z轴正向
                        offset = nearstBuilding.forward;
                        angleOffset = 0;
                    }else{
                        offset = -1 * nearstBuilding.forward;
                        angleOffset = 180;
                    }
                }else{
                    if(dir.x >= 0){
                        offset = nearstBuilding.right;
                        angleOffset = 90;
                    }else{
                        offset = -1 * nearstBuilding.right;
                        angleOffset = -90;
                    }
                }
                offset += virtualPos.y > nearstBuilding.position.y ? Vector3.up : -Vector2.up;
                trans_building.position =  nearstBuilding.position + offset * buildingRadius;
                angles.y += angleOffset;
                trans_building.eulerAngles = angles;
                break;
            }
            case "Wall":{
                //墙只需判断前后两个方向
                trans_building.position =  nearstBuilding.position + viewOffset * 2;
                trans_building.rotation = nearstBuilding.rotation;
                break;
            }
            default:{
                SetViewPos();
                break;
            }
        }
    }
    private void SetViewPos(){
        trans_building.position = virtualPos + viewOffset;
        trans_building.rotation = trans_player.rotation;
    }
}
