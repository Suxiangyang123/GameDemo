using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class BuildHandlerBase
{
    protected PlayerBuildController buildController;

    protected Vector3 viewOffset = Vector3.zero;
    /// <summary>
    /// 建筑半径,一般为0.5
    /// </summary>
    protected float buildingRadius = 0.5f;
    /// <summary>
    /// 通过区域检测可交互建筑时的检测半径
    /// </summary>
    protected float colCheckRadius = 1.5f;
    /// <summary>
    /// 通过射线检测可交互建筑时的最远检测距离
    /// </summary>
    protected float maxRayCastDistance = 4;
    /// <summary>
    /// 水平方向上随视角变化的最大延伸距离
    /// </summary>
    protected float maxDistance = 2;
    /// <summary>
    /// 垂直方向上随视角变化的最大高度
    /// </summary>
    protected float maxHeight = 4;
    /// <summary>
    /// 当前视角在垂直方向的转动角度,使用需要实时从移动组件获取
    /// </summary>
    protected float viewAngle;
    /// <summary>
    /// 水平方向延伸到最远时对应的视角
    /// </summary>
    protected float midAngle = 0;//或许不需要
    protected readonly float maxViewAngle = GameConfig.Instance.MaxViewAngle;
    protected readonly float minViewAngle = GameConfig.Instance.MinViewAngle;

    /// <summary>
    /// 建筑对应的layerMask
    /// </summary>
    protected LayerMask buildLayerMask;
    protected Vector3 virtualPos;

    /// <summary>
    /// 目前通过键提供可交互建筑列表,value用于在距离判断时添加一定权重来区分优先级(主要为了a离b更近,但在一定距离内优先吸附于c的情况,不过目前没用到..)
    /// </summary>
    protected Dictionary<string,float> interactableBuildingPriority = new Dictionary<string, float>{{"celling", 0}};
    protected GameObject curBuilding;
    protected Transform trans_building;
    protected Transform trans_player;

    public void Init(PlayerBuildController buildController, GameObject curBuilding){
        this.buildController = buildController;
        this.curBuilding = curBuilding;
        trans_building = curBuilding.transform;
        trans_player = buildController.transform;
        buildLayerMask = 1 << LayerMask.NameToLayer("Building");
    }
    protected void InitData(Vector3 viewOffset, float buildingRadius, float colCheckRadius, float maxRayCastDistance, float maxDistance, float maxHeight, float midAngle = 0){
        this.viewOffset = viewOffset;
        this.buildingRadius = buildingRadius;
        this.colCheckRadius = colCheckRadius;
        this.maxRayCastDistance = maxRayCastDistance;
        this.maxDistance = maxDistance;
        this.maxHeight = maxHeight;
        this.midAngle = midAngle;
    }
    public virtual void UpdateViewPos(){
        viewAngle = buildController.player.playerMoveController.GetVerticalViewAngle();//.......大问题
        virtualPos = GetVirtualPos();
        Transform nearstBuilding = FindNearstBuilding();
        trans_building.position = virtualPos + viewOffset;
        trans_building.rotation = trans_player.rotation;
        if(nearstBuilding != null){
            buildController.SetBuildAble(true);
        }else{
            buildController.SetBuildAble(false);
        }
    }

    /// <summary>
    /// 获取根据玩家位置,视角直接计算出的虚拟坐标,以最下方为原点,建筑实际显示加上与中心offset
    /// </summary>
    protected virtual Vector3 GetVirtualPos(){
        //水平方向midAngle时距离最远,最低和最高视角距离为0
        float angleDiff = viewAngle < midAngle ? midAngle - minViewAngle : maxViewAngle - midAngle;
        float ratio = Mathf.Abs(viewAngle - midAngle) / angleDiff;
        float offsetX = maxDistance * (1 - ratio);
        //垂直方向,从最低视角(对应的是maxViewAngle,淦)到最高对应从0到maxHeight,TODO:似乎应该分为两段,前后变化曲线不同
        float offsetY = maxHeight * (maxViewAngle - viewAngle) / (maxViewAngle - minViewAngle);
        return trans_player.position + trans_player.forward * offsetX + trans_player.up * offsetY;
    }

    /// <summary>
    /// 查找附近的建筑
    /// </summary>
    protected Transform FindNearstBuilding(){
        Transform nearstBuilding;
        nearstBuilding = FindBuildingByCollision();
        if(nearstBuilding == null){
            nearstBuilding = FindBuildingByRayCast();
        }
        return nearstBuilding;
    }

    /// <summary>
    /// 以虚拟坐标为中心点,检测一定区域内的可交互建筑
    /// </summary>
    protected Transform FindBuildingByCollision(){
        Transform nearstBuild = null;
        Vector3 virtualPos = GetVirtualPos();
        Collider[] cols = Physics.OverlapSphere(virtualPos, colCheckRadius, buildLayerMask);//通过layerMask指定只检测建筑
        if(cols.Length > 0){
            float minDis = Mathf.Infinity;
            foreach (var col in cols)
            {
                //首先判断是否可交互
                if(interactableBuildingPriority.ContainsKey(col.tag)){
                    float dis = interactableBuildingPriority[col.tag];
                    if(dis < minDis){//先判断一下优先级,通过筛选减少计算
                        dis += (col.transform.position - virtualPos).sqrMagnitude;
                        if(dis <= minDis){
                            nearstBuild = col.transform;
                            minDis = dis;
                        }
                    }  
                }
            }
        }
        return nearstBuild;
    }
    
    /// <summary>
    /// 沿着一定方向射线检测可交互建筑
    /// </summary>
    /// <returns></returns>
    protected Transform FindBuildingByRayCast(){
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Camera.main.rect.center);
        if(Physics.Raycast(ray, out hit, maxRayCastDistance, buildLayerMask)){
            if(interactableBuildingPriority.ContainsKey(hit.transform.tag)){
                return hit.transform;
            }
        }
        return null;
    }
    public void OnGUI() {
        // Debug.DrawLine(trans_player.position + Vector3.up * 2, virtualPos, Color.red, 0.1f);
    }
}

