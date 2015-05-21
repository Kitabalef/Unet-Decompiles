// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkSystem.StringMessage
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using UnityEngine.Networking;

namespace UnityEngine.Networking.NetworkSystem
{
  /// <summary>
  /// 
  /// <para>
  /// This is a utility class for simple network messages that contain only a string.
  /// </para>
  /// 
  /// </summary>
  public class StringMessage : MessageBase
  {
    /// <summary>
    /// 
    /// <para>
    /// The string that will be serialized.
    /// </para>
    /// 
    /// </summary>
    public string value;

    public StringMessage()
    {
    }

    public StringMessage(string v)
    {
      this.value = v;
    }

    public override void Deserialize(NetworkReader reader)
    {
      this.value = reader.ReadString();
    }

    public override void Serialize(NetworkWriter writer)
    {
      writer.Write(this.value);
    }
  }
}
