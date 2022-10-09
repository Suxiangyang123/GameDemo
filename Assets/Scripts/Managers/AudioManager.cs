using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager:Singleton<AudioManager>{
    private AudioSource[] sources;
    private int maxCount = 5;
    //凑合一下吧 TODO统一处理一下资源加载方式
    private Dictionary<string, string> audioPath = new Dictionary<string, string>{
        {"homeBgm","Assets/Audio/homeBgm.mp3"},
        {"battleBgm","Assets/Audio/battleBgm.mp3"},
        {"audio_die","Assets/Audio/audio_die.mp3"},
        {"audio_click","Assets/Audio/audio_click.mp3"},
    };
    private Dictionary<string, AudioClip> audioDict = new();

    private string curBgm;

    public void Init(){
        sources = new AudioSource[maxCount];
        //正常情况下初始只会获取到一个
        AudioSource[] existSources = MainManager.Instance.GetComponents<AudioSource>();
        for(int i = 0; i < existSources.Length; i++){
            if(i >= sources.Length){
                break;
            }
            sources[i] = existSources[i];
        }
        if(sources[0] == null){
            sources[0] = MainManager.Instance.gameObject.AddComponent<AudioSource>();
        }
        sources[0].loop = true;

        EventManager.Instance.AddEvent(EventType.FinishChangeScene, OnSceneLoaded);
    }
    public void OnSceneLoaded(object sender, object[] datas){
        string sceneName = datas[0].ToString();
        if(sceneName == "Home"){
            PlayBgm("homeBgm");
            curBgm = "homeBgm";
        }else if(sceneName == "Game"){
            PlayBgm("battleBgm"); 
            curBgm = "battleBgm";
        }
    }
    public void PlayBgm(string name){
        AudioSource bgmSource = sources[0];
        if(bgmSource.isPlaying && curBgm == name){
            return;
        }
        if(!audioDict.ContainsKey(name)){
            LoadAudio(name);
        }
        bgmSource.clip = audioDict[name];
        bgmSource.Play();
    }
    public void PlayEffect(string name){
        if(!audioDict.ContainsKey(name)){
            LoadAudio(name);
        }
        for(int i = 1; i < maxCount; i++){
            if(sources[i] == null){
                sources[i] = MainManager.Instance.gameObject.AddComponent<AudioSource>();
            }
            if(sources[i].isPlaying){
                continue;
            }
            sources[i].clip = audioDict[name];
            sources[i].Play();
        }
    }
    private void LoadAudio(string name){
        if(audioPath.TryGetValue(name, out string path)){
            AudioClip clip = LoadManager.Instance.LoadData<AudioClip>(name, path);
            audioDict[name] = clip;
        }
        Debug.Log("dont have audio " + name);
    }
}

