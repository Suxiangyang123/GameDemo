using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SavePanel : UIBase
{
    public override UIName UIName => UIName.SavePanel; 
    public override UIType UIType => UIType.Panel;
    public override bool DestroyWhenHided => false;

    public TMP_InputField field;
    public int maxLevel = 1;
    public override void OnShow(){
        base.OnShow();
        maxLevel = GameConfig.Instance.MaxLevel;
        field.text = (maxLevel + 1).ToString();
    }
    public void OnBtnSaveClick(){
        AudioManager.Instance.PlayEffect("audio_click");

        if(int.TryParse(field.text, out int level)){
            if(level > maxLevel + 1){
                UIManager.Instance.ShowFloatTip("只能新建关卡:" + (maxLevel + 1) + "或者覆盖之前的关卡,当前最大关卡为:" + maxLevel);
            }else if(level <= 0){
                UIManager.Instance.ShowFloatTip("关卡数必须大于0");
            }else{
                BuildManager.Instance.SaveData(level);
                //只是刷新,上面关卡没保存成功也没事
                GameConfig.Instance.UpdateMaxLevel();
            }
        }else{
            UIManager.Instance.ShowFloatTip("请输入数字");
        }
    }

    public void OnBtnCloseClick(){
        AudioManager.Instance.PlayEffect("audio_click");

        OnHide();
        EventManager.Instance.Invoke(EventType.UIHide, this, UIName);
    }
}
