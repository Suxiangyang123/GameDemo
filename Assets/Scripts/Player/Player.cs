using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour,IEntityBase
{
    public PlayerMoveController playerMoveController;
    public PlayerAnimController playerAnimController;
    public PlayerBuildController playerBuildController;
    public InputControls inputController;

    private bool isDead = false;

    private void Awake(){
        inputController = new InputControls();
        playerAnimController = new PlayerAnimController();
        if(GameConfig.Instance.GameMode == GameMode.Build){
            //建造模式中,相机跟随玩家视角转动,鼠标控制转向
            playerMoveController = new PlayerFreeMoveController();
            playerBuildController = new PlayerBuildController();
        }else{
            //普通模式中,相机锁定角度和跟随距离,移动键控制玩家移动和转向
            playerMoveController = new PlayerLockMoveController();
        }

        EventManager.Instance.AddEvent(EventType.SetPlayerMovable, OnPlayerMovableChanged);
    }

    void Start(){
        playerMoveController.Init(this);
        playerAnimController.Init(this);
        playerBuildController?.Init(this);
        inputController.Enable();
    }

    void Update(){
        if(isDead){
            return;
        }
        playerMoveController.Update();
        playerBuildController?.Update();
    }
    private void FixedUpdate(){
        if(isDead){
            return;
        }
        playerMoveController.FixedUpdate();
    }
    private void OnDestroy() {
        inputController?.Disable();
        EventManager.Instance.RemoveEvent(EventType.SetPlayerMovable, OnPlayerMovableChanged);
        playerBuildController?.ClearEvent();
    }
    private void OnPlayerMovableChanged(object sender, object[] data){
        bool movable = (bool) data[0];
        if(movable){
            inputController.Enable();
        }else{
            inputController.Disable();
        }
    }
    
    /// <summary>
    /// 碰撞监听,
    /// </summary>
    private void OnCollisionEnter(Collision other) {
        if(GameConfig.Instance.GameMode == GameMode.Normal){
            if(other.transform.tag == "Spike"){
                OnDie();
            }
        }
        playerMoveController.OnCollisionEnter(other);
    }
    private void OnCollisionStay(Collision other){
        playerMoveController.OnCollisionStay(other);
    }
    private void OnCollisionExit(Collision other)
    {
        playerMoveController.OnCollisionExit(other);
    }

    public void OnDie(){
        //音效,死亡动画等
        isDead = true;
        AudioManager.Instance.PlayEffect("audio_die");
        transform.localScale = Vector3.zero;
        playerMoveController.OnDie();
        inputController.Disable();
        Invoke("Respawn", 0.5f);
    }
    public void Respawn(){
        isDead = false;
        inputController.Enable();
        playerMoveController.ResetPos();
        transform.localScale = Vector3.one;
    }
}
