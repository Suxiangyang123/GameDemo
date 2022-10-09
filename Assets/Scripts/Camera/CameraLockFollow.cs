using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class CameraLockFollow : CameraFollow
{
    public Vector3 offset = new Vector3(0, 3f, -5);
    private Vector3 rayCastDir = new Vector3(0, -1f, 5f);
    private float rayCastDistance;
    private GameObject lastCastObject = null;
    private Material lastCastMaterial;
    public override void Init(CameraController controller)
    {
        base.Init(controller);
        transform.rotation = Quaternion.Euler(30, 0, 0); 
    }
    public override void LateUpdate() {
        LockAngleFollow();
        //没啥用
        // CullingRayCast();
    }
    /// <summary>
    /// 固定角度和距离跟随玩家
    /// </summary>
    private void LockAngleFollow(){
        transform.position = target.position + offset;
        rayCastDistance = rayCastDir.magnitude;
    }

    /**
     * @description: 将遮挡住玩家的墙体设为半透明
     * @param {*}
     * @return {*}
     */
    private void CullingRayCast(){
        RaycastHit hit;
        if(Physics.Raycast(transform.position, rayCastDir,out hit, rayCastDistance)){
            //TODO目前逻辑比较简陋,比如只在这里排除终点后面会有点问题
            if(hit.transform.tag != "Player" && hit.transform.tag != "End"){
                if(lastCastObject == hit.transform.gameObject){
                    return;
                }else{
                    if(lastCastObject != null){
                        SetObjectTransParency(lastCastObject, false);
                        lastCastObject = null;
                    }
                }
                SetObjectTransParency(hit.transform.gameObject, true);
                lastCastObject = hit.transform.gameObject;
            }
        }else{
            if(lastCastObject != null){
                SetObjectTransParency(lastCastObject, false);
                lastCastObject = null;
            }
        }
    }
    /**
     * @description: 设置物体是否透明及透明度
     * @param {*}
     * @return {*}
     */
    private void SetObjectTransParency(GameObject obj, bool isTransparent, float opacity = 0.3f){
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        if(isTransparent){
            lastCastMaterial = new Material(Shader.Find("Standard"));
            lastCastMaterial.CopyPropertiesFromMaterial(renderer.material);
            renderer.material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            Color color = renderer.material.color;
            color.a = opacity;
            renderer.material.color = color;
            Utils.SetMaterialRenderingMode(renderer.material, Utils.RenderingMode.Transparent);
        }else{
            renderer.material.CopyPropertiesFromMaterial(lastCastMaterial);
        }
    }
}
