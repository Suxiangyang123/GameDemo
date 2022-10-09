using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum UIType{
    /// <summary>
    /// 占据整个界面,不同UI间属于切换覆盖的关系,同时只会显示一个界面 TODO命名
    /// </summary>
    UI,
    /// <summary>
    /// 弹窗,先进后出,需要维护一个栈,可以同时显示多个弹窗
    /// </summary>
    Panel,
    /// <summary>
    /// 处于最上层,用于loading,消息等
    /// </summary>
    Top
}
/// <summary>
/// 所有UI的枚举
/// </summary>
public enum UIName{
    Null,
    HomeUI,
    MainUI,
    ItemPanel,
    SelectLevelUI,
    RebindKeyUI,
    SavePanel,
    PausePanel,
    FloatTip,
}
public class UIManager: Singleton<UIManager>
{
    // private struct PathData{
    //     UIName name;
    //     LoadType loadType;
    //     string path;
    //     string assetPath;
    // }
    private Camera uiCamera;
    private Canvas canvas;
    private Transform uiPanelRoot;
    private Transform uiPopupRoot;
    private Transform uiTopRoot;

    private Dictionary<UIName, UIBase> UIDict = new();
    /// <summary>
    /// 战斗场景设置能否点击(根据有无弹窗确定)
    /// </summary>
    private bool clickAble = true;
    private UIBase curPanel;
    private InputControls inputController;

    //感觉有几率关闭下层的弹窗,大致按栈处理,但不能定义为Stack
    private List<UIBase> popups = new();
    [HideInInspector]
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern int SetCursorPos(int x, int y);

    public Transform UiTopRoot { get => uiTopRoot; set => uiTopRoot = value; }
    public Canvas Canvas { get => canvas; set => canvas = value; }
    public bool ClickAble { get => clickAble;}

    public void Init(){
        EventManager.Instance.AddEvent(EventType.ShowUI, OnShowUI);
        EventManager.Instance.AddEvent(EventType.UIHide, OnUIHide);
        EventManager.Instance.AddEvent(EventType.StartChangeScene, OnSceneStartChange);
        EventManager.Instance.AddEvent(EventType.FinishChangeScene, OnSceneFinishChange);
        EventManager.Instance.AddEvent(EventType.StartBuild, OnStartBuild);
    }
    /// <summary>
    /// 初始化和切换场景时调用,TODO命名
    /// </summary>
    private void InitInfo(){
        uiCamera = GameObject.FindGameObjectWithTag("UICamera").GetComponent<Camera>();
        Canvas = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<Canvas>();
        uiPanelRoot = Canvas.transform.Find("Panel");
        uiPopupRoot = Canvas.transform.Find("PopUp");
        UiTopRoot = Canvas.transform.Find("Top");
    }
    private void OnSceneStartChange(object sender, object[] name){
        //TODO: 清空引用应该就行,销毁由切换场景完成
        ClearAll();
    }
    public void OnSceneFinishChange(object sender, object[] name){
        InitInfo(); 
        
        string sceneName = name[0].ToString();
        UIName uiName = UIName.Null;
        switch(sceneName){
            case "Load":{
                break;
            }
            case "Home":{
                uiName = UIName.HomeUI;
                SetCursorShowAndClickAble(true);
                break;
            }
            case "Game":{
                uiName = UIName.MainUI;
                SetCursorShowAndClickAble(false);
                break;
            }
        }
        if(uiName != UIName.Null){
            ShowUI(uiName);
        }
    }

    /// <summary>
    /// 事件响应方法
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="name"></param>
    public void OnShowUI(object sender, object[] data){
        if(data[0].GetType() != typeof(UIName)){
            Debug.LogError("显示UI参数错误");
            return;
        }
        UIName uiName = (UIName)data[0];
        ShowUI(uiName);
    }
    /// <summary>
    /// 正式调用显示UI
    /// </summary>
    /// <param name="uiName"></param>
    private void ShowUI(UIName uiName){
        if(!UIDict.TryGetValue(uiName, out UIBase ui)){
            ui = LoadUI(uiName);
        }
        switch(ui.UIType){
            case UIType.UI:{
                if(curPanel != null){
                    HidePanel(curPanel);
                }
                ShowPanel(ui);
                break;
            }
            case UIType.Panel:{
                PushPopUp(ui);
                break;
            }
            case UIType.Top:{
                
                break;
            }
        }
    }
    private UIBase LoadUI(UIName name){
        string label = name.ToString();
        GameObject go = LoadManager.Instance.LoadData<GameObject>(label);
        GameObject uiObj = GameObject.Instantiate(go,Vector3.zero, Quaternion.identity);
        UIBase ui = uiObj.GetComponent<UIBase>();
        switch(ui.UIType){
            case UIType.UI:{
                ui.transform.SetParent(uiPanelRoot, false);
                break;
            }
            case UIType.Panel:{
                ui.transform.SetParent(uiPopupRoot, false);
                break;
            }
            case UIType.Top:{
                ui.transform.SetParent(UiTopRoot, false);
                break;
            }
        }
        UIDict.Add(name, ui);
        return ui;
    }

    /// <summary>
    /// 一般只会是UI点击关闭后通知UIMgr,而不是通知UIMgr关闭某个界面
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="name"></param>
    public void OnUIHide(object sender, object name){
        // UIName uiName = (UIName)name;
        UIBase ui = (UIBase)sender;
        switch(ui.UIType){
            case UIType.UI:{
                //界面关闭时转到默认界面 
                switch(ui.UIName){
                    case UIName.RebindKeyUI:
                    case UIName.SelectLevelUI:{
                        ShowUI(UIName.HomeUI);
                        break;
                    }
                }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
                break;
            }
            case UIType.Panel:{
                PopPopUp(ui);
                break;
            }
            case UIType.Top:{
                if(ui.UIName == UIName.FloatTip){
                    RecycleFloatTip((FloatTip)ui);
                }
                break;
            }
        }
    }

    private void ShowPanel(UIBase ui){
        ui.OnShow();
        curPanel = ui;
    }
    private void HidePanel(UIBase ui){
        ui.OnHide();
    }

    #region 弹窗相关
    private void PushPopUp(UIBase ui){
        if(popups.Count > 0){
            popups[^1].OnPause();
        }else{
            curPanel?.OnPause();
            SetCursorShowAndClickAble(true);
        }
        ui.OnShow();
        popups.Add(ui);
    }
    private void PopPopUp(UIBase ui){
        if(popups.Contains(ui)){
            popups.Remove(ui);
            //不调用ui.OnHide,目前逻辑是ui里点击关闭触发OnHide,并发送UI已经关闭的事件
            //发送事件不写在Onhide,否则会出现循环,展示UIA - 关闭当前UIB OnHide - 触发关闭回调OnUIHide - 转到默认界面ShowUI
        }
        if(popups.Count > 0){
            popups[^1].OnResume();
        }else{
            curPanel?.OnResume();
            SetCursorShowAndClickAble(false);
        }
    }
    private void ClearPopUp(){
        foreach(var popup in popups){
            popup.OnHide();
        }
        popups.Clear();
        SetCursorShowAndClickAble(false);
        curPanel?.OnResume();
    }
    #endregion

    /// <summary>
    /// 监听到切换场景时调用
    /// </summary>
    public void ClearAll(){
        uiCamera = null;
        Canvas = null;
        uiPanelRoot = null;
        uiPopupRoot = null;
        UiTopRoot = null;

        curPanel?.OnHide();
        curPanel = null;
        foreach(UIBase ui in popups){
            ui.OnHide();
        }
        ClearPopUp();
        UIDict.Clear();
    }

    public UIBase GetUI(UIName name){
        UIBase ui = null;
        UIDict.TryGetValue(name, out ui);
        return ui;
    }
    private void OnStartBuild(object sender, object[] datas){
        ClearPopUp();
    }

    /// <summary>
    /// 设置战斗场景有无弹窗时鼠标显隐和点击功能
    /// </summary>
    private void SetCursorShowAndClickAble(bool canShow){
        if(GameConfig.Instance.GameState == GameState.Game){
            if(GameConfig.Instance.Platform == Platform.UnityEditor){
                //TODO暂时不知道怎么获取Game视图在Unity界面中的位置
                SetCursorPos(1030, 500);
            }else{
                SetCursorPos(Screen.width/2, Screen.height/2);
            }
            Cursor.visible = canShow;
            clickAble = canShow;
        }else{
            Cursor.visible = true;
            clickAble = true;
        }
    }

    #region FloatTip
    private List<FloatTip> tips = new List<FloatTip>(2);
    /// <summary>
    /// 弹窗
    /// </summary>
    public void ShowFloatTip(string message){
        FloatTip tip = GetFloatTip();
        tip.Init(message);
        tip.OnShow();
    }

    private FloatTip GetFloatTip(){
        if(tips.Count > 0){
            return tips[^1];
        }else{
            if(UIDict.TryGetValue(UIName.FloatTip, out UIBase ui)){
                GameObject go = GameObject.Instantiate(ui.gameObject);
                go.transform.SetParent(UiTopRoot, false);
                return go.GetComponent<FloatTip>();
            }else{
                ui = LoadUI(UIName.FloatTip);
                return (FloatTip)ui;
            }
        }
    }
    private void RecycleFloatTip(FloatTip tip){
        tips.Add(tip);
    }
    #endregion
}
