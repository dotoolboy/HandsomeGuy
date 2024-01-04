using Data;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.U2D.Animation;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class DataManager
{
    public Dictionary<string, EnemyData> Enemy = new();
    public Dictionary<string, Data.Skill> SkillDict { get; private set; } = new Dictionary<string, Data.Skill>();

    public void Initialize()
    {
        SkillDict = LoadJson<Data.SkillData, string, Data.Skill>("skillData").MakeDictionary();
        Enemy = LoadJson<EnemyDataLoader, string, EnemyData>("EnemyData").MakeDictionary();
    }


    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Main.ResourceManager.GetResource<TextAsset>(path);
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }

}

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDictionary();
}
