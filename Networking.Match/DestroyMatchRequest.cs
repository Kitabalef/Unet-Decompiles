// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.Match.DestroyMatchRequest
// Assembly: UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B9965B63-74A2-480C-BCFC-887FBCF7E9A7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\Managed\UnityEngine.dll

using UnityEngine;
using UnityEngine.Networking.Types;

namespace UnityEngine.Networking.Match
{
  /// <summary>
  /// 
  /// <para>
  /// JSON object to request a UNET match destruction.
  /// </para>
  /// 
  /// </summary>
  public class DestroyMatchRequest : Request
  {
    /// <summary>
    /// 
    /// <para>
    /// NetworkID of the match to destroy.
    /// </para>
    /// 
    /// </summary>
    public NetworkID networkId { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// Provides string description of current class data.
    /// </para>
    /// 
    /// </summary>
    public override string ToString()
    {
      string fmt = "[{0}]-networkId:0x{1}";
      object[] objArray = new object[2];
      int index1 = 0;
      string str1 = base.ToString();
      objArray[index1] = (object) str1;
      int index2 = 1;
      string str2 = this.networkId.ToString("X");
      objArray[index2] = (object) str2;
      return UnityString.Format(fmt, objArray);
    }

    /// <summary>
    /// 
    /// <para>
    /// Accessor to verify if the contained data is a valid request with respect to initialized variables and accepted parameters.
    /// </para>
    /// 
    /// </summary>
    public override bool IsValid()
    {
      if (base.IsValid())
        return this.networkId != NetworkID.Invalid;
      return false;
    }
  }
}
