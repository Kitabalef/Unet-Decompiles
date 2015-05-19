﻿// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.GlobalConfig
// Assembly: UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B9965B63-74A2-480C-BCFC-887FBCF7E9A7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\Managed\UnityEngine.dll

using System;
using UnityEngine;

namespace UnityEngine.Networking
{
  /// <summary>
  /// 
  /// <para>
  /// Defines global paramters for network library.
  /// </para>
  /// 
  /// </summary>
  [Serializable]
  public class GlobalConfig
  {
    [SerializeField]
    private uint m_ThreadAwakeTimeout;
    [SerializeField]
    private ReactorModel m_ReactorModel;
    [SerializeField]
    private ushort m_ReactorMaximumReceivedMessages;
    [SerializeField]
    private ushort m_ReactorMaximumSentMessages;
    [SerializeField]
    private ushort m_MaxPacketSize;

    /// <summary>
    /// 
    /// <para>
    /// Defines (1) for select reactor, minimum time period, when system will check if there are any messages for send (2) for fixrate reactor, minimum interval of time, when system will check for sending and receiving messages.
    /// </para>
    /// 
    /// </summary>
    public uint ThreadAwakeTimeout
    {
      get
      {
        return this.m_ThreadAwakeTimeout;
      }
      set
      {
        this.m_ThreadAwakeTimeout = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Defines reactor model for the network library.
    /// </para>
    /// 
    /// </summary>
    public ReactorModel ReactorModel
    {
      get
      {
        return this.m_ReactorModel;
      }
      set
      {
        this.m_ReactorModel = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Defines maximum amount of messages in the receive queue.
    /// </para>
    /// 
    /// </summary>
    public ushort ReactorMaximumReceivedMessages
    {
      get
      {
        return this.m_ReactorMaximumReceivedMessages;
      }
      set
      {
        this.m_ReactorMaximumReceivedMessages = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Defines maximum message count in sent queue.
    /// </para>
    /// 
    /// </summary>
    public ushort ReactorMaximumSentMessages
    {
      get
      {
        return this.m_ReactorMaximumSentMessages;
      }
      set
      {
        this.m_ReactorMaximumSentMessages = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Defines maximum possible packet size in bytes for all network connections.
    /// </para>
    /// 
    /// </summary>
    public ushort MaxPacketSize
    {
      get
      {
        return this.m_MaxPacketSize;
      }
      set
      {
        this.m_MaxPacketSize = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Create new global config object.
    /// </para>
    /// 
    /// </summary>
    public GlobalConfig()
    {
      this.m_ThreadAwakeTimeout = 1U;
      this.m_ReactorModel = ReactorModel.SelectReactor;
      this.m_ReactorMaximumReceivedMessages = (ushort) 1024;
      this.m_ReactorMaximumSentMessages = (ushort) 1024;
      this.m_MaxPacketSize = (ushort) 2000;
    }
  }
}
