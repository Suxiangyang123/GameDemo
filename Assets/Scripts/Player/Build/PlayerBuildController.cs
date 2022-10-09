using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerBuildController
{
    public Player player;
    public Transform transform;
    private Dictionary<BuildingType, BuildHandlerBase> handlerDic = new Dictionary<BuildingType, BuildHandlerBase>();
    private BuildHandlerBase buildHandler;
    private GameObject curBuilding;
    private BuildingData curData;
    private bool isBuilding = false;
    private bool buildAble = false;
    public void Init(Player player)
    {
        this.player = player;
        transform = player.transform;
        LayerMask mask = 1 << LayerMask.NameToLayer("Building");
        
        player.inputController.Player.LeftClick.performed += OnBuild;
        player.inputController.Player.RightClick.performed += OnCancelBuild;
        EventManager.Instance.AddEvent(EventType.StartBuild, OnStartBuild);
        EventManager.Instance.AddEvent(EventType.CancelBuild, OnCancelBuild);
    }
    /// <summary>
    /// 每次进入游戏会重复监听事件,所以需要退出时移除监听
    /// </summary>
    public void ClearEvent(){
        EventManager.Instance.RemoveEvent(EventType.StartBuild, OnStartBuild);
        EventManager.Instance.RemoveEvent(EventType.CancelBuild, OnCancelBuild);
    }

    public void Update()
    {
        if(isBuilding){
            buildHandler.UpdateViewPos();
        }
    }

    public void OnStartBuild(object sender, object[] datas){
        BuildingType type = (BuildingType)datas[0];
        BuildingMaterial material = (BuildingMaterial)datas[1];
        if(BuildManager.Instance.IsTypeExceedLimit(type)){
            return;
        }
        StartBuild(type, material);
    }
    /// <summary>
    /// 开始建造,创建一个随玩家视角和位置移动的虚体(确认后才真正建造并消耗数量)
    /// </summary>
    private void StartBuild(BuildingType type, BuildingMaterial material){
        if(isBuilding){
            CancelBuild();
        }
        curData = new BuildingData(type, material, transform.position, transform.eulerAngles.y);
        curBuilding = BuildManager.Instance.StartBuild(curData);
        SetBuildHandler(type, curBuilding);
        isBuilding = true;
    }

    private void SetBuildHandler(BuildingType type, GameObject building){
        if(handlerDic.ContainsKey(type)){
            buildHandler = handlerDic[type];
        }else{
            switch(type){
                case BuildingType.Spike:
                case BuildingType.Celling:{
                    buildHandler = new CellingBuildHandler();
                    break;
                }
                case BuildingType.Pillar:{
                    buildHandler = new PillarBuildHandler();
                    break;
                }
                case BuildingType.Wall:{
                    buildHandler = new WallBuildHandler();
                    break;
                }
                case BuildingType.Start:
                case BuildingType.End:{
                    buildHandler = new PillarBuildHandler();//TODO:临时用一下
                    break;
                }
                default:{
                    buildHandler = new BuildHandlerBase();
                    break;
                }
            }
            handlerDic.Add(type, buildHandler);
        }
        buildHandler.Init(this, building);
    }


    public void OnBuild(InputAction.CallbackContext ctx){
        if(isBuilding){
            if(buildAble){
                Build();
            }
        }
    }
    private void Build(){
        BuildManager.Instance.AddBuilding();
        //根据是否还有建筑确定能不能继续建造
        buildAble = ItemManager.Instance.Build();//TODO:感觉不太好,以及是否要用事件传递
        curBuilding = null;
        isBuilding = false;

        if(buildAble && !BuildManager.Instance.IsTypeExceedLimit(curData.type)){
            //如果数量大于0,继续建造(数量判断在ItemManager)
            StartBuild(curData.type, curData.material);
        }
    }

    public void OnCancelBuild(InputAction.CallbackContext ctx)
    {
        CancelBuild();
    }
    public void OnCancelBuild(object sender, object[] datas){
        CancelBuild();
    }
    /// <summary>
    /// 取消建造
    /// </summary>
    public void CancelBuild(){
        if(isBuilding){
            BuildManager.Instance.CancelBuild();
            curBuilding = null;
            isBuilding = false;
        }
    }

    /// <summary>
    /// 设置能否建造,能显示绿色,否红色
    /// </summary>
    /// <param name="buildAble"></param>
    public void SetBuildAble(bool buildAble){
        this.buildAble = buildAble;
        if(curBuilding != null){
            MeshRenderer renderer = curBuilding.GetComponent<MeshRenderer>();
            if(renderer != null){
                renderer.material.color = buildAble ? new Color(0, 1, 0, 0.4f) : new Color(1, 0, 0, 0.4f);
            }
        }
    }
    public void OnGUI() {
        buildHandler?.OnGUI();
    }
}