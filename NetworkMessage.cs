// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkMessage
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using System;

namespace UnityEngine.Networking
{
  /// <summary>
  /// 
  /// <para>
  /// The details of a network message received by a client or server on a network connection.
  /// </para>
  /// 
  /// </summary>
  public class NetworkMessage
  {
    /// <summary>
    /// 
    /// <para>
    /// The id of the message type of the message.
    /// </para>
    /// 
    /// </summary>
    public short msgType;
    /// <summary>
    /// 
    /// <para>
    /// The connection the message was recieved on.
    /// </para>
    /// 
    /// </summary>
    public NetworkConnection conn;
    /// <summary>
    /// 
    /// <para>
    /// A NetworkReader object that contains the contents of the message.
    /// </para>
    /// 
    /// </summary>
    public NetworkReader reader;
    /// <summary>
    /// 
    /// <para>
    /// The transport layer channel the message was sent on.
    /// </para>
    /// 
    /// </summary>
    public int channelId;

    /// <summary>
    /// 
    /// <para>
    /// Returns a string with the numeric representation of each byte in the payload.
    /// </para>
    /// 
    /// </summary>
    /// <param name="payload">Network message payload to dump.</param><param name="sz">Length of payload in bytes.</param>
    /// <returns>
    /// 
    /// <para>
    /// Dumped info from payload.
    /// </para>
    /// 
    /// </returns>
    public static string Dump(byte[] payload, int sz)
    {
      string str = "[";
      for (int index = 0; index < sz; ++index)
        str = str + payload[index].ToString() + " ";
      return str + "]";
    }

    public MSG ReadMessage<MSG>() where MSG : MessageBase, new()
    {
      MSG instance = Activator.CreateInstance<MSG>();
      instance.Deserialize(this.reader);
      return instance;
    }

    public void ReadMessage<MSG>(MSG msg) where MSG : MessageBase
    {
      msg.Deserialize(this.reader);
    }
  }
}
