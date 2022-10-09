using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class CellingBuildHandler : BuildHandlerBase
{
    public CellingBuildHandler(){
        //可以读表,暂时没必要
        InitData(new Vector3(0, 0 ,0), 0.5f, 1.5f, 4, 2, 4);
        interactableBuildingPriority = new Dictionary<string, float>(){
        {"Celling", 0},{"Spike", 0},{"Pillar", 0},{"Wall", 0}};
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
        Vector3 offset;
        switch(nearstBuilding.tag){
            case "Spike":
            case "Celling":{ 
                //判断当前位置与天花板前后左右哪个方位最贴近
                dir = Quaternion.AngleAxis(-nearstBuilding.eulerAngles.y, Vector3.up) * dir;
                if(Mathf.Abs(dir.z) >= Mathf.Abs(dir.x)){
                    //贴近z轴
                    offset = dir.z >= 0 ? nearstBuilding.forward : -1 * nearstBuilding.forward;
                }else{
                    offset = dir.x >= 0 ? nearstBuilding.right : -1 * nearstBuilding.right;
                }
                trans_building.position =  nearstBuilding.position + offset;
                break;
            }
            case "Wall":{
                //墙只需判断前后两个方向
                float dot = Vector3.Dot(nearstBuilding.forward, dir);
                offset = dot >= 0 ? nearstBuilding.forward : -1 * nearstBuilding.forward;
                offset += virtualPos.y > nearstBuilding.position.y ? Vector3.up : -Vector2.up;
                trans_building.position =  nearstBuilding.position + offset * buildingRadius;
                break;
            }
            case "Pillar":{
                //天花板直接放在柱子上方
                trans_building.position =  nearstBuilding.position + Vector3.up * buildingRadius;
                break;
            }
            default:{
                SetViewPos();
                break;
            }
        }
        trans_building.rotation = nearstBuilding.rotation;
    }
    private void SetViewPos(){
        trans_building.position = virtualPos + viewOffset;
        trans_building.rotation = trans_player.rotation;
    }
}
