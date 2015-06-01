// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.SyncListString
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B7B01925-49A3-43D3-9423-9C21FB062FE4
// Assembly location: C:\Program Files\Unity\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

namespace UnityEngine.Networking
{
  /// <summary>
  /// 
  /// <para>
  /// This is a list of strings that will be synchronized from the server to clients.
  /// </para>
  /// 
  /// </summary>
  public sealed class SyncListString : SyncList<string>
  {
    protected override void SerializeItem(NetworkWriter writer, string item)
    {
      writer.Write(item);
    }

    protected override string DeserializeItem(NetworkReader reader)
    {
      return reader.ReadString();
    }

    public static SyncListString ReadInstance(NetworkReader reader)
    {
      ushort num = reader.ReadUInt16();
      SyncListString syncListString = new SyncListString();
      for (ushort index = (ushort) 0; (int) index < (int) num; ++index)
        syncListString.AddInternal(reader.ReadString());
      return syncListString;
    }

    public static void WriteInstance(NetworkWriter writer, SyncListString items)
    {
      writer.Write((ushort) items.Count);
      foreach (string str in (SyncList<string>) items)
        writer.Write(str);
    }
  }
}
