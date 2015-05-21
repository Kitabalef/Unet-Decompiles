// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkSystem.IntegerMessage
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using UnityEngine.Networking;

namespace UnityEngine.Networking.NetworkSystem
{
  /// <summary>
  /// 
  /// <para>
  /// A utility class to send simple network messages that only contain an integer.
  /// </para>
  /// 
  /// </summary>
  public class IntegerMessage : MessageBase
  {
    /// <summary>
    /// 
    /// <para>
    /// The integer value to serialize.
    /// </para>
    /// 
    /// </summary>
    public int value;

    public IntegerMessage()
    {
    }

    public IntegerMessage(int v)
    {
      this.value = v;
    }

    public override void Deserialize(NetworkReader reader)
    {
      this.value = (int) reader.ReadPackedUInt32();
    }

    public override void Serialize(NetworkWriter writer)
    {
      writer.WritePackedUInt32((uint) this.value);
    }
  }
}
