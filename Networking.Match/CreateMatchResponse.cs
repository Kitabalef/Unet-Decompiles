﻿// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.Match.CreateMatchResponse
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
  /// JSON response for a CreateMatchRequest. It contains all information necessdary to continue joining a match.
  /// </para>
  /// 
  /// </summary>
  public class CreateMatchResponse : BasicResponse
  {
    /// <summary>
    /// 
    /// <para>
    /// Network address to connect to in order to join the match.
    /// </para>
    /// 
    /// </summary>
    public string address { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// Network port to connect to in order to join the match.
    /// </para>
    /// 
    /// </summary>
    public int port { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// The network id for the match created.
    /// </para>
    /// 
    /// </summary>
    public NetworkID networkId { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// JSON encoding for the binary access token this client uses to authenticate its session for future commands.
    /// </para>
    /// 
    /// </summary>
    public string accessTokenString { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// NodeId for the requesting client in the created match.
    /// </para>
    /// 
    /// </summary>
    public NodeID nodeId { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// If the match is hosted by a relay server.
    /// </para>
    /// 
    /// </summary>
    public bool usingRelay { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// Provides string description of current class data.
    /// </para>
    /// 
    /// </summary>
    public override string ToString()
    {
      string fmt = "[{0}]-address:{1},port:{2},networkId:0x{3},nodeId:0x{4},usingRelay:{5}";
      object[] objArray = new object[6];
      int index1 = 0;
      string str1 = base.ToString();
      objArray[index1] = (object) str1;
      int index2 = 1;
      string address = this.address;
      objArray[index2] = (object) address;
      int index3 = 2;
      // ISSUE: variable of a boxed type
      __Boxed<int> local1 = (ValueType) this.port;
      objArray[index3] = (object) local1;
      int index4 = 3;
      string str2 = this.networkId.ToString("X");
      objArray[index4] = (object) str2;
      int index5 = 4;
      string str3 = this.nodeId.ToString("X");
      objArray[index5] = (object) str3;
      int index6 = 5;
      // ISSUE: variable of a boxed type
      __Boxed<bool> local2 = (ValueType) (bool) (this.usingRelay ? 1 : 0);
      objArray[index6] = (object) local2;
      return UnityString.Format(fmt, objArray);
    }

    public override void Parse(object obj)
    {
      base.Parse(obj);
      IDictionary<string, object> dictJsonObj = obj as IDictionary<string, object>;
      if (dictJsonObj == null)
        throw new FormatException("While parsing JSON response, found obj is not of type IDictionary<string,object>:" + obj.ToString());
      this.address = this.ParseJSONString("address", obj, dictJsonObj);
      this.port = this.ParseJSONInt32("port", obj, dictJsonObj);
      this.networkId = (NetworkID) this.ParseJSONUInt64("networkId", obj, dictJsonObj);
      this.accessTokenString = this.ParseJSONString("accessTokenString", obj, dictJsonObj);
      this.nodeId = (NodeID) this.ParseJSONUInt16("nodeId", obj, dictJsonObj);
      this.usingRelay = this.ParseJSONBool("usingRelay", obj, dictJsonObj);
    }
  }
}
