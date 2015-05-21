// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.Match.Response
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
  /// Abstract class that contains shared accessors for any response.
  /// </para>
  /// 
  /// </summary>
  public abstract class Response : ResponseBase, IResponse
  {
    /// <summary>
    /// 
    /// <para>
    /// Bool describing if the request was successful.
    /// </para>
    /// 
    /// </summary>
    public bool success { get; private set; }

    /// <summary>
    /// 
    /// <para>
    /// Extended string information that is returned when the server encounters an error processing a request.
    /// </para>
    /// 
    /// </summary>
    public string extendedInfo { get; private set; }

    public void SetSuccess()
    {
      this.success = true;
      this.extendedInfo = string.Empty;
    }

    public void SetFailure(string info)
    {
      this.success = false;
      this.extendedInfo = info;
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
      string fmt = "[{0}]-success:{1}-extendedInfo:{2}";
      object[] objArray = new object[3];
      int index1 = 0;
      string str = base.ToString();
      objArray[index1] = (object) str;
      int index2 = 1;
      // ISSUE: variable of a boxed type
      __Boxed<bool> local = (ValueType) (bool) (this.success ? 1 : 0);
      objArray[index2] = (object) local;
      int index3 = 2;
      string extendedInfo = this.extendedInfo;
      objArray[index3] = (object) extendedInfo;
      return UnityString.Format(fmt, objArray);
    }

    public override void Parse(object obj)
    {
      IDictionary<string, object> dictJsonObj = obj as IDictionary<string, object>;
      if (dictJsonObj == null)
        return;
      this.success = this.ParseJSONBool("success", obj, dictJsonObj);
      this.extendedInfo = this.ParseJSONString("extendedInfo", obj, dictJsonObj);
      if (!this.success)
        throw new FormatException("FAILURE Returned from server: " + this.extendedInfo);
    }
  }
}
