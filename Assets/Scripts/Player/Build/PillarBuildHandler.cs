using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarBuildHandler : BuildHandlerBase
{
    private float criticalAngle = -40;

    public PillarBuildHandler(){
        InitData(new Vector3(0, 0.5f ,0), 0.5f, 1.5f, 2, 2, 4);
        interactableBuildingPriority = new Dictionary<string, float>(){
        {"Celling", 0},{"Spike", 0},{"Pillar", 0}};
    }

    private bool isGrounded = false;
    public override void UpdateViewPos(){
        viewAngle = buildController.player.playerMoveController.GetVerticalViewAngle();
        virtualPos = GetVirtualPos();
        Transform nearstBuilding = FindNearstBuilding();
        if(nearstBuilding != null){
            SetViewPos(nearstBuilding);
            buildController.SetBuildAble(true);
        }else{
            SetViewPos();
            if(isGrounded){
                buildController.SetBuildAble(true);
            }else{
                buildController.SetBuildAble(false);
            }
        }
    }

    /// <summary>
    /// 根据玩家位置和视角计算出虚拟坐标(周围没有可交互建筑,即没处理吸附等操作时的坐标)
    /// </summary>
    protected override Vector3 GetVirtualPos(){
        isGrounded = false;
        //柱子变化曲线:前面保持接地距离跟随角度增长到最大值(midAngle),之后保持不变到临界角度(criticalAngle),超过临界值根据普通算法突变到一定高度
        if(viewAngle > criticalAngle){
            //根据角度确定玩家同一水平线的对应点,然后射线检测能否接地
            float distance = maxDistance;
            if(viewAngle > midAngle){
                distance = maxDistance * (maxViewAngle - viewAngle) / (maxViewAngle - midAngle);
            }
            Vector3 pos = trans_player.position + trans_player.forward * distance;
            RaycastHit hit;
            if(Physics.Raycast(pos, Vector3.up, out hit, maxRayCastDistance / 2, 1 << LayerMask.NameToLayer("Ground"))){
                isGrounded = true;
                return hit.point;
            }else if(Physics.Raycast(pos, -Vector3.up, out hit, maxRayCastDistance / 2, 1 << LayerMask.NameToLayer("Ground"))){
                isGrounded = true;
                return hit.point;
            }
            return pos;
        }
        return base.GetVirtualPos();
    }

    private void SetViewPos(Transform nearstBuilding){
        switch(nearstBuilding.tag){
            case "Pillar":{
                //相邻柱子间隔一个单位,同样判断该柱子位于前后左右哪个方位
                Vector3 dir = virtualPos - nearstBuilding.position;
                dir.y = 0;//忽略y轴
                Vector3 offset;
                dir = Quaternion.AngleAxis(-nearstBuilding.eulerAngles.y, Vector3.up) * dir;
                if(Mathf.Abs(dir.z) >= Mathf.Abs(dir.x)){
                    //贴近z轴
                    offset = dir.z >= 0 ? nearstBuilding.forward : -1 * nearstBuilding.forward;
                }else{
                    offset = dir.x >= 0 ? nearstBuilding.right : -1 * nearstBuilding.right;
                }
                trans_building.position = nearstBuilding.position + offset;
                break;
            }
            case "Spike":
            case "Celling":{
                int flag = virtualPos.y > nearstBuilding.position.y ? 1 : -1;
                trans_building.position =  nearstBuilding.position - Vector3.up * buildingRadius * flag;
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
