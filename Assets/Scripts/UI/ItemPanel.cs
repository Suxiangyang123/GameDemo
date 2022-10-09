using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class ItemPanel: UIBase
{
    public enum ViewType{
        Inventory,
        Crafting,
    }
    public override UIName UIName => UIName.ItemPanel; 
    public override UIType UIType => UIType.Panel;
    public override bool DestroyWhenHided => false;


    public ScrollRect scrollRect;



    // private Vector2 size = new Vector2(100, 100); //暂时不需要,layout控制size
    private ViewType curView = ViewType.Inventory;
    public ViewType CurView { get => curView; set => curView = value; }
    public override void OnShow()
    {
        base.OnShow();                                                               
        UpdateView();
        EventManager.Instance.AddEvent(EventType.AddItem, OnAddNewItem);
    }
    public override void OnHide()
    {
        EventManager.Instance.RemoveEvent(EventType.AddItem, OnAddNewItem);
        base.OnHide();
    }
    private void OnAddNewItem(object sender, object[] data){
        if(CurView == ViewType.Inventory){
            int key = (int)data[0], index = (int)data[1];
            ItemCell cell = ShowData(ItemManager.Instance.ItemDataDict[key], scrollRect.content);
            cell.SetDataChangeCall(InventoryItemChangeCall);
            cell.transform.SetSiblingIndex(index);
        }
    }
    public void OnSwitchInventory(Toggle toggle){
        bool show = toggle.isOn;
        if(show && CurView != ViewType.Inventory){
            CurView = ViewType.Inventory;
            UpdateView();
        }
    }
    public void OnSwitchCrafting(Toggle toggle){
        bool show = toggle.isOn;
        if(show && CurView != ViewType.Crafting){
            CurView = ViewType.Crafting;
            UpdateView();
        }
    }

    public void OnBtnCloseClick(){
        AudioManager.Instance.PlayEffect("audio_click");
        EventManager.Instance.Invoke(EventType.UIHide, this, UIType);
        OnHide();
    }

    private void UpdateView(){
        ClearAllItemInView();
        if(CurView == ViewType.Inventory){
            CreateInventoryView();
        }else if(CurView == ViewType.Crafting){
            CreateCraftingView();
        }
    }

    private void CreateInventoryView(){
        var dataDict = ItemManager.Instance.ItemDataDict;
        var indexDict = ItemManager.Instance.ItemIdIndexDict;
        foreach(var list in indexDict.Values){
            for(int i = 0; i < list.Count; i++){
                ItemData data = dataDict[list[i]];
                ItemCell cell = ShowData(data, scrollRect.content);
                cell.SetDataChangeCall(InventoryItemChangeCall);
            }
        }
    }

    private void CreateCraftingView(){
        int[] itemIds = ItemManager.Instance.CreatableItemArray;
        for(int i = 0; i < itemIds.Length; i++){
            ItemData data = new(itemIds[i]);
            ShowData(data, scrollRect.content);
        }
    }
    private ItemCell ShowData(ItemData data, Transform parent){
        ItemCell cell = ItemManager.Instance.GetItemCell(data.key);
        cell.Init(data, UIName);
        cell.transform.SetParent(parent, false);
        cell.gameObject.SetActive(true);
        return cell;
    }
    /// <summary>
    /// 切换界面或者再次打开时会调用回收当前显示的Item到对象池
    /// </summary>
    private void ClearAllItemInView(){
        int count = scrollRect.content.childCount;
        for(int i = count - 1; i >= 0; i--){
            ItemCell cell = scrollRect.content.GetChild(i).GetComponent<ItemCell>();
            ItemManager.Instance.DeleteItemCell(cell);
        }
    }

    // private void CraftingItemChangeCall(ItemCell cell, ItemData data){
    //     //印痕不会有变化,不需要
    // }
    private void InventoryItemChangeCall(ItemCell cell, ItemData data){
        if(data != null && data.itemId > 0){
            cell.SetData(data);
        }else{
            ItemManager.Instance.DeleteItemCell(cell);
        }
        
    }
}