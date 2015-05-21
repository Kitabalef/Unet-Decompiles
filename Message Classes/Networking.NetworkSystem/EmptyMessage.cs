// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkSystem.EmptyMessage
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using UnityEngine.Networking;

namespace UnityEngine.Networking.NetworkSystem
{
  /// <summary>
  /// 
  /// <para>
  /// A utility class to send a network message with no contents.
  /// </para>
  /// 
  /// </summary>
  public class EmptyMessage : MessageBase
  {
    public override void Deserialize(NetworkReader reader)
    {
    }

    public override void Serialize(NetworkWriter writer)
    {
    }
  }
}
