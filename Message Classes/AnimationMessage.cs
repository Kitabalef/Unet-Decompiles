// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkSystem.AnimationMessage
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using UnityEngine.Networking;

namespace UnityEngine.Networking.NetworkSystem
{
  internal class AnimationMessage : MessageBase
  {
    public NetworkInstanceId netId;
    public int stateHash;
    public float normalizedTime;
    public byte[] parameters;

    public override void Deserialize(NetworkReader reader)
    {
      this.netId = reader.ReadNetworkId();
      this.stateHash = (int) reader.ReadPackedUInt32();
      this.normalizedTime = reader.ReadSingle();
      this.parameters = reader.ReadBytesAndSize();
    }

    public override void Serialize(NetworkWriter writer)
    {
      writer.Write(this.netId);
      writer.WritePackedUInt32((uint) this.stateHash);
      writer.Write(this.normalizedTime);
      writer.WriteBytesAndSize(this.parameters, this.parameters.Length);
    }
  }
}
