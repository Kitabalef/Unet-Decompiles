// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.ClientScene
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;

namespace UnityEngine.Networking
{
  /// <summary>
  /// 
  /// <para>
  /// A client manager which contains non-instance centrict client information and functions.
  /// </para>
  /// 
  /// </summary>
  public class ClientScene
  {
    private static List<PlayerController> s_LocalPlayers = new List<PlayerController>();
    internal static bool s_IsReady = false;
    internal static bool s_IsSpawnFinished = false;
    internal static NetworkScene s_NetworkScene = new NetworkScene();
    private static ObjectSpawnSceneMessage s_ObjectSpawnSceneMessage = new ObjectSpawnSceneMessage();
    private static ObjectSpawnFinishedMessage s_ObjectSpawnFinishedMessage = new ObjectSpawnFinishedMessage();
    private static ObjectDestroyMessage s_ObjectDestroyMessage = new ObjectDestroyMessage();
    private static ObjectSpawnMessage s_ObjectSpawnMessage = new ObjectSpawnMessage();
    private static OwnerMessage s_OwnerMessage = new OwnerMessage();
    private static List<ClientScene.PendingOwner> s_PendingOwnerIds = new List<ClientScene.PendingOwner>();
    private static NetworkConnection s_ReadyConnection;
    private static Dictionary<NetworkSceneId, NetworkIdentity> s_SpawnableObjects;

    /// <summary>
    /// 
    /// <para>
    /// A list of all players added to the game.
    /// </para>
    /// 
    /// </summary>
    public static List<PlayerController> localPlayers
    {
      get
      {
        return ClientScene.s_LocalPlayers;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Return true when a client connection has been set as ready.
    /// </para>
    /// 
    /// </summary>
    public static bool ready
    {
      get
      {
        return ClientScene.s_IsReady;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// This is a dictionary of networked objects that have been spawned on the client.
    /// </para>
    /// 
    /// </summary>
    public static Dictionary<NetworkInstanceId, NetworkIdentity> objects
    {
      get
      {
        return ClientScene.s_NetworkScene.m_LocalObjects;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// This is a dictionary of the prefabs that are registered on the client.
    /// </para>
    /// 
    /// </summary>
    public static Dictionary<NetworkHash128, GameObject> prefabs
    {
      get
      {
        return NetworkScene.s_GUIDToPrefab;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// This is dictionary of the disabled NetworkIdentity objects in the scene that could be spawned by messages from the server.
    /// </para>
    /// 
    /// </summary>
    public static Dictionary<NetworkSceneId, NetworkIdentity> spawnableObjects
    {
      get
      {
        return ClientScene.s_SpawnableObjects;
      }
    }

    internal static void Shutdown()
    {
      ClientScene.s_NetworkScene.Shutdown();
      ClientScene.s_LocalPlayers = new List<PlayerController>();
      ClientScene.s_PendingOwnerIds = new List<ClientScene.PendingOwner>();
      ClientScene.s_SpawnableObjects = (Dictionary<NetworkSceneId, NetworkIdentity>) null;
      ClientScene.s_ReadyConnection = (NetworkConnection) null;
      ClientScene.s_IsReady = false;
      ClientScene.s_IsSpawnFinished = false;
      NetworkTransport.Shutdown();
      NetworkTransport.Init();
    }

    internal static bool GetPlayerController(short playerControllerId, out PlayerController player)
    {
      player = (PlayerController) null;
      if ((int) playerControllerId >= ClientScene.localPlayers.Count)
      {
        if (LogFilter.logWarn)
          Debug.Log((object) ("ClientScene::GetPlayer: no local player found for: " + (object) playerControllerId));
        return false;
      }
      if (ClientScene.localPlayers[(int) playerControllerId] == null)
      {
        if (LogFilter.logWarn)
          Debug.LogWarning((object) ("ClientScene::GetPlayer: local player is null for: " + (object) playerControllerId));
        return false;
      }
      player = ClientScene.localPlayers[(int) playerControllerId];
      return (UnityEngine.Object) player.gameObject != (UnityEngine.Object) null;
    }

    internal static void InternalAddPlayer(NetworkIdentity view, short playerControllerId)
    {
      if (LogFilter.logDebug)
        Debug.LogWarning((object) ("ClientScene::InternalAddPlayer: playerControllerId : " + (object) playerControllerId));
      if ((int) playerControllerId >= ClientScene.s_LocalPlayers.Count)
      {
        if (LogFilter.logWarn)
          Debug.LogWarning((object) ("ClientScene::InternalAddPlayer: playerControllerId higher than expected: " + (object) playerControllerId));
        while ((int) playerControllerId >= ClientScene.s_LocalPlayers.Count)
          ClientScene.s_LocalPlayers.Add(new PlayerController());
      }
      PlayerController player = new PlayerController()
      {
        gameObject = view.gameObject,
        playerControllerId = playerControllerId,
        unetView = view
      };
      ClientScene.s_LocalPlayers[(int) playerControllerId] = player;
      ClientScene.s_ReadyConnection.SetPlayerController(player);
    }

    /// <summary>
    /// 
    /// <para>
    /// This is the local player ID for the player, for example like which controller a player is using. This is not the global overall player number.
    /// </para>
    /// 
    /// </summary>
    /// <param name="readyConn">The connection to become ready for this client.</param><param name="playerControllerId">The local player ID number	.</param>
    /// <returns>
    /// 
    /// <para>
    /// True if player was added.
    /// </para>
    /// 
    /// </returns>
    public static bool AddPlayer(short playerControllerId)
    {
      return ClientScene.AddPlayer((NetworkConnection) null, playerControllerId);
    }

    /// <summary>
    /// 
    /// <para>
    /// This is the local player ID for the player, for example like which controller a player is using. This is not the global overall player number.
    /// </para>
    /// 
    /// </summary>
    /// <param name="readyConn">The connection to become ready for this client.</param><param name="playerControllerId">The local player ID number	.</param>
    /// <returns>
    /// 
    /// <para>
    /// True if player was added.
    /// </para>
    /// 
    /// </returns>
    public static bool AddPlayer(NetworkConnection readyConn, short playerControllerId)
    {
      if ((int) playerControllerId < 0)
      {
        if (LogFilter.logError)
          Debug.LogError((object) ("ClientScene::AddPlayer: playerControllerId of " + (object) playerControllerId + " is negative"));
        return false;
      }
      if ((int) playerControllerId > 32)
      {
        if (LogFilter.logError)
        {
          object[] objArray = new object[4];
          int index1 = 0;
          string str1 = "ClientScene::AddPlayer: playerControllerId of ";
          objArray[index1] = (object) str1;
          int index2 = 1;
          // ISSUE: variable of a boxed type
          __Boxed<short> local1 = (ValueType) playerControllerId;
          objArray[index2] = (object) local1;
          int index3 = 2;
          string str2 = " is too high, max is ";
          objArray[index3] = (object) str2;
          int index4 = 3;
          // ISSUE: variable of a boxed type
          __Boxed<int> local2 = (ValueType) 32;
          objArray[index4] = (object) local2;
          Debug.LogError((object) string.Concat(objArray));
        }
        return false;
      }
      if ((int) playerControllerId > 16 && LogFilter.logWarn)
        Debug.LogWarning((object) ("ClientScene::AddPlayer: playerControllerId of " + (object) playerControllerId + " is unusually high"));
      while ((int) playerControllerId >= ClientScene.s_LocalPlayers.Count)
        ClientScene.s_LocalPlayers.Add(new PlayerController());
      if (readyConn == null)
      {
        if (!ClientScene.s_IsReady)
        {
          if (LogFilter.logError)
            Debug.LogError((object) "Must call AddPlayer() with a connection the first time to become ready.");
          return false;
        }
      }
      else
      {
        ClientScene.s_IsReady = true;
        ClientScene.s_ReadyConnection = readyConn;
      }
      PlayerController playerController;
      if (ClientScene.s_ReadyConnection.GetPlayerController(playerControllerId, out playerController) && playerController.IsValid && (UnityEngine.Object) playerController.gameObject != (UnityEngine.Object) null)
      {
        if (LogFilter.logError)
          Debug.LogError((object) ("ClientScene::AddPlayer: playerControllerId of " + (object) playerControllerId + " already in use."));
        return false;
      }
      if (LogFilter.logDebug)
      {
        object[] objArray = new object[5];
        int index1 = 0;
        string str1 = "ClientScene::AddPlayer() for ID ";
        objArray[index1] = (object) str1;
        int index2 = 1;
        // ISSUE: variable of a boxed type
        __Boxed<short> local = (ValueType) playerControllerId;
        objArray[index2] = (object) local;
        int index3 = 2;
        string str2 = " called with connection [";
        objArray[index3] = (object) str2;
        int index4 = 3;
        NetworkConnection networkConnection = ClientScene.s_ReadyConnection;
        objArray[index4] = (object) networkConnection;
        int index5 = 4;
        string str3 = "]";
        objArray[index5] = (object) str3;
        Debug.Log((object) string.Concat(objArray));
      }
      ClientScene.s_ReadyConnection.Send((short) 37, (MessageBase) new AddPlayerMessage()
      {
        playerControllerId = playerControllerId
      });
      return true;
    }

    /// <summary>
    /// 
    /// <para>
    /// Remove the specified player ID from the game.
    /// </para>
    /// 
    /// </summary>
    /// <param name="id">The local player ID number	.</param><param name="playerControllerId"/>
    /// <returns>
    /// 
    /// <para>
    /// Returns true if the player was successfully destoyed and removed.
    /// </para>
    /// 
    /// </returns>
    public static bool RemovePlayer(short playerControllerId)
    {
      if (LogFilter.logDebug)
      {
        object[] objArray = new object[5];
        int index1 = 0;
        string str1 = "ClientScene::RemovePlayer() for ID ";
        objArray[index1] = (object) str1;
        int index2 = 1;
        // ISSUE: variable of a boxed type
        __Boxed<short> local = (ValueType) playerControllerId;
        objArray[index2] = (object) local;
        int index3 = 2;
        string str2 = " called with connection [";
        objArray[index3] = (object) str2;
        int index4 = 3;
        NetworkConnection networkConnection = ClientScene.s_ReadyConnection;
        objArray[index4] = (object) networkConnection;
        int index5 = 4;
        string str3 = "]";
        objArray[index5] = (object) str3;
        Debug.Log((object) string.Concat(objArray));
      }
      PlayerController playerController;
      if (ClientScene.s_ReadyConnection.GetPlayerController(playerControllerId, out playerController))
      {
        ClientScene.s_ReadyConnection.Send((short) 38, (MessageBase) new RemovePlayerMessage()
        {
          playerControllerId = playerControllerId
        });
        ClientScene.s_ReadyConnection.RemovePlayerController(playerControllerId);
        ClientScene.s_LocalPlayers[(int) playerControllerId] = new PlayerController();
        UnityEngine.Object.Destroy((UnityEngine.Object) playerController.gameObject);
        return true;
      }
      if (LogFilter.logError)
        Debug.LogError((object) ("Failed to find player ID " + (object) playerControllerId));
      return false;
    }

    /// <summary>
    /// 
    /// <para>
    /// Signal that the client connection is ready to enter the game.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">The client connection which is ready.</param>
    public static bool Ready(NetworkConnection conn)
    {
      if (ClientScene.s_IsReady)
      {
        if (LogFilter.logError)
          Debug.LogError((object) "A connection has already been set as ready. There can only be one.");
        return false;
      }
      if (LogFilter.logDebug)
        Debug.Log((object) ("ClientScene::Ready() called with connection [" + (object) conn + "]"));
      if (conn != null)
      {
        ReadyMessage readyMessage = new ReadyMessage();
        conn.Send((short) 35, (MessageBase) readyMessage);
        ClientScene.s_IsReady = true;
        ClientScene.s_ReadyConnection = conn;
        ClientScene.s_ReadyConnection.isReady = true;
        return true;
      }
      if (LogFilter.logError)
        Debug.LogError((object) "Ready() called with invalid connection object: conn=null");
      return false;
    }

    /// <summary>
    /// 
    /// <para>
    /// Create and connect a local client instance to the local server.
    /// </para>
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// 
    /// <para>
    /// A client object for communicating with the local server.
    /// </para>
    /// 
    /// </returns>
    public static NetworkClient ConnectLocalServer()
    {
      LocalClient localClient = new LocalClient();
      NetworkServer.instance.ActivateLocalClientScene();
      localClient.InternalConnectLocalServer();
      return (NetworkClient) localClient;
    }

    internal static void HandleClientDisconnect(NetworkConnection conn)
    {
      if (ClientScene.s_ReadyConnection != conn || !ClientScene.s_IsReady)
        return;
      ClientScene.s_IsReady = false;
      ClientScene.s_ReadyConnection = (NetworkConnection) null;
    }

    internal static void PrepareToSpawnSceneObjects()
    {
      ClientScene.s_SpawnableObjects = new Dictionary<NetworkSceneId, NetworkIdentity>();
      foreach (NetworkIdentity networkIdentity in Resources.FindObjectsOfTypeAll<NetworkIdentity>())
      {
        if (!networkIdentity.gameObject.activeSelf && networkIdentity.gameObject.hideFlags != HideFlags.NotEditable && networkIdentity.gameObject.hideFlags != HideFlags.HideAndDontSave && !networkIdentity.sceneId.IsEmpty())
        {
          ClientScene.s_SpawnableObjects[networkIdentity.sceneId] = networkIdentity;
          if (LogFilter.logDebug)
            Debug.Log((object) ("ClientScene::PrepareSpawnObjects sceneId:" + (object) networkIdentity.sceneId));
        }
      }
    }

    internal static NetworkIdentity SpawnSceneObject(NetworkSceneId sceneId)
    {
      if (!ClientScene.s_SpawnableObjects.ContainsKey(sceneId))
        return (NetworkIdentity) null;
      NetworkIdentity networkIdentity = ClientScene.s_SpawnableObjects[sceneId];
      ClientScene.s_SpawnableObjects.Remove(sceneId);
      return networkIdentity;
    }

    internal static void RegisterSystemHandlers(NetworkClient client, bool localClient)
    {
      if (localClient)
      {
        client.RegisterHandlerSafe((short) 1, new NetworkMessageDelegate(ClientScene.OnLocalClientObjectDestroy));
        client.RegisterHandlerSafe((short) 13, new NetworkMessageDelegate(ClientScene.OnLocalClientObjectHide));
        client.RegisterHandlerSafe((short) 3, new NetworkMessageDelegate(ClientScene.OnLocalClientObjectSpawn));
        client.RegisterHandlerSafe((short) 10, new NetworkMessageDelegate(ClientScene.OnLocalClientObjectSpawnScene));
      }
      else
      {
        client.RegisterHandlerSafe((short) 3, new NetworkMessageDelegate(ClientScene.OnObjectSpawn));
        client.RegisterHandlerSafe((short) 10, new NetworkMessageDelegate(ClientScene.OnObjectSpawnScene));
        client.RegisterHandlerSafe((short) 12, new NetworkMessageDelegate(ClientScene.OnObjectSpawnFinished));
        client.RegisterHandlerSafe((short) 1, new NetworkMessageDelegate(ClientScene.OnObjectDestroy));
        client.RegisterHandlerSafe((short) 13, new NetworkMessageDelegate(ClientScene.OnObjectDestroy));
        client.RegisterHandlerSafe((short) 8, new NetworkMessageDelegate(ClientScene.OnUpdateVarsMessage));
        client.RegisterHandlerSafe((short) 4, new NetworkMessageDelegate(ClientScene.OnOwnerMessage));
        client.RegisterHandlerSafe((short) 9, new NetworkMessageDelegate(ClientScene.OnSyncListMessage));
        client.RegisterHandlerSafe((short) 40, new NetworkMessageDelegate(NetworkAnimator.OnAnimationClientMessage));
        client.RegisterHandlerSafe((short) 41, new NetworkMessageDelegate(NetworkAnimator.OnAnimationParametersClientMessage));
      }
      client.RegisterHandlerSafe((short) 2, new NetworkMessageDelegate(ClientScene.OnRPCMessage));
      client.RegisterHandlerSafe((short) 7, new NetworkMessageDelegate(ClientScene.OnSyncEventMessage));
      client.RegisterHandlerSafe((short) 42, new NetworkMessageDelegate(NetworkAnimator.OnAnimationTriggerClientMessage));
    }

    internal static string GetStringForAssetId(NetworkHash128 assetId)
    {
      GameObject prefab;
      if (NetworkScene.GetPrefab(assetId, out prefab))
        return prefab.name;
      SpawnDelegate handler;
      if (NetworkScene.GetSpawnHandler(assetId, out handler))
        return handler.Method.Name;
      return "unknown";
    }

    /// <summary>
    /// 
    /// <para>
    /// Registers a prefab with the UNET spawning system.
    /// </para>
    /// 
    /// </summary>
    /// <param name="prefab">A Prefab that will be spawned.</param><param name="spawnHandler">A method to use as a custom spawnhandler on clients.</param><param name="unspawnHandler">A method to use as a custom un-spawnhandler on clients.</param>
    public static void RegisterPrefab(GameObject prefab)
    {
      NetworkScene.RegisterPrefab(prefab);
    }

    /// <summary>
    /// 
    /// <para>
    /// Registers a prefab with the UNET spawning system.
    /// </para>
    /// 
    /// </summary>
    /// <param name="prefab">A Prefab that will be spawned.</param><param name="spawnHandler">A method to use as a custom spawnhandler on clients.</param><param name="unspawnHandler">A method to use as a custom un-spawnhandler on clients.</param>
    public static void RegisterPrefab(GameObject prefab, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
    {
      NetworkScene.RegisterPrefab(prefab, spawnHandler, unspawnHandler);
    }

    /// <summary>
    /// 
    /// <para>
    /// Removes a registered spawn prefab.
    /// </para>
    /// 
    /// </summary>
    /// <param name="prefab"/>
    public static void UnregisterPrefab(GameObject prefab)
    {
      NetworkScene.UnregisterPrefab(prefab);
    }

    /// <summary>
    /// 
    /// <para>
    /// This is an advanced spawning funciotn that registers a custom assetId with the UNET spawning system.
    /// </para>
    /// 
    /// </summary>
    /// <param name="assetId">Custom assetId string.</param><param name="spawnHandler">A method to use as a custom spawnhandler on clients.</param><param name="unspawnHandler">A method to use as a custom un-spawnhandler on clients.</param>
    public static void RegisterSpawnHandler(NetworkHash128 assetId, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
    {
      NetworkScene.RegisterSpawnHandler(assetId, spawnHandler, unspawnHandler);
    }

    /// <summary>
    /// 
    /// <para>
    /// Removes a registered spawn handler function.
    /// </para>
    /// 
    /// </summary>
    /// <param name="assetId"/>
    public static void UnregisterSpawnHandler(NetworkHash128 assetId)
    {
      NetworkScene.UnregisterSpawnHandler(assetId);
    }

    /// <summary>
    /// 
    /// <para>
    /// This clears the registered spawn prefabs and spawn handler functions for this client.
    /// </para>
    /// 
    /// </summary>
    public static void ClearSpawners()
    {
      NetworkScene.ClearSpawners();
    }

    /// <summary>
    /// 
    /// <para>
    /// Destroys all networked objects on the client.
    /// </para>
    /// 
    /// </summary>
    public static void DestroyAllClientObjects()
    {
      ClientScene.s_NetworkScene.DestroyAllClientObjects();
    }

    public static void SetLocalObject(NetworkInstanceId netId, GameObject obj)
    {
      ClientScene.s_NetworkScene.SetLocalObject(netId, obj, ClientScene.s_IsSpawnFinished, false);
    }

    public static GameObject FindLocalObject(NetworkInstanceId netId)
    {
      return ClientScene.s_NetworkScene.FindLocalObject(netId);
    }

    private static void ApplySpawnPayload(NetworkIdentity uv, Vector3 position, byte[] payload, NetworkInstanceId netId, GameObject newGameObject)
    {
      uv.transform.position = position;
      if (payload != null && payload.Length > 0)
      {
        NetworkReader reader = new NetworkReader(payload);
        uv.OnUpdateVars(reader, true);
      }
      if ((UnityEngine.Object) newGameObject == (UnityEngine.Object) null)
        return;
      newGameObject.SetActive(true);
      uv.SetNetworkInstanceId(netId);
      ClientScene.SetLocalObject(netId, newGameObject);
      if (!ClientScene.s_IsSpawnFinished)
        return;
      uv.OnStartClient();
      ClientScene.CheckForOwner(uv);
    }

    private static void OnObjectSpawn(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<ObjectSpawnMessage>(ClientScene.s_ObjectSpawnMessage);
      if (!ClientScene.s_ObjectSpawnMessage.assetId.IsValid())
      {
        if (!LogFilter.logError)
          return;
        Debug.LogError((object) ("OnObjSpawn netId: " + (object) ClientScene.s_ObjectSpawnMessage.netId + " has invalid asset Id"));
      }
      else
      {
        if (LogFilter.logDebug)
        {
          object[] objArray = new object[7];
          int index1 = 0;
          string str1 = "Client spawn handler instantiating [netId:";
          objArray[index1] = (object) str1;
          int index2 = 1;
          // ISSUE: variable of a boxed type
          __Boxed<NetworkInstanceId> local1 = (ValueType) ClientScene.s_ObjectSpawnMessage.netId;
          objArray[index2] = (object) local1;
          int index3 = 2;
          string str2 = " asset ID:";
          objArray[index3] = (object) str2;
          int index4 = 3;
          // ISSUE: variable of a boxed type
          __Boxed<NetworkHash128> local2 = (ValueType) ClientScene.s_ObjectSpawnMessage.assetId;
          objArray[index4] = (object) local2;
          int index5 = 4;
          string str3 = " pos:";
          objArray[index5] = (object) str3;
          int index6 = 5;
          // ISSUE: variable of a boxed type
          __Boxed<Vector3> local3 = (ValueType) ClientScene.s_ObjectSpawnMessage.position;
          objArray[index6] = (object) local3;
          int index7 = 6;
          string str4 = "]";
          objArray[index7] = (object) str4;
          Debug.Log((object) string.Concat(objArray));
        }
        NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Incoming, (short) 3, ClientScene.GetStringForAssetId(ClientScene.s_ObjectSpawnMessage.assetId), 1);
        NetworkIdentity uv;
        if (ClientScene.s_NetworkScene.GetNetworkIdentity(ClientScene.s_ObjectSpawnMessage.netId, out uv))
        {
          ClientScene.ApplySpawnPayload(uv, ClientScene.s_ObjectSpawnMessage.position, ClientScene.s_ObjectSpawnMessage.payload, ClientScene.s_ObjectSpawnMessage.netId, (GameObject) null);
        }
        else
        {
          GameObject prefab;
          if (NetworkScene.GetPrefab(ClientScene.s_ObjectSpawnMessage.assetId, out prefab))
          {
            GameObject newGameObject = (GameObject) UnityEngine.Object.Instantiate((UnityEngine.Object) prefab, ClientScene.s_ObjectSpawnMessage.position, Quaternion.identity);
            uv = newGameObject.GetComponent<NetworkIdentity>();
            if ((UnityEngine.Object) uv == (UnityEngine.Object) null)
            {
              if (!LogFilter.logError)
                return;
              Debug.LogError((object) ("Client object spawned for " + (object) ClientScene.s_ObjectSpawnMessage.assetId + " does not have a NetworkIdentity"));
            }
            else
              ClientScene.ApplySpawnPayload(uv, ClientScene.s_ObjectSpawnMessage.position, ClientScene.s_ObjectSpawnMessage.payload, ClientScene.s_ObjectSpawnMessage.netId, newGameObject);
          }
          else
          {
            SpawnDelegate handler;
            if (NetworkScene.GetSpawnHandler(ClientScene.s_ObjectSpawnMessage.assetId, out handler))
            {
              GameObject newGameObject = handler(ClientScene.s_ObjectSpawnMessage.position, ClientScene.s_ObjectSpawnMessage.assetId);
              if ((UnityEngine.Object) newGameObject == (UnityEngine.Object) null)
              {
                if (!LogFilter.logWarn)
                  return;
                Debug.LogWarning((object) ("Client spawn handler for " + (object) ClientScene.s_ObjectSpawnMessage.assetId + " returned null"));
              }
              else
              {
                uv = newGameObject.GetComponent<NetworkIdentity>();
                if ((UnityEngine.Object) uv == (UnityEngine.Object) null)
                {
                  if (!LogFilter.logError)
                    return;
                  Debug.LogError((object) ("Client object spawned for " + (object) ClientScene.s_ObjectSpawnMessage.assetId + " does not have a network identity"));
                }
                else
                {
                  uv.SetDynamicAssetId(ClientScene.s_ObjectSpawnMessage.assetId);
                  ClientScene.ApplySpawnPayload(uv, ClientScene.s_ObjectSpawnMessage.position, ClientScene.s_ObjectSpawnMessage.payload, ClientScene.s_ObjectSpawnMessage.netId, newGameObject);
                }
              }
            }
            else
            {
              if (!LogFilter.logError)
                return;
              object[] objArray = new object[4];
              int index1 = 0;
              string str1 = "Failed to spawn server object, assetId=";
              objArray[index1] = (object) str1;
              int index2 = 1;
              // ISSUE: variable of a boxed type
              __Boxed<NetworkHash128> local1 = (ValueType) ClientScene.s_ObjectSpawnMessage.assetId;
              objArray[index2] = (object) local1;
              int index3 = 2;
              string str2 = " netId=";
              objArray[index3] = (object) str2;
              int index4 = 3;
              // ISSUE: variable of a boxed type
              __Boxed<NetworkInstanceId> local2 = (ValueType) ClientScene.s_ObjectSpawnMessage.netId;
              objArray[index4] = (object) local2;
              Debug.LogError((object) string.Concat(objArray));
            }
          }
        }
      }
    }

    private static void OnObjectSpawnScene(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<ObjectSpawnSceneMessage>(ClientScene.s_ObjectSpawnSceneMessage);
      if (LogFilter.logDebug)
      {
        object[] objArray = new object[6];
        int index1 = 0;
        string str1 = "Client spawn scene handler instantiating [netId:";
        objArray[index1] = (object) str1;
        int index2 = 1;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local1 = (ValueType) ClientScene.s_ObjectSpawnSceneMessage.netId;
        objArray[index2] = (object) local1;
        int index3 = 2;
        string str2 = " sceneId:";
        objArray[index3] = (object) str2;
        int index4 = 3;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkSceneId> local2 = (ValueType) ClientScene.s_ObjectSpawnSceneMessage.sceneId;
        objArray[index4] = (object) local2;
        int index5 = 4;
        string str3 = " pos:";
        objArray[index5] = (object) str3;
        int index6 = 5;
        // ISSUE: variable of a boxed type
        __Boxed<Vector3> local3 = (ValueType) ClientScene.s_ObjectSpawnSceneMessage.position;
        objArray[index6] = (object) local3;
        Debug.Log((object) string.Concat(objArray));
      }
      NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Incoming, (short) 10, "sceneId", 1);
      NetworkIdentity uv1;
      if (ClientScene.s_NetworkScene.GetNetworkIdentity(ClientScene.s_ObjectSpawnSceneMessage.netId, out uv1))
      {
        ClientScene.ApplySpawnPayload(uv1, ClientScene.s_ObjectSpawnSceneMessage.position, ClientScene.s_ObjectSpawnSceneMessage.payload, ClientScene.s_ObjectSpawnSceneMessage.netId, uv1.gameObject);
      }
      else
      {
        NetworkIdentity uv2 = ClientScene.SpawnSceneObject(ClientScene.s_ObjectSpawnSceneMessage.sceneId);
        if ((UnityEngine.Object) uv2 == (UnityEngine.Object) null)
        {
          if (!LogFilter.logError)
            return;
          Debug.LogError((object) ("Spawn scene object not found for " + (object) ClientScene.s_ObjectSpawnSceneMessage.sceneId));
        }
        else
        {
          if (LogFilter.logDebug)
          {
            object[] objArray = new object[6];
            int index1 = 0;
            string str1 = "Client spawn for [netId:";
            objArray[index1] = (object) str1;
            int index2 = 1;
            // ISSUE: variable of a boxed type
            __Boxed<NetworkInstanceId> local1 = (ValueType) ClientScene.s_ObjectSpawnSceneMessage.netId;
            objArray[index2] = (object) local1;
            int index3 = 2;
            string str2 = "] [sceneId:";
            objArray[index3] = (object) str2;
            int index4 = 3;
            // ISSUE: variable of a boxed type
            __Boxed<NetworkSceneId> local2 = (ValueType) ClientScene.s_ObjectSpawnSceneMessage.sceneId;
            objArray[index4] = (object) local2;
            int index5 = 4;
            string str3 = "] obj:";
            objArray[index5] = (object) str3;
            int index6 = 5;
            string name = uv2.gameObject.name;
            objArray[index6] = (object) name;
            Debug.Log((object) string.Concat(objArray));
          }
          ClientScene.ApplySpawnPayload(uv2, ClientScene.s_ObjectSpawnSceneMessage.position, ClientScene.s_ObjectSpawnSceneMessage.payload, ClientScene.s_ObjectSpawnSceneMessage.netId, uv2.gameObject);
        }
      }
    }

    private static void OnObjectSpawnFinished(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<ObjectSpawnFinishedMessage>(ClientScene.s_ObjectSpawnFinishedMessage);
      if (LogFilter.logDebug)
        Debug.Log((object) ("SpawnFinished:" + (object) ClientScene.s_ObjectSpawnFinishedMessage.state));
      if ((int) ClientScene.s_ObjectSpawnFinishedMessage.state == 0)
      {
        ClientScene.PrepareToSpawnSceneObjects();
        ClientScene.s_IsSpawnFinished = false;
      }
      else
      {
        using (Dictionary<NetworkInstanceId, NetworkIdentity>.ValueCollection.Enumerator enumerator = ClientScene.objects.Values.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            NetworkIdentity current = enumerator.Current;
            if (!current.isClient)
            {
              current.OnStartClient();
              ClientScene.CheckForOwner(current);
            }
          }
        }
        ClientScene.s_IsSpawnFinished = true;
      }
    }

    private static void OnObjectDestroy(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<ObjectDestroyMessage>(ClientScene.s_ObjectDestroyMessage);
      if (LogFilter.logDebug)
        Debug.Log((object) ("ClientScene::OnObjDestroy netId:" + (object) ClientScene.s_ObjectDestroyMessage.netId));
      NetworkIdentity uv;
      if (ClientScene.s_NetworkScene.GetNetworkIdentity(ClientScene.s_ObjectDestroyMessage.netId, out uv))
      {
        NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Incoming, (short) 1, ClientScene.GetStringForAssetId(uv.assetId), 1);
        uv.OnNetworkDestroy();
        if (!NetworkScene.InvokeUnSpawnHandler(uv.assetId, uv.gameObject))
        {
          if (uv.sceneId.IsEmpty())
          {
            UnityEngine.Object.Destroy((UnityEngine.Object) uv.gameObject);
          }
          else
          {
            uv.gameObject.SetActive(false);
            ClientScene.s_SpawnableObjects[uv.sceneId] = uv;
          }
        }
        ClientScene.s_NetworkScene.RemoveLocalObject(ClientScene.s_ObjectDestroyMessage.netId);
      }
      else
      {
        if (!LogFilter.logDebug)
          return;
        Debug.LogWarning((object) ("Did not find target for destroy message for " + (object) ClientScene.s_ObjectDestroyMessage.netId));
      }
    }

    private static void OnLocalClientObjectDestroy(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<ObjectDestroyMessage>(ClientScene.s_ObjectDestroyMessage);
      if (LogFilter.logDebug)
        Debug.Log((object) ("ClientScene::OnLocalObjectObjDestroy netId:" + (object) ClientScene.s_ObjectDestroyMessage.netId));
      ClientScene.s_NetworkScene.RemoveLocalObject(ClientScene.s_ObjectDestroyMessage.netId);
    }

    private static void OnLocalClientObjectHide(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<ObjectDestroyMessage>(ClientScene.s_ObjectDestroyMessage);
      if (LogFilter.logDebug)
        Debug.Log((object) ("ClientScene::OnLocalObjectObjHide netId:" + (object) ClientScene.s_ObjectDestroyMessage.netId));
      NetworkIdentity uv;
      if (!ClientScene.s_NetworkScene.GetNetworkIdentity(ClientScene.s_ObjectDestroyMessage.netId, out uv))
        return;
      uv.OnSetLocalVisibility(false);
    }

    private static void OnLocalClientObjectSpawn(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<ObjectSpawnMessage>(ClientScene.s_ObjectSpawnMessage);
      NetworkIdentity uv;
      if (!ClientScene.s_NetworkScene.GetNetworkIdentity(ClientScene.s_ObjectSpawnMessage.netId, out uv))
        return;
      uv.OnSetLocalVisibility(true);
    }

    private static void OnLocalClientObjectSpawnScene(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<ObjectSpawnSceneMessage>(ClientScene.s_ObjectSpawnSceneMessage);
      NetworkIdentity uv;
      if (!ClientScene.s_NetworkScene.GetNetworkIdentity(ClientScene.s_ObjectSpawnSceneMessage.netId, out uv))
        return;
      uv.OnSetLocalVisibility(true);
    }

    private static void OnUpdateVarsMessage(NetworkMessage netMsg)
    {
      NetworkInstanceId netId = netMsg.reader.ReadNetworkId();
      if (LogFilter.logDev)
      {
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "ClientScene::OnUpdateVarsMessage ";
        objArray[index1] = (object) str1;
        int index2 = 1;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local1 = (ValueType) netId;
        objArray[index2] = (object) local1;
        int index3 = 2;
        string str2 = " channel:";
        objArray[index3] = (object) str2;
        int index4 = 3;
        // ISSUE: variable of a boxed type
        __Boxed<int> local2 = (ValueType) netMsg.channelId;
        objArray[index4] = (object) local2;
        Debug.Log((object) string.Concat(objArray));
      }
      NetworkIdentity uv;
      if (ClientScene.s_NetworkScene.GetNetworkIdentity(netId, out uv))
      {
        uv.OnUpdateVars(netMsg.reader, false);
      }
      else
      {
        if (!LogFilter.logWarn)
          return;
        Debug.LogWarning((object) ("Did not find target for sync message for " + (object) netId));
      }
    }

    private static void OnRPCMessage(NetworkMessage netMsg)
    {
      int cmdHash = (int) netMsg.reader.ReadPackedUInt32();
      NetworkInstanceId netId = netMsg.reader.ReadNetworkId();
      if (LogFilter.logDebug)
      {
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "ClientScene::OnRPCMessage hash:";
        objArray[index1] = (object) str1;
        int index2 = 1;
        // ISSUE: variable of a boxed type
        __Boxed<int> local1 = (ValueType) cmdHash;
        objArray[index2] = (object) local1;
        int index3 = 2;
        string str2 = " netId:";
        objArray[index3] = (object) str2;
        int index4 = 3;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local2 = (ValueType) netId;
        objArray[index4] = (object) local2;
        Debug.Log((object) string.Concat(objArray));
      }
      NetworkIdentity uv;
      if (ClientScene.s_NetworkScene.GetNetworkIdentity(netId, out uv))
      {
        uv.HandleRPC(cmdHash, netMsg.reader);
      }
      else
      {
        if (!LogFilter.logWarn)
          return;
        Debug.LogWarning((object) ("Did not find target for RPC message for " + (object) netId));
      }
    }

    private static void OnSyncEventMessage(NetworkMessage netMsg)
    {
      int cmdHash = (int) netMsg.reader.ReadPackedUInt32();
      NetworkInstanceId netId = netMsg.reader.ReadNetworkId();
      if (LogFilter.logDebug)
        Debug.Log((object) ("ClientScene::OnSyncEventMessage " + (object) netId));
      NetworkIdentity uv;
      if (ClientScene.s_NetworkScene.GetNetworkIdentity(netId, out uv))
        uv.HandleSyncEvent(cmdHash, netMsg.reader);
      else if (LogFilter.logWarn)
        Debug.LogWarning((object) ("Did not find target for SyncEvent message for " + (object) netId));
      NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Outgoing, (short) 7, NetworkBehaviour.GetCmdHashHandlerName(cmdHash), 1);
    }

    private static void OnSyncListMessage(NetworkMessage netMsg)
    {
      NetworkInstanceId netId = netMsg.reader.ReadNetworkId();
      int cmdHash = (int) netMsg.reader.ReadPackedUInt32();
      if (LogFilter.logDebug)
        Debug.Log((object) ("ClientScene::OnSyncListMessage " + (object) netId));
      NetworkIdentity uv;
      if (ClientScene.s_NetworkScene.GetNetworkIdentity(netId, out uv))
        uv.HandleSyncList(cmdHash, netMsg.reader);
      else if (LogFilter.logWarn)
        Debug.LogWarning((object) ("Did not find target for SyncList message for " + (object) netId));
      NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Outgoing, (short) 9, NetworkBehaviour.GetCmdHashHandlerName(cmdHash), 1);
    }

    private static void OnOwnerMessage(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<OwnerMessage>(ClientScene.s_OwnerMessage);
      if (LogFilter.logDebug)
      {
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "ClientScene::OnOwnerMessage - connectionId=";
        objArray[index1] = (object) str1;
        int index2 = 1;
        // ISSUE: variable of a boxed type
        __Boxed<int> local1 = (ValueType) netMsg.conn.connectionId;
        objArray[index2] = (object) local1;
        int index3 = 2;
        string str2 = " netId: ";
        objArray[index3] = (object) str2;
        int index4 = 3;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local2 = (ValueType) ClientScene.s_OwnerMessage.netId;
        objArray[index4] = (object) local2;
        Debug.Log((object) string.Concat(objArray));
      }
      PlayerController playerController;
      if (netMsg.conn.GetPlayerController(ClientScene.s_OwnerMessage.playerControllerId, out playerController))
        playerController.unetView.SetNotLocalPlayer();
      NetworkIdentity uv;
      if (ClientScene.s_NetworkScene.GetNetworkIdentity(ClientScene.s_OwnerMessage.netId, out uv))
      {
        uv.SetConnectionToServer(netMsg.conn);
        uv.SetLocalPlayer(ClientScene.s_OwnerMessage.playerControllerId);
        ClientScene.InternalAddPlayer(uv, ClientScene.s_OwnerMessage.playerControllerId);
      }
      else
      {
        ClientScene.PendingOwner pendingOwner = new ClientScene.PendingOwner()
        {
          netId = ClientScene.s_OwnerMessage.netId,
          playerControllerId = ClientScene.s_OwnerMessage.playerControllerId
        };
        ClientScene.s_PendingOwnerIds.Add(pendingOwner);
      }
    }

    private static void CheckForOwner(NetworkIdentity uv)
    {
      for (int index = 0; index < ClientScene.s_PendingOwnerIds.Count; ++index)
      {
        ClientScene.PendingOwner pendingOwner = ClientScene.s_PendingOwnerIds[index];
        if (pendingOwner.netId == uv.netId)
        {
          uv.SetConnectionToServer(ClientScene.s_ReadyConnection);
          uv.SetLocalPlayer(pendingOwner.playerControllerId);
          if (LogFilter.logDev)
            Debug.Log((object) ("ClientScene::OnOwnerMessage - player=" + uv.gameObject.name));
          if (ClientScene.s_ReadyConnection.connectionId < 0)
          {
            if (!LogFilter.logError)
              break;
            Debug.LogError((object) "Owner message received on a local client.");
            break;
          }
          ClientScene.InternalAddPlayer(uv, pendingOwner.playerControllerId);
          ClientScene.s_PendingOwnerIds.RemoveAt(index);
          break;
        }
      }
    }

    private struct PendingOwner
    {
      public NetworkInstanceId netId;
      public short playerControllerId;
    }
  }
}
