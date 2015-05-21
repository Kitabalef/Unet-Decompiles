// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkSystem.RemovePlayerMessage
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using UnityEngine.Networking;

namespace UnityEngine.Networking.NetworkSystem
{
  /// <summary>
  /// 
  /// <para>
  /// This is passed to handler funtions registered for the SYSTEM_REMOVE_PLAYER built-in message.
  /// </para>
  /// 
  /// </summary>
  public class RemovePlayerMessage : MessageBase
  {
    /// <summary>
    /// 
    /// <para>
    /// The player ID of the player object which should be removed.
    /// </para>
    /// 
    /// </summary>
    public short playerControllerId;

    public override void Deserialize(NetworkReader reader)
    {
      this.playerControllerId = (short) reader.ReadUInt16();
    }

    public override void Serialize(NetworkWriter writer)
    {
      writer.Write((ushort) this.playerControllerId);
    }
  }
}
