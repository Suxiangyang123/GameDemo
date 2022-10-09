using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public enum LoadType{
    Resources,
    AssetBundleLocal,
    AssetbundleRemote
}
public class LoadManager:Singleton<LoadManager>
{
    private Dictionary<string, AsyncOperationHandle> loadDict = new();

    public void Init(){
        EventManager.Instance.AddEvent(EventType.StartChangeScene, ClearAll);
    }
    private void ClearAll(object sender, object sceneName){
        foreach(var item in loadDict){
            //TODO:加载过程中release?
            Addressables.Release(item.Value);
        }
        loadDict.Clear();
    }
    /// <summary>
    /// 加载资源,name作为键值,如果不传address则name作为label加载,否则通过address加载
    /// </summary>
    public T LoadData<T>(string name, string address = null) where T : UnityEngine.Object{
        AsyncOperationHandle handle;
        if(loadDict.ContainsKey(name)){
            handle = loadDict[name];
            if(handle.IsDone){
                return (T)handle.Result;
            }else{
                return (T)handle.WaitForCompletion();
            }
        }
        string key = address ?? name;
        handle = Addressables.LoadAssetAsync<T>(key);
        loadDict.Add(name, handle);
        return (T)handle.WaitForCompletion();
    }

    // public T LoadDataWithResources<T>(string path) where T : UnityEngine.Object{
    //     return Resources.Load<T>(path);
    // }

    // public T LoadDataWithAssetBundleLocal<T>(string path, string name) where T : UnityEngine.Object{
    //     AssetBundle ab = AssetBundle.LoadFromFile(path);
    //     T obj = ab.LoadAsset<T>(name);
    //     return obj;
    // }
    // public IEnumerator LoadDataWithAssetBundleRemote<T>(string url, string dataPath) where T : UnityEngine.Object{
    //     url = Application.streamingAssetsPath + "/" + "prefab";
    //     var uwr = UnityWebRequestAssetBundle.GetAssetBundle(url);
    //     yield return uwr.SendWebRequest();

    //     AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
    //     var loadAsset = bundle.LoadAssetAsync<T>(dataPath);
    //     yield return loadAsset;
    // }
}
