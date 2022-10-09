using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class ItemCell : MonoBehaviour,IDragHandler,IBeginDragHandler,IEndDragHandler,IPointerClickHandler
{
    // public int itemId;
    // public ItemInfo info;
    public Image img;
    // private Vector2 size;
    public TMP_Text text_detail;
    public TMP_Text text_num;
    public TMP_Text text_weight;
    [SerializeField]
    public ItemData data = null;

    private SpriteAtlas atlas;

    private UIName parentUIName;
    private bool IsDraging = false;
    public RectTransform tempDragItem;

    //数据变化后的回调
    public Action<ItemCell,ItemData> dataChangeCall = null;
    
    /// <summary>
    /// 确定父UI的类型,所以所有cell都必须先调用一次
    /// </summary>
    /// <param name="data"></param>
    /// <param name="parentUIName"></param>
    public void Init(ItemData data, UIName parentUIName){
        this.data = data;
        this.parentUIName = parentUIName;

        UpdateView();
    }
    public void SetDataChangeCall(Action<ItemCell,ItemData> dataChangeCall){
        this.dataChangeCall = dataChangeCall;
    }
    public void SetAtlas(SpriteAtlas atlas){
        this.atlas = atlas;
    }
    /// <summary>
    /// 存在使用默认值的情况,所以不在通用方法Init里初始化(主要没有类似TS变量?:类型的语法去设置可选参数)
    /// </summary>
    /// <param name="size"></param>
    public void SetSize(Vector2 size){
        RectTransform trans = (RectTransform)transform;
        trans.sizeDelta = size;
    }

    public void SetData(ItemData data){
        if(data?.key > 0 && (this.data?.key != data.key)){
            ItemManager.Instance.UpdateDataCell(this, data);
        }
        this.data = data;
        UpdateView();
    }
    public ItemData GetData(){
        return data;
    }
    
    private void UpdateView(){
        if(data != null && data.itemId != 0){//为了将快捷栏统一管理,
            ItemInfo info = ItemManager.Instance.ItemInfoDict[data.itemId];
            text_detail.text = info.detail;
            img.sprite = atlas.GetSprite(info.img);
            if(data.num > 0){
                text_num.text = data.num.ToString();
                text_weight.text = (data.num * info.weight).ToString();
                text_num.enabled = true;
                text_weight.enabled = true;
            }else{
                text_num.enabled = false;
                text_weight.enabled = false;
            }
        }else{
            text_detail.enabled = false;//text_detail.text = "";
            text_num.enabled = false;
            text_weight.enabled = false;
            img.sprite = atlas.GetSprite("itemNull");//TODO
        }
    }

    /// <summary>
    /// 处理鼠标对格子的操作(后续想添加右键处理,所以不适用button)
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if(!UIManager.Instance.ClickAble){
            return;
        }
        if(eventData.button == PointerEventData.InputButton.Left){
            LeftClick();
        }
        //TODO: 右键显示细分选项
    }

    /// <summary>
    /// 处理左键点击,快捷栏按钮也能触发
    /// </summary>
    public void LeftClick(){
        if(data == null || data.itemId <= 0){
            return;
        }
        ItemInfo info = ItemManager.Instance.ItemInfoDict[data.itemId];
        ItemType type = (ItemType)info.mainType;
        if(data.num == 0){
            if(info.creatable){
                //正常情况下数量为0移除,在印痕和快捷栏可以数量为0,点击会建造物体
                UIManager.Instance.ShowFloatTip("虚空花费了材料,生成了" + info.limit + "个" + info.detail);
                ItemManager.Instance.AddItem(data.itemId, info.limit);
            }
        }else{
            switch(type){
                case ItemType.Building:{
                    //UI界面关闭所有弹窗,ItemMgr记录当前Data,BuildController开始建造
                    int[] buildData = Utils.StringToNumberArray(info.extra);
                    EventManager.Instance.Invoke(EventType.StartBuild, this, buildData[0], buildData[1], data);
                    break;
                }
                case ItemType.Food:{
                    ItemManager.Instance.UseItem(data.itemId, 1, data.key);
                    UIManager.Instance.ShowFloatTip("吃了一个" + info.detail + ",似乎什么也没发生");
                    break;
                }
            }
        }
    }
    
    #region 拖拽功能
    public void OnBeginDrag(PointerEventData eventData)
    {
        if(data == null || data.itemId == 0){
            return;
        }
        tempDragItem = (RectTransform)GameObject.Instantiate(img.transform, img.transform.position, Quaternion.identity);
        //原先物体是保持与父物体大小一直,克隆后需要调整锚点
        tempDragItem.anchorMin = Vector2.one * 0.5f;
        tempDragItem.anchorMax = Vector2.one * 0.5f;
        tempDragItem.sizeDelta = ((RectTransform)transform).sizeDelta;
        tempDragItem.SetParent(UIManager.Instance.UiTopRoot, false);
        tempDragItem.GetComponent<Image>().raycastTarget = false;
        IsDraging = true;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if(IsDraging ){
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)tempDragItem.parent.transform, eventData.position, eventData.enterEventCamera, out Vector2 pos)){
                tempDragItem.localPosition = pos;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //快捷栏的物体可以位置替换,其他的不行
        //快捷栏能将实际物体(非空且数量大于0)拖进Inventory界面,不能拖进印痕Crafting界面
        //印痕栏和个人物品界面可以拖进快捷栏
        if(!IsDraging){
            return;
        }
        // 理论上可交互区域只有UI的背景或者itemCell,其他位置比如按钮不需要处理拖拽交互
        UIName uiName = UIName.Null;
        UIBase ui = null;
        Transform trans = eventData.pointerCurrentRaycast.gameObject.transform.parent;
        //拖到itemcell上,获取cell所属的UI,并判断是否是快捷栏的cell相互替换  
        if(trans != null && trans.tag.Equals("Item")){
            ItemCell cell = trans.GetComponent<ItemCell>();
            ItemData cellData = cell.data;
            uiName = cell.parentUIName;
            ui = UIManager.Instance.GetUI(uiName);
            if(parentUIName == UIName.MainUI && uiName == parentUIName){
                //快捷栏itemcell相互替换,需要保证key不变
                int tempItemId = data.itemId, tempNum = data.num;
                data.itemId = cellData.itemId;
                data.num = cellData.num;
                InvokeDataChangeCall(data);
                cellData.itemId = tempItemId;
                cellData.num = tempNum;
                cell.InvokeDataChangeCall(cellData);
            }else if(parentUIName == UIName.ItemPanel && uiName == UIName.MainUI){
                ItemPanel itemPanel = (ItemPanel)UIManager.Instance.GetUI(parentUIName);
                if(itemPanel.CurView == ItemPanel.ViewType.Inventory){
                    //物品界面拖到快捷栏,快捷栏数据替换为cell数据,原数据添加到itemPanel(拖拽的cell数据清空而不是替换数据)
                    if(cellData.num > 0){
                        //itemId = 0时默认num也为0
                        ItemManager.Instance.AddItemData(cellData.itemId, cellData.num);
                    }
                    //保证key不变
                    cellData.itemId = data.itemId;
                    cellData.num = data.num;
                    cell.InvokeDataChangeCall(cellData);
                    InvokeDataChangeCall(null);
                }else{
                    //印痕界面拖到快捷栏.只有null(num和itemid均为0)或印痕(num都为0,itemId不为0)才能交互
                    if(cellData.num == 0){
                        cellData.itemId = data.itemId;
                        cell.InvokeDataChangeCall(cellData);
                    }
                }
            }
        }
        //拖拽到ui的背景,TODO:有点烂,目前是已知剩下的情况只会和物品界面交互,再根据物品界面层级查找UI(scrollView的viewPort->scrollView->ItemPanel)
        if(uiName == UIName.Null && trans.parent.TryGetComponent<UIBase>(out ui)){
            uiName = ui.UIName;
        }
        if(parentUIName == UIName.MainUI && uiName == UIName.ItemPanel){
            ItemPanel itemPanel = (ItemPanel)ui;
            if(itemPanel.CurView == ItemPanel.ViewType.Inventory){
                //从快捷栏拖到物品界面,不与itemPanel中的cell交互,只要拖到物品界面就会把数据拖过去
                if(data.num > 0){
                    //数量大于0时物品被拖到物品栏,原位置变为空
                    ItemManager.Instance.AddItemData(data.itemId, data.num);

                    data.itemId = 0;
                    data.num = 0;
                    InvokeDataChangeCall(data);
                }
            }
        }

        GameObject.Destroy(tempDragItem.gameObject);
        tempDragItem = null;
        IsDraging = false;
    }
    // 获取该点后面的所有可交互区域
    // GraphicRaycaster raycaster = UIManager.Instance.Canvas.GetComponent<GraphicRaycaster>();
    // List<RaycastResult> results = new();
    // raycaster.Raycast(eventData, results);
    // foreach(var result in results){
    // }

    public void InvokeDataChangeCall(ItemData data){
        dataChangeCall?.Invoke(this, data);
    }
    #endregion
}
