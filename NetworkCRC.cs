// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkCRC
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;

namespace UnityEngine.Networking
{
  /// <summary>
  /// 
  /// <para>
  /// This class holds information about which networked scripts use which QoS channels for updates.
  /// </para>
  /// 
  /// </summary>
  public class NetworkCRC
  {
    private Dictionary<string, int> m_Scripts = new Dictionary<string, int>();
    public static NetworkCRC singleton;

    /// <summary>
    /// 
    /// <para>
    /// A dictionary of script QoS channels.
    /// </para>
    /// 
    /// </summary>
    public Dictionary<string, int> scripts
    {
      get
      {
        return this.m_Scripts;
      }
    }

    public static void ReinitializeScriptCRCs(Assembly callingAssembly)
    {
      if (NetworkCRC.singleton == null)
        NetworkCRC.singleton = new NetworkCRC();
      NetworkCRC.singleton.m_Scripts.Clear();
      foreach (System.Type type in callingAssembly.GetTypes())
      {
        if (type.BaseType == typeof (NetworkBehaviour))
        {
          MethodInfo method = type.GetMethod(".cctor", BindingFlags.Static);
          if (method != null)
            method.Invoke((object) null, new object[0]);
        }
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// This is used to setup script network settings CRC data.
    /// </para>
    /// 
    /// </summary>
    /// <param name="name">Script name.</param><param name="channel">QoS Channel.</param>
    public static void RegisterBehaviour(string name, int channel)
    {
      if (NetworkCRC.singleton == null)
        NetworkCRC.singleton = new NetworkCRC();
      NetworkCRC.singleton.m_Scripts[name] = channel;
    }

    internal bool Validate(CRCMessageEntry[] scripts, int numChannels)
    {
      if (NetworkCRC.singleton.scripts.Count != scripts.Length)
      {
        if (LogFilter.logError)
        {
          object[] objArray = new object[4];
          int index1 = 0;
          string str1 = "HLAPI CRC channel count error local: ";
          objArray[index1] = (object) str1;
          int index2 = 1;
          // ISSUE: variable of a boxed type
          __Boxed<int> local1 = (ValueType) NetworkCRC.singleton.scripts.Count;
          objArray[index2] = (object) local1;
          int index3 = 2;
          string str2 = " remote: ";
          objArray[index3] = (object) str2;
          int index4 = 3;
          // ISSUE: variable of a boxed type
          __Boxed<int> local2 = (ValueType) scripts.Length;
          objArray[index4] = (object) local2;
          Debug.LogError((object) string.Concat(objArray));
        }
        this.Dump(scripts);
        return false;
      }
      foreach (CRCMessageEntry crcMessageEntry in scripts)
      {
        if (LogFilter.logDebug)
        {
          object[] objArray = new object[4];
          int index1 = 0;
          string str1 = "Script: ";
          objArray[index1] = (object) str1;
          int index2 = 1;
          string str2 = crcMessageEntry.name;
          objArray[index2] = (object) str2;
          int index3 = 2;
          string str3 = " Channel: ";
          objArray[index3] = (object) str3;
          int index4 = 3;
          // ISSUE: variable of a boxed type
          __Boxed<byte> local = (ValueType) crcMessageEntry.channel;
          objArray[index4] = (object) local;
          Debug.Log((object) string.Concat(objArray));
        }
        if (NetworkCRC.singleton.scripts.ContainsKey(crcMessageEntry.name))
        {
          int num = NetworkCRC.singleton.scripts[crcMessageEntry.name];
          if (num != (int) crcMessageEntry.channel)
          {
            if (LogFilter.logError)
            {
              object[] objArray = new object[6];
              int index1 = 0;
              string str1 = "HLAPI CRC Channel Mismatch. Script: ";
              objArray[index1] = (object) str1;
              int index2 = 1;
              string str2 = crcMessageEntry.name;
              objArray[index2] = (object) str2;
              int index3 = 2;
              string str3 = " LocalChannel: ";
              objArray[index3] = (object) str3;
              int index4 = 3;
              // ISSUE: variable of a boxed type
              __Boxed<int> local1 = (ValueType) num;
              objArray[index4] = (object) local1;
              int index5 = 4;
              string str4 = " RemoteChannel: ";
              objArray[index5] = (object) str4;
              int index6 = 5;
              // ISSUE: variable of a boxed type
              __Boxed<byte> local2 = (ValueType) crcMessageEntry.channel;
              objArray[index6] = (object) local2;
              Debug.LogError((object) string.Concat(objArray));
            }
            this.Dump(scripts);
            return false;
          }
        }
        if ((int) crcMessageEntry.channel >= numChannels)
        {
          if (LogFilter.logError)
          {
            object[] objArray = new object[4];
            int index1 = 0;
            string str1 = "HLAPI CRC channel out of range! Script: ";
            objArray[index1] = (object) str1;
            int index2 = 1;
            string str2 = crcMessageEntry.name;
            objArray[index2] = (object) str2;
            int index3 = 2;
            string str3 = " Channel: ";
            objArray[index3] = (object) str3;
            int index4 = 3;
            // ISSUE: variable of a boxed type
            __Boxed<byte> local = (ValueType) crcMessageEntry.channel;
            objArray[index4] = (object) local;
            Debug.LogError((object) string.Concat(objArray));
          }
          this.Dump(scripts);
          return false;
        }
      }
      return true;
    }

    private void Dump(CRCMessageEntry[] scripts)
    {
      using (Dictionary<string, int>.KeyCollection.Enumerator enumerator = this.m_Scripts.Keys.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          string current = enumerator.Current;
          object[] objArray = new object[4];
          int index1 = 0;
          string str1 = "CRC Local Dump ";
          objArray[index1] = (object) str1;
          int index2 = 1;
          string str2 = current;
          objArray[index2] = (object) str2;
          int index3 = 2;
          string str3 = " : ";
          objArray[index3] = (object) str3;
          int index4 = 3;
          // ISSUE: variable of a boxed type
          __Boxed<int> local = (ValueType) this.m_Scripts[current];
          objArray[index4] = (object) local;
          Debug.Log((object) string.Concat(objArray));
        }
      }
      foreach (CRCMessageEntry crcMessageEntry in scripts)
      {
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "CRC Remote Dump ";
        objArray[index1] = (object) str1;
        int index2 = 1;
        string str2 = crcMessageEntry.name;
        objArray[index2] = (object) str2;
        int index3 = 2;
        string str3 = " : ";
        objArray[index3] = (object) str3;
        int index4 = 3;
        // ISSUE: variable of a boxed type
        __Boxed<byte> local = (ValueType) crcMessageEntry.channel;
        objArray[index4] = (object) local;
        Debug.Log((object) string.Concat(objArray));
      }
    }
  }
}
