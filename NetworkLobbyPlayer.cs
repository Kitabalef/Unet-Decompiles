// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkLobbyPlayer
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using UnityEngine;
using UnityEngine.Networking.NetworkSystem;

namespace UnityEngine.Networking
{
  /// <summary>
  /// 
  /// <para>
  /// This component works in conjunction with the NetworkLobbyManager to make up the multiplayer lobby system.
  /// </para>
  /// 
  /// </summary>
  [AddComponentMenu("Network/NetworkLobbyPlayer")]
  public class NetworkLobbyPlayer : NetworkBehaviour
  {
    /// <summary>
    /// 
    /// <para>
    /// This flag controls whether the default UI is shown for the lobby player.
    /// </para>
    /// 
    /// </summary>
    [SerializeField]
    public bool ShowLobbyGUI = true;
    private byte m_Slot;
    private bool m_ReadyToBegin;

    /// <summary>
    /// 
    /// <para>
    /// The slot within the lobby that this player inhabits.
    /// </para>
    /// 
    /// </summary>
    public byte slot
    {
      get
      {
        return this.m_Slot;
      }
      set
      {
        this.m_Slot = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// This is a flag that control whether this player is ready for the game to begin.
    /// </para>
    /// 
    /// </summary>
    public bool readyToBegin
    {
      get
      {
        return this.m_ReadyToBegin;
      }
      set
      {
        this.m_ReadyToBegin = value;
      }
    }

    private void Start()
    {
      Object.DontDestroyOnLoad((Object) this.gameObject);
    }

    public override void OnStartClient()
    {
      NetworkLobbyManager networkLobbyManager = NetworkManager.singleton as NetworkLobbyManager;
      if ((bool) ((Object) networkLobbyManager))
      {

        networkLobbyManager.lobbySlots[(int) this.m_Slot] = this;
        this.m_ReadyToBegin = false;
        this.OnClientEnterLobby(); // hook into client side setup when player is entering the lobby
      }
      else
        Debug.LogError((object) "No Lobby for LobbyPlayer");
    }

    /// <summary>
    /// 
    /// <para>
    /// This is used on clients to tell the server that this player is ready for the game to begin.
    /// </para>
    /// 
    /// </summary>
    public void SendReadyToBeginMessage()
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkLobbyPlayer SendReadyToBeginMessage");

      NetworkLobbyManager networkLobbyManager = NetworkManager.singleton as NetworkLobbyManager;

      if (!(bool) ((Object) networkLobbyManager))
        return;
      
      // Send the players controller id to server to signal this player as ready
      IntegerMessage integerMessage = new IntegerMessage((int) this.playerControllerId);
      networkLobbyManager.client.Send((short) 43, (MessageBase) integerMessage);
    }

    /// <summary>
    /// 
    /// <para>
    /// This is used on clients to tell the server that the client has switched from the lobby to the GameScene and is ready to play.
    /// </para>
    /// 
    /// </summary>
    public void SendSceneLoadedMessage()
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkLobbyPlayer SendSceneLoadedMessage");

      NetworkLobbyManager networkLobbyManager = NetworkManager.singleton as NetworkLobbyManager;

      if (!(bool) ((Object) networkLobbyManager))
        return;

      IntegerMessage integerMessage = new IntegerMessage((int) this.playerControllerId);
      networkLobbyManager.client.Send((short) 44, (MessageBase) integerMessage);
    }



    private void OnLevelWasLoaded()
    {
      NetworkLobbyManager networkLobbyManager = NetworkManager.singleton as NetworkLobbyManager;

      if ((bool) ((Object) networkLobbyManager) && Application.loadedLevelName == networkLobbyManager.lobbyScene || !this.isLocalPlayer)
        return;

      this.SendSceneLoadedMessage();
    }

    /// <summary>
    /// 
    /// <para>
    /// This removes this player from the lobby.
    /// </para>
    /// 
    /// </summary>
    public void RemovePlayer()
    {
      if (!this.isLocalPlayer || this.m_ReadyToBegin) // you have to be unready to be able to be removed
        return;

      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkLobbyPlayer RemovePlayer");

      ClientScene.RemovePlayer(this.GetComponent<NetworkIdentity>().playerControllerId);
    }

 
    // This is a hook that is invoked on all player objects when entering the lobby.
    public virtual void OnClientEnterLobby()
    {
    }

  
    // This is a hook that is invoked on all player objects when exiting the lobby.
    public virtual void OnClientExitLobby()
    {
    }

    public virtual void OnClientReady(bool readyState)
    {
    }


    public override bool OnSerialize(NetworkWriter writer, bool initialState)
    {
      writer.WritePackedUInt32(1U);
      writer.Write(this.m_Slot);
      writer.Write(this.m_ReadyToBegin);
      return true;
    }

    // the server sends a message to clients after it processes a ready to begin message sent from a client
    // server is responsible for assigning a slot and setting the player ready state
    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
      if ((int) reader.ReadPackedUInt32() == 0)
        return;
      this.m_Slot = reader.ReadByte();
      this.m_ReadyToBegin = reader.ReadBoolean();
    }

    // The GUI shown in the old gui lobby example when ShowLobbyGUI is checked as true
    private void OnGUI()
    {
      if (!this.ShowLobbyGUI)
        return;

      NetworkLobbyManager networkLobbyManager = NetworkManager.singleton as NetworkLobbyManager;

      if ((bool) ((Object) networkLobbyManager) && (!networkLobbyManager.showLobbyGUI || Application.loadedLevelName != networkLobbyManager.lobbyScene))
        return;

      Rect position = new Rect((float) (100 + (int) this.m_Slot * 100), 200f, 90f, 20f);

      if (this.isLocalPlayer)
      {
        GUI.Label(position, " [ You ]");

        if (this.m_ReadyToBegin)
        {
          position.y += 25f;
          GUI.Label(position, "[ Ready ]");
        }
        else
        {
          position.y += 25f;
          if (GUI.Button(position, "Not Ready"))
            this.SendReadyToBeginMessage();

          position.y += 25f;
          if (!GUI.Button(position, "Remove"))
            return;

          ClientScene.RemovePlayer(this.GetComponent<NetworkIdentity>().playerControllerId);
        }
      }
      else // show other lobby players gui's
      {
        GUI.Label(position, "Player [" + (object) this.netId + "]");
        position.y += 25f;
        GUI.Label(position, "Ready [" + (object) (bool) (this.m_ReadyToBegin ? 1 : 0) + "]");
      }
    }
  }
}
