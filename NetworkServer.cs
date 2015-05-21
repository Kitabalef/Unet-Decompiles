// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkServer
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Types;

namespace UnityEngine.Networking
{
  /// <summary>
  /// 
  /// <para>
  /// High level UNET server.
  /// </para>
  /// 
  /// </summary>
  public sealed class NetworkServer
  {
    private static bool s_Active = false;
    private static object s_Sync = (object) new UnityEngine.Object();
    internal static NetworkScene s_NetworkScene = new NetworkScene();
    private static RemovePlayerMessage s_RemovePlayerMessage = new RemovePlayerMessage();
    private int m_ServerId = -1;
    private int m_ServerPort = -1;
    private int m_RelaySlotId = -1;
    private NetworkMessageHandlers m_MessageHandlers = new NetworkMessageHandlers();
    private ConnectionArray m_Connections = new ConnectionArray();
    private bool m_SendPeerInfo = true;
    private float m_MaxDelay = 0.1f;
    private List<LocalClient> m_LocalClients = new List<LocalClient>();
    private const int kMaxEventsPerFrame = 500;
    private const int m_RemoveListInterval = 100;
    private static volatile NetworkServer s_Instance;
    private static bool s_LocalClientActive;
    private HostTopology m_hostTopology;
    private byte[] m_MsgBuffer;
    private NetworkReader m_MsgReader;
    private HashSet<NetworkInstanceId> m_RemoveList;
    private int m_RemoveListCount;

    /// <summary>
    /// 
    /// <para>
    /// Dictionary of the message handlers registered with the server.
    /// </para>
    /// 
    /// </summary>
    public static Dictionary<short, NetworkMessageDelegate> handlers
    {
      get
      {
        return NetworkServer.instance.m_MessageHandlers.GetHandlers();
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// A list of all the current connections from clients.
    /// </para>
    /// 
    /// </summary>
    public static List<NetworkConnection> connections
    {
      get
      {
        return NetworkServer.instance.m_Connections.m_Connections;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// A list of local connections on the server.
    /// </para>
    /// 
    /// </summary>
    public static List<NetworkConnection> localConnections
    {
      get
      {
        return NetworkServer.instance.m_Connections.m_LocalConnections;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// This is a dictionary of networked objects that have been spawned on the server.
    /// </para>
    /// 
    /// </summary>
    public static Dictionary<NetworkInstanceId, NetworkIdentity> objects
    {
      get
      {
        return NetworkServer.s_NetworkScene.m_LocalObjects;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Setting this true will make the server send peer info to all participants of the network.
    /// </para>
    /// 
    /// </summary>
    public static bool sendPeerInfo
    {
      get
      {
        return NetworkServer.instance.m_SendPeerInfo;
      }
      set
      {
        NetworkServer.instance.m_SendPeerInfo = value;
      }
    }

    internal static NetworkServer instance
    {
      get
      {
        if (NetworkServer.s_Instance == null)
        {
          lock (NetworkServer.s_Sync)
          {
            if (NetworkServer.s_Instance == null)
              NetworkServer.s_Instance = new NetworkServer();
          }
        }
        return NetworkServer.s_Instance;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Checks if the server has been started.
    /// </para>
    /// 
    /// </summary>
    public static bool active
    {
      get
      {
        return NetworkServer.s_Active;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// True is a local client is currently active on the server.
    /// </para>
    /// 
    /// </summary>
    public static bool localClientActive
    {
      get
      {
        return NetworkServer.s_LocalClientActive;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The number of channels the network is configure with.
    /// </para>
    /// 
    /// </summary>
    public static int numChannels
    {
      get
      {
        return NetworkServer.instance.m_hostTopology.DefaultConfig.ChannelCount;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The maximum delay before sending packets on connections.
    /// </para>
    /// 
    /// </summary>
    public static float maxDelay
    {
      get
      {
        return NetworkServer.instance.m_MaxDelay;
      }
      set
      {
        NetworkServer.instance.InternalSetMaxDelay(value);
      }
    }

    private NetworkServer()
    {
      NetworkTransport.Init();
      if (LogFilter.logDev)
        Debug.Log((object) ("NetworkServer Created version " + (object) Version.Current));
      this.m_MsgBuffer = new byte[49152];
      this.m_MsgReader = new NetworkReader(this.m_MsgBuffer);
      this.m_RemoveList = new HashSet<NetworkInstanceId>();
    }

    public static bool Configure(ConnectionConfig config, int maxConnections)
    {
      return NetworkServer.Configure(new HostTopology(config, maxConnections));
    }

    public static bool Configure(HostTopology topology)
    {
      NetworkServer.instance.m_hostTopology = topology;
      return true;
    }

    /// <summary>
    /// 
    /// <para>
    /// Reset the NetworkServer singleton.
    /// </para>
    /// 
    /// </summary>
    public static void Reset()
    {
      NetworkDetailStats.ResetAll();
      NetworkTransport.Shutdown();
      NetworkTransport.Init();
      NetworkServer.s_Instance = (NetworkServer) null;
      NetworkServer.s_Active = false;
      NetworkServer.s_LocalClientActive = false;
    }

    /// <summary>
    /// 
    /// <para>
    /// This shuts down the server and disconnects all clients.
    /// </para>
    /// 
    /// </summary>
    public static void Shutdown()
    {
      if (NetworkServer.s_Instance != null)
      {
        NetworkServer.s_Instance.InternalDisconnectAll();
        if (NetworkServer.s_Instance.m_ServerId != -1)
        {
          NetworkTransport.RemoveHost(NetworkServer.s_Instance.m_ServerId);
          NetworkServer.s_Instance.m_ServerId = -1;
        }
        NetworkServer.s_Instance = (NetworkServer) null;
      }
      NetworkServer.s_Active = false;
      NetworkServer.s_LocalClientActive = false;
    }

    public static bool Listen(MatchInfo matchInfo, int listenPort)
    {
      if (!matchInfo.usingRelay)
        return NetworkServer.instance.InternalListen(listenPort);
      NetworkServer.instance.InternalListenRelay(matchInfo.address, matchInfo.port, matchInfo.networkId, Utility.GetSourceID(), matchInfo.nodeId, listenPort);
      return true;
    }

    /// <summary>
    /// 
    /// <para>
    /// UnderlyingModel.MemDoc.MemDocModelStarts a server using a relay server. This is the manual way of using the relay server, as the regular NetworkServer.Connect() will automatically  use the relay server if a UMatch exists.
    /// </para>
    /// 
    /// </summary>
    /// <param name="relayIp">Relay server IP Address.</param><param name="relayPort">Relay server port.</param><param name="netGuid">GUID of the network to create.</param><param name="sourceId">This server's sourceId.</param><param name="nodeId">The node to join the network with.</param>
    public static void ListenRelay(string relayIp, int relayPort, NetworkID netGuid, SourceID sourceId, NodeID nodeId)
    {
      NetworkServer.instance.InternalListenRelay(relayIp, relayPort, netGuid, sourceId, nodeId, 0);
    }

    internal void InternalListenRelay(string relayIp, int relayPort, NetworkID netGuid, SourceID sourceId, NodeID nodeId, int listenPort)
    {
      if (this.m_hostTopology == null)
      {
        ConnectionConfig defaultConfig = new ConnectionConfig();
        int num1 = (int) defaultConfig.AddChannel(QosType.Reliable);
        int num2 = (int) defaultConfig.AddChannel(QosType.Unreliable);
        this.m_hostTopology = new HostTopology(defaultConfig, 8);
      }
      this.m_ServerId = NetworkTransport.AddHost(this.m_hostTopology, listenPort);
      if (LogFilter.logDebug)
        Debug.Log((object) ("Server Host Slot Id: " + (object) this.m_ServerId));
      NetworkServer.Update();
      byte error;
      NetworkTransport.ConnectAsNetworkHost(this.m_ServerId, relayIp, relayPort, netGuid, sourceId, nodeId, out error);
      this.m_RelaySlotId = 0;
      if (LogFilter.logDebug)
        Debug.Log((object) ("Relay Slot Id: " + (object) this.m_RelaySlotId));
      if ((int) error != 0)
        Debug.Log((object) ("ListenRelay Error: " + (object) error));
      NetworkServer.s_Active = true;
      this.m_MessageHandlers.RegisterHandlerSafe((short) 35, new NetworkMessageDelegate(this.OnClientReadyMessage));
      this.m_MessageHandlers.RegisterHandlerSafe((short) 5, new NetworkMessageDelegate(this.OnCommandMessage));
      this.m_MessageHandlers.RegisterHandlerSafe((short) 6, new NetworkMessageDelegate(NetworkTransform.HandleTransform));
      this.m_MessageHandlers.RegisterHandlerSafe((short) 40, new NetworkMessageDelegate(NetworkAnimator.OnAnimationServerMessage));
      this.m_MessageHandlers.RegisterHandlerSafe((short) 41, new NetworkMessageDelegate(NetworkAnimator.OnAnimationParametersServerMessage));
      this.m_MessageHandlers.RegisterHandlerSafe((short) 42, new NetworkMessageDelegate(NetworkAnimator.OnAnimationTriggerServerMessage));
    }

    /// <summary>
    /// 
    /// <para>
    /// Start the server on the given port number. Note that if a UMatch has been created, this will list using the relay server in the UMatch.
    /// </para>
    /// 
    /// </summary>
    /// <param name="serverPort">Listen port number.</param>
    /// <returns>
    /// 
    /// <para>
    /// True is listen succeeded.
    /// </para>
    /// 
    /// </returns>
    public static bool Listen(int serverPort)
    {
      return NetworkServer.instance.InternalListen(serverPort);
    }

    internal bool InternalListen(int serverPort)
    {
      if (this.m_hostTopology == null)
      {
        ConnectionConfig defaultConfig = new ConnectionConfig();
        int num1 = (int) defaultConfig.AddChannel(QosType.Reliable);
        int num2 = (int) defaultConfig.AddChannel(QosType.Unreliable);
        this.m_hostTopology = new HostTopology(defaultConfig, 8);
      }
      if (LogFilter.logDebug)
        Debug.Log((object) ("Server Listen. port: " + (object) serverPort));
      this.m_ServerId = NetworkTransport.AddHost(this.m_hostTopology, serverPort);
      if (this.m_ServerId == -1)
        return false;
      this.m_ServerPort = serverPort;
      NetworkServer.s_Active = true;
      this.m_MessageHandlers.RegisterHandlerSafe((short) 35, new NetworkMessageDelegate(this.OnClientReadyMessage));
      this.m_MessageHandlers.RegisterHandlerSafe((short) 5, new NetworkMessageDelegate(this.OnCommandMessage));
      this.m_MessageHandlers.RegisterHandlerSafe((short) 6, new NetworkMessageDelegate(NetworkTransform.HandleTransform));
      this.m_MessageHandlers.RegisterHandlerSafe((short) 38, new NetworkMessageDelegate(this.OnRemovePlayerMessage));
      this.m_MessageHandlers.RegisterHandlerSafe((short) 40, new NetworkMessageDelegate(NetworkAnimator.OnAnimationServerMessage));
      this.m_MessageHandlers.RegisterHandlerSafe((short) 41, new NetworkMessageDelegate(NetworkAnimator.OnAnimationParametersServerMessage));
      this.m_MessageHandlers.RegisterHandlerSafe((short) 42, new NetworkMessageDelegate(NetworkAnimator.OnAnimationTriggerServerMessage));
      return true;
    }

    internal void InternalSetMaxDelay(float seconds)
    {
      for (int localIndex = this.m_Connections.LocalIndex; localIndex < this.m_Connections.Count; ++localIndex)
      {
        NetworkConnection networkConnection = this.m_Connections.Get(localIndex);
        if (networkConnection != null)
          networkConnection.SetMaxDelay(seconds);
      }
      this.m_MaxDelay = seconds;
    }

    internal int AddLocalClient(LocalClient localClient)
    {
      this.m_LocalClients.Add(localClient);
      return this.m_Connections.AddLocal((NetworkConnection) new ULocalConnectionToClient(localClient));
    }

    internal void SetLocalObjectOnServer(NetworkInstanceId netId, GameObject obj)
    {
      if (LogFilter.logDev)
      {
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "SetLocalObjectOnServer ";
        objArray[index1] = (object) str1;
        int index2 = 1;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local = (ValueType) netId;
        objArray[index2] = (object) local;
        int index3 = 2;
        string str2 = " ";
        objArray[index3] = (object) str2;
        int index4 = 3;
        GameObject gameObject = obj;
        objArray[index4] = (object) gameObject;
        Debug.Log((object) string.Concat(objArray));
      }
      NetworkServer.s_NetworkScene.SetLocalObject(netId, obj, false, true);
    }

    internal void ActivateLocalClientScene()
    {
      if (NetworkServer.s_LocalClientActive)
        return;
      NetworkServer.s_LocalClientActive = true;
      using (Dictionary<NetworkInstanceId, NetworkIdentity>.ValueCollection.Enumerator enumerator = NetworkServer.objects.Values.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          NetworkIdentity current = enumerator.Current;
          if (!current.isClient)
          {
            if (LogFilter.logDev)
            {
              object[] objArray = new object[4];
              int index1 = 0;
              string str1 = "ActivateClientScene ";
              objArray[index1] = (object) str1;
              int index2 = 1;
              // ISSUE: variable of a boxed type
              __Boxed<NetworkInstanceId> local = (ValueType) current.netId;
              objArray[index2] = (object) local;
              int index3 = 2;
              string str2 = " ";
              objArray[index3] = (object) str2;
              int index4 = 3;
              GameObject gameObject = current.gameObject;
              objArray[index4] = (object) gameObject;
              Debug.Log((object) string.Concat(objArray));
            }
            current.OnStartClient();
          }
        }
      }
    }

    public static bool SendToAll(short msgType, MessageBase msg)
    {
      if (LogFilter.logDev)
        Debug.Log((object) ("Server.SendToAll msgType:" + (object) msgType));

      ConnectionArray connectionArray = NetworkServer.instance.m_Connections;
      bool flag = true;

      for (int localIndex = connectionArray.LocalIndex; localIndex < connectionArray.Count; ++localIndex)
      {
        NetworkConnection networkConnection = connectionArray.Get(localIndex);
        if (networkConnection != null)
          flag &= networkConnection.Send(msgType, msg);
      }

      return flag;
    }

    private static bool SendToObservers(GameObject contextObj, short msgType, MessageBase msg)
    {
      if (LogFilter.logDev)
        Debug.Log((object) ("Server.SendToObservers id:" + (object) msgType));
      bool flag = true;
      NetworkIdentity component = contextObj.GetComponent<NetworkIdentity>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null || component.observers == null)
        return false;
      int count = component.observers.Count;
      for (int index = 0; index < count; ++index)
      {
        NetworkConnection networkConnection = component.observers[index];
        flag &= networkConnection.Send(msgType, msg);
      }
      return flag;
    }

    public static bool SendToReady(GameObject contextObj, short msgType, MessageBase msg)
    {
      if (LogFilter.logDev)
        Debug.Log((object) ("Server.SendToReady id:" + (object) msgType));
      if ((UnityEngine.Object) contextObj == (UnityEngine.Object) null)
      {
        for (int localIndex = NetworkServer.s_Instance.m_Connections.LocalIndex; localIndex < NetworkServer.s_Instance.m_Connections.Count; ++localIndex)
        {
          NetworkConnection networkConnection = NetworkServer.s_Instance.m_Connections.Get(localIndex);
          if (networkConnection != null && networkConnection.isReady)
            networkConnection.Send(msgType, msg);
        }
        return true;
      }
      bool flag = true;
      NetworkIdentity component = contextObj.GetComponent<NetworkIdentity>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null || component.observers == null)
        return false;
      int count = component.observers.Count;
      for (int index = 0; index < count; ++index)
      {
        NetworkConnection networkConnection = component.observers[index];
        if (networkConnection.isReady)
          flag &= networkConnection.Send(msgType, msg);
      }
      return flag;
    }

    public static void SendWriterToReady(GameObject contextObj, NetworkWriter writer, int channelId)
    {
      if (writer.AsArraySegment().Count > (int) short.MaxValue)
        throw new UnityException("NetworkWriter used buffer is too big!");
      NetworkServer.SendBytesToReady(contextObj, writer.AsArraySegment().Array, writer.AsArraySegment().Count, channelId);
    }

    public static void SendBytesToReady(GameObject contextObj, byte[] buffer, int numBytes, int channelId)
    {
      if ((UnityEngine.Object) contextObj == (UnityEngine.Object) null)
      {
        for (int localIndex = NetworkServer.s_Instance.m_Connections.LocalIndex; localIndex < NetworkServer.s_Instance.m_Connections.Count; ++localIndex)
        {
          NetworkConnection networkConnection = NetworkServer.s_Instance.m_Connections.Get(localIndex);
          if (networkConnection != null && networkConnection.isReady)
            networkConnection.SendBytes(buffer, numBytes, channelId);
        }
      }
      else
      {
        NetworkIdentity component = contextObj.GetComponent<NetworkIdentity>();
        try
        {
          int count = component.observers.Count;
          for (int index = 0; index < count; ++index)
          {
            NetworkConnection networkConnection = component.observers[index];
            if (networkConnection.isReady)
              networkConnection.SendBytes(buffer, numBytes, channelId);
          }
        }
        catch (NullReferenceException ex)
        {
          if (!LogFilter.logWarn)
            return;
          Debug.LogWarning((object) ("SendBytesToReady object " + (object) contextObj + " has not been spawned"));
        }
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// This sends an array of bytes to a specific player.
    /// </para>
    /// 
    /// </summary>
    /// <param name="player">The player to send he bytes to.</param><param name="buffer">Array of bytes to send.</param><param name="numBytes">Size of array.</param><param name="channelId">UNET channel id to send bytes on.</param>
    public static void SendBytesToPlayer(GameObject player, byte[] buffer, int numBytes, int channelId)
    {
      NetworkConnection conn;
      if (!NetworkServer.instance.m_Connections.ContainsPlayer(player, out conn))
        return;
      conn.SendBytes(buffer, numBytes, channelId);
    }

    public static bool SendUnreliableToAll(short msgType, MessageBase msg)
    {
      if (LogFilter.logDev)
        Debug.Log((object) ("Server.SendUnreliableToAll msgType:" + (object) msgType));
      ConnectionArray connectionArray = NetworkServer.instance.m_Connections;
      bool flag = true;
      for (int localIndex = connectionArray.LocalIndex; localIndex < connectionArray.Count; ++localIndex)
      {
        NetworkConnection networkConnection = connectionArray.Get(localIndex);
        if (networkConnection != null)
          flag &= networkConnection.SendUnreliable(msgType, msg);
      }
      return flag;
    }

    public static bool SendUnreliableToReady(GameObject contextObj, short msgType, MessageBase msg)
    {
      if (LogFilter.logDev)
        Debug.Log((object) ("Server.SendUnreliableToReady id:" + (object) msgType));
      if ((UnityEngine.Object) contextObj == (UnityEngine.Object) null)
      {
        for (int localIndex = NetworkServer.s_Instance.m_Connections.LocalIndex; localIndex < NetworkServer.s_Instance.m_Connections.Count; ++localIndex)
        {
          NetworkConnection networkConnection = NetworkServer.s_Instance.m_Connections.Get(localIndex);
          if (networkConnection != null && networkConnection.isReady)
            networkConnection.SendUnreliable(msgType, msg);
        }
        return true;
      }
      bool flag = true;
      NetworkIdentity component = contextObj.GetComponent<NetworkIdentity>();
      int count = component.observers.Count;
      for (int index = 0; index < count; ++index)
      {
        NetworkConnection networkConnection = component.observers[index];
        if (networkConnection.isReady)
          flag &= networkConnection.SendUnreliable(msgType, msg);
      }
      return flag;
    }

    public static bool SendByChannelToAll(short msgType, MessageBase msg, int channelId)
    {
      if (LogFilter.logDev)
        Debug.Log((object) ("Server.SendByChannelToAll id:" + (object) msgType));
      ConnectionArray connectionArray = NetworkServer.instance.m_Connections;
      bool flag = true;
      for (int localIndex = connectionArray.LocalIndex; localIndex < connectionArray.Count; ++localIndex)
      {
        NetworkConnection networkConnection = connectionArray.Get(localIndex);
        if (networkConnection != null)
          flag &= networkConnection.SendByChannel(msgType, msg, channelId);
      }
      return flag;
    }

    public static bool SendByChannelToReady(GameObject contextObj, short msgType, MessageBase msg, int channelId)
    {
      if (LogFilter.logDev)
        Debug.Log((object) ("Server.SendByChannelToReady msgType:" + (object) msgType));
      if ((UnityEngine.Object) contextObj == (UnityEngine.Object) null)
      {
        for (int localIndex = NetworkServer.s_Instance.m_Connections.LocalIndex; localIndex < NetworkServer.s_Instance.m_Connections.Count; ++localIndex)
        {
          NetworkConnection networkConnection = NetworkServer.s_Instance.m_Connections.Get(localIndex);
          if (networkConnection != null && networkConnection.isReady)
            networkConnection.SendByChannel(msgType, msg, channelId);
        }
        return true;
      }
      bool flag = true;
      NetworkIdentity component = contextObj.GetComponent<NetworkIdentity>();
      int count = component.observers.Count;
      for (int index = 0; index < count; ++index)
      {
        NetworkConnection networkConnection = component.observers[index];
        if (networkConnection.isReady)
          flag &= networkConnection.SendByChannel(msgType, msg, channelId);
      }
      return flag;
    }

    /// <summary>
    /// 
    /// <para>
    /// Disconnect all currently connected clients.
    /// </para>
    /// 
    /// </summary>
    public static void DisconnectAll()
    {
      NetworkServer.instance.InternalDisconnectAll();
    }

    internal void InternalDisconnectAll()
    {
      for (int localIndex = this.m_Connections.LocalIndex; localIndex < this.m_Connections.Count; ++localIndex)
      {
        NetworkConnection networkConnection = this.m_Connections.Get(localIndex);
        if (networkConnection != null)
        {
          networkConnection.Disconnect();
          networkConnection.Dispose();
        }
      }
      NetworkServer.s_Active = false;
      NetworkServer.s_LocalClientActive = false;
    }

    internal static void Update()
    {
      if (NetworkServer.s_Instance == null)
        return;
      NetworkServer.s_Instance.InternalUpdate();
    }

    internal void UpdateServerObjects()
    {
      using (Dictionary<NetworkInstanceId, NetworkIdentity>.ValueCollection.Enumerator enumerator = NetworkServer.objects.Values.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          NetworkIdentity current = enumerator.Current;
          try
          {
            current.UNetUpdate();
          }
          catch (NullReferenceException ex)
          {
          }
        }
      }
      if (this.m_RemoveListCount++ % 100 != 0)
        return;
      this.CheckForNullObjects();
    }

    private void CheckForNullObjects()
    {
      using (Dictionary<NetworkInstanceId, NetworkIdentity>.KeyCollection.Enumerator enumerator = NetworkServer.objects.Keys.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          NetworkInstanceId current = enumerator.Current;
          NetworkIdentity networkIdentity = NetworkServer.objects[current];
          if ((UnityEngine.Object) networkIdentity == (UnityEngine.Object) null || (UnityEngine.Object) networkIdentity.gameObject == (UnityEngine.Object) null)
            this.m_RemoveList.Add(current);
        }
      }
      if (this.m_RemoveList.Count <= 0)
        return;
      using (HashSet<NetworkInstanceId>.Enumerator enumerator = this.m_RemoveList.GetEnumerator())
      {
        while (enumerator.MoveNext())
          NetworkServer.objects.Remove(enumerator.Current);
      }
      this.m_RemoveList.Clear();
    }

    internal void InternalUpdate()
    {
      if (this.m_ServerId == -1 || !NetworkTransport.IsStarted)
        return;
      int num = 0;
      byte error1;
      if (this.m_RelaySlotId != -1)
      {
        NetworkEventType networkEventType = NetworkTransport.ReceiveRelayEventFromHost(this.m_ServerId, out error1);
        if (networkEventType != NetworkEventType.Nothing && LogFilter.logDebug)
          Debug.Log((object) ("NetGroup event:" + (object) networkEventType));
        if (networkEventType == NetworkEventType.ConnectEvent && LogFilter.logDebug)
          Debug.Log((object) "NetGroup server connected");
        if (networkEventType == NetworkEventType.DisconnectEvent && LogFilter.logDebug)
          Debug.Log((object) "NetGroup server disconnected");
      }
      NetworkEventType networkEventType1;
      do
      {
        int connectionId;
        int channelId;
        int receivedSize;
        networkEventType1 = NetworkTransport.ReceiveFromHost(this.m_ServerId, out connectionId, out channelId, this.m_MsgBuffer, (int) (ushort) this.m_MsgBuffer.Length, out receivedSize, out error1);
        if (networkEventType1 != NetworkEventType.Nothing && LogFilter.logDev)
        {
          object[] objArray = new object[6];
          int index1 = 0;
          string str1 = "Server event: host=";
          objArray[index1] = (object) str1;
          int index2 = 1;
          // ISSUE: variable of a boxed type
          __Boxed<int> local1 = (ValueType) this.m_ServerId;
          objArray[index2] = (object) local1;
          int index3 = 2;
          string str2 = " event=";
          objArray[index3] = (object) str2;
          int index4 = 3;
          // ISSUE: variable of a boxed type
          __Boxed<NetworkEventType> local2 = (Enum) networkEventType1;
          objArray[index4] = (object) local2;
          int index5 = 4;
          string str3 = " error=";
          objArray[index5] = (object) str3;
          int index6 = 5;
          // ISSUE: variable of a boxed type
          __Boxed<byte> local3 = (ValueType) error1;
          objArray[index6] = (object) local3;
          Debug.Log((object) string.Concat(objArray));
        }
        switch (networkEventType1)
        {
          case NetworkEventType.DataEvent:
            NetworkConnection conn = this.m_Connections.Get(connectionId);
            if ((int) error1 != 0)
            {
              this.GenerateDataError(conn, (int) error1);
              return;
            }
            NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Incoming, (short) 29, "msg", 1);
            if (conn != null)
            {
              this.m_MsgReader.SeekZero();
              conn.HandleMessage(this.m_MessageHandlers.GetHandlers(), this.m_MsgReader, receivedSize, channelId);
              goto case 3;
            }
            else if (LogFilter.logError)
            {
              Debug.LogError((object) "Unknown connection data event?!?");
              goto case 3;
            }
            else
              goto case 3;
          case NetworkEventType.ConnectEvent:
            if (LogFilter.logDebug)
              Debug.Log((object) ("Server accepted client:" + (object) connectionId));
            if ((int) error1 != 0)
            {
              this.GenerateConnectError((int) error1);
              return;
            }
            string address;
            int port;
            NetworkID network;
            NodeID dstNode;
            byte error2;
            NetworkTransport.GetConnectionInfo(this.m_ServerId, connectionId, out address, out port, out network, out dstNode, out error2);
            NetworkConnection networkConnection = new NetworkConnection();
            networkConnection.Initialize(address, this.m_ServerId, connectionId, this.m_hostTopology);
            networkConnection.SetMaxDelay(this.m_MaxDelay);
            this.m_Connections.Add(connectionId, networkConnection);
            this.m_MessageHandlers.InvokeHandlerNoData((short) 32, networkConnection);
            if (this.m_SendPeerInfo)
              this.SendNetworkInfo(networkConnection);
            this.SendCRC(networkConnection);
            goto case 3;
          case NetworkEventType.DisconnectEvent:
            NetworkConnection @unsafe = this.m_Connections.GetUnsafe(connectionId);
            if ((int) error1 != 0 && (int) error1 != 6)
              this.GenerateDisconnectError(@unsafe, (int) error1);
            this.m_Connections.Remove(connectionId);
            if (@unsafe != null)
            {
              this.m_MessageHandlers.InvokeHandlerNoData((short) 33, @unsafe);
              for (int index = 0; index < @unsafe.playerControllers.Count; ++index)
              {
                if ((UnityEngine.Object) @unsafe.playerControllers[index].gameObject != (UnityEngine.Object) null && LogFilter.logWarn)
                  Debug.LogWarning((object) "Player not destroyed when connection disconnected.");
              }
              if (LogFilter.logDebug)
                Debug.Log((object) ("Server lost client:" + (object) connectionId));
              @unsafe.RemoveObservers();
              @unsafe.Dispose();
            }
            else if (LogFilter.logDebug)
              Debug.Log((object) "Connection is null in disconnect event");
            if (this.m_SendPeerInfo)
            {
              this.SendNetworkInfo(@unsafe);
              goto case 3;
            }
            else
              goto case 3;
          case NetworkEventType.Nothing:
            if (++num >= 500)
            {
              if (LogFilter.logDebug)
              {
                Debug.Log((object) ("kMaxEventsPerFrame hit (" + (object) 500 + ")"));
                goto label_47;
              }
              else
                goto label_47;
            }
            else
              continue;
          default:
            if (LogFilter.logError)
            {
              Debug.LogError((object) ("Unknown network message type received: " + (object) networkEventType1));
              goto case 3;
            }
            else
              goto case 3;
        }
      }
      while (networkEventType1 != NetworkEventType.Nothing);
label_47:
      this.UpdateServerObjects();
      for (int localIndex = this.m_Connections.LocalIndex; localIndex < this.m_Connections.Count; ++localIndex)
      {
        NetworkConnection networkConnection = this.m_Connections.Get(localIndex);
        if (networkConnection != null)
          networkConnection.FlushInternalBuffer();
      }
    }

    private void GenerateConnectError(int error)
    {
      if (LogFilter.logError)
        Debug.LogError((object) ("UNet Server Connect Error: " + (object) error));
      this.GenerateError((NetworkConnection) null, error);
    }

    private void GenerateDataError(NetworkConnection conn, int error)
    {
      NetworkError networkError = (NetworkError) error;
      if (LogFilter.logError)
        Debug.LogError((object) ("UNet Server Data Error: " + (object) networkError));
      this.GenerateError(conn, error);
    }

    private void GenerateDisconnectError(NetworkConnection conn, int error)
    {
      NetworkError networkError = (NetworkError) error;
      if (LogFilter.logError)
      {
        object[] objArray = new object[6];
        int index1 = 0;
        string str1 = "UNet Server Disconnect Error: ";
        objArray[index1] = (object) str1;
        int index2 = 1;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkError> local1 = (Enum) networkError;
        objArray[index2] = (object) local1;
        int index3 = 2;
        string str2 = " conn:[";
        objArray[index3] = (object) str2;
        int index4 = 3;
        NetworkConnection networkConnection = conn;
        objArray[index4] = (object) networkConnection;
        int index5 = 4;
        string str3 = "]:";
        objArray[index5] = (object) str3;
        int index6 = 5;
        // ISSUE: variable of a boxed type
        __Boxed<int> local2 = (ValueType) conn.connectionId;
        objArray[index6] = (object) local2;
        Debug.LogError((object) string.Concat(objArray));
      }
      this.GenerateError(conn, error);
    }

    private void GenerateError(NetworkConnection conn, int error)
    {
      if (this.m_MessageHandlers.GetHandler((short) 34) == null)
        return;
      ErrorMessage errorMessage = new ErrorMessage();
      errorMessage.errorCode = error;
      NetworkWriter writer = new NetworkWriter();
      errorMessage.Serialize(writer);
      NetworkReader reader = new NetworkReader(writer);
      this.m_MessageHandlers.InvokeHandler((short) 34, conn, reader, 0);
    }

    /// <summary>
    /// 
    /// <para>
    /// Register a handler for a particular message type.
    /// </para>
    /// 
    /// </summary>
    /// <param name="msgType">Message type number.</param><param name="handler">Function handler which will be invoked for when this message type is received.</param>
    public static void RegisterHandler(short msgType, NetworkMessageDelegate handler)
    {
      NetworkServer.instance.m_MessageHandlers.RegisterHandler(msgType, handler);
    }

    /// <summary>
    /// 
    /// <para>
    /// Unregisters a handler for a particular message type.
    /// </para>
    /// 
    /// </summary>
    /// <param name="msgType">The message type to remove the handler for.</param>
    public static void UnregisterHandler(short msgType)
    {
      NetworkServer.instance.m_MessageHandlers.UnregisterHandler(msgType);
    }

    /// <summary>
    /// 
    /// <para>
    /// Clear all registered callback handlers.
    /// </para>
    /// 
    /// </summary>
    public static void ClearHandlers()
    {
      NetworkServer.instance.m_MessageHandlers.ClearMessageHandlers();
    }

    /// <summary>
    /// 
    /// <para>
    /// Clears all registered spawn prefab and spawn handler functions for this server.
    /// </para>
    /// 
    /// </summary>
    public static void ClearSpawners()
    {
      NetworkScene.ClearSpawners();
    }

    public static void GetStatsOut(out int numMsgs, out int numBufferedMsgs, out int numBytes, out int lastBufferedPerSecond)
    {
      numMsgs = 0;
      numBufferedMsgs = 0;
      numBytes = 0;
      lastBufferedPerSecond = 0;
      ConnectionArray connectionArray = NetworkServer.instance.m_Connections;
      for (int localIndex = connectionArray.LocalIndex; localIndex < connectionArray.Count; ++localIndex)
      {
        NetworkConnection networkConnection = connectionArray.Get(localIndex);
        if (networkConnection != null)
        {
          int numMsgs1 = 0;
          int numBufferedMsgs1 = 0;
          int numBytes1 = 0;
          int lastBufferedPerSecond1 = 0;
          networkConnection.GetStatsOut(out numMsgs1, out numBufferedMsgs1, out numBytes1, out lastBufferedPerSecond1);
          numMsgs = numMsgs + numMsgs1;
          numBufferedMsgs = numBufferedMsgs + numBufferedMsgs1;
          numBytes = numBytes + numBytes1;
          lastBufferedPerSecond = lastBufferedPerSecond + lastBufferedPerSecond1;
        }
      }
    }

    public static void GetStatsIn(out int numMsgs, out int numBytes)
    {
      numMsgs = 0;
      numBytes = 0;
      ConnectionArray connectionArray = NetworkServer.instance.m_Connections;
      for (int localIndex = connectionArray.LocalIndex; localIndex < connectionArray.Count; ++localIndex)
      {
        NetworkConnection networkConnection = connectionArray.Get(localIndex);
        if (networkConnection != null)
        {
          int numMsgs1;
          int numBytes1;
          networkConnection.GetStatsIn(out numMsgs1, out numBytes1);
          numMsgs = numMsgs + numMsgs1;
          numBytes = numBytes + numBytes1;
        }
      }
    }

    public static void SendToClientOfPlayer(GameObject player, short msgType, MessageBase msg)
    {
      NetworkConnection conn;
      if (NetworkServer.instance.m_Connections.ContainsPlayer(player, out conn))
      {
        conn.Send(msgType, msg);
      }
      else
      {
        if (!LogFilter.logError)
          return;
        Debug.LogError((object) ("Failed to send message to player object '" + player.name + ", not found in connection list"));
      }
    }

    public static void SendToClient(int connectionId, short msgType, MessageBase msg)
    {
      NetworkConnection networkConnection = NetworkServer.instance.m_Connections.Get(connectionId);
      if (networkConnection != null)
      {
        networkConnection.Send(msgType, msg);
      }
      else
      {
        if (!LogFilter.logError)
          return;
        Debug.LogError((object) ("Failed to send message to connection ID '" + (object) connectionId + ", not found in connection list"));
      }
    }

    public static bool ReplacePlayerForConnection(NetworkConnection conn, GameObject player, short playerControllerId, NetworkHash128 assetId)
    {
      NetworkIdentity view;
      if (NetworkServer.GetNetworkIdentity(player, out view))
        view.SetDynamicAssetId(assetId);
      return NetworkServer.instance.InternalReplacePlayerForConnection(conn, player, playerControllerId);
    }

    /// <summary>
    /// 
    /// <para>
    /// This replaces the player object for a connection with a different player object. The old player object is not destroyed.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">Connection which is adding the player.</param><param name="player">Player object spawned for the player.</param><param name="playerControllerId">The player controller ID number as specified by client.</param>
    /// <returns>
    /// 
    /// <para>
    /// True if player was replaced.
    /// </para>
    /// 
    /// </returns>
    public static bool ReplacePlayerForConnection(NetworkConnection conn, GameObject player, short playerControllerId)
    {
      return NetworkServer.instance.InternalReplacePlayerForConnection(conn, player, playerControllerId);
    }

    public static bool AddPlayerForConnection(NetworkConnection conn, GameObject player, short playerControllerId, NetworkHash128 assetId)
    {
      NetworkIdentity view;
      if (NetworkServer.GetNetworkIdentity(player, out view))
        view.SetDynamicAssetId(assetId);
      return NetworkServer.instance.InternalAddPlayerForConnection(conn, player, playerControllerId);
    }

    /// <summary>
    /// 
    /// <para>
    /// When a SYSTEM_ADD_PLAYER message handler has received a request from a player, the server should call this when he's created the player's object.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">Connection which is adding the player.</param><param name="player">Player object spawned for the player.</param><param name="playerControllerId">The player controller ID number as specified by client.</param>
    /// <returns>
    /// 
    /// <para>
    /// True if player was added.
    /// </para>
    /// 
    /// </returns>
    public static bool AddPlayerForConnection(NetworkConnection conn, GameObject player, short playerControllerId)
    {
      return NetworkServer.instance.InternalAddPlayerForConnection(conn, player, playerControllerId);
    }

    internal bool InternalAddPlayerForConnection(NetworkConnection conn, GameObject playerGameObject, short playerControllerId)
    {
      NetworkIdentity view;
      if (!NetworkServer.GetNetworkIdentity(playerGameObject, out view))
      {
        if (LogFilter.logError)
          Debug.Log((object) ("AddPlayer: playerGameObject has no NetworkIdentity. Please add a NetworkIdentity to " + (object) playerGameObject));
        return false;
      }
      if (!this.CheckPlayerControllerIdForConnection(conn, playerControllerId))
        return false;
      PlayerController playerController1 = (PlayerController) null;
      GameObject gameObject = (GameObject) null;
      if (conn.GetPlayerController(playerControllerId, out playerController1))
        gameObject = playerController1.gameObject;
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
      {
        if (LogFilter.logError)
          Debug.Log((object) ("AddPlayer: player object already exists for playerControllerId of " + (object) playerControllerId));
        return false;
      }
      PlayerController playerController2 = new PlayerController(playerGameObject, playerControllerId);
      conn.SetPlayerController(playerController2);
      view.m_PlayerId = playerController2.playerControllerId;
      view.SetConnectionToClient(conn);
      NetworkServer.SetClientReady(conn);
      if (this.SetupLocalPlayerForConnection(conn, view, playerController2))
        return true;
      if (LogFilter.logDebug)
      {
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "Adding new playerGameObject object netId: ";
        objArray[index1] = (object) str1;
        int index2 = 1;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local1 = (ValueType) playerGameObject.GetComponent<NetworkIdentity>().netId;
        objArray[index2] = (object) local1;
        int index3 = 2;
        string str2 = " asset ID ";
        objArray[index3] = (object) str2;
        int index4 = 3;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkHash128> local2 = (ValueType) playerGameObject.GetComponent<NetworkIdentity>().assetId;
        objArray[index4] = (object) local2;
        Debug.Log((object) string.Concat(objArray));
      }
      this.FinishPlayerForConnection(conn, view, playerGameObject);
      return true;
    }

    private bool CheckPlayerControllerIdForConnection(NetworkConnection conn, short playerControllerId)
    {
      if ((int) playerControllerId < 0)
      {
        if (LogFilter.logError)
          Debug.LogError((object) ("AddPlayer: playerControllerId of " + (object) playerControllerId + " is negative"));
        return false;
      }
      if ((int) playerControllerId > 32)
      {
        if (LogFilter.logError)
        {
          object[] objArray = new object[4];
          int index1 = 0;
          string str1 = "AddPlayer: playerControllerId of ";
          objArray[index1] = (object) str1;
          int index2 = 1;
          // ISSUE: variable of a boxed type
          __Boxed<short> local1 = (ValueType) playerControllerId;
          objArray[index2] = (object) local1;
          int index3 = 2;
          string str2 = " is too high. max is ";
          objArray[index3] = (object) str2;
          int index4 = 3;
          // ISSUE: variable of a boxed type
          __Boxed<int> local2 = (ValueType) 32;
          objArray[index4] = (object) local2;
          Debug.Log((object) string.Concat(objArray));
        }
        return false;
      }
      if ((int) playerControllerId > 16 && LogFilter.logWarn)
        Debug.LogWarning((object) ("AddPlayer: playerControllerId of " + (object) playerControllerId + " is unusually high"));
      return true;
    }

    private bool SetupLocalPlayerForConnection(NetworkConnection conn, NetworkIdentity uv, PlayerController newPlayerController)
    {
      if (LogFilter.logDev)
        Debug.Log((object) ("NetworkServer SetupLocalPlayerForConnection netID:" + (object) uv.netId));
      ULocalConnectionToClient connectionToClient = conn as ULocalConnectionToClient;
      if (connectionToClient == null)
        return false;
      if (LogFilter.logDev)
        Debug.Log((object) "NetworkServer AddPlayer handling ULocalConnectionToClient");
      if (uv.netId.IsEmpty())
        uv.OnStartServer();
      uv.RebuildObservers(true);
      this.SendSpawnMessage(uv, (NetworkConnection) null);
      connectionToClient.localClient.AddLocalPlayer(newPlayerController);
      uv.SetLocalPlayer(newPlayerController.playerControllerId);
      return true;
    }

    private void FinishPlayerForConnection(NetworkConnection conn, NetworkIdentity uv, GameObject playerGameObject)
    {
      if (uv.netId.IsEmpty())
        NetworkServer.Spawn(playerGameObject);
      conn.Send((short) 4, (MessageBase) new OwnerMessage()
      {
        netId = uv.netId,
        playerControllerId = uv.playerControllerId
      });
    }

    internal bool InternalReplacePlayerForConnection(NetworkConnection conn, GameObject playerGameObject, short playerControllerId)
    {
      NetworkIdentity view;
      if (!NetworkServer.GetNetworkIdentity(playerGameObject, out view))
      {
        if (LogFilter.logError)
          Debug.LogError((object) ("ReplacePlayer: playerGameObject has no NetworkIdentity. Please add a NetworkIdentity to " + (object) playerGameObject));
        return false;
      }
      if (!this.CheckPlayerControllerIdForConnection(conn, playerControllerId))
        return false;
      if (LogFilter.logDev)
        Debug.Log((object) "NetworkServer ReplacePlayer");
      PlayerController playerController = new PlayerController(playerGameObject, playerControllerId);
      conn.SetPlayerController(playerController);
      view.m_PlayerId = playerController.playerControllerId;
      view.SetConnectionToClient(conn);
      if (LogFilter.logDev)
        Debug.Log((object) "NetworkServer ReplacePlayer setup local");
      if (this.SetupLocalPlayerForConnection(conn, view, playerController))
        return true;
      if (LogFilter.logDebug)
      {
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "Replacing playerGameObject object netId: ";
        objArray[index1] = (object) str1;
        int index2 = 1;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local1 = (ValueType) playerGameObject.GetComponent<NetworkIdentity>().netId;
        objArray[index2] = (object) local1;
        int index3 = 2;
        string str2 = " asset ID ";
        objArray[index3] = (object) str2;
        int index4 = 3;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkHash128> local2 = (ValueType) playerGameObject.GetComponent<NetworkIdentity>().assetId;
        objArray[index4] = (object) local2;
        Debug.Log((object) string.Concat(objArray));
      }
      this.FinishPlayerForConnection(conn, view, playerGameObject);
      return true;
    }

    private static bool GetNetworkIdentity(GameObject go, out NetworkIdentity view)
    {
      view = go.GetComponent<NetworkIdentity>();
      if (!((UnityEngine.Object) view == (UnityEngine.Object) null))
        return true;
      if (LogFilter.logError)
        Debug.LogError((object) "UNET failure. GameObject doesn't have NetworkIdentity.");
      return false;
    }

    /// <summary>
    /// 
    /// <para>
    /// Sets the client to be ready.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">The connection of the client to make ready.</param>
    public static void SetClientReady(NetworkConnection conn)
    {
      NetworkServer.instance.SetClientReadyInternal(conn);
    }

    internal void SetClientReadyInternal(NetworkConnection conn)
    {
      if (conn.isReady)
      {
        if (!LogFilter.logDebug)
          return;
        Debug.Log((object) ("SetClientReady conn " + (object) conn.connectionId + " already ready"));
      }
      else
      {
        if (conn.playerControllers.Count == 0 && LogFilter.logDebug)
          Debug.LogWarning((object) "Ready with no player object");
        conn.isReady = true;
        if (conn is ULocalConnectionToClient)
        {
          if (LogFilter.logDev)
            Debug.Log((object) "NetworkServer Ready handling ULocalConnectionToClient");
          using (Dictionary<NetworkInstanceId, NetworkIdentity>.ValueCollection.Enumerator enumerator = NetworkServer.objects.Values.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              NetworkIdentity current = enumerator.Current;
              if ((UnityEngine.Object) current != (UnityEngine.Object) null && (UnityEngine.Object) current.gameObject != (UnityEngine.Object) null && !current.isClient)
              {
                if (LogFilter.logDev)
                  Debug.Log((object) "LocalClient.SetSpawnObject calling OnStartClient");
                current.OnStartClient();
              }
            }
          }
        }
        else
        {
          ObjectSpawnFinishedMessage spawnFinishedMessage = new ObjectSpawnFinishedMessage();
          spawnFinishedMessage.state = 0U;
          conn.Send((short) 12, (MessageBase) spawnFinishedMessage);
          using (Dictionary<NetworkInstanceId, NetworkIdentity>.ValueCollection.Enumerator enumerator = NetworkServer.objects.Values.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              NetworkIdentity current = enumerator.Current;
              if ((UnityEngine.Object) current == (UnityEngine.Object) null)
              {
                if (LogFilter.logWarn)
                  Debug.LogWarning((object) "Invalid object found in server local object list (null NetworkIdentity).");
              }
              else
              {
                if (LogFilter.logDebug)
                {
                  object[] objArray = new object[4];
                  int index1 = 0;
                  string str1 = "Sending spawn message for current server objects name='";
                  objArray[index1] = (object) str1;
                  int index2 = 1;
                  string name = current.gameObject.name;
                  objArray[index2] = (object) name;
                  int index3 = 2;
                  string str2 = "' netId=";
                  objArray[index3] = (object) str2;
                  int index4 = 3;
                  // ISSUE: variable of a boxed type
                  __Boxed<NetworkInstanceId> local = (ValueType) current.netId;
                  objArray[index4] = (object) local;
                  Debug.Log((object) string.Concat(objArray));
                }
                if (current.OnCheckObserver(conn))
                  current.AddObserver(conn);
              }
            }
          }
          spawnFinishedMessage.state = 1U;
          conn.Send((short) 12, (MessageBase) spawnFinishedMessage);
        }
      }
    }

    internal static void ShowForConnection(NetworkIdentity uv, NetworkConnection conn)
    {
      if (!conn.isReady)
        return;
      NetworkServer.instance.SendSpawnMessage(uv, conn);
    }

    internal static void HideForConnection(NetworkIdentity uv, NetworkConnection conn)
    {
      conn.Send((short) 13, (MessageBase) new ObjectDestroyMessage()
      {
        netId = uv.netId
      });
    }

    /// <summary>
    /// 
    /// <para>
    /// Marks all connected clients as no longer ready.
    /// </para>
    /// 
    /// </summary>
    public static void SetAllClientsNotReady()
    {
      ConnectionArray connectionArray = NetworkServer.instance.m_Connections;
      for (int localIndex = connectionArray.LocalIndex; localIndex < connectionArray.Count; ++localIndex)
      {
        NetworkConnection conn = connectionArray.Get(localIndex);
        if (conn != null)
          NetworkServer.SetClientNotReady(conn);
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Sets the client of the connection to be not-ready.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">The connection of the client to make not ready.</param>
    public static void SetClientNotReady(NetworkConnection conn)
    {
      NetworkServer.instance.InternalSetClientNotReady(conn);
    }

    internal void InternalSetClientNotReady(NetworkConnection conn)
    {
      if (!conn.isReady)
        return;
      if (LogFilter.logDebug)
        Debug.Log((object) ("PlayerNotReady " + (object) conn));
      conn.isReady = false;
      conn.RemoveObservers();
      NotReadyMessage notReadyMessage = new NotReadyMessage();
      conn.Send((short) 36, (MessageBase) notReadyMessage);
    }

    private void OnClientReadyMessage(NetworkMessage netMsg)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) ("Default handler for ready message from " + (object) netMsg.conn));
      NetworkServer.SetClientReady(netMsg.conn);
    }

    private void OnRemovePlayerMessage(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<RemovePlayerMessage>(NetworkServer.s_RemovePlayerMessage);
      PlayerController playerController = (PlayerController) null;
      netMsg.conn.GetPlayerController(NetworkServer.s_RemovePlayerMessage.playerControllerId, out playerController);
      if (playerController != null)
      {
        netMsg.conn.RemovePlayerController(NetworkServer.s_RemovePlayerMessage.playerControllerId);
        NetworkServer.Destroy(playerController.gameObject);
      }
      else
      {
        if (!LogFilter.logError)
          return;
        Debug.LogError((object) ("Received remove player message but could not find the player ID: " + (object) NetworkServer.s_RemovePlayerMessage.playerControllerId));
      }
    }

    private void OnCommandMessage(NetworkMessage netMsg)
    {
      int cmdHash = (int) netMsg.reader.ReadPackedUInt32();
      short playerControllerId = (short) netMsg.reader.ReadPackedUInt32();
      PlayerController playerController;
      if (!netMsg.conn.GetPlayerController(playerControllerId, out playerController))
      {
        if (!LogFilter.logWarn)
          return;
        Debug.LogWarning((object) ("Player instance not found when handling Command message [playerControllerId=" + (object) playerControllerId + "]"));
      }
      else if ((UnityEngine.Object) playerController.unetView == (UnityEngine.Object) null)
      {
        if (!LogFilter.logWarn)
          return;
        Debug.LogWarning((object) ("Player unetview deleted when handling Command message [playerControllerId=" + (object) playerControllerId + "]"));
      }
      else
      {
        if (LogFilter.logDev)
        {
          object[] objArray = new object[4];
          int index1 = 0;
          string str1 = "OnCommandMessage for playerControllerId=";
          objArray[index1] = (object) str1;
          int index2 = 1;
          // ISSUE: variable of a boxed type
          __Boxed<short> local = (ValueType) playerControllerId;
          objArray[index2] = (object) local;
          int index3 = 2;
          string str2 = " conn=";
          objArray[index3] = (object) str2;
          int index4 = 3;
          NetworkConnection networkConnection = netMsg.conn;
          objArray[index4] = (object) networkConnection;
          Debug.Log((object) string.Concat(objArray));
        }
        playerController.unetView.HandleCommand(cmdHash, netMsg.reader);
      }
    }

    internal void SpawnObject(GameObject obj)
    {
      NetworkIdentity view;
      if (!NetworkServer.GetNetworkIdentity(obj, out view))
      {
        if (!LogFilter.logError)
          return;
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "SpawnObject ";
        objArray[index1] = (object) str1;
        int index2 = 1;
        GameObject gameObject1 = obj;
        objArray[index2] = (object) gameObject1;
        int index3 = 2;
        string str2 = " has no NetworkIdentity. Please add a NetworkIdentity to ";
        objArray[index3] = (object) str2;
        int index4 = 3;
        GameObject gameObject2 = obj;
        objArray[index4] = (object) gameObject2;
        Debug.LogError((object) string.Concat(objArray));
      }
      else
      {
        view.OnStartServer();
        if (LogFilter.logDebug)
        {
          object[] objArray = new object[4];
          int index1 = 0;
          string str1 = "SpawnObject instance ID ";
          objArray[index1] = (object) str1;
          int index2 = 1;
          // ISSUE: variable of a boxed type
          __Boxed<NetworkInstanceId> local1 = (ValueType) view.netId;
          objArray[index2] = (object) local1;
          int index3 = 2;
          string str2 = " asset ID ";
          objArray[index3] = (object) str2;
          int index4 = 3;
          // ISSUE: variable of a boxed type
          __Boxed<NetworkHash128> local2 = (ValueType) view.assetId;
          objArray[index4] = (object) local2;
          Debug.Log((object) string.Concat(objArray));
        }
        view.RebuildObservers(true);
      }
    }

    internal void SendSpawnMessage(NetworkIdentity uv, NetworkConnection conn)
    {
      if (uv.serverOnly)
        return;
      if (uv.sceneId.IsEmpty())
      {
        ObjectSpawnMessage objectSpawnMessage = new ObjectSpawnMessage();
        objectSpawnMessage.netId = uv.netId;
        objectSpawnMessage.assetId = uv.assetId;
        objectSpawnMessage.position = uv.transform.position;
        NetworkWriter writer = new NetworkWriter();
        uv.UNetSerializeAllVars(writer);
        if ((int) writer.Position > 0)
          objectSpawnMessage.payload = writer.ToArray();
        if (conn != null)
          conn.Send((short) 3, (MessageBase) objectSpawnMessage);
        else
          NetworkServer.SendToReady(uv.gameObject, (short) 3, (MessageBase) objectSpawnMessage);
        NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Outgoing, (short) 3, uv.assetId.ToString(), 1);
      }
      else
      {
        ObjectSpawnSceneMessage spawnSceneMessage = new ObjectSpawnSceneMessage();
        spawnSceneMessage.netId = uv.netId;
        spawnSceneMessage.sceneId = uv.sceneId;
        spawnSceneMessage.position = uv.transform.position;
        NetworkWriter writer = new NetworkWriter();
        uv.UNetSerializeAllVars(writer);
        if ((int) writer.Position > 0)
          spawnSceneMessage.payload = writer.ToArray();
        if (conn != null)
          conn.Send((short) 10, (MessageBase) spawnSceneMessage);
        else
          NetworkServer.SendToReady(uv.gameObject, (short) 3, (MessageBase) spawnSceneMessage);
        NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Outgoing, (short) 10, "sceneId", 1);
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// This destroys all the player objects associated with a NetworkConnections on a server.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">The connections object to clean up for.</param>
    public static void DestroyPlayersForConnection(NetworkConnection conn)
    {
      if (conn.playerControllers.Count == 0)
      {
        if (!LogFilter.logWarn)
          return;
        Debug.LogWarning((object) "Empty player list given to NetworkServer.Destroy(), nothing to do.");
      }
      else
      {
        using (List<PlayerController>.Enumerator enumerator = conn.playerControllers.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            PlayerController current = enumerator.Current;
            if (current.IsValid)
            {
              NetworkServer.instance.DestroyObject(current.unetView);
              current.gameObject = (GameObject) null;
            }
          }
        }
        conn.playerControllers.Clear();
      }
    }

    private void DestroyObject(GameObject obj)
    {
      if ((UnityEngine.Object) obj == (UnityEngine.Object) null)
      {
        if (!LogFilter.logDev)
          return;
        Debug.Log((object) "NetworkServer DestroyObject is null");
      }
      else
      {
        NetworkIdentity view;
        if (!NetworkServer.GetNetworkIdentity(obj, out view))
          return;
        this.DestroyObject(view);
      }
    }

    private void DestroyObject(NetworkIdentity uv)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) ("DestroyObject instance:" + (object) uv.netId));
      if (NetworkServer.objects.ContainsKey(uv.netId))
        NetworkServer.objects.Remove(uv.netId);
      NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Outgoing, (short) 1, uv.assetId.ToString(), 1);
      ObjectDestroyMessage objectDestroyMessage = new ObjectDestroyMessage();
      objectDestroyMessage.netId = uv.netId;
      NetworkServer.SendToObservers(uv.gameObject, (short) 1, (MessageBase) objectDestroyMessage);
      uv.ClearObservers();
      if (NetworkClient.active && NetworkServer.s_LocalClientActive)
      {
        uv.OnNetworkDestroy();
        ClientScene.SetLocalObject(objectDestroyMessage.netId, (GameObject) null);
      }
      UnityEngine.Object.Destroy((UnityEngine.Object) uv.gameObject);
      uv.SetNoServer();
    }

    /// <summary>
    /// 
    /// <para>
    /// This clears all of the networked objects that the server is aware of. This can be required if a scene change deleted all of the objects without destroying them in the normal manner.
    /// </para>
    /// 
    /// </summary>
    public static void ClearLocalObjects()
    {
      NetworkServer.objects.Clear();
    }

    /// <summary>
    /// 
    /// <para>
    /// Spawn the given game object on all clients which are ready.
    /// </para>
    /// 
    /// </summary>
    /// <param name="obj">Game object with NetworkIdentity to spawn.</param>
    public static void Spawn(GameObject obj)
    {
      NetworkServer.instance.SpawnObject(obj);
    }

    public static void Spawn(GameObject obj, NetworkHash128 assetId)
    {
      NetworkIdentity view;
      if (NetworkServer.GetNetworkIdentity(obj, out view))
        view.SetDynamicAssetId(assetId);
      NetworkServer.instance.SpawnObject(obj);
    }

    /// <summary>
    /// 
    /// <para>
    /// Destroys this object and corresponding objects on all clients.
    /// </para>
    /// 
    /// </summary>
    /// <param name="obj">Game object to destroy.</param>
    public static void Destroy(GameObject obj)
    {
      NetworkServer.instance.DestroyObject(obj);
    }

    internal bool InvokeBytes(ULocalConnectionToServer conn, byte[] buffer, int numBytes, int channelId)
    {
      NetworkReader reader = new NetworkReader(buffer);
      int num = (int) reader.ReadInt16();
      short msgType = reader.ReadInt16();
      if (this.m_MessageHandlers.GetHandler(msgType) != null)
      {
        NetworkConnection networkConnection = this.m_Connections.Get(conn.connectionId);
        if (networkConnection != null)
        {
          ULocalConnectionToClient connectionToClient = (ULocalConnectionToClient) networkConnection;
          this.m_MessageHandlers.InvokeHandler(msgType, (NetworkConnection) connectionToClient, reader, channelId);
          return true;
        }
      }
      return false;
    }

    internal bool InvokeHandlerOnServer(ULocalConnectionToServer conn, short msgType, MessageBase msg, int channelId)
    {
      if (this.m_MessageHandlers.GetHandler(msgType) != null)
      {
        NetworkConnection networkConnection = this.m_Connections.Get(conn.connectionId);
        if (networkConnection != null)
        {
          ULocalConnectionToClient connectionToClient = (ULocalConnectionToClient) networkConnection;
          NetworkWriter writer = new NetworkWriter();
          msg.Serialize(writer);
          NetworkReader reader = new NetworkReader(writer);
          this.m_MessageHandlers.InvokeHandler(msgType, (NetworkConnection) connectionToClient, reader, channelId);
          return true;
        }
        if (LogFilter.logError)
          Debug.LogError((object) ("Local invoke: Failed to find local connection to invoke handler on [connectionId=" + (object) conn.connectionId + "]"));
        return false;
      }
      if (LogFilter.logError)
        Debug.LogError((object) ("Local invoke: Failed to find message handler for message ID " + (object) msgType));
      return false;
    }

    public static GameObject FindLocalObject(NetworkInstanceId netId)
    {
      return NetworkServer.s_NetworkScene.FindLocalObject(netId);
    }

    /// <summary>
    /// 
    /// <para>
    /// Gets aggregate packet stats for all connections.
    /// </para>
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// 
    /// <para>
    /// Dictionary of msg types and packet statistics.
    /// </para>
    /// 
    /// </returns>
    public static Dictionary<short, NetworkConnection.PacketStat> GetConnectionStats()
    {
      ConnectionArray connectionArray = NetworkServer.instance.m_Connections;
      Dictionary<short, NetworkConnection.PacketStat> dictionary = new Dictionary<short, NetworkConnection.PacketStat>();
      for (int localIndex = connectionArray.LocalIndex; localIndex < connectionArray.Count; ++localIndex)
      {
        NetworkConnection networkConnection = connectionArray.Get(localIndex);
        if (networkConnection != null)
        {
          using (Dictionary<short, NetworkConnection.PacketStat>.KeyCollection.Enumerator enumerator = networkConnection.m_PacketStats.Keys.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              short current = enumerator.Current;
              if (dictionary.ContainsKey(current))
              {
                NetworkConnection.PacketStat packetStat = dictionary[current];
                packetStat.count += networkConnection.m_PacketStats[current].count;
                packetStat.bytes += networkConnection.m_PacketStats[current].bytes;
                dictionary[current] = packetStat;
              }
              else
                dictionary[current] = networkConnection.m_PacketStats[current];
            }
          }
        }
      }
      return dictionary;
    }

    /// <summary>
    /// 
    /// <para>
    /// Resets the packet stats on all connections.
    /// </para>
    /// 
    /// </summary>
    public static void ResetConnectionStats()
    {
      ConnectionArray connectionArray = NetworkServer.instance.m_Connections;
      for (int localIndex = connectionArray.LocalIndex; localIndex < connectionArray.Count; ++localIndex)
      {
        NetworkConnection networkConnection = connectionArray.Get(localIndex);
        if (networkConnection != null)
          networkConnection.ResetStats();
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// This causes NetworkIdentity objects in a scene to be spawned on a server.
    /// </para>
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// 
    /// <para>
    /// Success if objects where spawned.
    /// </para>
    /// 
    /// </returns>
    public static bool SpawnObjects()
    {
      if (NetworkServer.active)
      {
        NetworkIdentity[] objectsOfTypeAll = Resources.FindObjectsOfTypeAll<NetworkIdentity>();
        foreach (NetworkIdentity networkIdentity in objectsOfTypeAll)
        {
          if (networkIdentity.gameObject.hideFlags != HideFlags.NotEditable && networkIdentity.gameObject.hideFlags != HideFlags.HideAndDontSave && !networkIdentity.sceneId.IsEmpty())
          {
            if (LogFilter.logDebug)
            {
              object[] objArray = new object[4];
              int index1 = 0;
              string str1 = "SpawnObjects sceneId:";
              objArray[index1] = (object) str1;
              int index2 = 1;
              // ISSUE: variable of a boxed type
              __Boxed<NetworkSceneId> local = (ValueType) networkIdentity.sceneId;
              objArray[index2] = (object) local;
              int index3 = 2;
              string str2 = " name:";
              objArray[index3] = (object) str2;
              int index4 = 3;
              string name = networkIdentity.gameObject.name;
              objArray[index4] = (object) name;
              Debug.Log((object) string.Concat(objArray));
            }
            networkIdentity.gameObject.SetActive(true);
          }
        }
        foreach (NetworkIdentity networkIdentity in objectsOfTypeAll)
        {
          if (networkIdentity.gameObject.hideFlags != HideFlags.NotEditable && networkIdentity.gameObject.hideFlags != HideFlags.HideAndDontSave && (!networkIdentity.sceneId.IsEmpty() && !networkIdentity.isServer && !((UnityEngine.Object) networkIdentity.gameObject == (UnityEngine.Object) null)))
            NetworkServer.Spawn(networkIdentity.gameObject);
        }
      }
      return true;
    }

    private void SendCRC(NetworkConnection targetConnection)
    {
      if (NetworkCRC.singleton == null)
        return;
      CRCMessage crcMessage = new CRCMessage();
      List<CRCMessageEntry> list = new List<CRCMessageEntry>();
      using (Dictionary<string, int>.KeyCollection.Enumerator enumerator = NetworkCRC.singleton.scripts.Keys.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          string current = enumerator.Current;
          list.Add(new CRCMessageEntry()
          {
            name = current,
            channel = (byte) NetworkCRC.singleton.scripts[current]
          });
        }
      }
      crcMessage.scripts = list.ToArray();
      targetConnection.Send((short) 14, (MessageBase) crcMessage);
    }

    /// <summary>
    /// 
    /// <para>
    /// This sends information about all participants in the current network game to the connection.
    /// </para>
    /// 
    /// </summary>
    /// <param name="targetConnection">Connection to send peer info to.</param>
    public void SendNetworkInfo(NetworkConnection targetConnection)
    {
      PeerListMessage peerListMessage = new PeerListMessage();
      List<PeerInfoMessage> list = new List<PeerInfoMessage>();
      for (int connId = 0; connId < this.m_Connections.Count; ++connId)
      {
        NetworkConnection networkConnection = this.m_Connections.Get(connId);
        if (networkConnection != null)
        {
          PeerInfoMessage peerInfoMessage = new PeerInfoMessage();
          string address;
          int port;
          NetworkID network;
          NodeID dstNode;
          byte error;
          NetworkTransport.GetConnectionInfo(this.m_ServerId, networkConnection.connectionId, out address, out port, out network, out dstNode, out error);
          peerInfoMessage.connectionId = networkConnection.connectionId;
          peerInfoMessage.address = address;
          peerInfoMessage.port = port;
          peerInfoMessage.isHost = false;
          peerInfoMessage.isYou = networkConnection == targetConnection;
          list.Add(peerInfoMessage);
        }
      }
      if (NetworkServer.localClientActive)
        list.Add(new PeerInfoMessage()
        {
          address = "HOST",
          port = this.m_ServerPort,
          connectionId = 0,
          isHost = true,
          isYou = false
        });
      peerListMessage.peers = list.ToArray();
      for (int connId = 0; connId < this.m_Connections.Count; ++connId)
      {
        NetworkConnection networkConnection = this.m_Connections.Get(connId);
        if (networkConnection != null)
          networkConnection.Send((short) 11, (MessageBase) peerListMessage);
      }
    }
  }
}
