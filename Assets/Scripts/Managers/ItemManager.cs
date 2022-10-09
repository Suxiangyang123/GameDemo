using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
public enum ItemType{
    Building,
    Food,
}
/// <summary>
/// 用于本地存储数据,只保存最基本的信息
/// </summary>
[System.Serializable]
public class ItemBaseData{
    public int itemId;
    public int num;
    public ItemBaseData(int itemId, int num){
        this.itemId = itemId;
        this.num = num;
    }
    public ItemBaseData(ItemData data){
        itemId = data.itemId;
        num = data.num;
    }
}
/// <summary>
/// 用于ItemMgr中进行管理,可额外存储一些其他信息,例如唯一标识id以及一些临时的状态
/// </summary>
public class ItemData{
    /// <summary>
    /// 对于每个data唯一
    /// </summary>
    public int key;
    public int itemId;
    public int num;
    public ItemData(int itemId){
        key = -1;
        this.itemId = itemId;
        this.num = 0;
    }
    public ItemData(int key, int itemId, int num){
        this.key = key;
        this.itemId = itemId;
        this.num = num;
    }
    public ItemData(int key, ItemBaseData data){
        this.key = key;
        this.itemId = data.itemId;
        this.num = data.num;
    }
}
/// <summary>
/// 存储每个ItemId通用的信息
/// </summary>
[System.Serializable]
public struct ItemInfo{
    public int id;
    public int itemId;
    public string detail;
    /// <summary>
    /// 是否可创造,可建造物品会在印痕页面展示
    /// </summary>
    public bool creatable;
    public int limit;
    public int weight;
    public int spoilCd;
    public string img;
    public int mainType;
    /// <summary>
    /// 建筑:类型*材质类型
    /// </summary>
    public string extra;
    // public string atlasName;
}
public class ItemManager: Singleton<ItemManager>
{
    private Dictionary<int, ItemInfo> itemInfoDict = new();
    /// <summary>
    /// 一个单增用于保证唯一性的key,与数据(而不是cell)一一对应,cell通过持有数据间接对应key(因为有可能直接消耗物品而并没有显示UI)
    /// </summary>
    private int _key = 0;
    private Dictionary<int, ItemData> itemDataDict = new();
    /// <summary>
    /// key为itemId,value为数据对应的唯一key,方便根据itemid查找数据
    /// </summary>
    private Dictionary<int, List<int>> itemIdIndexDict = new();
    /*
    TODO暂时两种思路,空data不具有唯一key,快捷栏非空的数据才具有key并统一进data,cell,indexDict管理,但为了实现优先消耗快捷栏物品,
    indexDict需要对快捷栏数据优先排序,进而需要data添加标记属性,且每次数据类型变化(null或者数量为0/大于0)都需要专门处理;
    
    目前实现的是快捷栏固定具有唯一key(空data转为itemId为0的data,进而可以具有key),在data,cell字典中存储,不在index字典中存储,
    遍历物品需要结合快捷栏和index字典(会导致data判空需要多判断itemId是否为0)
    */
    /// <summary>
    /// 快捷栏物品,可以为空,或者数量为0,并且优先从快捷栏消耗/添加物品;
    /// </summary>
    private ItemData[] mainCellDataArray;
    //目前有个隐藏问题,itemCellDict不存储印痕(印痕本身不代表数据,不能与key一一对应) 
    private Dictionary<int, ItemCell> itemCellDict = new();
    /// <summary>
    /// 可创造的物体记录一个数组,会在ItemPanel的Crafting界面展示
    /// </summary>
    private int[] creatableItemArray;

    private GameObject pre_itemCell;
    private SpriteAtlas atlas_item;
    
    private List<ItemCell> itemCellPool = new();

    #region 属性
    public Dictionary<int, ItemInfo> ItemInfoDict { get => itemInfoDict;}
    public Dictionary<int, ItemData> ItemDataDict { get => itemDataDict;}
    public int[] CreatableItemArray { get => creatableItemArray;}
    private int Key { get => _key++; }
    public ItemData[] MainCellDataArray { get => mainCellDataArray;}
    public Dictionary<int, List<int>> ItemIdIndexDict { get => itemIdIndexDict; set => itemIdIndexDict = value; }
    #endregion

    #region 初始化
    public void Init(){
        EventManager.Instance.AddEvent(EventType.FinishChangeScene, OnSceneChanged);
        EventManager.Instance.AddEvent(EventType.StartBuild, OnStartBuild);
    }
    public void OnSceneChanged(object sender, object[] data){
        string sceneName = data[0].ToString();
        if(sceneName == "Game"){
            Load();
        }else{
            //进入主界面时有可能时初始进入,只有关卡中退出才需要保存
            if(!GameConfig.Instance.IsFirstInHome){
                SaveData();
                Clear();
            }
        }
    }
    private void Load(){
        pre_itemCell = LoadManager.Instance.LoadData<GameObject>("itemCellPrefab", AddressPaths.itemCellPrefab);
        atlas_item = LoadManager.Instance.LoadData<SpriteAtlas>("itemAtlas", AddressPaths.itemAtlas);
        LoadItemInfo();
        LoadMainCellItemData();
        LoadUserItemData();
    }

    /// <summary>
    /// 加载Item本身的信息(类型,功能,堆叠上限等)
    /// </summary>
    private void LoadItemInfo(){
        string name = "item";
        List<int> creatableList = new();
        TextAsset textAsset = LoadManager.Instance.LoadData<TextAsset>(name, AddressPaths.itemJson);
        JsonWrapper<ItemInfo> wrapper = JsonUtility.FromJson<JsonWrapper<ItemInfo>>(textAsset.text);
        ItemInfo[] itemInfoList = wrapper.data;
        foreach(ItemInfo info in itemInfoList){
            itemInfoDict.Add(info.itemId, info);
            if(info.creatable){ 
                creatableList.Add(info.itemId);
            }
        }
        creatableItemArray = creatableList.ToArray();
    }
    
    /// <summary>
    /// 加载快捷栏数据
    /// </summary>
    private void LoadMainCellItemData(){
        var list = GameData.Instance.GetMainUICellData();
        mainCellDataArray = new ItemData[list.Length];
        for(int i = 0; i < list.Length; i++){
            ItemBaseData baseData = list[i];
            //为了将快捷栏统一管理, data为null时用itemid为0时代替,从而拥有唯一key
            ItemData data;
            if(baseData == null || baseData.itemId < 0){
                data = new ItemData(Key, 0, 0);
            }else{
                data = new ItemData(Key, baseData.itemId, baseData.num);
            }
            mainCellDataArray[i] = data;
            ItemDataDict.Add(data.key, data);
        }
    }
    public void SetMainCellItemData(int index, ItemData data){
        mainCellDataArray[index] = data;
    }
    /// <summary>
    /// 加载玩家已拥有的Item数据
    /// </summary>
    private void LoadUserItemData(){
        var array = GameData.Instance.GetUserItemData();
        for(int i = 0; i < array.Length; i++){
            AddItemData(array[i]);
        }
    }

    /// <summary>
    /// 用于从本地存储中读取初始化
    /// </summary>
    private void AddItemData(ItemBaseData baseData){
        ItemData data = new ItemData(Key, baseData);
        ItemDataDict.Add(data.key, data);
        AddItemIdIndex(data);
    }
    /// <summary>
    /// 添加新组物品时调用,并且保存到本地(TODO:目前还没保存)
    /// </summary>
    public void AddItemData(int itemId, int num){
        ItemData data = new ItemData(Key, itemId, num);
        ItemDataDict.Add(data.key, data);
        int index = AddItemIdIndex(data);
        index += GetNewItemIndex(itemId);
        EventManager.Instance.Invoke(EventType.AddItem, this, data.key, index);
    }
    private void DeleteItemData(int key){
        int itemId = itemDataDict[key].itemId;
        ItemDataDict.Remove(key);
        DeleteItemIdIndex(itemId, key);
        //TODO 同步到本地
    }
    /// <summary>
    /// 添加itemId的数据索引
    /// </summary>
    private int AddItemIdIndex(ItemData itemData){
        if(itemIdIndexDict.TryGetValue(itemData.itemId, out List<int> list)){
            //TODO:目前不会出现重复添加key,暂时不用检查是否重复
            //按照数量进行排序
            for(int i = 0; i < list.Count; i++){
                ItemData data = itemDataDict[list[i]];
                if(itemData.num >= data.num){
                    list.Insert(i, itemData.key);
                    return i;
                }
            }
            //前面没有返回说明是最小的,直接添加到最后
            list.Add(itemData.key);
        }else{
            list = new List<int>(){itemData.key};
            itemIdIndexDict.Add(itemData.itemId, list);
            //按照信息表中的id进行排序
            itemIdIndexDict.OrderBy((data) => itemInfoDict[data.Key].id)
                .ToDictionary(data => data.Key,data => data.Value);
        }
        return list.Count - 1;
    }
    /// <summary>
    /// 移除itemId的数据索引
    /// </summary>
    private void DeleteItemIdIndex(int itemId, int key){
        if(itemIdIndexDict.TryGetValue(itemId, out List<int> list)){
            list.Remove(key);
            if(list.Count == 0){
                itemIdIndexDict.Remove(itemId);
            }
        }else{
            Debug.LogWarning(string.Format("不存在itemId{0}对应的key{1}",itemId, key));
        }
    }
    /// <summary>
    /// 用于添加物品时计算在列表中的显示顺序,TODO或许可以优化下思路
    /// </summary>
    private int GetNewItemIndex(int itemId){
        int count = 0;
        foreach(var data in itemIdIndexDict){
            if(data.Key == itemId){
                return count;
            }
            count += data.Value.Count;
        }
        // for(int i = 0; i < itemIdIndexDict.Count; i++){
        //     var data = itemIdIndexDict.ElementAt(i);
        // }
        return count;
    }


    private void SaveData(){
        ItemBaseData[] mainCellDatas = new ItemBaseData[mainCellDataArray.Length];
        for(int i = 0; i < mainCellDataArray.Length; i++){
            mainCellDatas[i] = new ItemBaseData(mainCellDataArray[i]);
        }
        GameData.Instance.SaveMainUICellData(mainCellDatas);

        //itemDataDict包含快捷栏的,所以要通过itemIdIndexDict获取(TODO:怪怪的...)
        List<ItemBaseData> userItemDatas = new();
        foreach(var list in itemIdIndexDict.Values){
            for(int i = 0; i < list.Count; i++){
                userItemDatas.Add(new ItemBaseData(itemDataDict[list[i]]));
            }
        }
        GameData.Instance.SaveUserItemData(userItemDatas);
    }
    private void Clear(){
        pre_itemCell = null;
        atlas_item = null;

        _key = 0;
        itemDataDict.Clear();
        itemInfoDict.Clear();
        itemIdIndexDict.Clear();
        creatableItemArray = null;
        ClearObjectPool();
    }
    #endregion

    #region Cell对象池管理
    public ItemCell GetItemCell(int key = -1){
        ItemCell cell;
        if(itemCellPool.Count > 0){
            cell = itemCellPool[0];
            itemCellPool.RemoveAt(0);
        }else{
            GameObject go = GameObject.Instantiate(pre_itemCell);
            cell = go.GetComponent<ItemCell>();
            cell.SetAtlas(atlas_item);
        }
        cell.SetDataChangeCall(null);
        if(key >= 0){//排除印痕界面的印痕,因为不代表实际拥有的物品,也不需要唯一性
            itemCellDict[key] = cell;
        }
        return cell;
    }

    public void DeleteItemCell(ItemCell cell){
        cell.gameObject.SetActive(false);
        cell.transform.SetParent(UIManager.Instance.UiTopRoot, false);//TODO
        ItemData data = cell.GetData();
        if(data.key >= 0){
            itemCellDict.Remove(cell.GetData().key);
        }
        itemCellPool.Add(cell);
    }
    
    /// <summary>
    /// 用于在ItemCell更新数据时,把CellDict中的key同步为新data的key
    /// </summary>
    public void UpdateDataCell(ItemCell cell, ItemData data){
        if(data?.key >= 0){
            itemCellDict[data.key] = cell;
        }
    }
    private void ClearObjectPool(){
        //切场景时会自动销毁,手动调用会报空
        // foreach(ItemCell cell in itemCellPool){
        //     GameObject.Destroy(cell.gameObject);
        // }
        itemCellPool.Clear();
    }
    #endregion

    #region 增删Item
    /// <summary>
    /// 添加物品
    /// </summary>
    /// <param name="key">key大于0代表优先将该组添加至堆叠上限,否则添加按顺序遍历查询</param>
    public void AddItem(int itemId, int num, int key = -1){
        ItemInfo info = itemInfoDict[itemId];
        if(key >= 0){
            //优先将key对应的数据数量加至堆叠上限
            ItemData data = itemDataDict[key];
            AddItemNum(data, ref num);
            if(num > 0){
                AddItem(itemId, num);
            }
        }else{
            //以快捷栏优先,查找未到堆叠上限的组
            List<ItemData> datas = FindDatasWithItemId(itemId);
            if(datas.Count > 0){
                for(int i = 0; i < datas.Count; i++){
                    ItemData data = datas[i];
                    if(data.num >= info.limit){
                        continue;
                    }                                 
                    AddItemNum(data, ref num);
                    if(num < 0){
                        break;  
                    }
                }
            }
            //直接添加新组物品
            while(num > 0){
                int add = num > info.limit ? info.limit : num;
                AddItemData(itemId, add);
                num -= add;
            }
        }
    }
    private void AddItemNum(ItemData data, ref int num){
        ItemInfo info = itemInfoDict[data.itemId];
        if(num + data.num <= info.limit){
            data.num += num;
            num = 0;
        }else{
            data.num = info.limit;
            num -= info.limit - data.num;
        }
        if(itemCellDict.TryGetValue(data.key, out ItemCell cell)){
            cell.InvokeDataChangeCall(data);//TODO
        }
    }
    /// <summary>
    /// 消耗物品
    /// </summary>
    public void UseItem(int itemId, int num = 1, int key = -1){
        ItemData data;
        if(key < 0){
            data = FindDataWithItemId(itemId);
        }else{
            data = ItemDataDict[key];
        }
        if(data.num > num){
            data.num -= num;
        }else{
            num -= data.num;
            data.num = 0;
            DeleteItemData(data.key);
            if(num > 0){
                UseItem(itemId, num);
            }
        }
        //TODO东西使用后的效果触发逻辑(..是否放在这处理?)
        itemCellDict[data.key].InvokeDataChangeCall(data);
    }
    
    //正常流程应该是cell使用了一个物体,修改cell对应的数据
    //对于建造是cell通知要使用某物体建造,在实际确认建造后才扣除数量
    private ItemData curBuildData;
    public void OnStartBuild(object sender, object[] datas){
        curBuildData = (ItemData)datas[2];
    }
    /// <summary>
    /// 实际建造,扣除数量后查找同样Id且有数量的ItemData,没有则通知停止建造
    /// </summary>
    public bool Build(){
        UseItem(curBuildData.itemId, 1, curBuildData.key);
        bool buildAble = true;
        if(curBuildData.num <= 0){
            curBuildData = FindDataWithItemId(curBuildData.itemId);
        }
        if(curBuildData == null){
            //通知停止建造
            buildAble = false;
        }
        return buildAble;
    }

    /// <summary>
    /// 查找对应ItemId的单个数据,优先快捷栏,不考虑是否达到堆叠上限,目前单个数据默认用于消耗
    /// </summary>
    /// <param name="isAdd">对查找到的数据是添加还是消耗数量</param>
    private ItemData FindDataWithItemId(int itemId, bool isAdd = false){
        foreach(ItemData data in mainCellDataArray){
            //快捷栏的物品有可能数量为0
            int minNum = isAdd ? 0 : 1;
            if(data.itemId == itemId && data.num >= minNum){
                return data;
            }
        }
        if(itemIdIndexDict.TryGetValue(itemId, out List<int> list)){
            return itemDataDict[list[0]];
        }
        return null;
    }
    /// <summary>
    /// 查找对应ItemId的多个数据,默认用于添加(通常一次可以添加很多,但只能一个一个使用物品)
    /// </summary>
    /// <param name="isAdd">对查找到的数据是添加还是消耗数量</param>
    private List<ItemData> FindDatasWithItemId(int itemId, bool isAdd = true){
        List<ItemData> dataList = new();
        foreach(ItemData data in mainCellDataArray){
            //快捷栏的物品有可能数量为0
            int minNum = isAdd ? 0 : 1;
            if(data.itemId == itemId && data.num >= minNum){
                dataList.Add(data);
            }
        }
        if(itemIdIndexDict.TryGetValue(itemId, out List<int> list)){
            foreach(int key in list){
                dataList.Add(itemDataDict[key]);
            }
        }
        return dataList;
    }
    #endregion
}
