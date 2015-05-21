// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.Match.CreateMatchRequest
// Assembly: UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B9965B63-74A2-480C-BCFC-887FBCF7E9A7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\Managed\UnityEngine.dll

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Networking.Match
{
  /// <summary>
  /// 
  /// <para>
  /// JSON object to request a UNET match creation.
  /// </para>
  /// 
  /// </summary>
  public class CreateMatchRequest : Request
  {
    /// <summary>
    /// 
    /// <para>
    /// Name of the match to create.
    /// </para>
    /// 
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// Max number of clients that may join the match to create, including the host.
    /// </para>
    /// 
    /// </summary>
    public uint size { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// The (optional) public network address for the client making the request. This is the internet available public address another client on the internet (but not the local network) could use to connect directly to the client making the request and may be used to better connect multiple clients. If it is not supplied the networking layer will still be completely functional.
    /// </para>
    /// 
    /// </summary>
    public string publicAddress { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// The (optional) private network address for the client making the request. This is the local network available private address another client on the same network could use to connect directly to the client making the request and may be used to better connect multiple clients. If it is not supplied the networking layer will still be completely functional.
    /// </para>
    /// 
    /// </summary>
    public string privateAddress { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// The optional game defined Elo score for the client making the request. The Elo score is averaged against all clients in a match and that value is used to produce better search results when listing available matches.
    /// If the Elo is provided the result set will be ordered according to the magnitude of the absoloute value of the difference of the a client searching for a match and the network average for all clients in each match. If the Elo score is not provided (and therefore 0 for all matches) the Elo score will not affect the search results.
    /// Each game can calculate this value as they wish according to whatever scale is best for that game.
    /// </para>
    /// 
    /// </summary>
    public int eloScore { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// Bool to describe if the created match should be advertised.
    /// </para>
    /// 
    /// </summary>
    public bool advertise { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// Password for the match to create. Leave blank for no password. Cannot be null.
    /// </para>
    /// 
    /// </summary>
    public string password { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// Match attributes describing game specific features for this match. Each attribute is a key/value pair of a string key with a long value. Each match may have up to 10 of these values.
    /// The game is free to use this as desired to assist in finding better match results when clients search for matches to join.
    /// </para>
    /// 
    /// </summary>
    public Dictionary<string, long> matchAttributes { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// Provides string description of current class data.
    /// </para>
    /// 
    /// </summary>
    public override string ToString()
    {
      string fmt = "[{0}]-name:{1},size:{2},advertise:{3},HasPassword:{4},matchAttributes.Count:{5}";
      object[] objArray = new object[6];
      int index1 = 0;
      string str1 = base.ToString();
      objArray[index1] = (object) str1;
      int index2 = 1;
      string name = this.name;
      objArray[index2] = (object) name;
      int index3 = 2;
      // ISSUE: variable of a boxed type
      __Boxed<uint> local1 = (ValueType) this.size;
      objArray[index3] = (object) local1;
      int index4 = 3;
      // ISSUE: variable of a boxed type
      __Boxed<bool> local2 = (ValueType) (bool) (this.advertise ? 1 : 0);
      objArray[index4] = (object) local2;
      int index5 = 4;
      string str2 = !(this.password == string.Empty) ? "YES" : "NO";
      objArray[index5] = (object) str2;
      int index6 = 5;
      // ISSUE: variable of a boxed type
      __Boxed<int> local3 = (ValueType) (this.matchAttributes != null ? this.matchAttributes.Count : 0);
      objArray[index6] = (object) local3;
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
      if (base.IsValid() && this.size >= 2U && this.matchAttributes == null)
        return true;
      return this.matchAttributes.Count <= 10;
    }
  }
}
