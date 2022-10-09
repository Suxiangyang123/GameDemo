using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏整体进程管理, 给其他Manager提供生命周期调用
/// </summary>
public class MainManager : SingletonComponent<MainManager>
{
    private void Awake() {
        if(IsInstanceExist()){
            Destroy(gameObject);
        }else{
            DontDestroyOnLoad(gameObject);
            Init();
        }
    }
    /// <summary>
    /// 初始化,目前需要维持的先后顺序有
    /// 1.玩家位置初始化要在建筑初始化之后
    /// 2.GameConfig游玩关卡初始化在玩家数据GameData初始化之后
    /// </summary>
    private void Init(){
        SceneManager.sceneLoaded += OnSceneLoaded;

        AudioManager.Instance.Init();
        GameData.Instance.Init();
        GameConfig.Instance.Init();
        ItemManager.Instance.Init();
        UIManager.Instance.Init();
        BuildManager.Instance.Init();
    }

    // void Start()
    // {

    // }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }
    // private void OnEnable() {
    //     // InputManager.Instance.OnEnable();
    // }
    // private void OnDisable() {
    //     // InputManager.Instance.OnDisable();
    // }
    public void StartGame(){
        ChangeSceneAsync("Game");
    }

    /// <summary>
    /// 游戏胜利
    /// </summary>
    public void GameSuccess(){
        GameConfig.Instance.Level += 1;
        if(GameConfig.Instance.Level > GameData.Instance.GetLevel()){
            GameData.Instance.SaveLevel(GameConfig.Instance.Level);
        }
        
        ExitBattle();
    }
    /// <summary>
    /// 退出游戏(死亡时会重生在复活点,只有退出没有游戏失败)
    /// </summary>
    public void ExitBattle(){
        ChangeSceneAsync("Home");
    }
    public void ChangeScene(string sceneName){
        EventManager.Instance.Invoke(EventType.StartChangeScene, this, sceneName);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
    public void ChangeSceneAsync(string sceneName){
        EventManager.Instance.Invoke(EventType.StartChangeScene, this, sceneName);
        SceneManager.LoadSceneAsync(sceneName);
        //获取进度
        //AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        // if(!asyncOperation.isDone){
        //     Debug.Log(asyncOperation.progress);
        // }
    }
    private void OnSceneLoaded(Scene s, LoadSceneMode l){
        if(s.name == "Home"){
            if(GameConfig.Instance.GameState == GameState.Game){
                GameConfig.Instance.IsFirstInHome = false;
            }
            GameConfig.Instance.GameState = GameState.Home;
        }else{
            GameConfig.Instance.GameState = GameState.Game;
        }

        EventManager.Instance.Invoke(EventType.FinishChangeScene, this, s.name);
    }
}
