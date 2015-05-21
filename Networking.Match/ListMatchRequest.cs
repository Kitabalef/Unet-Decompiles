// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.Match.ListMatchRequest
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
  /// JSON object to request a list of UNET matches. This list is page based with a 1 index.
  /// </para>
  /// 
  /// </summary>
  public class ListMatchRequest : Request
  {
    /// <summary>
    /// 
    /// <para>
    /// Number of results per page to be returned.
    /// </para>
    /// 
    /// </summary>
    public int pageSize { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// 1 based page number requested.
    /// </para>
    /// 
    /// </summary>
    public int pageNum { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// Name filter to apply to the match list.
    /// </para>
    /// 
    /// </summary>
    public string nameFilter { get; set; }

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
    /// List of match attributes to filter against. This will filter down to matches that both have a name that contains the entire text string provided and the value specified in the filter is less than the attribute value for the matching name.
    /// No additional wildcards are allowed in the name. A maximum of 10 filters can be specified between all 3 filter lists.
    /// </para>
    /// 
    /// </summary>
    public Dictionary<string, long> matchAttributeFilterLessThan { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// List of match attributes to filter against. This will filter down to matches that both have a name that contains the entire text string provided and the value specified in the filter is equal to the attribute value for the matching name.
    /// No additional wildcards are allowed in the name. A maximum of 10 filters can be specified between all 3 filter lists.
    /// </para>
    /// 
    /// </summary>
    public Dictionary<string, long> matchAttributeFilterEqualTo { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// List of match attributes to filter against. This will filter down to matches that both have a name that contains the entire text string provided and the value specified in the filter is greater than the attribute value for the matching name.
    /// No additional wildcards are allowed in the name. A maximum of 10 filters can be specified between all 3 filter lists.
    /// </para>
    /// 
    /// </summary>
    public Dictionary<string, long> matchAttributeFilterGreaterThan { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// Provides string description of current class data.
    /// </para>
    /// 
    /// </summary>
    public override string ToString()
    {
      string fmt = "[{0}]-pageSize:{1},pageNum:{2},nameFilter:{3},matchAttributeFilterLessThan.Count:{4}, matchAttributeFilterGreaterThan.Count:{5}";
      object[] objArray = new object[6];
      int index1 = 0;
      string str = base.ToString();
      objArray[index1] = (object) str;
      int index2 = 1;
      // ISSUE: variable of a boxed type
      __Boxed<int> local1 = (ValueType) this.pageSize;
      objArray[index2] = (object) local1;
      int index3 = 2;
      // ISSUE: variable of a boxed type
      __Boxed<int> local2 = (ValueType) this.pageNum;
      objArray[index3] = (object) local2;
      int index4 = 3;
      string nameFilter = this.nameFilter;
      objArray[index4] = (object) nameFilter;
      int index5 = 4;
      // ISSUE: variable of a boxed type
      __Boxed<int> local3 = (ValueType) (this.matchAttributeFilterLessThan != null ? this.matchAttributeFilterLessThan.Count : 0);
      objArray[index5] = (object) local3;
      int index6 = 5;
      // ISSUE: variable of a boxed type
      __Boxed<int> local4 = (ValueType) (this.matchAttributeFilterGreaterThan != null ? this.matchAttributeFilterGreaterThan.Count : 0);
      objArray[index6] = (object) local4;
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
      int num = (this.matchAttributeFilterLessThan != null ? this.matchAttributeFilterLessThan.Count : 0) + (this.matchAttributeFilterEqualTo != null ? this.matchAttributeFilterEqualTo.Count : 0) + (this.matchAttributeFilterGreaterThan != null ? this.matchAttributeFilterGreaterThan.Count : 0);
      if (base.IsValid() && (this.pageSize >= 1 || this.pageSize <= 1000))
        return num <= 10;
      return false;
    }
  }
}
