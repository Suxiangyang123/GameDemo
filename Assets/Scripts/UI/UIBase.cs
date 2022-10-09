using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBase : MonoBehaviour
{
    public virtual UIName UIName { get => UIName.Null;}
    public virtual UIType UIType { get => UIType.Panel;}
    public virtual string Tag { get => string.Empty;}
    /// <summary>
    /// 不显示后是直接销毁还是隐藏....暂时没用到
    /// </summary>
    public virtual bool DestroyWhenHided { get => false;}

    private CanvasGroup group;

    public virtual void OnShow(){
        group = transform.GetComponent<CanvasGroup>();
        if(group != null){
            group.alpha = 1;
            group.interactable = true;
        }
    }
    public virtual void OnPause(){
        if(group != null){
            group.interactable = false;
            //TODO 测试下效果
            // group.blocksRaycasts = false;
        }
    }
    public virtual void OnResume(){
        if(group != null){
            group.interactable = true;
        }
    }
    public virtual void OnHide(){
        if(group != null){
            group.alpha = 0;
            group.interactable = false;
        }
    }
}
