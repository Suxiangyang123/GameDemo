using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausePanel: UIBase
{
    public override UIName UIName => UIName.PausePanel; 
    public override UIType UIType => UIType.Panel;
    public override bool DestroyWhenHided => false;
    public override void OnShow()
    {
        base.OnShow();
        Time.timeScale = 0;
    }
    public override void OnHide()
    {
        Time.timeScale = 1;
        base.OnHide();
    }
    public void OnBtnContinueClick(){
        AudioManager.Instance.PlayEffect("audio_click");
        OnHide();
        EventManager.Instance.Invoke(EventType.UIHide, this, UIName);
    }
    public void OnBtnExitClick(){
        AudioManager.Instance.PlayEffect("audio_click");
        MainManager.Instance.ChangeScene("Home");
        OnHide();
        EventManager.Instance.Invoke(EventType.UIHide, this, UIName);
    }
}
