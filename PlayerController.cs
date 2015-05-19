// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.PlayerController
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using UnityEngine;

namespace UnityEngine.Networking
{
  /// <summary>
  /// 
  /// <para>
  /// This represents a networked player.
  /// </para>
  /// 
  /// </summary>
  public class PlayerController
  {
    /// <summary>
    /// 
    /// <para>
    /// The local player ID number of this player.
    /// </para>
    /// 
    /// </summary>
    public short playerControllerId = (short) -1;
    internal const short kMaxLocalPlayers = (short) 8;
    /// <summary>
    /// 
    /// <para>
    /// The maximum number of local players that a client connection can have.
    /// </para>
    /// 
    /// </summary>
    public const int MaxPlayersPerClient = 32;
    /// <summary>
    /// 
    /// <para>
    /// The NetworkIdentity component of the player.
    /// </para>
    /// 
    /// </summary>
    public NetworkIdentity unetView;
    /// <summary>
    /// 
    /// <para>
    /// The game object for this player.
    /// </para>
    /// 
    /// </summary>
    public GameObject gameObject;

    /// <summary>
    /// 
    /// <para>
    /// Checks if this PlayerController has an actual player attached to it.
    /// </para>
    /// 
    /// </summary>
    public bool IsValid
    {
      get
      {
        return (int) this.playerControllerId != -1;
      }
    }

    public PlayerController()
    {
    }

    internal PlayerController(GameObject go, short playerControllerId)
    {
      this.gameObject = go;
      this.unetView = go.GetComponent<NetworkIdentity>();
      this.playerControllerId = playerControllerId;
    }

    /// <summary>
    /// 
    /// <para>
    /// String representation of the player objects state.
    /// </para>
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// 
    /// <para>
    /// String with the object state.
    /// </para>
    /// 
    /// </returns>
    public override string ToString()
    {
      return string.Format("ID={0} NetworkIdentity NetID={1} Player={2}", (object) this.playerControllerId, !((Object) this.unetView != (Object) null) ? (object) "null" : (object) this.unetView.netId.ToString(), !((Object) this.gameObject != (Object) null) ? (object) "null" : (object) this.gameObject.name);
    }
  }
}
