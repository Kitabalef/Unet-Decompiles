// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkClient
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.NetworkSystem;

namespace UnityEngine.Networking
{
  /// <summary>
  /// 
  /// <para>
  /// High level UNET client.
  /// </para>
  /// 
  /// </summary>
  public class NetworkClient
  {
    private static int MaxEventsPerFrame = 500;
    internal static List<NetworkClient> s_Clients = new List<NetworkClient>();
    internal static bool s_IsActive = false;
    private static PeerListMessage s_PeerListMessage = new PeerListMessage();
    private static CRCMessage s_CRCMessage = new CRCMessage();
    private string m_ServerIp = string.Empty;
    private int m_ClientId = -1;
    private int m_ClientConnectionId = -1;
    private int m_RelaySlotId = -1;
    internal NetworkMessageHandlers m_MessageHandlers = new NetworkMessageHandlers();
    private string m_RequestedServerHost = string.Empty;
    private HostTopology m_hostTopology;
    private bool m_UseSimulator;
    private int m_SimulatedLatency;
    private float m_PacketLoss;
    private int m_ServerPort;
    private int m_StatResetTime;
    private EndPoint m_RemoteEndPoint;
    protected NetworkConnection m_Connection;
    private byte[] m_MsgBuffer;
    private NetworkReader m_MsgReader;
    private PeerInfoMessage[] m_Peers;
    protected NetworkClient.ConnectState m_AsyncConnect;

    /// <summary>
    /// 
    /// <para>
    /// A list of all the active network clients.
    /// </para>
    /// 
    /// </summary>
    public static List<NetworkClient> allClients
    {
      get
      {
        return NetworkClient.s_Clients;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// True if a network client is currently active.
    /// </para>
    /// 
    /// </summary>
    public static bool active
    {
      get
      {
        return NetworkClient.s_IsActive;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The IP address of the server that this client is connected to.
    /// </para>
    /// 
    /// </summary>
    public string serverIp
    {
      get
      {
        return this.m_ServerIp;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The port of the server that this client is connected to.
    /// </para>
    /// 
    /// </summary>
    public int serverPort
    {
      get
      {
        return this.m_ServerPort;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The NetworkConnection object this client is using.
    /// </para>
    /// 
    /// </summary>
    public NetworkConnection connection
    {
      get
      {
        return this.m_Connection;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The other network participants in the current game.
    /// </para>
    /// 
    /// </summary>
    public PeerInfoMessage[] peers
    {
      get
      {
        return this.m_Peers;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The registered network message handlers.
    /// </para>
    /// 
    /// </summary>
    public Dictionary<short, NetworkMessageDelegate> handlers
    {
      get
      {
        return this.m_MessageHandlers.GetHandlers();
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The number of QoS channels currently configured for this client.
    /// </para>
    /// 
    /// </summary>
    public int numChannels
    {
      get
      {
        return this.m_hostTopology.DefaultConfig.ChannelCount;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// This gives the current connection status of the client.
    /// </para>
    /// 
    /// </summary>
    public bool isConnected
    {
      get
      {
        return this.m_AsyncConnect == NetworkClient.ConnectState.Connected;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Creates a new NetworkClient instance.
    /// </para>
    /// 
    /// </summary>
    public NetworkClient()
    {
      if (LogFilter.logDev)
        Debug.Log((object) ("Client created version " + (object) Version.Current));
      this.m_MsgBuffer = new byte[49152];
      this.m_MsgReader = new NetworkReader(this.m_MsgBuffer);
      NetworkClient.AddClient(this);
    }


    public bool Configure(ConnectionConfig config, int maxConnections)
    {
      return this.Configure(new HostTopology(config, maxConnections));
    }

    public bool Configure(HostTopology topology)
    {
      this.m_hostTopology = topology;
      return true;
    }

    public void Connect(MatchInfo matchInfo)
    {
      this.PrepareForConnect();
      this.ConnectWithRelay(matchInfo);
    }

    /// <summary>
    /// 
    /// <para>
    /// Connect client to a NetworkServer instance with simulated latency and packet loss.
    /// </para>
    /// 
    /// </summary>
    /// <param name="serverIp">Target IP address or hostname.</param><param name="serverPort">Target port number.</param><param name="latency">Simulated latency in milliseconds.</param><param name="packetLoss">Simulated packet loss percentage.</param>
    public void ConnectWithSimulator(string serverIp, int serverPort, int latency, float packetLoss)
    {
      this.m_UseSimulator = true;
      this.m_SimulatedLatency = latency;
      this.m_PacketLoss = packetLoss;
      this.Connect(serverIp, serverPort);
    }

    /// <summary>
    /// 
    /// <para>
    /// Connect client to a NetworkServer instance.
    /// </para>
    /// 
    /// </summary>
    /// <param name="serverIp">Target IP address or hostname.</param><param name="serverPort">Target port number.</param>
    public void Connect(string serverIp, int serverPort)
    {
      this.PrepareForConnect();
      if (LogFilter.logDebug)
      {
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "Client Connect: ";
        objArray[index1] = (object) str1;
        int index2 = 1;
        string str2 = serverIp;
        objArray[index2] = (object) str2;
        int index3 = 2;
        string str3 = ":";
        objArray[index3] = (object) str3;
        int index4 = 3;
        // ISSUE: variable of a boxed type
        __Boxed<int> local = (ValueType) serverPort;
        objArray[index4] = (object) local;
        Debug.Log((object) string.Concat(objArray));
      }
      string hostNameOrAddress = serverIp;
      this.m_ServerPort = serverPort;
      if (serverIp.Equals("127.0.0.1") || serverIp.Equals("localhost"))
      {
        this.m_ServerIp = "127.0.0.1";
        this.m_AsyncConnect = NetworkClient.ConnectState.Resolved;
      }
      else
      {
        if (LogFilter.logDebug)
          Debug.Log((object) ("Async DNS START:" + hostNameOrAddress));
        this.m_RequestedServerHost = hostNameOrAddress;
        this.m_AsyncConnect = NetworkClient.ConnectState.Resolving;
        Dns.BeginGetHostAddresses(hostNameOrAddress, new AsyncCallback(NetworkClient.GetHostAddressesCallback), (object) this);
      }
    }




    public void Connect(EndPoint secureTunnelEndPoint)
    {
      this.PrepareForConnect();
      if (LogFilter.logDebug)
        Debug.Log((object) "Client Connect to remoteSockAddr");
      if (secureTunnelEndPoint == null)
      {
        if (LogFilter.logError)
          Debug.LogError((object) "Connect failed: null endpoint passed in");
        this.m_AsyncConnect = NetworkClient.ConnectState.Failed;
      }
      else if (secureTunnelEndPoint.AddressFamily != AddressFamily.InterNetwork && secureTunnelEndPoint.AddressFamily != AddressFamily.InterNetworkV6)
      {
        if (LogFilter.logError)
          Debug.LogError((object) "Connect failed: Endpoint AddressFamily must be either InterNetwork or InterNetworkV6");
        this.m_AsyncConnect = NetworkClient.ConnectState.Failed;
      }
      else
      {
        string fullName = secureTunnelEndPoint.GetType().FullName;
        if (fullName == "System.Net.IPEndPoint")
        {
          IPEndPoint ipEndPoint = (IPEndPoint) secureTunnelEndPoint;
          this.Connect(ipEndPoint.Address.ToString(), ipEndPoint.Port);
        }
        else if (fullName != "UnityEngine.XboxOne.XboxOneEndPoint")
        {
          if (LogFilter.logError)
            Debug.LogError((object) "Connect failed: invalid Endpoint (not IPEndPoint or XboxOneEndPoint)");
          this.m_AsyncConnect = NetworkClient.ConnectState.Failed;
        }
        else
        {
          byte error = (byte) 0;
          this.m_RemoteEndPoint = secureTunnelEndPoint;
          this.m_AsyncConnect = NetworkClient.ConnectState.Connecting;
          try
          {
            this.m_ClientConnectionId = NetworkTransport.ConnectEndPoint(this.m_ClientId, this.m_RemoteEndPoint, 0, out error);
          }
          catch (Exception ex)
          {
            Debug.LogError((object) ("Connect failed: Exception when trying to connect to EndPoint: " + ex.ToString()));
          }
          if (this.m_ClientConnectionId == 0 && LogFilter.logError)
            Debug.LogError((object) ("Connect failed: Unable to connect to EndPoint (" + (object) error + ")"));
          this.m_Connection = new NetworkConnection();
          this.m_Connection.Initialize(this.m_ServerIp, this.m_ClientId, this.m_ClientConnectionId, this.m_hostTopology);
        }
      }
    }



    private void PrepareForConnect()
    {
      NetworkClient.SetActive(true);
      this.RegisterSystemHandlers(false);
      if (this.m_hostTopology == null)
      {
        ConnectionConfig defaultConfig = new ConnectionConfig();
        int num1 = (int) defaultConfig.AddChannel(QosType.Reliable);
        int num2 = (int) defaultConfig.AddChannel(QosType.Unreliable);
        this.m_hostTopology = new HostTopology(defaultConfig, 8);
      }
      if (this.m_UseSimulator)
      {
        int minTimeout = this.m_SimulatedLatency / 3 - 1;
        if (minTimeout < 1)
          minTimeout = 1;
        int maxTimeout = this.m_SimulatedLatency * 3;
        if (LogFilter.logDebug)
        {
          object[] objArray = new object[4];
          int index1 = 0;
          string str1 = "AddHost Using Simulator ";
          objArray[index1] = (object) str1;
          int index2 = 1;
          // ISSUE: variable of a boxed type
          __Boxed<int> local1 = (ValueType) minTimeout;
          objArray[index2] = (object) local1;
          int index3 = 2;
          string str2 = "/";
          objArray[index3] = (object) str2;
          int index4 = 3;
          // ISSUE: variable of a boxed type
          __Boxed<int> local2 = (ValueType) maxTimeout;
          objArray[index4] = (object) local2;
          Debug.Log((object) string.Concat(objArray));
        }
        this.m_ClientId = NetworkTransport.AddHostWithSimulator(this.m_hostTopology, minTimeout, maxTimeout, 0);
      }
      else
        this.m_ClientId = NetworkTransport.AddHost(this.m_hostTopology, 0);
    }




    internal static void GetHostAddressesCallback(IAsyncResult ar)
    {
      try
      {
        IPAddress[] hostAddresses = Dns.EndGetHostAddresses(ar);
        NetworkClient networkClient = (NetworkClient) ar.AsyncState;
        if (hostAddresses.Length == 0)
        {
          if (LogFilter.logError)
            Debug.LogError((object) ("DNS lookup failed for:" + networkClient.m_RequestedServerHost));
          networkClient.m_AsyncConnect = NetworkClient.ConnectState.Failed;
        }
        else
        {
          networkClient.m_ServerIp = hostAddresses[0].ToString();
          networkClient.m_AsyncConnect = NetworkClient.ConnectState.Resolved;
          if (!LogFilter.logDebug)
            return;
          string[] strArray = new string[6];
          int index1 = 0;
          string str1 = "Async DNS Result:";
          strArray[index1] = str1;
          int index2 = 1;
          string str2 = networkClient.m_ServerIp;
          strArray[index2] = str2;
          int index3 = 2;
          string str3 = " for ";
          strArray[index3] = str3;
          int index4 = 3;
          string str4 = networkClient.m_RequestedServerHost;
          strArray[index4] = str4;
          int index5 = 4;
          string str5 = ": ";
          strArray[index5] = str5;
          int index6 = 5;
          string str6 = networkClient.m_ServerIp;
          strArray[index6] = str6;
          Debug.Log((object) string.Concat(strArray));
        }
      }
      catch (SocketException ex)
      {
        NetworkClient networkClient = (NetworkClient) ar.AsyncState;
        if (LogFilter.logError)
          Debug.LogError((object) ("DNS resolution failed: " + (object) ex.ErrorCode));
        if (LogFilter.logDebug)
          Debug.Log((object) ("Exception:" + (object) ex));
        networkClient.m_AsyncConnect = NetworkClient.ConnectState.Failed;
      }
    }



    internal void ContinueConnect()
    {
      byte error;
      if (this.m_UseSimulator)
      {
        if (LogFilter.logDebug)
        {
          object[] objArray = new object[4];
          int index1 = 0;
          string str1 = "Connect Using Simulator ";
          objArray[index1] = (object) str1;
          int index2 = 1;
          // ISSUE: variable of a boxed type
          __Boxed<int> local1 = (ValueType) (this.m_SimulatedLatency / 3);
          objArray[index2] = (object) local1;
          int index3 = 2;
          string str2 = "/";
          objArray[index3] = (object) str2;
          int index4 = 3;
          // ISSUE: variable of a boxed type
          __Boxed<int> local2 = (ValueType) this.m_SimulatedLatency;
          objArray[index4] = (object) local2;
          Debug.Log((object) string.Concat(objArray));
        }
        ConnectionSimulatorConfig conf = new ConnectionSimulatorConfig(this.m_SimulatedLatency / 3, this.m_SimulatedLatency, this.m_SimulatedLatency / 3, this.m_SimulatedLatency, this.m_PacketLoss);
        this.m_ClientConnectionId = NetworkTransport.ConnectWithSimulator(this.m_ClientId, this.m_ServerIp, this.m_ServerPort, 0, out error, conf);
      }
      else
        this.m_ClientConnectionId = NetworkTransport.Connect(this.m_ClientId, this.m_ServerIp, this.m_ServerPort, 0, out error);
      this.m_Connection = new NetworkConnection();
      this.m_Connection.Initialize(this.m_ServerIp, this.m_ClientId, this.m_ClientConnectionId, this.m_hostTopology);
    }




    private void ConnectWithRelay(MatchInfo info)
    {
      this.m_AsyncConnect = NetworkClient.ConnectState.Connecting;
      this.Update();
      byte error;
      this.m_ClientConnectionId = NetworkTransport.ConnectToNetworkPeer(this.m_ClientId, info.address, info.port, 0, 0, info.networkId, Utility.GetSourceID(), info.nodeId, out error);
      this.m_Connection = new NetworkConnection();
      this.m_Connection.Initialize(info.address, this.m_ClientId, this.m_ClientConnectionId, this.m_hostTopology);
      if (LogFilter.logDebug)
        Debug.Log((object) ("Client Relay Slot Id: " + (object) this.m_RelaySlotId));
      if ((int) error == 0)
        return;
      Debug.LogError((object) ("ConnectToNetworkPeer Error: " + (object) error));
    }

    /// <summary>
    /// 
    /// <para>
    /// Disconnect from server.
    /// </para>
    /// 
    /// </summary>
    public virtual void Disconnect()
    {
      this.m_AsyncConnect = NetworkClient.ConnectState.Disconnected;
      ClientScene.HandleClientDisconnect(this.m_Connection);
      if (this.m_Connection == null)
        return;
      this.m_Connection.Disconnect();
      this.m_Connection.Dispose();
      this.m_Connection = (NetworkConnection) null;
    }

    public bool Send(short msgType, MessageBase msg)
    {
      if (this.m_Connection == null)
        return false;
      NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Outgoing, (short) 0, msgType.ToString() + ":" + msg.GetType().Name, 1);
      return this.m_Connection.Send(msgType, msg);
    }

    /// <summary>
    /// 
    /// <para>
    /// This sends the contents of the NetworkWriter's buffer to the connected server.
    /// </para>
    /// 
    /// </summary>
    /// <param name="writer">Writer object containing data to send.</param><param name="channelId">QoS channel to send data on.</param>
    /// <returns>
    /// 
    /// <para>
    /// True if data successfully sent.
    /// </para>
    /// 
    /// </returns>
    public bool SendWriter(NetworkWriter writer, int channelId)
    {
      if (this.m_Connection != null)
        return this.m_Connection.SendWriter(writer, channelId);
      return false;
    }

    /// <summary>
    /// 
    /// <para>
    /// This sends the data in an array of bytes to the server that the client is connected to.
    /// </para>
    /// 
    /// </summary>
    /// <param name="data">Data to send.</param><param name="numBytes">Number of bytes of data.</param><param name="channelId">The QoS channel to send data on.</param>
    /// <returns>
    /// 
    /// <para>
    /// True if successfully sent.
    /// </para>
    /// 
    /// </returns>
    public bool SendBytes(byte[] data, int numBytes, int channelId)
    {
      if (this.m_Connection != null)
        return this.m_Connection.SendBytes(data, numBytes, channelId);
      return false;
    }

    public bool SendUnreliable(short msgType, MessageBase msg)
    {
      if (this.m_Connection == null)
        return false;
      NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Outgoing, (short) 0, msgType.ToString() + ":" + msg.GetType().Name, 1);
      return this.m_Connection.SendUnreliable(msgType, msg);
    }

    public bool SendByChannel(short msgType, MessageBase msg, int channelId)
    {
      NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Outgoing, (short) 0, msgType.ToString() + ":" + msg.GetType().Name, 1);
      if (this.m_Connection != null)
        return this.m_Connection.SendByChannel(msgType, msg, channelId);
      return false;
    }

    /// <summary>
    /// 
    /// <para>
    /// Set the maximum amount of time that can pass for transmitting the send buffer.
    /// </para>
    /// 
    /// </summary>
    /// <param name="seconds">Delay in seconds.</param>
    public void SetMaxDelay(float seconds)
    {
      if (this.m_Connection == null)
      {
        if (!LogFilter.logWarn)
          return;
        Debug.LogWarning((object) "SetMaxDelay failed, not connected.");
      }
      else
        this.m_Connection.SetMaxDelay(seconds);
    }

    /// <summary>
    /// 
    /// <para>
    /// Shut down a client.
    /// </para>
    /// 
    /// </summary>
    public void Shutdown()
    {
      if (LogFilter.logDebug)
        Debug.Log((object) ("Shutting down client " + (object) this.m_ClientId));
      this.m_ClientId = -1;
      NetworkClient.RemoveClient(this);
      if (NetworkClient.s_Clients.Count != 0)
        return;
      NetworkClient.SetActive(false);
    }

    internal virtual void Update()
    {
      if (this.m_ClientId == -1)
        return;
      switch (this.m_AsyncConnect)
      {
        case NetworkClient.ConnectState.None:
          break;
        case NetworkClient.ConnectState.Resolving:
          break;
        case NetworkClient.ConnectState.Resolved:
          this.m_AsyncConnect = NetworkClient.ConnectState.Connecting;
          this.ContinueConnect();
          break;
        case NetworkClient.ConnectState.Disconnected:
          break;
        case NetworkClient.ConnectState.Failed:
          this.GenerateConnectError(11);
          this.m_AsyncConnect = NetworkClient.ConnectState.Disconnected;
          break;
        default:
          if (this.m_Connection != null && (int) Time.time != this.m_StatResetTime)
          {
            this.m_Connection.ResetStats();
            this.m_StatResetTime = (int) Time.time;
          }
          int num = 0;
          NetworkEventType networkEventType;
          do
          {
            int connectionId;
            int channelId;
            int receivedSize;
            byte error;
            networkEventType = NetworkTransport.ReceiveFromHost(this.m_ClientId, out connectionId, out channelId, this.m_MsgBuffer, (int) (ushort) this.m_MsgBuffer.Length, out receivedSize, out error);
            if (networkEventType != NetworkEventType.Nothing && LogFilter.logDev)
            {
              object[] objArray = new object[6];
              int index1 = 0;
              string str1 = "Client event: host=";
              objArray[index1] = (object) str1;
              int index2 = 1;
              // ISSUE: variable of a boxed type
              __Boxed<int> local1 = (ValueType) this.m_ClientId;
              objArray[index2] = (object) local1;
              int index3 = 2;
              string str2 = " event=";
              objArray[index3] = (object) str2;
              int index4 = 3;
              // ISSUE: variable of a boxed type
              __Boxed<NetworkEventType> local2 = (System.Enum) networkEventType;
              objArray[index4] = (object) local2;
              int index5 = 4;
              string str3 = " error=";
              objArray[index5] = (object) str3;
              int index6 = 5;
              // ISSUE: variable of a boxed type
              __Boxed<byte> local3 = (ValueType) error;
              objArray[index6] = (object) local3;
              Debug.Log((object) string.Concat(objArray));
            }
            switch (networkEventType)
            {
              case NetworkEventType.DataEvent:
                if ((int) error != 0)
                {
                  this.GenerateDataError((int) error);
                  return;
                }
                NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Incoming, (short) 29, "msg", 1);
                this.m_MsgReader.SeekZero();
                this.m_Connection.HandleMessage(this.m_MessageHandlers.GetHandlers(), this.m_MsgReader, receivedSize, channelId);
                goto case 3;

              case NetworkEventType.ConnectEvent:
                if (LogFilter.logDebug)
                  Debug.Log((object) "Client connected");
                if ((int) error != 0)
                {
                  this.GenerateConnectError((int) error);
                  return;
                }
                this.m_AsyncConnect = NetworkClient.ConnectState.Connected;
                this.m_MessageHandlers.InvokeHandlerNoData((short) 32, this.m_Connection);
                goto case 3;

              case NetworkEventType.DisconnectEvent:
                if (LogFilter.logDebug)
                  Debug.Log((object) "Client disconnected");
                this.m_AsyncConnect = NetworkClient.ConnectState.Disconnected;
                if ((int) error != 0)
                  this.GenerateDisconnectError((int) error);
                ClientScene.HandleClientDisconnect(this.m_Connection);
                this.m_MessageHandlers.InvokeHandlerNoData((short) 33, this.m_Connection);
                goto case 3;

              case NetworkEventType.Nothing:
                if (++num >= NetworkClient.MaxEventsPerFrame)
                {
                  if (LogFilter.logDebug)
                  {
                    Debug.Log((object) ("MaxEventsPerFrame hit (" + (object) NetworkClient.MaxEventsPerFrame + ")"));
                    goto label_34;
                  }
                  else
                    goto label_34;
                }
                else
                  continue;
              default:
                if (LogFilter.logError)
                {
                  Debug.LogError((object) ("Unknown network message type received: " + (object) networkEventType));
                  goto case 3;
                }
                else
                  goto case 3;
            }
          }
          while (this.m_ClientId != -1 && networkEventType != NetworkEventType.Nothing);
label_34:
          if (this.m_Connection == null || this.m_AsyncConnect != NetworkClient.ConnectState.Connected)
            break;
          this.m_Connection.FlushInternalBuffer();
          break;
      }
    }

    private void GenerateConnectError(int error)
    {
      if (LogFilter.logError)
        Debug.LogError((object) ("UNet Client Error Connect Error: " + (object) error));
      this.GenerateError(error);
    }

    private void GenerateDataError(int error)
    {
      NetworkError networkError = (NetworkError) error;
      if (LogFilter.logError)
        Debug.LogError((object) ("UNet Client Data Error: " + (object) networkError));
      this.GenerateError(error);
    }

    private void GenerateDisconnectError(int error)
    {
      NetworkError networkError = (NetworkError) error;
      if (LogFilter.logError)
        Debug.LogError((object) ("UNet Client Disconnect Error: " + (object) networkError));
      this.GenerateError(error);
    }

    private void GenerateError(int error)
    {
      NetworkMessageDelegate networkMessageDelegate = this.m_MessageHandlers.GetHandler((short) 34) ?? this.m_MessageHandlers.GetHandler((short) 34);
      if (networkMessageDelegate == null)
        return;
      ErrorMessage errorMessage = new ErrorMessage();
      errorMessage.errorCode = error;
      byte[] buffer = new byte[200];
      NetworkWriter writer = new NetworkWriter(buffer);
      errorMessage.Serialize(writer);
      NetworkReader networkReader = new NetworkReader(buffer);
      networkMessageDelegate(new NetworkMessage()
      {
        msgType = (short) 34,
        reader = networkReader,
        conn = this.m_Connection,
        channelId = 0
      });
    }

    public void GetStatsOut(out int numMsgs, out int numBufferedMsgs, out int numBytes, out int lastBufferedPerSecond)
    {
      numMsgs = 0;
      numBufferedMsgs = 0;
      numBytes = 0;
      lastBufferedPerSecond = 0;
      if (this.m_Connection == null)
        return;
      this.m_Connection.GetStatsOut(out numMsgs, out numBufferedMsgs, out numBytes, out lastBufferedPerSecond);
    }

    public void GetStatsIn(out int numMsgs, out int numBytes)
    {
      numMsgs = 0;
      numBytes = 0;
      if (this.m_Connection == null)
        return;
      this.m_Connection.GetStatsIn(out numMsgs, out numBytes);
    }

    /// <summary>
    /// 
    /// <para>
    /// Retrieves statistics about the network packets sent on this connection.
    /// </para>
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// 
    /// <para>
    /// Dictionary of packet statistics for the client's connection.
    /// </para>
    /// 
    /// </returns>
    public Dictionary<short, NetworkConnection.PacketStat> GetConnectionStats()
    {
      if (this.m_Connection == null)
        return (Dictionary<short, NetworkConnection.PacketStat>) null;
      return this.m_Connection.m_PacketStats;
    }

    /// <summary>
    /// 
    /// <para>
    /// Resets the statistics return by NetworkClient.GetConnectionStats() to zero values.
    /// </para>
    /// 
    /// </summary>
    public void ResetConnectionStats()
    {
      if (this.m_Connection == null)
        return;
      this.m_Connection.ResetStats();
    }

    /// <summary>
    /// 
    /// <para>
    /// Gets the Return Trip Time for this connection.
    /// </para>
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// 
    /// <para>
    /// Return trip time in milliseconds.
    /// </para>
    /// 
    /// </returns>
    public int GetRTT()
    {
      if (this.m_ClientId == -1)
        return 0;
      byte error;
      return NetworkTransport.GetCurrentRtt(this.m_ClientId, this.m_ClientConnectionId, out error);
    }

    internal void RegisterSystemHandlers(bool localClient)
    {
      this.RegisterHandlerSafe((short) 11, new NetworkMessageDelegate(this.OnPeerInfo));
      ClientScene.RegisterSystemHandlers(this, localClient);
      this.RegisterHandlerSafe((short) 14, new NetworkMessageDelegate(this.OnCRC));
    }

    private void OnPeerInfo(NetworkMessage netMsg)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "OnPeerInfo");
      netMsg.ReadMessage<PeerListMessage>(NetworkClient.s_PeerListMessage);
      this.m_Peers = NetworkClient.s_PeerListMessage.peers;
    }

    private void OnCRC(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<CRCMessage>(NetworkClient.s_CRCMessage);
      NetworkCRC.singleton.Validate(NetworkClient.s_CRCMessage.scripts, this.numChannels);
    }

    /// <summary>
    /// 
    /// <para>
    /// Register a handler for a particular message type.
    /// </para>
    /// 
    /// </summary>
    /// <param name="msgType">Message type number.</param><param name="handler">Function handler which will be invoked for when this message type is received.</param>
    public void RegisterHandler(short msgType, NetworkMessageDelegate handler)
    {
      this.m_MessageHandlers.RegisterHandler(msgType, handler);
    }

    public void RegisterHandlerSafe(short msgType, NetworkMessageDelegate handler)
    {
      this.m_MessageHandlers.RegisterHandlerSafe(msgType, handler);
    }

    /// <summary>
    /// 
    /// <para>
    /// Unregisters a network message handler.
    /// </para>
    /// 
    /// </summary>
    /// <param name="msgType">The message type to unregister.</param>
    public void UnregisterHandler(short msgType)
    {
      this.m_MessageHandlers.UnregisterHandler(msgType);
    }

    /// <summary>
    /// 
    /// <para>
    /// Retrieves statistics about the network packets sent on all connections.
    /// </para>
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// 
    /// <para>
    /// Dictionary of stats.
    /// </para>
    /// 
    /// </returns>
    public static Dictionary<short, NetworkConnection.PacketStat> GetTotalConnectionStats()
    {
      Dictionary<short, NetworkConnection.PacketStat> dictionary = new Dictionary<short, NetworkConnection.PacketStat>();
      using (List<NetworkClient>.Enumerator enumerator1 = NetworkClient.s_Clients.GetEnumerator())
      {
        while (enumerator1.MoveNext())
        {
          Dictionary<short, NetworkConnection.PacketStat> connectionStats = enumerator1.Current.GetConnectionStats();
          using (Dictionary<short, NetworkConnection.PacketStat>.KeyCollection.Enumerator enumerator2 = connectionStats.Keys.GetEnumerator())
          {
            while (enumerator2.MoveNext())
            {
              short current = enumerator2.Current;
              if (dictionary.ContainsKey(current))
              {
                NetworkConnection.PacketStat packetStat = dictionary[current];
                packetStat.count += connectionStats[current].count;
                packetStat.bytes += connectionStats[current].bytes;
                dictionary[current] = packetStat;
              }
              else
                dictionary[current] = connectionStats[current];
            }
          }
        }
      }
      return dictionary;
    }

    internal static void AddClient(NetworkClient client)
    {
      NetworkClient.s_Clients.Add(client);
    }

    internal static void RemoveClient(NetworkClient client)
    {
      NetworkClient.s_Clients.Remove(client);
    }

    internal static void UpdateClients()
    {
      for (int index = 0; index < NetworkClient.s_Clients.Count; ++index)
      {
        if (NetworkClient.s_Clients[index] != null)
          NetworkClient.s_Clients[index].Update();
        else
          NetworkClient.s_Clients.RemoveAt(index);
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Shuts down all network clients.
    /// </para>
    /// 
    /// </summary>
    public static void ShutdownAll()
    {
      while (NetworkClient.s_Clients.Count != 0)
        NetworkClient.s_Clients[0].Shutdown();
      NetworkClient.s_Clients = new List<NetworkClient>();
      NetworkClient.s_IsActive = false;
      ClientScene.Shutdown();
      NetworkDetailStats.ResetAll();
    }

    internal static void SetActive(bool state)
    {
      if (!NetworkClient.s_IsActive && state)
        NetworkTransport.Init();
      NetworkClient.s_IsActive = state;
    }

    protected enum ConnectState
    {
      None,
      Resolving,
      Resolved,
      Connecting,
      Connected,
      Disconnected,
      Failed,
    }
  }
}
