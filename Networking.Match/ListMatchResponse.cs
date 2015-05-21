// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.Match.ListMatchResponse
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
  /// JSON response for a ListMatchRequest. It contains a list of matches that can be parsed through to describe a page of matches.
  /// </para>
  /// 
  /// </summary>
  public class ListMatchResponse : BasicResponse
  {
    /// <summary>
    /// 
    /// <para>
    /// List of matches fitting the requested description.
    /// </para>
    /// 
    /// </summary>
    public List<MatchDesc> matches { get; set; }

    /// <summary>
    /// 
    /// <para>
    /// Constructor for response class.
    /// </para>
    /// 
    /// </summary>
    /// <param name="matches">A list of matches to give to the object. Only used when generating a new response and not used by callers of a ListMatchRequest.</param><param name="otherMatches"/>
    public ListMatchResponse()
    {
    }

    public ListMatchResponse(List<MatchDesc> otherMatches)
    {
      this.matches = otherMatches;
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
      string fmt = "[{0}]-matches.Count:{1}";
      object[] objArray = new object[2];
      int index1 = 0;
      string str = base.ToString();
      objArray[index1] = (object) str;
      int index2 = 1;
      // ISSUE: variable of a boxed type
      __Boxed<int> local = (ValueType) this.matches.Count;
      objArray[index2] = (object) local;
      return UnityString.Format(fmt, objArray);
    }

    public override void Parse(object obj)
    {
      base.Parse(obj);
      IDictionary<string, object> dictJsonObj = obj as IDictionary<string, object>;
      if (dictJsonObj == null)
        throw new FormatException("While parsing JSON response, found obj is not of type IDictionary<string,object>:" + obj.ToString());
      this.matches = this.ParseJSONList<MatchDesc>("matches", obj, dictJsonObj);
    }
  }
}
