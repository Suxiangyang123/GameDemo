using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeUI: UIBase{
    public override UIName UIName => UIName.HomeUI; 
    public override UIType UIType => UIType.UI;
    public override bool DestroyWhenHided => false;

    public void OnBtnStartClick(){
        OnPause();
        AudioManager.Instance.PlayEffect("audio_click");
        GameConfig.Instance.GameMode = GameMode.Normal;
        MainManager.Instance.StartGame();
    }
    public void OnBtnSelectLevelClick(){
        //敬请期待
    }
    public void OnBtnBuildClick(){
        OnPause();
        AudioManager.Instance.PlayEffect("audio_click");
        GameConfig.Instance.GameMode = GameMode.Build;
        MainManager.Instance.StartGame();
    }
    public void OnBtnKeyRebindClick(){
        //敬请期待
    }
}