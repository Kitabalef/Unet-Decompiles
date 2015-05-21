// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.ChannelQOS
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
  /// Defines parameters of channels.
  /// </para>
  /// 
  /// </summary>
  [Serializable]
  public class ChannelQOS
  {
    [SerializeField]
    internal QosType m_Type;

    /// <summary>
    /// 
    /// <para>
    /// Channel quality of service.
    /// </para>
    /// 
    /// </summary>
    public QosType QOS
    {
      get
      {
        return this.m_Type;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// UnderlyingModel.MemDoc.MemDocModel.
    /// </para>
    /// 
    /// </summary>
    /// <param name="value">Requested type of quality of service (default Unreliable).</param><param name="channel">Copy constructor.</param>
    public ChannelQOS(QosType value)
    {
      this.m_Type = value;
    }

    /// <summary>
    /// 
    /// <para>
    /// UnderlyingModel.MemDoc.MemDocModel.
    /// </para>
    /// 
    /// </summary>
    /// <param name="value">Requested type of quality of service (default Unreliable).</param><param name="channel">Copy constructor.</param>
    public ChannelQOS()
    {
      this.m_Type = QosType.Unreliable;
    }

    /// <summary>
    /// 
    /// <para>
    /// UnderlyingModel.MemDoc.MemDocModel.
    /// </para>
    /// 
    /// </summary>
    /// <param name="value">Requested type of quality of service (default Unreliable).</param><param name="channel">Copy constructor.</param>
    public ChannelQOS(ChannelQOS channel)
    {
      if (channel == null)
        throw new NullReferenceException("channel is not defined");
      this.m_Type = channel.m_Type;
    }
  }
}
