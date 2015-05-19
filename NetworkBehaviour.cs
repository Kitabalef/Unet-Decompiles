// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkBehaviour
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

namespace UnityEngine.Networking
{
  /// <summary>
  /// 
  /// <para>
  /// Base class which should be inherited by scripts which contain networking functionality.
  /// </para>
  /// 
  /// </summary>
  [RequireComponent(typeof (NetworkIdentity))]
  [AddComponentMenu("")]
  public class NetworkBehaviour : MonoBehaviour
  {
    private static Dictionary<int, NetworkBehaviour.Invoker> s_CmdHandlerDelegates = new Dictionary<int, NetworkBehaviour.Invoker>();
    private const float DefaultSendInterval = 0.1f;
    [EditorBrowsable(EditorBrowsableState.Never)]
    private uint m_SyncVarDirtyBits;
    internal float m_LastSendTime;
    internal NetworkIdentity m_MyView;

    /// <summary>
    /// 
    /// <para>
    /// This value is set on the NetworkIdentity and is accessible here for convenient access for scripts.
    /// </para>
    /// 
    /// </summary>
    public bool localPlayerAuthority
    {
      get
      {
        return this.myView.localPlayerAuthority;
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
        return this.myView.isServer;
      }
    }

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
        return this.myView.isClient;
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
        return this.myView.isLocalPlayer;
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
        return this.myView.hasAuthority;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The unique network Id of this object.
    /// </para>
    /// 
    /// </summary>
    public NetworkInstanceId netId
    {
      get
      {
        return this.myView.netId;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The [[NetworkConnection]] associated with this [[NetworkIdentity]]. This is only valid for player objects on the server.
    /// </para>
    /// 
    /// </summary>
    public NetworkConnection connectionToServer
    {
      get
      {
        return this.myView.m_ConnectionToServer;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The [[NetworkConnection]] associated with this [[NetworkIdentity]]. This is only valid for player objects on the server.
    /// </para>
    /// 
    /// </summary>
    public NetworkConnection connectionToClient
    {
      get
      {
        return this.myView.m_ConnectionToClient;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The id of the player associated with thei behaviour.
    /// </para>
    /// 
    /// </summary>
    public short playerControllerId
    {
      get
      {
        return this.myView.m_PlayerId;
      }
    }

    protected uint syncVarDirtyBits
    {
      get
      {
        return this.m_SyncVarDirtyBits;
      }
    }

    private NetworkIdentity myView
    {
      get
      {
        if (!((UnityEngine.Object) this.m_MyView == (UnityEngine.Object) null))
          return this.m_MyView;
        this.m_MyView = this.GetComponent<NetworkIdentity>();
        if ((UnityEngine.Object) this.m_MyView == (UnityEngine.Object) null && LogFilter.logError)
          Debug.LogError((object) "There is no NetworkIdentity on this object. Please add one.");
        return this.m_MyView;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected void SendCommandInternal(NetworkWriter writer, int channelId, string cmdName)
    {
      if (!this.isLocalPlayer)
      {
        if (!LogFilter.logWarn)
          return;
        Debug.LogWarning((object) "Trying to send command for non-local player.");
      }
      else if (this.connectionToServer == null)
      {
        if (!LogFilter.logError)
          return;
        Debug.LogError((object) ("Send command attempted with no client running [client=" + (object) this.connectionToServer + "]."));
      }
      else
      {
        writer.FinishMessage();
        this.connectionToServer.SendWriter(writer, channelId);
        NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Outgoing, (short) 5, cmdName, 1);
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Manually invoke a Command.
    /// </para>
    /// 
    /// </summary>
    /// <param name="cmdHash">Hash of the Command name.</param><param name="reader">Parameters to pass to the command.</param>
    /// <returns>
    /// 
    /// <para>
    /// Returns true if successful.
    /// </para>
    /// 
    /// </returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public virtual bool InvokeCommand(int cmdHash, NetworkReader reader)
    {
      return this.InvokeCommandDelegate(cmdHash, reader);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected void SendRPCInternal(NetworkWriter writer, int channelId, string rpcName)
    {
      if (!this.isServer)
      {
        if (!LogFilter.logWarn)
          return;
        Debug.LogWarning((object) "ClientRpc call on un-spawned object");
      }
      else
      {
        writer.FinishMessage();
        NetworkServer.SendWriterToReady(this.gameObject, writer, channelId);
        NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Outgoing, (short) 2, rpcName, 1);
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Manually invoke an RPC function.
    /// </para>
    /// 
    /// </summary>
    /// <param name="cmdHash">Hash of the RPC name.</param><param name="reader">Parameters to pass to the RPC function.</param>
    /// <returns>
    /// 
    /// <para>
    /// Returns true if successful.
    /// </para>
    /// 
    /// </returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public virtual bool InvokeRPC(int cmdHash, NetworkReader reader)
    {
      return this.InvokeRpcDelegate(cmdHash, reader);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected void SendEventInternal(NetworkWriter writer, int channelId, string eventName)
    {
      if (!NetworkServer.active)
      {
        if (!LogFilter.logWarn)
          return;
        Debug.LogWarning((object) "SendEvent no server?");
      }
      else
      {
        writer.FinishMessage();
        NetworkServer.SendWriterToReady(this.gameObject, writer, channelId);
        NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Outgoing, (short) 7, eventName, 1);
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Manually invoke a SyncEvent.
    /// </para>
    /// 
    /// </summary>
    /// <param name="cmdHash">Hash of the SyncEvent name.</param><param name="reader">Parameters to pass to the SyncEvent.</param>
    /// <returns>
    /// 
    /// <para>
    /// Returns true if successful.
    /// </para>
    /// 
    /// </returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public virtual bool InvokeSyncEvent(int cmdHash, NetworkReader reader)
    {
      return this.InvokeSyncEventDelegate(cmdHash, reader);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public virtual bool InvokeSyncList(int cmdHash, NetworkReader reader)
    {
      return this.InvokeSyncListDelegate(cmdHash, reader);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected static void RegisterCommandDelegate(System.Type invokeClass, int cmdHash, NetworkBehaviour.CmdDelegate func)
    {
      if (NetworkBehaviour.s_CmdHandlerDelegates.ContainsKey(cmdHash))
        return;
      NetworkBehaviour.s_CmdHandlerDelegates[cmdHash] = new NetworkBehaviour.Invoker()
      {
        invokeType = NetworkBehaviour.UNetInvokeType.Command,
        invokeClass = invokeClass,
        invokeFunction = func
      };
      if (!LogFilter.logDev)
        return;
      object[] objArray = new object[4];
      int index1 = 0;
      string str1 = "RegisterCommandDelegate hash:";
      objArray[index1] = (object) str1;
      int index2 = 1;
      // ISSUE: variable of a boxed type
      __Boxed<int> local = (ValueType) cmdHash;
      objArray[index2] = (object) local;
      int index3 = 2;
      string str2 = " ";
      objArray[index3] = (object) str2;
      int index4 = 3;
      string name = func.Method.Name;
      objArray[index4] = (object) name;
      Debug.Log((object) string.Concat(objArray));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected static void RegisterRpcDelegate(System.Type invokeClass, int cmdHash, NetworkBehaviour.CmdDelegate func)
    {
      if (NetworkBehaviour.s_CmdHandlerDelegates.ContainsKey(cmdHash))
        return;
      NetworkBehaviour.s_CmdHandlerDelegates[cmdHash] = new NetworkBehaviour.Invoker()
      {
        invokeType = NetworkBehaviour.UNetInvokeType.ClientRpc,
        invokeClass = invokeClass,
        invokeFunction = func
      };
      if (!LogFilter.logDev)
        return;
      object[] objArray = new object[4];
      int index1 = 0;
      string str1 = "RegisterRpcDelegate hash:";
      objArray[index1] = (object) str1;
      int index2 = 1;
      // ISSUE: variable of a boxed type
      __Boxed<int> local = (ValueType) cmdHash;
      objArray[index2] = (object) local;
      int index3 = 2;
      string str2 = " ";
      objArray[index3] = (object) str2;
      int index4 = 3;
      string name = func.Method.Name;
      objArray[index4] = (object) name;
      Debug.Log((object) string.Concat(objArray));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected static void RegisterEventDelegate(System.Type invokeClass, int cmdHash, NetworkBehaviour.CmdDelegate func)
    {
      if (NetworkBehaviour.s_CmdHandlerDelegates.ContainsKey(cmdHash))
        return;
      NetworkBehaviour.s_CmdHandlerDelegates[cmdHash] = new NetworkBehaviour.Invoker()
      {
        invokeType = NetworkBehaviour.UNetInvokeType.SyncEvent,
        invokeClass = invokeClass,
        invokeFunction = func
      };
      if (!LogFilter.logDev)
        return;
      object[] objArray = new object[4];
      int index1 = 0;
      string str1 = "RegisterEventDelegate hash:";
      objArray[index1] = (object) str1;
      int index2 = 1;
      // ISSUE: variable of a boxed type
      __Boxed<int> local = (ValueType) cmdHash;
      objArray[index2] = (object) local;
      int index3 = 2;
      string str2 = " ";
      objArray[index3] = (object) str2;
      int index4 = 3;
      string name = func.Method.Name;
      objArray[index4] = (object) name;
      Debug.Log((object) string.Concat(objArray));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected static void RegisterSyncListDelegate(System.Type invokeClass, int cmdHash, NetworkBehaviour.CmdDelegate func)
    {
      if (NetworkBehaviour.s_CmdHandlerDelegates.ContainsKey(cmdHash))
        return;
      NetworkBehaviour.s_CmdHandlerDelegates[cmdHash] = new NetworkBehaviour.Invoker()
      {
        invokeType = NetworkBehaviour.UNetInvokeType.SyncList,
        invokeClass = invokeClass,
        invokeFunction = func
      };
      if (!LogFilter.logDev)
        return;
      object[] objArray = new object[4];
      int index1 = 0;
      string str1 = "RegisterSyncListDelegate hash:";
      objArray[index1] = (object) str1;
      int index2 = 1;
      // ISSUE: variable of a boxed type
      __Boxed<int> local = (ValueType) cmdHash;
      objArray[index2] = (object) local;
      int index3 = 2;
      string str2 = " ";
      objArray[index3] = (object) str2;
      int index4 = 3;
      string name = func.Method.Name;
      objArray[index4] = (object) name;
      Debug.Log((object) string.Concat(objArray));
    }

    internal static string GetInvoker(int cmdHash)
    {
      if (!NetworkBehaviour.s_CmdHandlerDelegates.ContainsKey(cmdHash))
        return (string) null;
      return NetworkBehaviour.s_CmdHandlerDelegates[cmdHash].DebugString();
    }

    internal static void DumpInvokers()
    {
      Debug.Log((object) ("DumpInvokers size:" + (object) NetworkBehaviour.s_CmdHandlerDelegates.Count));
      using (Dictionary<int, NetworkBehaviour.Invoker>.Enumerator enumerator = NetworkBehaviour.s_CmdHandlerDelegates.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          KeyValuePair<int, NetworkBehaviour.Invoker> current = enumerator.Current;
          object[] objArray = new object[8];
          int index1 = 0;
          string str1 = "  Invoker:";
          objArray[index1] = (object) str1;
          int index2 = 1;
          System.Type type = current.Value.invokeClass;
          objArray[index2] = (object) type;
          int index3 = 2;
          string str2 = ":";
          objArray[index3] = (object) str2;
          int index4 = 3;
          string name = current.Value.invokeFunction.Method.Name;
          objArray[index4] = (object) name;
          int index5 = 4;
          string str3 = " ";
          objArray[index5] = (object) str3;
          int index6 = 5;
          // ISSUE: variable of a boxed type
          __Boxed<NetworkBehaviour.UNetInvokeType> local1 = (Enum) current.Value.invokeType;
          objArray[index6] = (object) local1;
          int index7 = 6;
          string str4 = " ";
          objArray[index7] = (object) str4;
          int index8 = 7;
          // ISSUE: variable of a boxed type
          __Boxed<int> local2 = (ValueType) current.Key;
          objArray[index8] = (object) local2;
          Debug.Log((object) string.Concat(objArray));
        }
      }
    }

    internal bool ContainsCommandDelegate(int cmdHash)
    {
      return NetworkBehaviour.s_CmdHandlerDelegates.ContainsKey(cmdHash);
    }

    internal bool InvokeCommandDelegate(int cmdHash, NetworkReader reader)
    {
      if (!NetworkBehaviour.s_CmdHandlerDelegates.ContainsKey(cmdHash))
        return false;
      NetworkBehaviour.Invoker invoker = NetworkBehaviour.s_CmdHandlerDelegates[cmdHash];
      if (invoker.invokeType != NetworkBehaviour.UNetInvokeType.Command)
        return false;
      if (this.GetType() != invoker.invokeClass && !this.GetType().IsSubclassOf(invoker.invokeClass))
      {
        string[] strArray = new string[5];
        int index1 = 0;
        string str1 = "InvokeCommand class [";
        strArray[index1] = str1;
        int index2 = 1;
        string name1 = this.GetType().Name;
        strArray[index2] = name1;
        int index3 = 2;
        string str2 = "] doesn't match [";
        strArray[index3] = str2;
        int index4 = 3;
        string name2 = invoker.invokeClass.Name;
        strArray[index4] = name2;
        int index5 = 4;
        string str3 = "])";
        strArray[index5] = str3;
        Debug.LogError((object) string.Concat(strArray));
        return false;
      }
      invoker.invokeFunction(this, reader);
      return true;
    }

    internal bool InvokeRpcDelegate(int cmdHash, NetworkReader reader)
    {
      if (!NetworkBehaviour.s_CmdHandlerDelegates.ContainsKey(cmdHash))
        return false;
      NetworkBehaviour.Invoker invoker = NetworkBehaviour.s_CmdHandlerDelegates[cmdHash];
      if (invoker.invokeType != NetworkBehaviour.UNetInvokeType.ClientRpc || this.GetType() != invoker.invokeClass)
        return false;
      invoker.invokeFunction(this, reader);
      return true;
    }

    internal bool InvokeSyncEventDelegate(int cmdHash, NetworkReader reader)
    {
      if (!NetworkBehaviour.s_CmdHandlerDelegates.ContainsKey(cmdHash))
        return false;
      NetworkBehaviour.Invoker invoker = NetworkBehaviour.s_CmdHandlerDelegates[cmdHash];
      if (invoker.invokeType != NetworkBehaviour.UNetInvokeType.SyncEvent || this.GetType() != invoker.invokeClass)
        return false;
      invoker.invokeFunction(this, reader);
      return true;
    }

    internal bool InvokeSyncListDelegate(int cmdHash, NetworkReader reader)
    {
      if (!NetworkBehaviour.s_CmdHandlerDelegates.ContainsKey(cmdHash))
        return false;
      NetworkBehaviour.Invoker invoker = NetworkBehaviour.s_CmdHandlerDelegates[cmdHash];
      if (invoker.invokeType != NetworkBehaviour.UNetInvokeType.SyncList || this.GetType() != invoker.invokeClass)
        return false;
      invoker.invokeFunction(this, reader);
      return true;
    }

    internal static string GetCmdHashHandlerName(int cmdHash)
    {
      if (!NetworkBehaviour.s_CmdHandlerDelegates.ContainsKey(cmdHash))
        return cmdHash.ToString();
      NetworkBehaviour.Invoker invoker = NetworkBehaviour.s_CmdHandlerDelegates[cmdHash];
      return invoker.invokeType.ToString() + ":" + invoker.invokeFunction.Method.Name;
    }

    private static string GetCmdHashPrefixName(int cmdHash, string prefix)
    {
      if (!NetworkBehaviour.s_CmdHandlerDelegates.ContainsKey(cmdHash))
        return cmdHash.ToString();
      string str = NetworkBehaviour.s_CmdHandlerDelegates[cmdHash].invokeFunction.Method.Name;
      if (str.IndexOf(prefix) > -1)
        str = str.Substring(prefix.Length);
      return str;
    }

    internal static string GetCmdHashCmdName(int cmdHash)
    {
      return NetworkBehaviour.GetCmdHashPrefixName(cmdHash, "InvokeCmd");
    }

    internal static string GetCmdHashRpcName(int cmdHash)
    {
      return NetworkBehaviour.GetCmdHashPrefixName(cmdHash, "InvokeRpc");
    }

    internal static string GetCmdHashEventName(int cmdHash)
    {
      return NetworkBehaviour.GetCmdHashPrefixName(cmdHash, "InvokeSyncEvent");
    }

    internal static string GetCmdHashListName(int cmdHash)
    {
      return NetworkBehaviour.GetCmdHashPrefixName(cmdHash, "InvokeSyncList");
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected bool SetSyncVarGameObject(GameObject newGameObject, ref GameObject gameObjectField, uint dirtyBit, ref NetworkInstanceId netIdField)
    {
      NetworkInstanceId networkInstanceId1 = new NetworkInstanceId();
      if ((UnityEngine.Object) newGameObject != (UnityEngine.Object) null)
      {
        NetworkIdentity component = newGameObject.GetComponent<NetworkIdentity>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          networkInstanceId1 = component.netId;
          if (networkInstanceId1.IsEmpty() && LogFilter.logWarn)
            Debug.LogWarning((object) ("SetSyncVarGameObject GameObject " + (object) newGameObject + " has a zero netId. Maybe it is not spawned yet?"));
        }
      }
      NetworkInstanceId networkInstanceId2 = new NetworkInstanceId();
      if ((UnityEngine.Object) gameObjectField != (UnityEngine.Object) null)
        networkInstanceId2 = gameObjectField.GetComponent<NetworkIdentity>().netId;
      if (!(networkInstanceId1 != networkInstanceId2))
        return false;
      if (LogFilter.logDev)
      {
        object[] objArray = new object[8];
        int index1 = 0;
        string str1 = "SetSyncVar GameObject ";
        objArray[index1] = (object) str1;
        int index2 = 1;
        string name = this.GetType().Name;
        objArray[index2] = (object) name;
        int index3 = 2;
        string str2 = " bit [";
        objArray[index3] = (object) str2;
        int index4 = 3;
        // ISSUE: variable of a boxed type
        __Boxed<uint> local1 = (ValueType) dirtyBit;
        objArray[index4] = (object) local1;
        int index5 = 4;
        string str3 = "] netfieldId:";
        objArray[index5] = (object) str3;
        int index6 = 5;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local2 = (ValueType) networkInstanceId2;
        objArray[index6] = (object) local2;
        int index7 = 6;
        string str4 = "->";
        objArray[index7] = (object) str4;
        int index8 = 7;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local3 = (ValueType) networkInstanceId1;
        objArray[index8] = (object) local3;
        Debug.Log((object) string.Concat(objArray));
      }
      this.SetDirtyBit(dirtyBit);
      gameObjectField = newGameObject;
      netIdField = networkInstanceId1;
      return true;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected bool SetSyncVar<T>(T value, ref T fieldValue, uint dirtyBit)
    {
      if (value.Equals((object) fieldValue))
        return false;
      if (LogFilter.logDev)
      {
        object[] objArray = new object[8];
        int index1 = 0;
        string str1 = "SetSyncVar ";
        objArray[index1] = (object) str1;
        int index2 = 1;
        string name = this.GetType().Name;
        objArray[index2] = (object) name;
        int index3 = 2;
        string str2 = " bit [";
        objArray[index3] = (object) str2;
        int index4 = 3;
        // ISSUE: variable of a boxed type
        __Boxed<uint> local1 = (ValueType) dirtyBit;
        objArray[index4] = (object) local1;
        int index5 = 4;
        string str3 = "] ";
        objArray[index5] = (object) str3;
        int index6 = 5;
        // ISSUE: variable of a boxed type
        __Boxed<T> local2 = (object) fieldValue;
        objArray[index6] = (object) local2;
        int index7 = 6;
        string str4 = "->";
        objArray[index7] = (object) str4;
        int index8 = 7;
        // ISSUE: variable of a boxed type
        __Boxed<T> local3 = (object) value;
        objArray[index8] = (object) local3;
        Debug.Log((object) string.Concat(objArray));
      }
      this.SetDirtyBit(dirtyBit);
      fieldValue = value;
      return true;
    }

    /// <summary>
    /// 
    /// <para>
    /// Used to set the behaviour as dirty, so that a network update will be sent for the object.
    /// </para>
    /// 
    /// </summary>
    /// <param name="dirtyBit">Bit mask to set.</param>
    public void SetDirtyBit(uint dirtyBit)
    {
      this.m_SyncVarDirtyBits |= dirtyBit;
    }

    /// <summary>
    /// 
    /// <para>
    /// This clears all the dirty bits that were set on this script by SetDirtyBits();
    /// </para>
    /// 
    /// </summary>
    public void ClearAllDirtyBits()
    {
      this.m_LastSendTime = Time.time;
      this.m_SyncVarDirtyBits = 0U;
    }

    internal int GetDirtyChannel()
    {
      if ((double) Time.time - (double) this.m_LastSendTime > (double) this.GetNetworkSendInterval() && (int) this.m_SyncVarDirtyBits != 0)
        return this.GetNetworkChannel();
      return -1;
    }

    /// <summary>
    /// 
    /// <para>
    /// Virtual function to override to send custom serialization data.
    /// </para>
    /// 
    /// </summary>
    /// <param name="writer">Writer to use to write to the stream.</param><param name="initialState">If this is being called to send initial state.</param>
    /// <returns>
    /// 
    /// <para>
    /// True if data was written.
    /// </para>
    /// 
    /// </returns>
    public virtual bool OnSerialize(NetworkWriter writer, bool initialState)
    {
      if (!initialState)
        writer.WritePackedUInt32(0U);
      return false;
    }

    /// <summary>
    /// 
    /// <para>
    /// Virtual function to override to receive custom serialization data.
    /// </para>
    /// 
    /// </summary>
    /// <param name="reader">Reader to read from the stream.</param><param name="initialState">True if being sent initial state.</param>
    public virtual void OnDeserialize(NetworkReader reader, bool initialState)
    {
      if (initialState)
        return;
      int num = (int) reader.ReadPackedUInt32();
    }

    /// <summary>
    /// 
    /// <para>
    /// An internal method called on client objects to resolve GameObject references.
    /// </para>
    /// 
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public virtual void PreStartClient()
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// </para>
    /// 
    /// </summary>
    public virtual void OnNetworkDestroy()
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// Called when the server starts listening.
    /// </para>
    /// 
    /// </summary>
    public virtual void OnStartServer()
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// Called when the client connects to a server.
    /// </para>
    /// 
    /// </summary>
    public virtual void OnStartClient()
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// Called when the local player object has been set up.
    /// </para>
    /// 
    /// </summary>
    public virtual void OnStartLocalPlayer()
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This is invoked on behaviours that haev authority, based on context and the LocalPlayerAuthority value on the NetworkIdentity.
    /// </para>
    /// 
    /// </summary>
    public virtual void OnStartAuthority()
    {
    }

    public virtual bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize)
    {
      return false;
    }

    /// <summary>
    /// 
    /// <para>
    /// Callback used by the visibility system for objects on a host.
    /// </para>
    /// 
    /// </summary>
    /// <param name="vis">New visibility state.</param>
    public virtual void OnSetLocalVisibility(bool vis)
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// Callback used by the visibility system to determine if an observer (player) can see this object.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">Network connection of a player.</param>
    /// <returns>
    /// 
    /// <para>
    /// True if the player can see this object.
    /// </para>
    /// 
    /// </returns>
    public virtual bool OnCheckObserver(NetworkConnection conn)
    {
      return true;
    }

    /// <summary>
    /// 
    /// <para>
    /// This virtual function is used to specify the QoS channel to use for SyncVar updates for this script.
    /// </para>
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// 
    /// <para>
    /// The QoS channel for this script.
    /// </para>
    /// 
    /// </returns>
    public virtual int GetNetworkChannel()
    {
      return 0;
    }

    /// <summary>
    /// 
    /// <para>
    /// This virtual function is used to specify the send interval to use for SyncVar updates for this script.
    /// </para>
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// 
    /// <para>
    /// The time in seconds between updates.
    /// </para>
    /// 
    /// </returns>
    public virtual float GetNetworkSendInterval()
    {
      return 0.1f;
    }

    protected enum UNetInvokeType
    {
      Command,
      ClientRpc,
      SyncEvent,
      SyncList,
    }

    protected class Invoker
    {
      public NetworkBehaviour.UNetInvokeType invokeType;
      public System.Type invokeClass;
      public NetworkBehaviour.CmdDelegate invokeFunction;

      public string DebugString()
      {
        string[] strArray = new string[5];
        int index1 = 0;
        string str1 = this.invokeType.ToString();
        strArray[index1] = str1;
        int index2 = 1;
        string str2 = ":";
        strArray[index2] = str2;
        int index3 = 2;
        string str3 = this.invokeClass.ToString();
        strArray[index3] = str3;
        int index4 = 3;
        string str4 = ":";
        strArray[index4] = str4;
        int index5 = 4;
        string name = this.invokeFunction.Method.Name;
        strArray[index5] = name;
        return string.Concat(strArray);
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Delegate for Command functions.
    /// </para>
    /// 
    /// </summary>
    /// <param name="obj"/><param name="reader"/>
    protected delegate void CmdDelegate(NetworkBehaviour obj, NetworkReader reader);

    /// <summary>
    /// 
    /// <para>
    /// Delegate for Event functions.
    /// </para>
    /// 
    /// </summary>
    /// <param name="targets"/><param name="reader"/>
    protected delegate void EventDelegate(List<Delegate> targets, NetworkReader reader);
  }
}
