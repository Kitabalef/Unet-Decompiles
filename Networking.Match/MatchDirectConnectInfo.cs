// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.Match.MatchDirectConnectInfo
// Assembly: UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B9965B63-74A2-480C-BCFC-887FBCF7E9A7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\Managed\UnityEngine.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace UnityEngine.Networking.Match
{
  /// <summary>
  /// 
  /// <para>
  /// Class describing a client in a network match.
  /// </para>
  /// 
  /// </summary>
  public class MatchDirectConnectInfo : ResponseBase
  {
    /// <summary>
    /// 
    /// <para>
    /// NodeID of the client described in this direct connect info.
    /// </para>
    /// 
    /// </summary>
    public NodeID nodeId { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// Public address the client described by this class provided.
    /// </para>
    /// 
    /// </summary>
    public string publicAddress { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// Private address the client described by this class provided.
    /// </para>
    /// 
    /// </summary>
    public string privateAddress { get; set; }

    public override string ToString()
    {
      string fmt = "[{0}]-nodeId:{1},publicAddress:{2},privateAddress:{3}";
      object[] objArray = new object[4];
      int index1 = 0;
      string str = base.ToString();
      objArray[index1] = (object) str;
      int index2 = 1;
      // ISSUE: variable of a boxed type
      __Boxed<NodeID> local = (Enum) this.nodeId;
      objArray[index2] = (object) local;
      int index3 = 2;
      string publicAddress = this.publicAddress;
      objArray[index3] = (object) publicAddress;
      int index4 = 3;
      string privateAddress = this.privateAddress;
      objArray[index4] = (object) privateAddress;
      return UnityString.Format(fmt, objArray);
    }

    public override void Parse(object obj)
    {
      IDictionary<string, object> dictJsonObj = obj as IDictionary<string, object>;
      if (dictJsonObj == null)
        throw new FormatException("While parsing JSON response, found obj is not of type IDictionary<string,object>:" + obj.ToString());
      this.nodeId = (NodeID) this.ParseJSONUInt16("nodeId", obj, dictJsonObj);
      this.publicAddress = this.ParseJSONString("public_address", obj, dictJsonObj);
      this.privateAddress = this.ParseJSONString("private_address", obj, dictJsonObj);
    }
  }
}
