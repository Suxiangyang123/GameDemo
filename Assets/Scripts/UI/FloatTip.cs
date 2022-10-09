using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class FloatTip : UIBase
{
    public override UIName UIName => UIName.FloatTip; 
    public override UIType UIType => UIType.Top;
    public override bool DestroyWhenHided => false;

    public TextMeshProUGUI text;
    public Image bg;

    /// <summary>
    /// FloatTip独有,目前其他UI没有传参数的需求
    /// </summary>
    public void Init(string message){
        text.text = message;
        bg.rectTransform.sizeDelta = text.rectTransform.sizeDelta;
    }
    public override void OnShow(){
        base.OnShow();
        RectTransform trans = (RectTransform)transform;
        trans.anchoredPosition = new Vector2(0, -100);
        bg.DOFade(1, 0);
        text.DOFade(1, 0);
        trans.DOAnchorPosY(100, 1, true).SetEase(Ease.InSine);
        bg.DOFade(0, 1).SetDelay(1);
        text.DOFade(0, 1).SetDelay(1).OnComplete(()=>{
            EventManager.Instance.Invoke(EventType.UIHide, this, UIType);
            OnHide();
        });
    }
}
