// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkSystem.PeerListMessage
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using UnityEngine.Networking;

namespace UnityEngine.Networking.NetworkSystem
{
  /// <summary>
  /// 
  /// <para>
  /// Internal UNET message for sending information about network peers to clients.
  /// </para>
  /// 
  /// </summary>
  public class PeerListMessage : MessageBase
  {
    /// <summary>
    /// 
    /// <para>
    /// The list of participants in a networked game.
    /// </para>
    /// 
    /// </summary>
    public PeerInfoMessage[] peers;

    public override void Deserialize(NetworkReader reader)
    {
      this.peers = new PeerInfoMessage[(int) reader.ReadUInt16()];
      for (int index = 0; index < this.peers.Length; ++index)
      {
        PeerInfoMessage peerInfoMessage = new PeerInfoMessage();
        peerInfoMessage.Deserialize(reader);
        this.peers[index] = peerInfoMessage;
      }
    }

    public override void Serialize(NetworkWriter writer)
    {
      writer.Write((ushort) this.peers.Length);
      foreach (PeerInfoMessage peerInfoMessage in this.peers)
        peerInfoMessage.Serialize(writer);
    }
  }
}
