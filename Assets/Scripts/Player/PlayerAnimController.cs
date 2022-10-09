using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimController
{
    private Player player;
    private Animator animator;
    public void Init(Player player){
        this.player = player;
        animator = player.GetComponent<Animator>();
        string label = GameConfig.Instance.GameMode == GameMode.Normal ? "PlayerAnimatorController" : "PlayerAnimatorControllerFree";
        RuntimeAnimatorController controller =  LoadManager.Instance.LoadData<RuntimeAnimatorController>(label);
        animator.runtimeAnimatorController = controller;
    }
    public void SetInt(int nameHash, int value){
        animator.SetInteger(nameHash, value);
    }
    public void SetFloat(int nameHash, float value){
        animator.SetFloat(nameHash, value);
    }
    public void SetBool(int nameHash, bool value){
        animator.SetBool(nameHash, value);
    }
}
