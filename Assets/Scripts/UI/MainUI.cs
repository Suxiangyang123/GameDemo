using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;

public class MainUI: UIBase
{
    public override UIName UIName => UIName.MainUI; 
    public override UIType UIType => UIType.UI;
    public override bool DestroyWhenHided => false;

    /// <summary>
    /// 是否显示,物品栏等UI覆盖时为false,否则为true,显示时可以使用快捷键;
    /// </summary>
    private bool isShow;
    public Transform itemCellRoot;
    private int cellCount = 8;
    private InputControls inputController;
    private SpriteAtlas atlas;

    private ItemCell[] itemCells;

    public override void OnShow(){
        base.OnShow();

        InitItemCells();
        InitInputController();
    }
    private void InitItemCells(){
        atlas = LoadManager.Instance.LoadData<SpriteAtlas>("itemAtlas", AddressPaths.itemAtlas);

        itemCells = new ItemCell[cellCount];
        ItemData[] userItemCellData = ItemManager.Instance.MainCellDataArray;
        for(int i = 0; i < cellCount; i++){
            ItemData data = null;
            if(userItemCellData != null && userItemCellData.Length > i){  
                data = userItemCellData[i];
            }
            ItemCell cell = ItemManager.Instance.GetItemCell(data.key);
            cell.Init(data, UIName);
            cell.transform.SetParent(itemCellRoot, false);
            cell.SetDataChangeCall(ItemDataChangeCall);
            itemCells[i] = cell;
        }
    }
    private void ItemDataChangeCall(ItemCell cell, ItemData data){
        cell.SetData(data);
        //快捷栏的数据变化同步到ItemMgr
        //cell目前不存储自身次序,但因为快捷栏比较特殊,永远最先且按顺序确定key,,所以暂时key作为index使用,TODO:有点危险
        ItemManager.Instance.SetMainCellItemData(data.key, data);
    }

    private void InitInputController(){
        inputController = new InputControls();
        inputController.MainUI.Enable();
        inputController.MainUI.Save.performed += (InputAction.CallbackContext ctx)=>{
            EventManager.Instance.Invoke(EventType.ShowUI, this, UIName.SavePanel);
        };
        inputController.MainUI.Pause.performed += (InputAction.CallbackContext ctx)=>{
            EventManager.Instance.Invoke(EventType.ShowUI, this, UIName.PausePanel);
        };
        inputController.MainUI.Item.performed += (InputAction.CallbackContext ctx)=>{
            EventManager.Instance.Invoke(EventType.ShowUI, this, UIName.ItemPanel);
        };
        inputController.MainUI.ShortCut.performed += OnShortCutClick;
    }
    private void OnShortCutClick(InputAction.CallbackContext context){
        int type = int.Parse(context.control.displayName);
        itemCells[type - 1].LeftClick();
    }   
    public override void OnResume()
    {
        base.OnResume();
        EventManager.Instance.Invoke(EventType.SetPlayerMovable, this, true);
        inputController.MainUI.Enable();
    }
    public override void OnPause()
    {
        base.OnPause();
        inputController.MainUI.Disable();
        EventManager.Instance.Invoke(EventType.SetPlayerMovable, this, false);
    }
    public override void OnHide()
    {
        base.OnHide();
        EventManager.Instance.Invoke(EventType.UIHide, this, UIName);
        inputController.Dispose();//TODO:是否会影响到玩家控制
    }
}
