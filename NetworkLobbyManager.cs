// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkLobbyManager
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;

namespace UnityEngine.Networking
{
  /// <summary>
  /// 
  /// <para>
  /// This is a specialized NetworkManager that includes a networked lobby.
  /// </para>
  /// 
  /// </summary>
  [AddComponentMenu("Network/NetworkLobbyManager")]
  public class NetworkLobbyManager : NetworkManager
  {
    private static IntegerMessage s_ReadyToBeginMessage = new IntegerMessage();
    private static IntegerMessage s_SceneLoadedMessage = new IntegerMessage();
    private static LobbyReadyToBeginMessage s_LobbyReadyToBeginMessage = new LobbyReadyToBeginMessage();
    [SerializeField]
    private bool m_ShowLobbyGUI = true;
    [SerializeField]
    private int m_MaxPlayers = 4;
    [SerializeField]
    private int m_MaxPlayersPerConnection = 1;
    [SerializeField]
    private string m_LobbyScene = string.Empty;
    [SerializeField]
    private string m_PlayScene = string.Empty;
    private List<NetworkLobbyManager.PendingPlayer> pendingPlayers = new List<NetworkLobbyManager.PendingPlayer>();
    [SerializeField]
    private NetworkLobbyPlayer m_LobbyPlayerPrefab;
    [SerializeField]
    private GameObject m_GamePlayerPrefab;
    /// <summary>
    /// 
    /// <para>
    /// These slots track players that enter the lobby.
    /// </para>
    /// 
    /// </summary>
    public NetworkLobbyPlayer[] lobbySlots;

    /// <summary>
    /// 
    /// <para>
    /// This flag enables display of the default lobby UI.
    /// </para>
    /// 
    /// </summary>
    public bool showLobbyGUI
    {
      get
      {
        return this.m_ShowLobbyGUI;
      }
      set
      {
        this.m_ShowLobbyGUI = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The maximum number of players allowed in the game.
    /// </para>
    /// 
    /// </summary>
    public int maxPlayers
    {
      get
      {
        return this.m_MaxPlayers;
      }
      set
      {
        this.m_MaxPlayers = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The maximum number of players per connection.
    /// </para>
    /// 
    /// </summary>
    public int maxPlayersPerConnection
    {
      get
      {
        return this.m_MaxPlayersPerConnection;
      }
      set
      {
        this.m_MaxPlayersPerConnection = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// This is the prefab of the player to be created in the LobbyScene.
    /// </para>
    /// 
    /// </summary>
    public NetworkLobbyPlayer lobbyPlayerPrefab
    {
      get
      {
        return this.m_LobbyPlayerPrefab;
      }
      set
      {
        this.m_LobbyPlayerPrefab = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// This is the prefab of the player to be created in the PlayScene.
    /// </para>
    /// 
    /// </summary>
    public GameObject gamePlayerPrefab
    {
      get
      {
        return this.m_GamePlayerPrefab;
      }
      set
      {
        this.m_GamePlayerPrefab = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The scene to use for the lobby. This is similar to the offlineScene of the NetworkManager.
    /// </para>
    /// 
    /// </summary>
    public string lobbyScene
    {
      get
      {
        return this.m_LobbyScene;
      }
      set
      {
        this.m_LobbyScene = value;
        this.offlineScene = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The scene to use for the playing the game from the lobby. This is similar to the onlineScene of the NetworkManager.
    /// </para>
    /// 
    /// </summary>
    public string playScene
    {
      get
      {
        return this.m_PlayScene;
      }
      set
      {
        this.m_PlayScene = value;
      }
    }


    private void OnValidate()
    {
      if (this.maxPlayersPerConnection <= this.maxPlayers)
        return;

      this.maxPlayersPerConnection = this.maxPlayers;
    }


    private byte FindSlot()
    {
      for (byte index = (byte) 0; (int) index < this.maxPlayers; ++index)
      {
        if ((UnityEngine.Object) this.lobbySlots[(int) index] == (UnityEngine.Object) null)
          return index;
      }

      return byte.MaxValue;
    }


    private void SceneLoadedForPlayer(NetworkConnection conn, GameObject lobbyPlayerGameObject)
    {

      // if the the gameobject that is supposed to be used for game play doesnt have a LobbyPlayer attached return
      if ((UnityEngine.Object) lobbyPlayerGameObject.GetComponent<NetworkLobbyPlayer>() == (UnityEngine.Object) null)
        return;

      if (LogFilter.logDebug)
      {
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "NetworkLobby SceneLoadedForPlayer scene:";
        objArray[index1] = (object) str1;
        int index2 = 1;
        string loadedLevelName = Application.loadedLevelName;
        objArray[index2] = (object) loadedLevelName;
        int index3 = 2;
        string str2 = " ";
        objArray[index3] = (object) str2;
        int index4 = 3;
        NetworkConnection networkConnection = conn;
        objArray[index4] = (object) networkConnection;
        Debug.Log((object) string.Concat(objArray));
      }


      if (Application.loadedLevelName == this.m_LobbyScene)
      {
        NetworkLobbyManager.PendingPlayer pendingPlayer;
        pendingPlayer.conn = conn;
        pendingPlayer.lobbyPlayer = lobbyPlayerGameObject;
        this.pendingPlayers.Add(pendingPlayer);
      }
      else // if we are not in the lobby scene then we are setting up for game scene
      {
        short playerControllerId = lobbyPlayerGameObject.GetComponent<NetworkIdentity>().playerControllerId;

        // docs say this virtual function enables you to change the lobby player
        GameObject gameObject = this.OnLobbyServerCreateGamePlayer(conn, playerControllerId);

        // if we did not create a new player in the OnLobbyServerCreateGamePlayer it will return null
        if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
        {
          Transform startPosition = this.GetStartPosition();
          gameObject = !((UnityEngine.Object) startPosition != (UnityEngine.Object) null) ? (GameObject) UnityEngine.Object.Instantiate((UnityEngine.Object) this.gamePlayerPrefab, Vector3.zero, Quaternion.identity) : (GameObject) UnityEngine.Object.Instantiate((UnityEngine.Object) this.gamePlayerPrefab, startPosition.position, startPosition.rotation);
        }


        // OnLobbyServerSceneLoadedForPlayer is an empty server virtual method we can override to add lobby player state/data to a game player object
        // once loaded into the game scene, if it returns false we dont proceed to ReplacePlayerForConnection
        // default value is to return true
        if (!this.OnLobbyServerSceneLoadedForPlayer(lobbyPlayerGameObject, gameObject))
          return;

        NetworkServer.ReplacePlayerForConnection(conn, gameObject, playerControllerId);

      }
    }


    // method for checking if a connection is ready, a connection can have more than one player associated with it, think split screen
    // internal method used by the public api CheckReadyToBegin()
    private bool CheckConnnectionIsReadyToBegin(NetworkConnection conn)
    {

      using (List<PlayerController>.Enumerator enumerator = conn.playerControllers.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          PlayerController current = enumerator.Current;
          if (current.IsValid && !current.gameObject.GetComponent<NetworkLobbyPlayer>().readyToBegin)
            return false;
        }
      }

      return true;
    }

    /// <summary>
    /// 
    /// <para>
    /// CheckReadyToBegin checks all of the players in the lobby to see if their readyToBegin flag is set.
    /// Called automatically in response to NetworkLobbyPlayer.SendReadyToBeginMessage()
    /// </para>
    /// 
    /// </summary>
    public void CheckReadyToBegin()
    {
      if (Application.loadedLevelName != this.m_LobbyScene) // make sure we are still in the lobby before anything
        return;

     // iterate over each connection the server currently holds
      using (List<NetworkConnection>.Enumerator enumerator = NetworkServer.connections.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          NetworkConnection current = enumerator.Current;
          if (current != null && !this.CheckConnnectionIsReadyToBegin(current))
            return;
        }
      }

    // check also the ready state of the local client/s
      using (List<NetworkConnection>.Enumerator enumerator = NetworkServer.localConnections.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          NetworkConnection current = enumerator.Current;
          if (current != null && !this.CheckConnnectionIsReadyToBegin(current))
            return;
        }
      }
       // if all are ready then there is no more pending, clear out the pending list and load the game scene for play
      this.pendingPlayers.Clear();
      this.ServerChangeScene(this.m_PlayScene);
    }

    /// <summary>
    /// 
    /// <para>
    /// Calling this causes the server to switch back to the lobby scene.
    /// </para>
    /// 
    /// </summary>
    public void ServerReturnToLobby()
    {
      if (!NetworkServer.active)
        Debug.Log((object) "ServerReturnToLobby called on client");
      else
        this.ServerChangeScene(this.m_LobbyScene);
    }



    private void CallOnClientEnterLobby()
    {
      this.OnLobbyClientEnter(); // Call to virtual method to run any server hooks for when a player enters the lobby

      foreach (NetworkLobbyPlayer networkLobbyPlayer in this.lobbySlots)
      {
        if (!((UnityEngine.Object) networkLobbyPlayer == (UnityEngine.Object) null))
        {
            // set each player as not ready
          networkLobbyPlayer.readyToBegin = false;
            // then call into the virtual client hook method of each lobby player for any implementation there
          networkLobbyPlayer.OnClientEnterLobby();
        }
      }
    }



    private void CallOnClientExitLobby()
    {
      this.OnLobbyClientExit(); // call to virtual hooks on server for when a player leaves the lobby

      foreach (NetworkLobbyPlayer networkLobbyPlayer in this.lobbySlots)
      {
        if (!((UnityEngine.Object) networkLobbyPlayer == (UnityEngine.Object) null))
            // call into the virtual client hook that may be implemented for when a lobby player leaves
          networkLobbyPlayer.OnClientExitLobby();
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Sends a message to the server to make the game return to the lobby scene.
    /// </para>
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// 
    /// <para>
    /// True if message was sent.
    /// </para>
    /// 
    /// </returns>
    public bool SendReturnToLobby() // I guess it uses its own client property to tell itself to switch back to lobby.. why would player have access
    {
      if (this.client == null || !this.client.isConnected)
        return false;

      this.client.Send((short) 46, (MessageBase) new EmptyMessage());
      return true;
    }


     // override when a new client connects to the server
    public override void OnServerConnect(NetworkConnection conn)
    {
      if (this.numPlayers >= this.maxPlayers)
        conn.Disconnect();
      else if (Application.loadedLevelName != this.m_LobbyScene) // this will prevent players from joining after game is already started
      {
        conn.Disconnect();
      }
      else // otherwise we continue the process
      {
        base.OnServerConnect(conn);
        this.OnLobbyServerConnect(conn);
      }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
      base.OnServerDisconnect(conn);
      this.OnLobbyServerDisconnect(conn);
    }


    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
      if (Application.loadedLevelName != this.m_LobbyScene)
        return;

        // see how many players are playing from a connection
      int num = 0;
      using (List<PlayerController>.Enumerator enumerator = conn.playerControllers.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          if (enumerator.Current.IsValid)
            ++num;
        }
      }

        // default value is one player per connection
      if (num >= this.maxPlayersPerConnection)
      {
        if (LogFilter.logWarn)
          Debug.LogWarning((object) "NetworkLobbyManager no more players for this connection.");
        EmptyMessage emptyMessage = new EmptyMessage();
        conn.Send((short) 45, (MessageBase) emptyMessage);
      }
      else // if the connection meets the value set for maxPlayersPerConnection then attempt to fill a lobby slot with that player
      {
        byte slot = this.FindSlot();
        if ((int) slot == (int) byte.MaxValue) // then all lobby slots are full, no more room in game
        {
          if (LogFilter.logWarn)
            Debug.LogWarning((object) "NetworkLobbyManager no space for more players");
          EmptyMessage emptyMessage = new EmptyMessage();
          conn.Send((short) 45, (MessageBase) emptyMessage);
        }
        else // set up the lobby player to be spawned into the lobby scene
        {
          GameObject player = this.OnLobbyServerCreateLobbyPlayer(conn, playerControllerId);

          if ((UnityEngine.Object) player == (UnityEngine.Object) null)
            player = (GameObject) UnityEngine.Object.Instantiate((UnityEngine.Object) this.lobbyPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);

          NetworkLobbyPlayer component = player.GetComponent<NetworkLobbyPlayer>();
          component.slot = slot;
          this.lobbySlots[(int) slot] = component;
          NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        }
      }
    }


    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
      short playerControllerId = player.playerControllerId;
      this.lobbySlots[(int) player.gameObject.GetComponent<NetworkLobbyPlayer>().slot] = (NetworkLobbyPlayer) null;
      base.OnServerRemovePlayer(conn, player);
      foreach (NetworkLobbyPlayer networkLobbyPlayer in this.lobbySlots)
      {
        if ((UnityEngine.Object) networkLobbyPlayer != (UnityEngine.Object) null)
        {
          networkLobbyPlayer.GetComponent<NetworkLobbyPlayer>().readyToBegin = false;
          NetworkLobbyManager.s_LobbyReadyToBeginMessage.slotId = networkLobbyPlayer.slot;
          NetworkLobbyManager.s_LobbyReadyToBeginMessage.readyState = false;
          NetworkServer.SendToReady((GameObject) null, (short) 43, (MessageBase) NetworkLobbyManager.s_LobbyReadyToBeginMessage);
        }
      }
      this.OnLobbyServerPlayerRemoved(conn, playerControllerId);
    }


    public override void ServerChangeScene(string sceneName)
    {
      if (sceneName == this.m_LobbyScene)
      {
        foreach (NetworkLobbyPlayer networkLobbyPlayer in this.lobbySlots)
        {
          if (!((UnityEngine.Object) networkLobbyPlayer == (UnityEngine.Object) null))
          {
            NetworkIdentity component = networkLobbyPlayer.GetComponent<NetworkIdentity>();
            PlayerController playerController;
            if (component.connectionToClient.GetPlayerController(component.playerControllerId, out playerController))
              NetworkServer.Destroy(playerController.gameObject);
            networkLobbyPlayer.GetComponent<NetworkLobbyPlayer>().readyToBegin = false;
            NetworkServer.ReplacePlayerForConnection(component.connectionToClient, networkLobbyPlayer.gameObject, component.playerControllerId);
          }
        }
      }
      base.ServerChangeScene(sceneName);
    }


    public override void OnServerSceneChanged(string sceneName)
    {
      if (sceneName != this.m_LobbyScene)
      {
        using (List<NetworkLobbyManager.PendingPlayer>.Enumerator enumerator = this.pendingPlayers.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            NetworkLobbyManager.PendingPlayer current = enumerator.Current;
            this.SceneLoadedForPlayer(current.conn, current.lobbyPlayer);
          }
        }
        this.pendingPlayers.Clear();
      }
      this.OnLobbyServerSceneChanged(sceneName);
    }


    // callback registered for when a client send the ready to begin message to server, once all are ready the game scene loads
    private void OnServerReadyToBeginMessage(NetworkMessage netMsg)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkLobbyManager OnServerReadyToBeginMessage");

      netMsg.ReadMessage<IntegerMessage>(NetworkLobbyManager.s_ReadyToBeginMessage);
      PlayerController playerController;

      if (!netMsg.conn.GetPlayerController((short) NetworkLobbyManager.s_ReadyToBeginMessage.value, out playerController))
      {
        if (!LogFilter.logError)
          return;
        Debug.LogError((object) ("NetworkLobbyManager OnServerReadyToBeginMessage invalid playerControllerId " + (object) NetworkLobbyManager.s_ReadyToBeginMessage.value));
      }
      else
      {
        NetworkLobbyPlayer component = playerController.gameObject.GetComponent<NetworkLobbyPlayer>();
        component.readyToBegin = true;
        // notify each player of the current ready state of the lobby, including the player who sent the ready message
        // server should be responsible for the set the ready value of each client after ready mesage has been received
        NetworkServer.SendToReady((GameObject) null, (short) 43, (MessageBase) new LobbyReadyToBeginMessage()
        {
          slotId = component.slot,
          readyState = true
        });

        this.CheckReadyToBegin();
      }
    }


    // internal handler when server receives message that a player had loaded the game scene
    private void OnServerSceneLoadedMessage(NetworkMessage netMsg)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkLobbyManager OnSceneLoadedMessage");

      netMsg.ReadMessage<IntegerMessage>(NetworkLobbyManager.s_SceneLoadedMessage);
      PlayerController playerController;


      if (!netMsg.conn.GetPlayerController((short) NetworkLobbyManager.s_SceneLoadedMessage.value, out playerController))
      {
        if (!LogFilter.logError)
          return;
        Debug.LogError((object) ("NetworkLobbyManager OnServerSceneLoadedMessage invalid playerControllerId " + (object) NetworkLobbyManager.s_SceneLoadedMessage.value));
      }
      else
        this.SceneLoadedForPlayer(netMsg.conn, playerController.gameObject);
    }


    private void OnServerReturnToLobbyMessage(NetworkMessage netMsg)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkLobbyManager OnServerReturnToLobbyMessage");

      this.ServerReturnToLobby();
    }


    public override void OnStartServer()
    {
      if (this.lobbySlots.Length == 0)
        this.lobbySlots = new NetworkLobbyPlayer[this.maxPlayers];

      NetworkServer.RegisterHandler((short) 43, new NetworkMessageDelegate(this.OnServerReadyToBeginMessage));
      NetworkServer.RegisterHandler((short) 44, new NetworkMessageDelegate(this.OnServerSceneLoadedMessage));
      NetworkServer.RegisterHandler((short) 46, new NetworkMessageDelegate(this.OnServerReturnToLobbyMessage));
      this.OnLobbyStartServer();
    }


    public override void OnStartHost()
    {
      this.OnLobbyStartHost();
    }

    public override void OnStopHost()
    {
      this.OnLobbyStopHost();
    }


    public override void OnStartClient(NetworkClient client)
    {
      if (this.lobbySlots.Length == 0)
        this.lobbySlots = new NetworkLobbyPlayer[this.maxPlayers];

      if ((UnityEngine.Object) this.m_LobbyPlayerPrefab == (UnityEngine.Object) null || (UnityEngine.Object) this.m_LobbyPlayerPrefab.gameObject == (UnityEngine.Object) null)
      {
        if (LogFilter.logError)
          Debug.LogError((object) "NetworkLobbyManager no LobbyPlayer prefab is registered. Please add a LobbyPlayer prefab.");
      }
      else
        ClientScene.RegisterPrefab(this.m_LobbyPlayerPrefab.gameObject);

      if ((UnityEngine.Object) this.m_GamePlayerPrefab == (UnityEngine.Object) null)
      {
        if (LogFilter.logError)
          Debug.LogError((object) "NetworkLobbyManager no GamePlayer prefab is registered. Please add a GamePlayer prefab.");
      }
      else
        ClientScene.RegisterPrefab(this.m_GamePlayerPrefab);

      client.RegisterHandler((short) 43, new NetworkMessageDelegate(this.OnClientReadyToBegin));
      client.RegisterHandler((short) 45, new NetworkMessageDelegate(this.OnClientAddPlayerFailedMessage));
      this.OnLobbyStartClient(client);
    }


    public override void OnClientConnect(NetworkConnection conn)
    {
      this.OnLobbyClientConnect(conn);
      this.CallOnClientEnterLobby();
      base.OnClientConnect(conn);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
      this.OnLobbyClientDisconnect(conn);
      base.OnClientDisconnect(conn);
    }

    public override void OnStopClient()
    {
      this.OnLobbyStopClient();
      this.CallOnClientExitLobby();
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
      if (Application.loadedLevelName == this.lobbyScene)
      {
        if (this.client.isConnected)
          this.CallOnClientEnterLobby();
      }
      else
        this.CallOnClientExitLobby();

      base.OnClientSceneChanged(conn);
      this.OnLobbyClientSceneChanged(conn);
    }

    private void OnClientReadyToBegin(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<LobbyReadyToBeginMessage>(NetworkLobbyManager.s_LobbyReadyToBeginMessage);
      if ((int) NetworkLobbyManager.s_LobbyReadyToBeginMessage.slotId >= Enumerable.Count<NetworkLobbyPlayer>((IEnumerable<NetworkLobbyPlayer>) this.lobbySlots))
      {
        if (!LogFilter.logError)
          return;
        Debug.LogError((object) ("NetworkLobbyManager OnClientReadyToBegin invalid lobby slot " + (object) NetworkLobbyManager.s_LobbyReadyToBeginMessage.slotId));
      }
      else
      {
        NetworkLobbyPlayer networkLobbyPlayer = this.lobbySlots[(int) NetworkLobbyManager.s_LobbyReadyToBeginMessage.slotId];
        if ((UnityEngine.Object) networkLobbyPlayer == (UnityEngine.Object) null || (UnityEngine.Object) networkLobbyPlayer.gameObject == (UnityEngine.Object) null)
        {
          if (!LogFilter.logError)
            return;
          Debug.LogError((object) ("NetworkLobbyManager OnClientReadyToBegin no player at lobby slot " + (object) NetworkLobbyManager.s_LobbyReadyToBeginMessage.slotId));
        }
        else
        {
          networkLobbyPlayer.readyToBegin = NetworkLobbyManager.s_LobbyReadyToBeginMessage.readyState;
          networkLobbyPlayer.OnClientReady(NetworkLobbyManager.s_LobbyReadyToBeginMessage.readyState);
        }
      }
    }


    private void OnClientAddPlayerFailedMessage(NetworkMessage netMsg)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkLobbyManager Add Player failed.");
      this.OnLobbyClientAddPlayerFailed();
    }

    /// <summary>
    /// 
    /// <para>
    /// This is called on the host when a host is started.
    /// </para>
    /// 
    /// </summary>
    public virtual void OnLobbyStartHost()
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This is called on the host when the host is stopped.
    /// </para>
    /// 
    /// </summary>
    public virtual void OnLobbyStopHost()
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This is called on the server when the server is started - including when a host is started.
    /// </para>
    /// 
    /// </summary>
    public virtual void OnLobbyStartServer()
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This is called on the server when a new client connects to the server.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">The new connection.</param>
    public virtual void OnLobbyServerConnect(NetworkConnection conn)
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This is called on the server when a client disconnects.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">The connection that disconnected.</param>
    public virtual void OnLobbyServerDisconnect(NetworkConnection conn)
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This is called on the server when a networked scene finishes loading.
    /// </para>
    /// 
    /// </summary>
    /// <param name="sceneName">Name of the new scene.</param>
    public virtual void OnLobbyServerSceneChanged(string sceneName)
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This allows customization of the creation of the lobby-player object on the server.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param><param name="playerControllerId">The controllerId of the player.</param>
    /// <returns>
    /// 
    /// <para>
    /// The new lobby-player object.
    /// </para>
    /// 
    /// </returns>
    public virtual GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
    {
      return (GameObject) null;
    }

    /// <summary>
    /// 
    /// <para>
    /// This allows customization of the creation of the GamePlayer object on the server.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param><param name="playerControllerId">The controllerId of the player on the connnection.</param>
    /// <returns>
    /// 
    /// <para>
    /// A new GamePlayer object.
    /// </para>
    /// 
    /// </returns>
    public virtual GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
    {
      return (GameObject) null;
    }

    /// <summary>
    /// 
    /// <para>
    /// This is called on the server when a player is removed.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn"/><param name="playerControllerId"/>
    public virtual void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This is called on the server when it is told that a client has finished switching from the lobby scene to a game player scene.
    /// </para>
    /// 
    /// </summary>
    /// <param name="lobbyPlayer">The lobby player object.</param><param name="gamePlayer">The game player object.</param>
    /// <returns>
    /// 
    /// <para>
    /// False to not allow this player to replace the lobby player.
    /// </para>
    /// 
    /// </returns>
    public virtual bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
    {
      return true;
    }

    /// <summary>
    /// 
    /// <para>
    /// This is a hook to allow custom behaviour when the game client enters the lobby.
    /// </para>
    /// 
    /// </summary>
    public virtual void OnLobbyClientEnter()
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This is a hook to allow custom behaviour when the game client exits the lobby.
    /// </para>
    /// 
    /// </summary>
    public virtual void OnLobbyClientExit()
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This is called on the client when it connects to server.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">The connection that connected.</param>
    public virtual void OnLobbyClientConnect(NetworkConnection conn)
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This is called on the client when disconnected from a server.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">The connection that disconnected.</param>
    public virtual void OnLobbyClientDisconnect(NetworkConnection conn)
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This is called on the client when a client is started.
    /// </para>
    /// 
    /// </summary>
    /// <param name="client"/>
    public virtual void OnLobbyStartClient(NetworkClient client)
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This is called on the client when the client stops.
    /// </para>
    /// 
    /// </summary>
    public virtual void OnLobbyStopClient()
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This is called on the client when the client is finished loading a new networked scene.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn"/>
    public virtual void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// Called on the client when adding a player to the lobby fails.
    /// </para>
    /// 
    /// </summary>
    public virtual void OnLobbyClientAddPlayerFailed()
    {
    }

    private void OnGUI()
    {
      if (!this.showLobbyGUI || Application.loadedLevelName != this.m_LobbyScene)
        return;

      GUI.Box(new Rect(90f, 180f, 500f, 150f), "Players:");
        
      if (!NetworkClient.active || !GUI.Button(new Rect(100f, 300f, 120f, 20f), "Add Player"))
        return;

      this.TryToAddPlayer();
    }

    /// <summary>
    /// 
    /// <para>
    /// This is used on clients to attempt to add a player to the game.
    /// </para>
    /// 
    /// </summary>
    public void TryToAddPlayer()
    {
      if (NetworkClient.active)
      {
        short playerControllerId = (short) -1;
        List<PlayerController> playerControllers = NetworkClient.allClients[0].connection.playerControllers;
        if (playerControllers.Count < this.maxPlayers)
        {
          playerControllerId = (short) playerControllers.Count;
        }
        else
        {
          for (short index = (short) 0; (int) index < this.maxPlayers; ++index)
          {
            if (!playerControllers[(int) index].IsValid)
            {
              playerControllerId = index;
              break;
            }
          }
        }
        if (LogFilter.logDebug)
        {
          object[] objArray = new object[4];
          int index1 = 0;
          string str1 = "NetworkLobbyManager TryToAddPlayer controllerId ";
          objArray[index1] = (object) str1;
          int index2 = 1;
          // ISSUE: variable of a boxed type
          __Boxed<short> local1 = (ValueType) playerControllerId;
          objArray[index2] = (object) local1;
          int index3 = 2;
          string str2 = " ready:";
          objArray[index3] = (object) str2;
          int index4 = 3;
          // ISSUE: variable of a boxed type
          __Boxed<bool> local2 = (ValueType) (bool) (ClientScene.ready ? 1 : 0);
          objArray[index4] = (object) local2;
          Debug.Log((object) string.Concat(objArray));
        }
        if ((int) playerControllerId == -1)
        {
          if (!LogFilter.logDebug)
            return;
          Debug.Log((object) "NetworkLobbyManager No Space!");
        }
        else if (ClientScene.ready)
          ClientScene.AddPlayer(playerControllerId);
        else
          ClientScene.AddPlayer(NetworkClient.allClients[0].connection, playerControllerId);
      }
      else
      {
        if (!LogFilter.logDebug)
          return;
        Debug.Log((object) "NetworkLobbyManager NetworkClient not active!");
      }
    }

    private struct PendingPlayer
    {
      public NetworkConnection conn;
      public GameObject lobbyPlayer;
    }
  }
}
