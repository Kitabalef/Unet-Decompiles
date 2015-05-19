// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkConnection
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityEngine.Networking
{
  /// <summary>
  /// 
  /// <para>
  /// High level UNET connection.
  /// </para>
  /// 
  /// </summary>
  public class NetworkConnection : IDisposable
  {
    private static int s_MaxPacketStats = (int) byte.MaxValue;
    private List<PlayerController> m_PlayerControllers = new List<PlayerController>();
    private NetworkMessage m_netMsg = new NetworkMessage();
    private HashSet<NetworkIdentity> m_VisList = new HashSet<NetworkIdentity>();
    internal NetworkWriter m_writer = new NetworkWriter();
    /// <summary>
    /// 
    /// <para>
    /// Transport level host id for this connection.
    /// </para>
    /// 
    /// </summary>
    public int hostId = -1;
    /// <summary>
    /// 
    /// <para>
    /// Unique identifier for this connection.
    /// </para>
    /// 
    /// </summary>
    public int connectionId = -1;
    internal Dictionary<short, NetworkConnection.PacketStat> m_PacketStats = new Dictionary<short, NetworkConnection.PacketStat>();
    private ChannelBuffer[] m_Channels;
    /// <summary>
    /// 
    /// <para>
    /// Flag that tells if the connection has been marked as "ready" by a client calling NetworkClient.Ready().
    /// </para>
    /// 
    /// </summary>
    public bool isReady;
    /// <summary>
    /// 
    /// <para>
    /// The IP address associated with the connection.
    /// </para>
    /// 
    /// </summary>
    public string address;
    private bool disposed;

    internal HashSet<NetworkIdentity> visList
    {
      get
      {
        return this.m_VisList;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The list of players for this connection.
    /// </para>
    /// 
    /// </summary>
    public List<PlayerController> playerControllers
    {
      get
      {
        return this.m_PlayerControllers;
      }
    }

    public NetworkConnection()
    {
      this.m_writer = new NetworkWriter();
    }

    ~NetworkConnection()
    {
      this.Dispose(false);
    }

    /// <summary>
    /// 
    /// <para>
    /// This inializes the internal data structures of a NetworkConnection object, including channel buffers.
    /// </para>
    /// 
    /// </summary>
    /// <param name="address">The host or IP connected to.</param><param name="hostId">The transport hostId for the connection.</param><param name="connectionId">The transport connectionId for the connection.</param><param name="hostTopology">The topology to be used.</param>
    public void Initialize(string address, int hostId, int connectionId, HostTopology hostTopology)
    {
      this.m_writer = new NetworkWriter();
      this.address = address;
      this.hostId = hostId;
      this.connectionId = connectionId;
      int channelCount = hostTopology.DefaultConfig.ChannelCount;
      int bufferSize = (int) hostTopology.DefaultConfig.PacketSize;
      this.m_Channels = new ChannelBuffer[channelCount];
      for (int index = 0; index < channelCount; ++index)
      {
        ChannelQOS channelQos = hostTopology.DefaultConfig.Channels[index];
        this.m_Channels[index] = new ChannelBuffer(hostId, connectionId, bufferSize, (byte) index, this.IsReliableQoS(channelQos.m_Type));
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Disposes of this connection, releasing channel buffers that it holds.
    /// </para>
    /// 
    /// </summary>
    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!this.disposed && this.m_Channels != null)
      {
        for (int index = 0; index < this.m_Channels.Length; ++index)
          this.m_Channels[index].Dispose();
      }
      this.m_Channels = (ChannelBuffer[]) null;
      this.disposed = true;
    }

    private bool IsReliableQoS(QosType qos)
    {
      if (qos != QosType.Reliable && qos != QosType.ReliableFragmented && qos != QosType.ReliableSequenced)
        return qos == QosType.ReliableStateUpdate;
      return true;
    }

    /// <summary>
    /// 
    /// <para>
    /// This sets an option on the network channel.
    /// </para>
    /// 
    /// </summary>
    /// <param name="channelId">The channel the option will be set on.</param><param name="option">The option to set.</param><param name="value">The value for the option.</param>
    /// <returns>
    /// 
    /// <para>
    /// True if the option was set.
    /// </para>
    /// 
    /// </returns>
    public bool SetChannelOption(int channelId, ChannelOption option, int value)
    {
      if (this.m_Channels == null || channelId < 0 || channelId >= this.m_Channels.Length)
        return false;
      return this.m_Channels[channelId].SetOption(option, value);
    }

    /// <summary>
    /// 
    /// <para>
    /// Disconnects this connection.
    /// </para>
    /// 
    /// </summary>
    public void Disconnect()
    {
      this.address = string.Empty;
      this.isReady = false;
      ClientScene.HandleClientDisconnect(this);
      if (this.hostId == -1)
        return;
      byte error;
      NetworkTransport.Disconnect(this.hostId, this.connectionId, out error);
      this.RemoveObservers();
    }

    internal void SetPlayerController(PlayerController player)
    {
      while ((int) player.playerControllerId >= this.m_PlayerControllers.Count)
        this.m_PlayerControllers.Add(new PlayerController());
      this.m_PlayerControllers[(int) player.playerControllerId] = player;
    }

    internal void RemovePlayerController(short playerControllerId)
    {
      for (int count = this.m_PlayerControllers.Count; count >= 0; --count)
      {
        if ((int) playerControllerId == count && (int) playerControllerId == (int) this.m_PlayerControllers[count].playerControllerId)
        {
          this.m_PlayerControllers[count] = new PlayerController();
          return;
        }
      }
      if (!LogFilter.logError)
        return;
      Debug.LogError((object) ("RemovePlayer player at playerControllerId " + (object) playerControllerId + " not found"));
    }

    internal bool GetPlayerController(short playerControllerId, out PlayerController playerController)
    {
      playerController = (PlayerController) null;
      if (this.playerControllers.Count <= 0)
        return false;
      for (int index = 0; index < this.playerControllers.Count; ++index)
      {
        if (this.playerControllers[index].IsValid && (int) this.playerControllers[index].playerControllerId == (int) playerControllerId)
        {
          playerController = this.playerControllers[index];
          return true;
        }
      }
      return false;
    }

    internal void FlushInternalBuffer()
    {
      if (this.m_Channels == null)
        return;
      foreach (ChannelBuffer channelBuffer in this.m_Channels)
        channelBuffer.CheckInternalBuffer();
    }

    /// <summary>
    /// 
    /// <para>
    /// The maximum time in seconds that messages are buffered before being sent.
    /// </para>
    /// 
    /// </summary>
    /// <param name="seconds">Time in seconds.</param>
    public void SetMaxDelay(float seconds)
    {
      if (this.m_Channels == null)
        return;
      foreach (ChannelBuffer channelBuffer in this.m_Channels)
        channelBuffer.maxDelay = seconds;
    }

    public virtual bool Send(short msgType, MessageBase msg)
    {
      this.m_writer.StartMessage(msgType);
      msg.Serialize(this.m_writer);
      this.m_writer.FinishMessage();
      return this.SendWriter(this.m_writer, 0);
    }

    public virtual bool SendUnreliable(short msgType, MessageBase msg)
    {
      return this.SendByChannel(msgType, msg, 1);
    }

    public virtual bool SendByChannel(short msgType, MessageBase msg, int channelId)
    {
      if (this.CheckChannel(channelId))
        return this.m_Channels[channelId].Send(msgType, msg);
      return false;
    }

    /// <summary>
    /// 
    /// <para>
    /// This sends an array of bytes on the connection.
    /// </para>
    /// 
    /// </summary>
    /// <param name="bytes">The array of data to be sent.</param><param name="numBytes">The number of bytes in the array to be sent.</param><param name="channelId">The transport channel to send on.</param>
    /// <returns>
    /// 
    /// <para>
    /// Success if data was sent.
    /// </para>
    /// 
    /// </returns>
    public virtual bool SendBytes(byte[] bytes, int numBytes, int channelId)
    {
      if (this.CheckChannel(channelId))
        return this.m_Channels[channelId].SendBytes(bytes, numBytes);
      return false;
    }

    /// <summary>
    /// 
    /// <para>
    /// This sends the contents of a NetworkWriter object on the connection.
    /// </para>
    /// 
    /// </summary>
    /// <param name="writer">A writer object containing data to send.</param><param name="channelId">The transport channel to send on.</param>
    /// <returns>
    /// 
    /// <para>
    /// True if the data was sent.
    /// </para>
    /// 
    /// </returns>
    public virtual bool SendWriter(NetworkWriter writer, int channelId)
    {
      if (this.CheckChannel(channelId))
        return this.m_Channels[channelId].SendWriter(writer);
      return false;
    }

    private bool CheckChannel(int channelId)
    {
      if (this.m_Channels == null)
      {
        if (LogFilter.logWarn)
          Debug.LogWarning((object) ("Channels not initialized sending on id '" + (object) channelId));
        return false;
      }
      if (channelId >= 0 && channelId < this.m_Channels.Length)
        return true;
      if (LogFilter.logError)
      {
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "Invalid channel when sending buffered data, '";
        objArray[index1] = (object) str1;
        int index2 = 1;
        // ISSUE: variable of a boxed type
        __Boxed<int> local1 = (ValueType) channelId;
        objArray[index2] = (object) local1;
        int index3 = 2;
        string str2 = "'. Current channel count is ";
        objArray[index3] = (object) str2;
        int index4 = 3;
        // ISSUE: variable of a boxed type
        __Boxed<int> local2 = (ValueType) this.m_Channels.Length;
        objArray[index4] = (object) local2;
        Debug.LogError((object) string.Concat(objArray));
      }
      return false;
    }

    /// <summary>
    /// 
    /// <para>
    /// Resets the statistics that are returned from NetworkClient.GetConnectionStats().
    /// </para>
    /// 
    /// </summary>
    public void ResetStats()
    {
      for (short key = (short) 0; (int) key < NetworkConnection.s_MaxPacketStats; ++key)
      {
        if (this.m_PacketStats.ContainsKey(key))
        {
          NetworkConnection.PacketStat packetStat = this.m_PacketStats[key];
          packetStat.count = 0;
          packetStat.bytes = 0;
          NetworkTransport.SetPacketStat(0, (int) key, 0, 0);
          NetworkTransport.SetPacketStat(1, (int) key, 0, 0);
        }
      }
    }

    internal void HandleMessage(Dictionary<short, NetworkMessageDelegate> handler, byte[] buffer, int channelId, int receivedSize)
    {
      NetworkReader reader = new NetworkReader(buffer);
      this.HandleMessage(handler, reader, receivedSize, channelId);
    }

    internal void HandleMessage(Dictionary<short, NetworkMessageDelegate> handler, NetworkReader reader, int receivedSize, int channelId)
    {
      while ((long) reader.Position < (long) receivedSize)
      {
        ushort num = reader.ReadUInt16();
        short key = reader.ReadInt16();
        NetworkReader networkReader = new NetworkReader(reader.ReadBytes((int) num));
        NetworkMessageDelegate networkMessageDelegate = (NetworkMessageDelegate) null;
        if (handler.ContainsKey(key))
          networkMessageDelegate = handler[key];
        if (networkMessageDelegate != null)
        {
          this.m_netMsg.msgType = key;
          this.m_netMsg.reader = networkReader;
          this.m_netMsg.conn = this;
          this.m_netMsg.channelId = channelId;
          networkMessageDelegate(this.m_netMsg);
          NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Incoming, (short) 28, "msg", 1);
          if ((int) key > 46)
            NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Incoming, (short) 0, key.ToString() + ":" + key.GetType().Name, 1);
          if (this.m_PacketStats.ContainsKey(key))
          {
            NetworkConnection.PacketStat packetStat = this.m_PacketStats[key];
            ++packetStat.count;
            packetStat.bytes += (int) num;
          }
          else
          {
            NetworkConnection.PacketStat packetStat = new NetworkConnection.PacketStat();
            packetStat.msgType = key;
            ++packetStat.count;
            packetStat.bytes += (int) num;
            this.m_PacketStats[key] = packetStat;
          }
        }
        else
        {
          if (!LogFilter.logError)
            break;
          Debug.LogError((object) ("Unknown message ID " + (object) key));
          break;
        }
      }
    }

    public virtual void GetStatsOut(out int numMsgs, out int numBufferedMsgs, out int numBytes, out int lastBufferedPerSecond)
    {
      numMsgs = 0;
      numBufferedMsgs = 0;
      numBytes = 0;
      lastBufferedPerSecond = 0;
      foreach (ChannelBuffer channelBuffer in this.m_Channels)
      {
        numMsgs = numMsgs + channelBuffer.numMsgsOut;
        numBufferedMsgs = numBufferedMsgs + channelBuffer.numBufferedMsgsOut;
        numBytes = numBytes + channelBuffer.numBytesOut;
        lastBufferedPerSecond = lastBufferedPerSecond + channelBuffer.lastBufferedPerSecond;
      }
    }

    public virtual void GetStatsIn(out int numMsgs, out int numBytes)
    {
      numMsgs = 0;
      numBytes = 0;
      foreach (ChannelBuffer channelBuffer in this.m_Channels)
      {
        numMsgs = numMsgs + channelBuffer.numMsgsIn;
        numBytes = numBytes + channelBuffer.numBytesIn;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Returns a string representation of the NetworkConnection object state.
    /// </para>
    /// 
    /// </summary>
    public override string ToString()
    {
      string format = "hostId: {0} connectionId: {1} isReady: {2} channel count: {3}";
      object[] objArray = new object[4];
      int index1 = 0;
      // ISSUE: variable of a boxed type
      __Boxed<int> local1 = (ValueType) this.hostId;
      objArray[index1] = (object) local1;
      int index2 = 1;
      // ISSUE: variable of a boxed type
      __Boxed<int> local2 = (ValueType) this.connectionId;
      objArray[index2] = (object) local2;
      int index3 = 2;
      // ISSUE: variable of a boxed type
      __Boxed<bool> local3 = (ValueType) (bool) (this.isReady ? 1 : 0);
      objArray[index3] = (object) local3;
      int index4 = 3;
      // ISSUE: variable of a boxed type
      __Boxed<int> local4 = (ValueType) (this.m_Channels == null ? 0 : this.m_Channels.Length);
      objArray[index4] = (object) local4;
      return string.Format(format, objArray);
    }

    internal void AddToVisList(NetworkIdentity uv)
    {
      this.m_VisList.Add(uv);
      NetworkServer.ShowForConnection(uv, this);
    }

    internal void RemoveFromVisList(NetworkIdentity uv, bool isDestroyed)
    {
      this.m_VisList.Remove(uv);
      if (isDestroyed)
        return;
      NetworkServer.HideForConnection(uv, this);
    }

    internal void RemoveObservers()
    {
      using (HashSet<NetworkIdentity>.Enumerator enumerator = this.m_VisList.GetEnumerator())
      {
        while (enumerator.MoveNext())
          enumerator.Current.RemoveObserverInternal(this);
      }
      this.m_VisList.Clear();
    }

    /// <summary>
    /// 
    /// <para>
    /// Structure used to track the number and size of packets of each packets type.
    /// </para>
    /// 
    /// </summary>
    public class PacketStat
    {
      /// <summary>
      /// 
      /// <para>
      /// The message type these stats are for.
      /// </para>
      /// 
      /// </summary>
      public short msgType;
      /// <summary>
      /// 
      /// <para>
      /// The total number of messages of this type.
      /// </para>
      /// 
      /// </summary>
      public int count;
      /// <summary>
      /// 
      /// <para>
      /// Total bytes of all messages of this type.
      /// </para>
      /// 
      /// </summary>
      public int bytes;

      public override string ToString()
      {
        object[] objArray = new object[5];
        int index1 = 0;
        string str1 = MsgType.MsgTypeToString(this.msgType);
        objArray[index1] = (object) str1;
        int index2 = 1;
        string str2 = ": count=";
        objArray[index2] = (object) str2;
        int index3 = 2;
        // ISSUE: variable of a boxed type
        __Boxed<int> local1 = (ValueType) this.count;
        objArray[index3] = (object) local1;
        int index4 = 3;
        string str3 = " bytes=";
        objArray[index4] = (object) str3;
        int index5 = 4;
        // ISSUE: variable of a boxed type
        __Boxed<int> local2 = (ValueType) this.bytes;
        objArray[index5] = (object) local2;
        return string.Concat(objArray);
      }
    }
  }
}
