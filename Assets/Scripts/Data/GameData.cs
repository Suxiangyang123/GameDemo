using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
/// <summary>
/// 需要存储的玩家数据
/// </summary>
public class GameData : Singleton<GameData>
{
    private class UserData{//: ISerializationCallbackReceiver
        public int level = 1;
        public ItemBaseData[] mainUICellDatas;
        public ItemBaseData[] userItemDatas;
        
        public UserData(){
            level = 1;
            mainUICellDatas = new ItemBaseData[8];
            userItemDatas = new ItemBaseData[0];
        }

        #region 序列化字典
        // //同种类型的物品可以分多组,且需要存储这个分组信息
        // public Dictionary<int, List<ItemBaseData>> userItemDataDict;
        

        // [SerializeField]
        // private int[] userItemIdArray;
        // [SerializeField]
        // private ItemBaseData[][] userItemDataArray;
        // public void OnBeforeSerialize()
        // {
        //     userItemIdArray = new int[userItemDataDict.Count];
        //     userItemDataArray = new ItemBaseData[userItemDataDict.Count][];
        //     int i = 0;
        //     foreach(var data in userItemDataDict){
        //         userItemIdArray[i] = data.Key;
        //         userItemDataArray[i++] = data.Value.ToArray();
        //     }
        // }

        // public void OnAfterDeserialize()
        // {
        //     userItemDataDict.Clear();
        //     for(int i = 0; i < userItemIdArray.Length; i++){
        //         userItemDataDict.Add(userItemIdArray[i], new List<ItemBaseData>(userItemDataArray[i]));
        //     }
        // }
        #endregion
    }

    private int save = 1;
    private UserData userData;

    public void Init(){
        LoadData();
    }
    public void LoadData(){
        string path = Application.persistentDataPath + "/save_" + save;
        if(File.Exists(path)){
            StreamReader reader = new StreamReader(path);
            string data = reader.ReadToEnd();
            if(data != ""){
                userData = JsonUtility.FromJson<UserData>(data);
            }else{
                userData = new UserData();
            }
        }else{
            userData = new UserData();
        }
    }
    
    public void SaveData(){
        string path = Application.persistentDataPath + "/save_" + save;
        string dataStr = JsonUtility.ToJson(userData);
        StreamWriter writer = new StreamWriter(path);
        writer.WriteLine(dataStr);
        writer.Close();
    }
    
    public int GetLevel(){
        return userData.level;
    }
    public void SaveLevel(int level){
        userData.level = level;
    }

    public void SaveMainUICellData(ItemBaseData[] datas){
        userData.mainUICellDatas = datas;
        SaveData();
    }
    public ItemBaseData[] GetMainUICellData(){
        return userData.mainUICellDatas;
    }
    
    public void SaveUserItemData(List<ItemBaseData> list){
        userData.userItemDatas = list.ToArray();
        SaveData();
    }
    public ItemBaseData[] GetUserItemData(){
        return userData.userItemDatas;
    }
    #region 两种保存方式,如果需要每次数据变化都立即存储,应该这样会好点,或者有更好的处理方式
    // /// <summary>
    // /// ItemMgr同步本地数据
    // /// </summary>
    // /// <param name="beforeNum">根据修改前数量确定修改哪个数据</param>
    // public void ChangeUserItemData(int itemId, int num, int beforeNum){
    //     List<ItemBaseData> dataList = userData.userItemDataDict[itemId];
    //     for(int i = 0; i < dataList.Count; i++){
    //         if(dataList[i].num == beforeNum){//对本地存储来说同itemId和数量的数据即是相同的
    //             dataList[i].num += num;//主要逻辑在ItemMgr处理,这里只做简单同步,所以不判断是否超过每组最大数量等
    //             break;
    //         }
    //     }
    // }
    // public void AddUserItemData(int itemId, int num){
    //     userData.userItemDataDict[itemId].Add(new ItemBaseData(itemId, num));
    // }
    // public void DeleteItemData(int itemId, int num){
    //     List<ItemBaseData> dataList = userData.userItemDataDict[itemId];
    //     for(int i = 0; i < dataList.Count; i++){
    //         if(dataList[i].num == num){//对本地存储来说同itemId和数量的数据即是相同的
    //             dataList.RemoveAt(i);
    //             break;
    //         }
    //     }
    // }
    // public Dictionary<int, List<ItemBaseData>> GetUserItemData(){
    //     return userData.userItemDataDict;
    // }
    #endregion
}
