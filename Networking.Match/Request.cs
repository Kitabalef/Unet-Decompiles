// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.Match.Request
// Assembly: UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B9965B63-74A2-480C-BCFC-887FBCF7E9A7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\Managed\UnityEngine.dll

using System;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace UnityEngine.Networking.Match
{
  /// <summary>
  /// 
  /// <para>
  /// Abstract base for requests, which includes common info in all requests.
  /// </para>
  /// 
  /// </summary>
  public abstract class Request
  {
    /// <summary>
    /// 
    /// <para>
    /// Matchmaker protocol version info.
    /// </para>
    /// 
    /// </summary>
    public int version = 1;

    /// <summary>
    /// 
    /// <para>
    /// SourceID for the current client, required in every request. This is generated from the Cloud API.
    /// </para>
    /// 
    /// </summary>
    public SourceID sourceId { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// AppID for the current game, required in every request. This is generated from the Cloud API.
    /// </para>
    /// 
    /// </summary>
    public AppID appId { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// The JSON encoded binary access token this client uses to authenticate its session for future commands.
    /// </para>
    /// 
    /// </summary>
    public string accessTokenString { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// Domain for the request. All commands will be sandboxed to their own domain; For example no clients with domain 1 will see matches with domain 2. This can be used to prevent incompatible client versions from communicating.
    /// </para>
    /// 
    /// </summary>
    public int domain { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// Accessor to verify if the contained data is a valid request with respect to initialized variables and accepted parameters.
    /// </para>
    /// 
    /// </summary>
    public virtual bool IsValid()
    {
      if (this.appId != AppID.Invalid)
        return this.sourceId != SourceID.Invalid;
      return false;
    }

    /// <summary>
    /// 
    /// <para>
    /// Provides string description of current class data.
    /// </para>
    /// 
    /// </summary>
    public override string ToString()
    {
      string fmt = "[{0}]-SourceID:0x{1},AppID:0x{2},domain:{3}";
      object[] objArray = new object[4];
      int index1 = 0;
      string str1 = base.ToString();
      objArray[index1] = (object) str1;
      int index2 = 1;
      string str2 = this.sourceId.ToString("X");
      objArray[index2] = (object) str2;
      int index3 = 2;
      string str3 = this.appId.ToString("X");
      objArray[index3] = (object) str3;
      int index4 = 3;
      // ISSUE: variable of a boxed type
      __Boxed<int> local = (ValueType) this.domain;
      objArray[index4] = (object) local;
      return UnityString.Format(fmt, objArray);
    }
  }
}
