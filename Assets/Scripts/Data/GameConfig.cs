using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public enum GameMode{
    Normal,
    Build,
}
public enum GameState{
    Home,
    Game,
}
public enum Platform{
    UnityEditor,
    PC,
    Android,
    IOS
}
/// <summary>
/// 运行时用到的数据,不会被存储
/// </summary>
public class GameConfig : Singleton<GameConfig>
{
    private GameMode gameMode = GameMode.Build;
    private GameState gameState = GameState.Home;
    private float minViewAngle = -90;
    private float maxViewAngle = 90;
    private int level = 1;
    private int maxLevel;
    private bool isFirstInHome = true;
    private Platform platform = Platform.UnityEditor;

    
    /// <summary>
    /// 游戏模式,建造模式还是普通模式
    /// </summary>
    public GameMode GameMode { get => gameMode; set => gameMode = value; }
    /// <summary>
    /// 游戏状态,主界面还是在游戏中
    /// </summary>
    public GameState GameState { get => gameState; set => gameState = value; }
    public float MinViewAngle { get => minViewAngle; set => minViewAngle = value; }
    public float MaxViewAngle { get => maxViewAngle; set => maxViewAngle = value; }
    /// <summary>
    /// 当前开放最大关卡
    /// </summary>
    /// <value></value>
    public int MaxLevel { get => maxLevel; set => maxLevel = value; }
    /// <summary>
    /// 玩家当前玩的关卡
    /// </summary>
    public int Level { get => level; set{
        level = value > maxLevel ? maxLevel : value;
    }}
    /// <summary>
    /// 是否刚进入游戏,区分第一次进主界面还是从关卡中退出到主界面
    /// </summary>
    public bool IsFirstInHome { get => isFirstInHome; set => isFirstInHome = value; }
    public Platform Platform { get => platform; set => platform = value; }

    public void Init(){
        InitPlatform();
        UpdateMaxLevel();
        level = GameData.Instance.GetLevel();
    }

    /// <summary>
    /// 获取最大关卡数(目前限制只能新建或覆盖关卡,新建只能从当前最大关卡数递增)
    /// </summary>
    /// <returns></returns>
    public void UpdateMaxLevel(){
        string path = Application.persistentDataPath + "/level";
        DirectoryInfo info = new DirectoryInfo(path);
        maxLevel = info.GetFiles().Length;
    }
    /// <summary>
    /// 瞎写的,应该不够严谨也没啥用暂时
    /// </summary>
    private void InitPlatform(){
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
                Platform = Platform.PC;
                break;
            case RuntimePlatform.WindowsEditor:
                Platform = Platform.UnityEditor;
                break;
            case RuntimePlatform.Android:
                Platform = Platform.Android;
                break;
            case RuntimePlatform.IPhonePlayer:
                Platform = Platform.IOS;
                break;
        }
    }
}