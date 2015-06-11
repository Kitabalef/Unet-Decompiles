// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.Match.MatchInfo
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
  /// Details about a UNET Matchmaker match.
  /// </para>
  /// 
  /// </summary>
  public class MatchInfo
  {
    /// <summary>
    /// 
    /// <para>
    /// IP address of the host of the match,.
    /// </para>
    /// 
    /// </summary>
    public string address { get; private set; }

    /// <summary>
    /// 
    /// <para>
    /// Port of the host of the match.
    /// </para>
    /// 
    /// </summary>
    public int port { get; private set; }

    /// <summary>
    /// 
    /// <para>
    /// The unique ID of this match.
    /// </para>
    /// 
    /// </summary>
    public NetworkID networkId { get; private set; }

    /// <summary>
    /// 
    /// <para>
    /// The binary access token this client uses to authenticate its session for future commands.
    /// </para>
    /// 
    /// </summary>
    public NetworkAccessToken accessToken { get; private set; }

    /// <summary>
    /// 
    /// <para>
    /// NodeID for this member client in the match.
    /// </para>
    /// 
    /// </summary>
    public NodeID nodeId { get; private set; }

    /// <summary>
    /// 
    /// <para>
    /// Flag to say if the match uses a relay server.
    /// </para>
    /// 
    /// </summary>
    public bool usingRelay { get; private set; }

    public MatchInfo(CreateMatchResponse matchResponse)
    {
      this.address = matchResponse.address;
      this.port = matchResponse.port;
      this.networkId = matchResponse.networkId;
      this.accessToken = new NetworkAccessToken(matchResponse.accessTokenString);
      this.nodeId = matchResponse.nodeId;
      this.usingRelay = matchResponse.usingRelay;
    }

    public MatchInfo(JoinMatchResponse matchResponse)
    {
      this.address = matchResponse.address;
      this.port = matchResponse.port;
      this.networkId = matchResponse.networkId;
      this.accessToken = new NetworkAccessToken(matchResponse.accessTokenString);
      this.nodeId = matchResponse.nodeId;
      this.usingRelay = matchResponse.usingRelay;
    }

    public override string ToString()
    {
      string fmt = "{0} @ {1}:{2} [{3},{4}]";
      object[] objArray = new object[5];
      int index1 = 0;
      // ISSUE: variable of a boxed type
      __Boxed<NetworkID> local1 = (Enum) this.networkId;
      objArray[index1] = (object) local1;
      int index2 = 1;
      string address = this.address;
      objArray[index2] = (object) address;
      int index3 = 2;
      // ISSUE: variable of a boxed type
      __Boxed<int> local2 = (ValueType) this.port;
      objArray[index3] = (object) local2;
      int index4 = 3;
      // ISSUE: variable of a boxed type
      __Boxed<NodeID> local3 = (Enum) this.nodeId;
      objArray[index4] = (object) local3;
      int index5 = 4;
      // ISSUE: variable of a boxed type
      __Boxed<bool> local4 = (ValueType) (bool) (this.usingRelay ? 1 : 0);
      objArray[index5] = (object) local4;
      return UnityString.Format(fmt, objArray);
    }
  }
}
