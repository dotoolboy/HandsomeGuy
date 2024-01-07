using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectManager
{
    public List<Projectile> Projectiles { get; private set; } = new();
    public Player Player { get; private set; } = new();
    public List<Enemy> Enemies { get; private set; } = new();

    public T Spawn<T>(string key, Vector2 position) where T : MonoBehaviour
    {
        System.Type type = typeof(T);

        if (type == typeof(Player))
        {
            GameObject obj = PhotonNetwork.Instantiate("Prefabs/Player", position, Quaternion.identity);
            Debug.Log(PhotonNetwork.CurrentRoom.Players.Count);

            //GameObject obj = Main.ResourceManager.Instantiate("SeongGyuPlayer");
            //obj.transform.position = position;
            
            Player player = obj.GetOrAddComponent<Player>();
            PhotonView pv = player.GetComponent<PhotonView>();

            player.SetInfo();

            if (pv.IsMine)
            {
                player.SetSprite($"{Main.GameManager.CharacterType}.sprite");
            }
            
            Player = player;

            return Player as T;
        }

        else if (type == typeof(Enemy))
        {
            GameObject obj = Main.ResourceManager.Instantiate($"Enemy.prefab", pooling: true);
            obj.transform.position = position;

            Enemy enemy = obj.GetOrAddComponent<Enemy>();
            enemy.SetInfo(key);
            Enemies.Add(enemy);

            return enemy as T;
        }

        else if (type == typeof(Boss))
        {
            GameObject obj = Main.ResourceManager.Instantiate($"Boss.prefab", pooling: true);
            obj.transform.position = position;

            Enemy enemy = obj.GetOrAddComponent<Boss>();
            enemy.SetInfo(key);
            Enemies.Add(enemy);

            return enemy as T;
        }


        else if (type == typeof(Projectile))
        {
            GameObject obj = Main.ResourceManager.Instantiate($"Gunbullet.prefab", pooling: true);
            obj.transform.position = position;

            Projectile projectile = obj.GetOrAddComponent<Projectile>();
            Projectiles.Add(projectile);

            return projectile as T;
        }
        
        return null;
    }


    public void Despawn<T>(T obj) where T : MonoBehaviour
    {
        System.Type type = typeof(T);

        if (type == typeof(Enemy))
        {
            Enemies.Remove(obj as Enemy);
            Main.ResourceManager.Destroy(obj.gameObject);
        }

        if (type == typeof(Boss))
        {
            Enemies.Remove(obj as Boss);
            Main.ResourceManager.Destroy(obj.gameObject);
        }

        if (type == typeof(Projectile))
        {
            Projectiles.Remove(obj as Projectile);
            Main.ResourceManager.Destroy(obj.gameObject);
        }
  
    }
}