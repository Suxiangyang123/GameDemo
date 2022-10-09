using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public enum BuildingType{
    Celling,
    Pillar,
    Wall,
    Start,
    End,
    Spike,
}
public enum BuildingMaterial{
    Wood,
    Stone,
    Metal,
}
[System.Serializable]
public struct BuildingData{
    public BuildingType type;
    public BuildingMaterial material;
    public Vector3 pos;
    public float rotation;
    public BuildingData(BuildingType type, BuildingMaterial material, Vector3 pos, float rotation){
        this.type = type;
        this.material = material;
        this.pos = pos;
        this.rotation = rotation;
    }
}
public class BuildManager : Singleton<BuildManager>
{
    private Transform buildingRoot;
    private Dictionary<BuildingType, GameObject> prefabDict = new();
    private Dictionary<BuildingMaterial, Material> materialDict = new();
    /// <summary>
    /// 建造模式中已建造的建筑
    /// </summary>
    private Dictionary<int, BuildingData> buildingDatas = new();
    /// <summary>
    /// 普通模式中读取到需要创建的建筑列表
    /// </summary>
    private List<BuildingData> levelDatas = new();
    private GameObject curBuilding;
    private BuildingData curData;
    //开始和结束比较特殊,一关有且只能由一个
    private GameObject startObj;
    private GameObject endObj;

    #region 初始化
    public void Init(){
        EventManager.Instance.AddEvent(EventType.StartChangeScene, OnSceneStartChange);
        EventManager.Instance.AddEvent(EventType.FinishChangeScene, OnSceneFinishChange);
    }
    public void OnSceneStartChange(object sender, object[] data){
        ClearAll();
    }
    public void OnSceneFinishChange(object sender, object[] data){
        string sceneName = data[0].ToString();
        if(sceneName == "Game"){
            buildingRoot = GameObject.FindGameObjectWithTag("BuildingRoot").transform;
            LoadRes();
            if(GameConfig.Instance.GameMode == GameMode.Normal){
                LoadData();
                InitLevel();
            }
        }
    }
    private void LoadRes(){
        //TODO 所有预制指定一个label,一次性加载后根据tag或者别的区分并放进字典
        foreach(BuildingType type in System.Enum.GetValues(typeof(BuildingType))){
            string label = type.ToString();
            GameObject prefab = LoadManager.Instance.LoadData<GameObject>(label);
            prefabDict.Add(type, prefab);
        }
        foreach(BuildingMaterial materialType in System.Enum.GetValues(typeof(BuildingMaterial))){
            string label = materialType.ToString();
            Material mat = LoadManager.Instance.LoadData<Material>(label);
            materialDict.Add(materialType, mat);
        }
    }
    private void LoadData(){
        string path = Application.persistentDataPath + "/level/level_" + GameConfig.Instance.Level;
        StreamReader reader = new StreamReader(path);
        string data = reader.ReadToEnd();
        if(data != null){
            JsonWrapper<BuildingData> wrapper = JsonUtility.FromJson<JsonWrapper<BuildingData>>(data);
            levelDatas = wrapper.data.ToList();
        }
    }
    private void InitLevel(){
        levelDatas.ForEach((BuildingData data)=>{
            AddBuilding(data);
        });
    }

    private void ClearAll(){
        buildingRoot = null;

        prefabDict.Clear();
        materialDict.Clear();
        buildingDatas.Clear();
        levelDatas.Clear();
    }
    #endregion
    
    public void SaveData(int level){
        if(startObj == null){
            UIManager.Instance.ShowFloatTip("缺少起点");
            return;
        }
        if(endObj == null){
            UIManager.Instance.ShowFloatTip("缺少终点");
            return;
        }
        JsonWrapper<BuildingData> wrapper = new JsonWrapper<BuildingData>();
        wrapper.data = new BuildingData[buildingDatas.Count];
        wrapper.data = buildingDatas.Values.ToArray();
        string dataStr = JsonUtility.ToJson(wrapper);
        string path = Path.Combine(Application.persistentDataPath, "level");//文件夹目录
        //StreamWriter不能创建不存在的文件夹
        if(!Directory.Exists(path)){
            Directory.CreateDirectory(path);
        }
        path += "/level_" + level;//实际目录
        using(StreamWriter writer = new StreamWriter(path, false)){
            writer.Write(dataStr);
        }
        MainManager.Instance.ExitBattle();
    }

    /**
     * @description: 玩家选择建筑后创建一个半透明虚体,选择位置确定建造后才实际创建
     * @param {*}
     * @return {*}
     */    
    public GameObject StartBuild(BuildingData data){
        if(curBuilding != null){
            DestroyBuilding(curBuilding, false);
        }
        curData = data;

        curBuilding = GetBuilding(data.type);
        curBuilding.transform.position = data.pos;
        curBuilding.transform.eulerAngles = new Vector3(0, data.rotation, 0);
        //TODO: 后面需要优化下处理方式
        if(data.type != BuildingType.Start && data.type != BuildingType.End){
            curBuilding.GetComponent<BoxCollider>().enabled = false;
            MeshRenderer renderer = curBuilding.GetComponent<MeshRenderer>();
            if(renderer){
                renderer.material = materialDict[data.material];
                Utils.SetMaterialRenderingMode(renderer.material, Utils.RenderingMode.Transparent);
                renderer.material.color = new Color(0, 1, 0, 0.4f);
            }
        }
        return curBuilding;
    }

    public void CancelBuild(){
        if(curBuilding != null){
            DestroyBuilding(curBuilding, false);    
        }
    }
    /**
     * @description: 玩家确定创建后固定位置,并保存数据
     * @param {*}
     * @return {*}
     */    
    public void AddBuilding(){
        curData.pos = curBuilding.transform.position;
        curData.rotation = curBuilding.transform.eulerAngles.y;
        //修改了renderer变成半透明绿色,实际建造时需要恢复过来
        if(curData.type == BuildingType.Start){
            startObj = curBuilding;
        }else if(curData.type == BuildingType.End){
            endObj = curBuilding;
        }else{
            MeshRenderer renderer = curBuilding.GetComponent<MeshRenderer>();
            if(renderer){
                renderer.material = materialDict[curData.material];;
            }
            curBuilding.GetComponent<BoxCollider>().enabled = true;
        }
        buildingDatas.Add(curBuilding.GetInstanceID(), curData);
        curBuilding = null;
    }
    /**
     * @description: 游戏开始时根据数据创建建筑
     * @param {*}
     * @return {*}
     */    
    private void AddBuilding(BuildingData data){
        GameObject building = GetBuilding(data.type);
        building.transform.position = data.pos;
        building.transform.eulerAngles = new Vector3(0, data.rotation, 0);
        if(data.type == BuildingType.Start){
            startObj = building;
        }else if(data.type == BuildingType.End){
            endObj = building;
        }else{
            MeshRenderer render = building.GetComponent<MeshRenderer>();
            if(render){
                render.material = materialDict[data.material];
            }
        }
    }

    /// <summary>
    /// 某种类型是否超出上限,暂时只用作判断起点和终点是否存在(目前起点和终点只会有一个)
    /// </summary>
    public bool IsTypeExceedLimit(BuildingType type){
        if(type == BuildingType.Start && startObj != null){
            UIManager.Instance.ShowFloatTip("起点只能有一个");
            return true;
        }else if(type == BuildingType.End && endObj != null){
            UIManager.Instance.ShowFloatTip("终点只能有一个");
            return true;
        }
        return false;
    }

    public Vector3 GetPlayerStartPos(){
        return startObj ? startObj.transform.position : Vector3.zero;
    }
    //TODO 对象池处理
    private GameObject GetBuilding(BuildingType type){
        return GameObject.Instantiate(prefabDict[type], buildingRoot);
    }
    public void DestroyBuilding(GameObject obj, bool isBuilt = true){
        if(isBuilt){
            Collider[] colliders =  Physics.OverlapBox(obj.transform.position,Vector3.one,obj.transform.rotation);
            if(colliders.Length > 0){
                //TODO判断是否有搭载在该建筑上的其他建筑,如果有,则一并移除
            }
        }
        GameObject.Destroy(obj);
    }
}
