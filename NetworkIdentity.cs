// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkIdentity
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;

namespace UnityEngine.Networking
{
  /// <summary>
  /// 
  /// <para>
  /// A component used to add an object to the UNET networking system.
  /// </para>
  /// 
  /// </summary>
  [ExecuteInEditMode]
  [AddComponentMenu("Network/NetworkIdentity")]
  public sealed class NetworkIdentity : MonoBehaviour
  {
    private static uint s_NextNetworkId = 1U;
    private static NetworkWriter s_UpdateWriter = new NetworkWriter();
    private NetworkInstanceId m_NetId = new NetworkInstanceId();
    internal short m_PlayerId = (short) -1;
    [SerializeField]
    private NetworkSceneId m_SceneId;
    [SerializeField]
    private NetworkHash128 m_AssetId;
    [SerializeField]
    private bool m_ServerOnly;
    [SerializeField]
    private bool m_LocalPlayerAuthority;
    private bool m_IsClient;
    private bool m_IsServer;
    private bool m_HasAuthority;
    private bool m_IsLocalPlayer;
    internal NetworkConnection m_ConnectionToServer;
    internal NetworkConnection m_ConnectionToClient;
    private NetworkBehaviour[] m_NetworkBehaviours;
    private List<NetworkConnection> m_Observers;

    /// <summary>
    /// 
    /// <para>
    /// Returns true if running as a client and this object was spawned by a server.
    /// </para>
    /// 
    /// </summary>
    public bool isClient
    {
      get
      {
        return this.m_IsClient;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Returns true if running as a server, which spawned the object.
    /// </para>
    /// 
    /// </summary>
    public bool isServer
    {
      get
      {
        return this.m_IsServer;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// This returns true if this object is the authoritative version of the object in the distributed network application.
    /// </para>
    /// 
    /// </summary>
    public bool hasAuthority
    {
      get
      {
        return this.m_HasAuthority;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Unique identifier for this particular object instance, used for tracking objects between networked clients and the server.
    /// </para>
    /// 
    /// </summary>
    public NetworkInstanceId netId
    {
      get
      {
        return this.m_NetId;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// A unique identifier for NetworkIdentity objects within a scene.
    /// </para>
    /// 
    /// </summary>
    public NetworkSceneId sceneId
    {
      get
      {
        return this.m_SceneId;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Flag to make this object only exist when the game is running as a server (or host).
    /// </para>
    /// 
    /// </summary>
    public bool serverOnly
    {
      get
      {
        return this.m_ServerOnly;
      }
      set
      {
        this.m_ServerOnly = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// LocalPlayerAuthority means that the client of the "owning" player has authority over their own player object.
    /// </para>
    /// 
    /// </summary>
    public bool localPlayerAuthority
    {
      get
      {
        return this.m_LocalPlayerAuthority;
      }
      set
      {
        this.m_LocalPlayerAuthority = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Unique identifier used to find the source assets when server spawns the on clients.
    /// </para>
    /// 
    /// </summary>
    public NetworkHash128 assetId
    {
      get
      {
        if (!this.m_AssetId.IsValid())
          this.SetupIDs();
        return this.m_AssetId;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// This returns true if this object is the one that represents the player on the local machine.
    /// </para>
    /// 
    /// </summary>
    public bool isLocalPlayer
    {
      get
      {
        return this.m_IsLocalPlayer;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The id of the player associated with this object.
    /// </para>
    /// 
    /// </summary>
    public short playerControllerId
    {
      get
      {
        return this.m_PlayerId;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The UConnection associated with this NetworkIdentity. This is only valid for player objects on a local client.
    /// </para>
    /// 
    /// </summary>
    public NetworkConnection connectionToServer
    {
      get
      {
        return this.m_ConnectionToServer;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The UConnection associated with this NetworkIdentity. This is only valid for player objects on the server.
    /// </para>
    /// 
    /// </summary>
    public NetworkConnection connectionToClient
    {
      get
      {
        return this.m_ConnectionToClient;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The set of network connections (players) that can see this object.
    /// </para>
    /// 
    /// </summary>
    public ReadOnlyCollection<NetworkConnection> observers
    {
      get
      {
        if (this.m_Observers == null)
          return (ReadOnlyCollection<NetworkConnection>) null;
        return new ReadOnlyCollection<NetworkConnection>((IList<NetworkConnection>) this.m_Observers);
      }
    }

    internal void SetDynamicAssetId(NetworkHash128 assetId)
    {
      if (!this.m_AssetId.IsValid() || this.m_AssetId.Equals((object) assetId))
      {
        this.m_AssetId = assetId;
      }
      else
      {
        if (!LogFilter.logWarn)
          return;
        Debug.LogWarning((object) ("SetDynamicAssetId object already has an assetId <" + (object) this.m_AssetId + ">"));
      }
    }

    internal static NetworkInstanceId GetNextNetworkId()
    {
      uint num = NetworkIdentity.s_NextNetworkId;
      ++NetworkIdentity.s_NextNetworkId;
      return new NetworkInstanceId(num);
    }

    private void CacheBehaviours()
    {
      if (this.m_NetworkBehaviours != null)
        return;
      this.m_NetworkBehaviours = this.GetComponents<NetworkBehaviour>();
    }

    internal void SetNetworkInstanceId(NetworkInstanceId netId)
    {
      this.m_NetId = netId;
    }

    /// <summary>
    /// 
    /// <para>
    /// Force the scene ID to a specific value.
    /// </para>
    /// 
    /// </summary>
    /// <param name="sceneId">The new scene ID.</param>
    public void ForceSceneId(int sceneId)
    {
      this.m_SceneId = new NetworkSceneId((uint) sceneId);
    }

    internal void UpdateClientServer(bool isClient, bool isServer)
    {
      this.m_IsClient |= isClient;
      this.m_IsServer |= isServer;
    }

    internal void SetNoServer()
    {
      this.m_IsServer = false;
    }

    internal void SetNotLocalPlayer()
    {
      this.m_IsLocalPlayer = false;
    }

    internal void RemoveObserverInternal(NetworkConnection conn)
    {
      if (this.m_Observers == null)
        return;
      this.m_Observers.Remove(conn);
    }

    private void OnValidate()
    {
      this.SetupIDs();
    }

    private void AssignAssetID(GameObject prefab)
    {
      this.m_AssetId = NetworkHash128.Parse(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath((UnityEngine.Object) prefab)));
    }

    private bool ThisIsAPrefab()
    {
      return PrefabUtility.GetPrefabType((UnityEngine.Object) this.gameObject) == PrefabType.Prefab;
    }

    private bool ThisIsASceneObjectWithPrefabParent(out GameObject prefab)
    {
      prefab = (GameObject) null;
      if (PrefabUtility.GetPrefabType((UnityEngine.Object) this.gameObject) == PrefabType.None)
        return false;
      prefab = (GameObject) PrefabUtility.GetPrefabParent((UnityEngine.Object) this.gameObject);
      if (!((UnityEngine.Object) prefab == (UnityEngine.Object) null))
        return true;
      if (LogFilter.logError)
        Debug.LogError((object) ("Failed to find prefab parent for scene object [name:" + this.gameObject.name + "]"));
      return false;
    }

    private void SetupIDs()
    {
      if (this.ThisIsAPrefab())
      {
        if (LogFilter.logDev)
          Debug.Log((object) ("This is a prefab: " + this.gameObject.name));
        this.AssignAssetID(this.gameObject);
      }
      else
      {
        GameObject prefab;
        if (this.ThisIsASceneObjectWithPrefabParent(out prefab))
        {
          if (LogFilter.logDev)
            Debug.Log((object) ("This is a scene object with prefab link: " + this.gameObject.name));
          this.AssignAssetID(prefab);
        }
        else
        {
          if (LogFilter.logDev)
            Debug.Log((object) ("This is a pure scene object: " + this.gameObject.name));
          this.m_AssetId.Reset();
        }
      }
    }

    private void OnDestroy()
    {
      if (!this.isServer)
        return;
      NetworkServer.Destroy(this.gameObject);
    }

    internal void OnStartServer()
    {
      if (this.isServer)
        return;
      this.m_IsServer = true;
      this.m_HasAuthority = !this.m_LocalPlayerAuthority;
      this.m_Observers = new List<NetworkConnection>();
      this.CacheBehaviours();
      if (this.netId.IsEmpty())
      {
        this.m_NetId = NetworkIdentity.GetNextNetworkId();
        if (LogFilter.logDev)
        {
          object[] objArray = new object[4];
          int index1 = 0;
          string str1 = "OnStartServer ";
          objArray[index1] = (object) str1;
          int index2 = 1;
          GameObject gameObject = this.gameObject;
          objArray[index2] = (object) gameObject;
          int index3 = 2;
          string str2 = " GUID:";
          objArray[index3] = (object) str2;
          int index4 = 3;
          // ISSUE: variable of a boxed type
          __Boxed<NetworkInstanceId> local = (ValueType) this.netId;
          objArray[index4] = (object) local;
          Debug.Log((object) string.Concat(objArray));
        }
        NetworkServer.instance.SetLocalObjectOnServer(this.netId, this.gameObject);
        for (int index = 0; index < this.m_NetworkBehaviours.Length; ++index)
        {
          NetworkBehaviour networkBehaviour = this.m_NetworkBehaviours[index];
          try
          {
            networkBehaviour.OnStartServer();
          }
          catch (Exception ex)
          {
            Debug.LogError((object) ("Exception in OnStartServer:" + ex.Message + " " + ex.StackTrace));
          }
        }
        if (NetworkClient.active && NetworkServer.localClientActive)
        {
          ClientScene.SetLocalObject(this.netId, this.gameObject);
          this.OnStartClient();
        }
        if (!this.hasAuthority)
          return;
        this.OnStartAuthority();
      }
      else
      {
        if (!LogFilter.logError)
          return;
        object[] objArray = new object[5];
        int index1 = 0;
        string str1 = "Object has non-zero netId ";
        objArray[index1] = (object) str1;
        int index2 = 1;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local = (ValueType) this.netId;
        objArray[index2] = (object) local;
        int index3 = 2;
        string str2 = " for ";
        objArray[index3] = (object) str2;
        int index4 = 3;
        GameObject gameObject = this.gameObject;
        objArray[index4] = (object) gameObject;
        int index5 = 4;
        string str3 = " !!1";
        objArray[index5] = (object) str3;
        Debug.LogError((object) string.Concat(objArray));
      }
    }

    internal void OnStartClient()
    {
      if (!this.m_IsClient)
        this.m_IsClient = true;
      this.CacheBehaviours();
      if (LogFilter.logDev)
      {
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "OnStartClient ";
        objArray[index1] = (object) str1;
        int index2 = 1;
        GameObject gameObject = this.gameObject;
        objArray[index2] = (object) gameObject;
        int index3 = 2;
        string str2 = " GUID:";
        objArray[index3] = (object) str2;
        int index4 = 3;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local = (ValueType) this.netId;
        objArray[index4] = (object) local;
        Debug.Log((object) string.Concat(objArray));
      }
      for (int index = 0; index < this.m_NetworkBehaviours.Length; ++index)
      {
        NetworkBehaviour networkBehaviour = this.m_NetworkBehaviours[index];
        try
        {
          networkBehaviour.PreStartClient();
          networkBehaviour.OnStartClient();
        }
        catch (Exception ex)
        {
          Debug.LogError((object) ("Exception in OnStartClient:" + ex.Message + " " + ex.StackTrace));
        }
      }
    }

    internal void OnStartAuthority()
    {
      for (int index = 0; index < this.m_NetworkBehaviours.Length; ++index)
      {
        NetworkBehaviour networkBehaviour = this.m_NetworkBehaviours[index];
        try
        {
          networkBehaviour.OnStartAuthority();
        }
        catch (Exception ex)
        {
          Debug.LogError((object) ("Exception in OnStartAuthority:" + ex.Message + " " + ex.StackTrace));
        }
      }
    }

    internal void OnSetLocalVisibility(bool vis)
    {
      for (int index = 0; index < this.m_NetworkBehaviours.Length; ++index)
      {
        NetworkBehaviour networkBehaviour = this.m_NetworkBehaviours[index];
        try
        {
          networkBehaviour.OnSetLocalVisibility(vis);
        }
        catch (Exception ex)
        {
          Debug.LogError((object) ("Exception in OnSetLocalVisibility:" + ex.Message + " " + ex.StackTrace));
        }
      }
    }

    internal bool OnCheckObserver(NetworkConnection conn)
    {
      for (int index = 0; index < this.m_NetworkBehaviours.Length; ++index)
      {
        if (!this.m_NetworkBehaviours[index].OnCheckObserver(conn))
          return false;
      }
      return true;
    }

    internal void UNetSerializeAllVars(NetworkWriter writer)
    {
      for (int index = 0; index < this.m_NetworkBehaviours.Length; ++index)
        this.m_NetworkBehaviours[index].OnSerialize(writer, true);
    }

    internal void HandleSyncEvent(int cmdHash, NetworkReader reader)
    {
      if ((UnityEngine.Object) this.gameObject == (UnityEngine.Object) null)
      {
        if (!LogFilter.logWarn)
          return;
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "SyncEvent [";
        objArray[index1] = (object) str1;
        int index2 = 1;
        string cmdHashHandlerName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
        objArray[index2] = (object) cmdHashHandlerName;
        int index3 = 2;
        string str2 = "] received for deleted object ";
        objArray[index3] = (object) str2;
        int index4 = 3;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local = (ValueType) this.netId;
        objArray[index4] = (object) local;
        Debug.LogWarning((object) string.Concat(objArray));
      }
      else
      {
        for (int index = 0; index < this.m_NetworkBehaviours.Length; ++index)
        {
          if (this.m_NetworkBehaviours[index].InvokeSyncEvent(cmdHash, reader))
          {
            NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Incoming, (short) 7, NetworkBehaviour.GetCmdHashEventName(cmdHash), 1);
            break;
          }
        }
      }
    }

    internal void HandleSyncList(int cmdHash, NetworkReader reader)
    {
      if ((UnityEngine.Object) this.gameObject == (UnityEngine.Object) null)
      {
        if (!LogFilter.logWarn)
          return;
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "SyncList [";
        objArray[index1] = (object) str1;
        int index2 = 1;
        string cmdHashHandlerName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
        objArray[index2] = (object) cmdHashHandlerName;
        int index3 = 2;
        string str2 = "] received for deleted object ";
        objArray[index3] = (object) str2;
        int index4 = 3;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local = (ValueType) this.netId;
        objArray[index4] = (object) local;
        Debug.LogWarning((object) string.Concat(objArray));
      }
      else
      {
        for (int index = 0; index < this.m_NetworkBehaviours.Length; ++index)
        {
          if (this.m_NetworkBehaviours[index].InvokeSyncList(cmdHash, reader))
          {
            NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Incoming, (short) 9, NetworkBehaviour.GetCmdHashListName(cmdHash), 1);
            break;
          }
        }
      }
    }

    internal void HandleCommand(int cmdHash, NetworkReader reader)
    {
      if ((UnityEngine.Object) this.gameObject == (UnityEngine.Object) null)
      {
        string cmdHashHandlerName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
        if (!LogFilter.logWarn)
          return;
        object[] objArray = new object[5];
        int index1 = 0;
        string str1 = "Command [";
        objArray[index1] = (object) str1;
        int index2 = 1;
        string str2 = cmdHashHandlerName;
        objArray[index2] = (object) str2;
        int index3 = 2;
        string str3 = "] received for deleted object [netId=";
        objArray[index3] = (object) str3;
        int index4 = 3;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local = (ValueType) this.netId;
        objArray[index4] = (object) local;
        int index5 = 4;
        string str4 = "]";
        objArray[index5] = (object) str4;
        Debug.LogWarning((object) string.Concat(objArray));
      }
      else
      {
        bool flag = false;
        for (int index = 0; index < this.m_NetworkBehaviours.Length; ++index)
        {
          if (this.m_NetworkBehaviours[index].InvokeCommand(cmdHash, reader))
          {
            flag = true;
            break;
          }
        }
        if (!flag)
        {
          string cmdHashHandlerName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
          if (LogFilter.logError)
          {
            object[] objArray = new object[7];
            int index1 = 0;
            string str1 = "Found no receiver for incoming command [";
            objArray[index1] = (object) str1;
            int index2 = 1;
            string str2 = cmdHashHandlerName;
            objArray[index2] = (object) str2;
            int index3 = 2;
            string str3 = "] on ";
            objArray[index3] = (object) str3;
            int index4 = 3;
            GameObject gameObject = this.gameObject;
            objArray[index4] = (object) gameObject;
            int index5 = 4;
            string str4 = ",  the server and client should have the same NetworkBehaviour instances [netId=";
            objArray[index5] = (object) str4;
            int index6 = 5;
            // ISSUE: variable of a boxed type
            __Boxed<NetworkInstanceId> local = (ValueType) this.netId;
            objArray[index6] = (object) local;
            int index7 = 6;
            string str5 = "].";
            objArray[index7] = (object) str5;
            Debug.LogError((object) string.Concat(objArray));
          }
        }
        NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Incoming, (short) 5, NetworkBehaviour.GetCmdHashCmdName(cmdHash), 1);
      }
    }

    internal void HandleRPC(int cmdHash, NetworkReader reader)
    {
      if ((UnityEngine.Object) this.gameObject == (UnityEngine.Object) null)
      {
        if (!LogFilter.logWarn)
          return;
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "ClientRpc [";
        objArray[index1] = (object) str1;
        int index2 = 1;
        string cmdHashHandlerName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
        objArray[index2] = (object) cmdHashHandlerName;
        int index3 = 2;
        string str2 = "] received for deleted object ";
        objArray[index3] = (object) str2;
        int index4 = 3;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local = (ValueType) this.netId;
        objArray[index4] = (object) local;
        Debug.LogWarning((object) string.Concat(objArray));
      }
      else if (this.m_NetworkBehaviours.Length == 0)
      {
        if (!LogFilter.logWarn)
          return;
        Debug.LogWarning((object) ("No receiver found for ClientRpc [" + NetworkBehaviour.GetCmdHashHandlerName(cmdHash) + "]. Does the script with the function inherit NetworkBehaviour?"));
      }
      else
      {
        for (int index = 0; index < this.m_NetworkBehaviours.Length; ++index)
        {
          if (this.m_NetworkBehaviours[index].InvokeRPC(cmdHash, reader))
          {
            NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Incoming, (short) 2, NetworkBehaviour.GetCmdHashRpcName(cmdHash), 1);
            return;
          }
        }
        string str1 = NetworkBehaviour.GetInvoker(cmdHash) ?? "[unknown:" + (object) cmdHash + "]";
        if (LogFilter.logWarn)
        {
          object[] objArray = new object[6];
          int index1 = 0;
          string str2 = "Failed to invoke RPC ";
          objArray[index1] = (object) str2;
          int index2 = 1;
          string str3 = str1;
          objArray[index2] = (object) str3;
          int index3 = 2;
          string str4 = "(";
          objArray[index3] = (object) str4;
          int index4 = 3;
          // ISSUE: variable of a boxed type
          __Boxed<int> local1 = (ValueType) cmdHash;
          objArray[index4] = (object) local1;
          int index5 = 4;
          string str5 = ") on netID ";
          objArray[index5] = (object) str5;
          int index6 = 5;
          // ISSUE: variable of a boxed type
          __Boxed<NetworkInstanceId> local2 = (ValueType) this.netId;
          objArray[index6] = (object) local2;
          Debug.LogWarning((object) string.Concat(objArray));
        }
        NetworkBehaviour.DumpInvokers();
      }
    }

    internal void UNetUpdate()
    {
      uint num = 0U;
      for (int index = 0; index < this.m_NetworkBehaviours.Length; ++index)
      {
        int dirtyChannel = this.m_NetworkBehaviours[index].GetDirtyChannel();
        if (dirtyChannel != -1)
          num |= (uint) (1 << dirtyChannel);
      }
      if ((int) num == 0)
        return;
      for (int channelId = 0; channelId < NetworkServer.numChannels; ++channelId)
      {
        if (((int) num & 1 << channelId) != 0)
        {
          NetworkIdentity.s_UpdateWriter.StartMessage((short) 8);
          NetworkIdentity.s_UpdateWriter.Write(this.netId);
          bool flag = false;
          for (int index = 0; index < this.m_NetworkBehaviours.Length; ++index)
          {
            NetworkBehaviour networkBehaviour = this.m_NetworkBehaviours[index];
            if (networkBehaviour.GetDirtyChannel() != channelId)
              NetworkIdentity.s_UpdateWriter.WritePackedUInt32(0U);
            else if (networkBehaviour.OnSerialize(NetworkIdentity.s_UpdateWriter, false))
            {
              networkBehaviour.ClearAllDirtyBits();
              NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Outgoing, (short) 8, networkBehaviour.GetType().Name, 1);
              flag = true;
            }
          }
          if (flag)
          {
            NetworkIdentity.s_UpdateWriter.FinishMessage();
            NetworkServer.SendWriterToReady(this.gameObject, NetworkIdentity.s_UpdateWriter, channelId);
          }
        }
      }
    }

    internal void OnUpdateVars(NetworkReader reader, bool initialState)
    {
      if (initialState && this.m_NetworkBehaviours == null)
        this.m_NetworkBehaviours = this.GetComponents<NetworkBehaviour>();
      for (int index = 0; index < this.m_NetworkBehaviours.Length; ++index)
      {
        NetworkBehaviour networkBehaviour = this.m_NetworkBehaviours[index];
        uint position = reader.Position;
        networkBehaviour.OnDeserialize(reader, initialState);
        if (reader.Position - position > 1U)
          NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Incoming, (short) 8, networkBehaviour.GetType().Name, 1);
      }
    }

    internal void SetLocalPlayer(short playerControllerId)
    {
      this.m_IsLocalPlayer = true;
      this.m_PlayerId = playerControllerId;
      if (this.localPlayerAuthority)
        this.m_HasAuthority = true;
      for (int index = 0; index < this.m_NetworkBehaviours.Length; ++index)
      {
        NetworkBehaviour networkBehaviour = this.m_NetworkBehaviours[index];
        networkBehaviour.OnStartLocalPlayer();
        if (this.localPlayerAuthority)
          networkBehaviour.OnStartAuthority();
      }
    }

    internal void SetConnectionToServer(NetworkConnection conn)
    {
      this.m_ConnectionToServer = conn;
    }

    internal void SetConnectionToClient(NetworkConnection conn)
    {
      this.m_ConnectionToClient = conn;
    }

    internal void OnNetworkDestroy()
    {
      for (int index = 0; index < this.m_NetworkBehaviours.Length; ++index)
        this.m_NetworkBehaviours[index].OnNetworkDestroy();
      this.m_IsServer = false;
    }

    internal void ClearObservers()
    {
      if (this.m_Observers == null)
        return;
      int count = this.m_Observers.Count;
      for (int index = 0; index < count; ++index)
        this.m_Observers[index].RemoveFromVisList(this, true);
      this.m_Observers.Clear();
    }

    internal void AddObserver(NetworkConnection conn)
    {
      if (this.m_Observers == null)
        return;
      if (this.m_Observers.Contains(conn))
      {
        if (!LogFilter.logWarn)
          return;
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "Duplicate observer ";
        objArray[index1] = (object) str1;
        int index2 = 1;
        string str2 = conn.address;
        objArray[index2] = (object) str2;
        int index3 = 2;
        string str3 = " added for ";
        objArray[index3] = (object) str3;
        int index4 = 3;
        GameObject gameObject = this.gameObject;
        objArray[index4] = (object) gameObject;
        Debug.LogWarning((object) string.Concat(objArray));
      }
      else
      {
        if (LogFilter.logDev)
        {
          object[] objArray = new object[4];
          int index1 = 0;
          string str1 = "Added observer ";
          objArray[index1] = (object) str1;
          int index2 = 1;
          string str2 = conn.address;
          objArray[index2] = (object) str2;
          int index3 = 2;
          string str3 = " added for ";
          objArray[index3] = (object) str3;
          int index4 = 3;
          GameObject gameObject = this.gameObject;
          objArray[index4] = (object) gameObject;
          Debug.Log((object) string.Concat(objArray));
        }
        this.m_Observers.Add(conn);
        conn.AddToVisList(this);
      }
    }

    internal void RemoveObserver(NetworkConnection conn)
    {
      if (this.m_Observers == null)
        return;
      this.m_Observers.Remove(conn);
      conn.RemoveFromVisList(this, false);
    }

    /// <summary>
    /// 
    /// <para>
    /// This causes the set of players that can see this object to be rebuild. The OnRebuildObservers callback function will be invoked on each NetworkBehaviour.
    /// </para>
    /// 
    /// </summary>
    /// <param name="initialize">True if this is the first time.</param>
    public void RebuildObservers(bool initialize)
    {
      if (this.m_Observers == null)
        return;
      bool flag = false;
      HashSet<NetworkConnection> observers = new HashSet<NetworkConnection>();
      HashSet<NetworkConnection> hashSet = new HashSet<NetworkConnection>((IEnumerable<NetworkConnection>) this.m_Observers);
      for (int index = 0; index < this.m_NetworkBehaviours.Length; ++index)
      {
        NetworkBehaviour networkBehaviour = this.m_NetworkBehaviours[index];
        flag |= networkBehaviour.OnRebuildObservers(observers, initialize);
      }
      if (!flag)
      {
        if (!initialize)
          return;
        using (List<NetworkConnection>.Enumerator enumerator = NetworkServer.connections.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            NetworkConnection current = enumerator.Current;
            if (current != null && current.isReady)
              this.AddObserver(current);
          }
        }
        using (List<NetworkConnection>.Enumerator enumerator = NetworkServer.localConnections.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            NetworkConnection current = enumerator.Current;
            if (current != null && current.isReady)
              this.AddObserver(current);
          }
        }
      }
      else
      {
        using (HashSet<NetworkConnection>.Enumerator enumerator = observers.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            NetworkConnection current = enumerator.Current;
            if (initialize || !hashSet.Contains(current))
            {
              current.AddToVisList(this);
              if (LogFilter.logDebug)
              {
                object[] objArray = new object[4];
                int index1 = 0;
                string str1 = "New Observer for ";
                objArray[index1] = (object) str1;
                int index2 = 1;
                GameObject gameObject = this.gameObject;
                objArray[index2] = (object) gameObject;
                int index3 = 2;
                string str2 = " ";
                objArray[index3] = (object) str2;
                int index4 = 3;
                NetworkConnection networkConnection = current;
                objArray[index4] = (object) networkConnection;
                Debug.Log((object) string.Concat(objArray));
              }
            }
          }
        }
        using (HashSet<NetworkConnection>.Enumerator enumerator = hashSet.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            NetworkConnection current = enumerator.Current;
            if (!observers.Contains(current))
            {
              current.RemoveFromVisList(this, false);
              if (LogFilter.logDebug)
              {
                object[] objArray = new object[4];
                int index1 = 0;
                string str1 = "Removed Observer for ";
                objArray[index1] = (object) str1;
                int index2 = 1;
                GameObject gameObject = this.gameObject;
                objArray[index2] = (object) gameObject;
                int index3 = 2;
                string str2 = " ";
                objArray[index3] = (object) str2;
                int index4 = 3;
                NetworkConnection networkConnection = current;
                objArray[index4] = (object) networkConnection;
                Debug.Log((object) string.Concat(objArray));
              }
            }
          }
        }
        if (initialize)
        {
          using (List<NetworkConnection>.Enumerator enumerator = NetworkServer.localConnections.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              NetworkConnection current = enumerator.Current;
              if (!observers.Contains(current))
                this.OnSetLocalVisibility(false);
            }
          }
        }
        this.m_Observers = new List<NetworkConnection>((IEnumerable<NetworkConnection>) observers);
      }
    }

    internal static void UNetStaticUpdate()
    {
      NetworkServer.Update();
      NetworkClient.UpdateClients();
      NetworkDetailStats.NewProfilerTick(Time.time);
    }
  }
}
