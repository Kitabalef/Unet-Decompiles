// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.Match.JoinMatchRequest
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
  /// JSON object to request joining an existing UNET match.
  /// </para>
  /// 
  /// </summary>
  public class JoinMatchRequest : Request
  {
    /// <summary>
    /// 
    /// <para>
    /// NetworkID of the match to join.
    /// </para>
    /// 
    /// </summary>
    public NetworkID networkId { get; set; }

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
    /// Password for the match to join. Leave blank for no password. Cannot be null.
    /// </para>
    /// 
    /// </summary>
    public string password { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// Provides string description of current class data.
    /// </para>
    /// 
    /// </summary>
    public override string ToString()
    {
      string fmt = "[{0}]-networkId:0x{1},HasPassword:{2}";
      object[] objArray = new object[3];
      int index1 = 0;
      string str1 = base.ToString();
      objArray[index1] = (object) str1;
      int index2 = 1;
      string str2 = this.networkId.ToString("X");
      objArray[index2] = (object) str2;
      int index3 = 2;
      string str3 = !(this.password == string.Empty) ? "YES" : "NO";
      objArray[index3] = (object) str3;
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
