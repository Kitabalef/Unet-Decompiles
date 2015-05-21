// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.Types.NetworkAccessToken
// Assembly: UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B9965B63-74A2-480C-BCFC-887FBCF7E9A7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\Managed\UnityEngine.dll

using System;

namespace UnityEngine.Networking.Types
{
  /// <summary>
  /// 
  /// <para>
  /// Access token used to authenticate a client session for the purposes of allowing or disallowing match operations requested by that client.
  /// </para>
  /// 
  /// </summary>
  public class NetworkAccessToken
  {
    private const int NETWORK_ACCESS_TOKEN_SIZE = 64;
    /// <summary>
    /// 
    /// <para>
    /// Binary field for the actual token.
    /// </para>
    /// 
    /// </summary>
    public byte[] array;

    public NetworkAccessToken()
    {
      this.array = new byte[64];
    }

    public NetworkAccessToken(byte[] array)
    {
      this.array = array;
    }

    public NetworkAccessToken(string strArray)
    {
      this.array = Convert.FromBase64String(strArray);
    }

    /// <summary>
    /// 
    /// <para>
    /// Accessor to get an encoded string from the m_array data.
    /// </para>
    /// 
    /// </summary>
    public string GetByteString()
    {
      return Convert.ToBase64String(this.array);
    }

    /// <summary>
    /// 
    /// <para>
    /// Checks if the token is a valid set of data with respect to default values (returns true if the values are not default, does not validate the token is a current legitimate token with respect to the server's auth framework).
    /// </para>
    /// 
    /// </summary>
    public bool IsValid()
    {
      if (this.array == null || this.array.Length != 64)
        return false;
      bool flag = false;
      foreach (int num in this.array)
      {
        if (num != 0)
        {
          flag = true;
          break;
        }
      }
      return flag;
    }
  }
}
