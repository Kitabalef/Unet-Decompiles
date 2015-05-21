// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkManager
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Types;

namespace UnityEngine.Networking
{
  /// <summary>
  /// 
  /// <para>
  /// The NetworkManager is a convenience class for the HLAPI for managing networking systems.
  /// </para>
  /// 
  /// </summary>
  [AddComponentMenu("Network/NetworkManager")]
  public class NetworkManager : MonoBehaviour
  {
    /// <summary>
    /// 
    /// <para>
    /// The name of the current network scene.
    /// </para>
    /// 
    /// </summary>
    public static string networkSceneName = string.Empty;
    private static List<Transform> s_StartPositions = new List<Transform>();
    private static int s_StartPositionIndex = 0;
    private static AddPlayerMessage s_AddPlayerMessage = new AddPlayerMessage();
    private static RemovePlayerMessage s_RemovePlayerMessage = new RemovePlayerMessage();
    private static ErrorMessage s_ErrorMessage = new ErrorMessage();
    private static AsyncOperation s_LoadingSceneAsync = (AsyncOperation) null;
    [SerializeField]
    private int m_NetworkPort = 7777;
    [SerializeField]
    private string m_NetworkAddress = "localhost";
    [SerializeField]
    private bool m_DontDestroyOnLoad = true;
    [SerializeField]
    private bool m_RunInBackground = true;
    [SerializeField]
    private float m_MaxDelay = 0.01f;
    [SerializeField]
    private LogFilter.FilterLevel m_LogLevel = LogFilter.FilterLevel.Info;
    [SerializeField]
    private bool m_AutoCreatePlayer = true;
    [SerializeField]
    private string m_OfflineScene = string.Empty;
    [SerializeField]
    private string m_OnlineScene = string.Empty;
    [SerializeField]
    private List<GameObject> m_SpawnPrefabs = new List<GameObject>();
    [SerializeField]
    private int m_MaxConnections = 4;
    [SerializeField]
    private List<QosType> m_Channels = new List<QosType>();
    [SerializeField]
    private int m_SimulatedLatency = 1;
    [SerializeField]
    private string m_MatchHost = "mm.unet.unity3d.com";
    [SerializeField]
    private int m_MatchPort = 443;
    /// <summary>
    /// 
    /// <para>
    /// The name of the current match.
    /// </para>
    /// 
    /// </summary>
    public string matchName = "default";
    /// <summary>
    /// 
    /// <para>
    /// The maximum number of players in the current match.
    /// </para>
    /// 
    /// </summary>
    public uint matchSize = 4U;
    [SerializeField]
    private bool m_SendPeerInfo;
    [SerializeField]
    private GameObject m_PlayerPrefab;
    [SerializeField]
    private PlayerSpawnMethod m_PlayerSpawnMethod;
    [SerializeField]
    private bool m_CustomConfig;
    [SerializeField]
    private ConnectionConfig m_ConnectionConfig;
    [SerializeField]
    private bool m_UseSimulator;
    [SerializeField]
    private float m_PacketLossPercentage;
    private EndPoint m_EndPoint;
    /// <summary>
    /// 
    /// <para>
    /// True if the NetworkServer or NetworkClient isactive.
    /// </para>
    /// 
    /// </summary>
    public bool isNetworkActive;
    /// <summary>
    /// 
    /// <para>
    /// The current NetworkClient being used by the manager.
    /// </para>
    /// 
    /// </summary>
    public NetworkClient client;
    /// <summary>
    /// 
    /// <para>
    /// A MatchInfo instance that will be used when StartServer() or StartClient() are called.
    /// </para>
    /// 
    /// </summary>
    public MatchInfo matchInfo;
    /// <summary>
    /// 
    /// <para>
    /// The UMatch matchmaker object.
    /// </para>
    /// 
    /// </summary>
    public NetworkMatch matchMaker;
    /// <summary>
    /// 
    /// <para>
    /// The list of matches that are available to join.
    /// </para>
    /// 
    /// </summary>
    public List<MatchDesc> matches;
    /// <summary>
    /// 
    /// <para>
    /// The NetworkManager singleton object.
    /// </para>
    /// 
    /// </summary>
    public static NetworkManager singleton;
    private static NetworkConnection s_ClientReadyConnection;
    private static string s_address;

    /// <summary>
    /// 
    /// <para>
    /// The network port currently in use.
    /// </para>
    /// 
    /// </summary>
    public int networkPort
    {
      get
      {
        return this.m_NetworkPort;
      }
      set
      {
        this.m_NetworkPort = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The network address currently in use.
    /// </para>
    /// 
    /// </summary>
    public string networkAddress
    {
      get
      {
        return this.m_NetworkAddress;
      }
      set
      {
        this.m_NetworkAddress = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// A flag to control whether the NetworkManager object is destroyed when the scene changes.
    /// </para>
    /// 
    /// </summary>
    public bool dontDestroyOnLoad
    {
      get
      {
        return this.m_DontDestroyOnLoad;
      }
      set
      {
        this.m_DontDestroyOnLoad = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Control whether the program runs when it is in the background.
    /// </para>
    /// 
    /// </summary>
    public bool runInBackground
    {
      get
      {
        return this.m_RunInBackground;
      }
      set
      {
        this.m_RunInBackground = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// A flag to control sending the network information about every peer to all members of a match.
    /// </para>
    /// 
    /// </summary>
    public bool sendPeerInfo
    {
      get
      {
        return this.m_SendPeerInfo;
      }
      set
      {
        this.m_SendPeerInfo = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The maximum delay before sending packets on connections.
    /// </para>
    /// 
    /// </summary>
    public float maxDelay
    {
      get
      {
        return this.m_MaxDelay;
      }
      set
      {
        this.m_MaxDelay = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The log level specifically to user for network log messages.
    /// </para>
    /// 
    /// </summary>
    public LogFilter.FilterLevel logLevel
    {
      get
      {
        return this.m_LogLevel;
      }
      set
      {
        this.m_LogLevel = value;
        LogFilter.currentLogLevel = (int) value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The default prefab to be used to create player objects on the server.
    /// </para>
    /// 
    /// </summary>
    public GameObject playerPrefab
    {
      get
      {
        return this.m_PlayerPrefab;
      }
      set
      {
        this.m_PlayerPrefab = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// A flag to control whether or not player objects are automatically created on connect, and on scene change.
    /// </para>
    /// 
    /// </summary>
    public bool autoCreatePlayer
    {
      get
      {
        return this.m_AutoCreatePlayer;
      }
      set
      {
        this.m_AutoCreatePlayer = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The current method of spawning players used by the NetworkManager.
    /// </para>
    /// 
    /// </summary>
    public PlayerSpawnMethod playerSpawnMethod
    {
      get
      {
        return this.m_PlayerSpawnMethod;
      }
      set
      {
        this.m_PlayerSpawnMethod = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The scene to switch to when offline.
    /// </para>
    /// 
    /// </summary>
    public string offlineScene
    {
      get
      {
        return this.m_OfflineScene;
      }
      set
      {
        this.m_OfflineScene = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The scene to switch to when online.
    /// </para>
    /// 
    /// </summary>
    public string onlineScene
    {
      get
      {
        return this.m_OnlineScene;
      }
      set
      {
        this.m_OnlineScene = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// List of prefabs that will be registered with the spawning system.
    /// </para>
    /// 
    /// </summary>
    public List<GameObject> spawnPrefabs
    {
      get
      {
        return this.m_SpawnPrefabs;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The list of currently registered player start positions for the current scene.
    /// </para>
    /// 
    /// </summary>
    public List<Transform> startPositions
    {
      get
      {
        return NetworkManager.s_StartPositions;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Flag to enable custom network configuration.
    /// </para>
    /// 
    /// </summary>
    public bool customConfig
    {
      get
      {
        return this.m_CustomConfig;
      }
      set
      {
        this.m_CustomConfig = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The custom network configuration to use.
    /// </para>
    /// 
    /// </summary>
    public ConnectionConfig connectionConfig
    {
      get
      {
        if (this.m_ConnectionConfig == null)
          this.m_ConnectionConfig = new ConnectionConfig();
        return this.m_ConnectionConfig;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The maximum number of concurrent network connections to support.
    /// </para>
    /// 
    /// </summary>
    public int maxConnections
    {
      get
      {
        return this.m_MaxConnections;
      }
      set
      {
        this.m_MaxConnections = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The Quality-of-Service channels to use for the network transport layer.
    /// </para>
    /// 
    /// </summary>
    public List<QosType> channels
    {
      get
      {
        return this.m_Channels;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Allows you to specify an EndPoint object instead of setting networkAddress and networkPort (required for some platforms such as Xbox One).
    /// </para>
    /// 
    /// </summary>
    public EndPoint secureTunnelEndpoint
    {
      get
      {
        return this.m_EndPoint;
      }
      set
      {
        this.m_EndPoint = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Flag that control whether clients started by this NetworkManager will use simulated latency and packet loss.
    /// </para>
    /// 
    /// </summary>
    public bool useSimulator
    {
      get
      {
        return this.m_UseSimulator;
      }
      set
      {
        this.m_UseSimulator = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The delay in milliseconds to be added to incoming and outgoing packets for clients.
    /// </para>
    /// 
    /// </summary>
    public int simulatedLatency
    {
      get
      {
        return this.m_SimulatedLatency;
      }
      set
      {
        this.m_SimulatedLatency = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The percentage of incoming and outgoing packets to be dropped for clients.
    /// </para>
    /// 
    /// </summary>
    public float packetLossPercentage
    {
      get
      {
        return this.m_PacketLossPercentage;
      }
      set
      {
        this.m_PacketLossPercentage = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The hostname of the matchmaking server.
    /// </para>
    /// 
    /// </summary>
    public string matchHost
    {
      get
      {
        return this.m_MatchHost;
      }
      set
      {
        this.m_MatchHost = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The port of the matchmaking service.
    /// </para>
    /// 
    /// </summary>
    public int matchPort
    {
      get
      {
        return this.m_MatchPort;
      }
      set
      {
        this.m_MatchPort = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// NumPlayers is the number of active player objects across all connections on the server.
    /// </para>
    /// 
    /// </summary>
    public int numPlayers
    {
      get
      {
        int num = 0;
        using (List<NetworkConnection>.Enumerator enumerator1 = NetworkServer.connections.GetEnumerator())
        {
          while (enumerator1.MoveNext())
          {
            NetworkConnection current = enumerator1.Current;
            if (current != null)
            {
              using (List<PlayerController>.Enumerator enumerator2 = current.playerControllers.GetEnumerator())
              {
                while (enumerator2.MoveNext())
                {
                  if (enumerator2.Current.IsValid)
                    ++num;
                }
              }
            }
          }
        }

        // Checking here if there is more than one player connected from this client to make total player number correct
        using (List<NetworkConnection>.Enumerator enumerator1 = NetworkServer.localConnections.GetEnumerator())
        {
          while (enumerator1.MoveNext())
          {
            NetworkConnection current = enumerator1.Current;
            if (current != null)
            {
              using (List<PlayerController>.Enumerator enumerator2 = current.playerControllers.GetEnumerator())
              {
                while (enumerator2.MoveNext())
                {
                  if (enumerator2.Current.IsValid)
                    ++num;
                }
              }
            }
          }
        }
        return num;
      }
    }

    private void Awake()
    {
      LogFilter.currentLogLevel = (int) this.m_LogLevel;
      if (this.m_DontDestroyOnLoad)
      {
          // Reinforce the singelton idea here by deleting a copy if made
        if ((UnityEngine.Object) NetworkManager.singleton != (UnityEngine.Object) null)
        {
          if (LogFilter.logDebug)
            Debug.Log((object) "NetworkManager created but singleton already exists");
          UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
          return;
        }
        if (LogFilter.logDev)
          Debug.Log((object) "NetworkManager created singleton (DontDestroyOnLoad)");
        NetworkManager.singleton = this; // set the singleton field for the network game
        UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) this.gameObject); // persist the same NetManager/singleton object between scenes
      }
      else // NetManager will not persist between scenes
      {
        if (LogFilter.logDev)
          Debug.Log((object) "NetworkManager created singleton (ForScene)");
        NetworkManager.singleton = this;
      }

      
      if (this.m_NetworkAddress != string.Empty)
      {
        NetworkManager.s_address = this.m_NetworkAddress;
      }
      else
      {
        if (!(NetworkManager.s_address != string.Empty))
          return;
        this.m_NetworkAddress = NetworkManager.s_address;
      }
    }

    private void OnValidate()
    {
      if (this.m_SimulatedLatency < 1)
        this.m_SimulatedLatency = 1;
      if (this.m_SimulatedLatency > 500)
        this.m_SimulatedLatency = 500;
      if ((double) this.m_PacketLossPercentage < 0.0)
        this.m_PacketLossPercentage = 0.0f;
      if ((double) this.m_PacketLossPercentage > 99.0)
        this.m_PacketLossPercentage = 99f;
      if (!((UnityEngine.Object) this.m_PlayerPrefab != (UnityEngine.Object) null) || !((UnityEngine.Object) this.m_PlayerPrefab.GetComponent<NetworkIdentity>() == (UnityEngine.Object) null))
        return;
      if (LogFilter.logError)
        Debug.LogError((object) "NetworkManager - playerPrefab must have a NetworkIdentity.");
      this.m_PlayerPrefab = (GameObject) null;
    }

    internal void RegisterServerMessages()
    {
      NetworkServer.RegisterHandler((short) 32, new NetworkMessageDelegate(this.OnServerConnectInternal));
      NetworkServer.RegisterHandler((short) 33, new NetworkMessageDelegate(this.OnServerDisconnectInternal));
      NetworkServer.RegisterHandler((short) 35, new NetworkMessageDelegate(this.OnServerReadyMessageInternal));
      NetworkServer.RegisterHandler((short) 37, new NetworkMessageDelegate(this.OnServerAddPlayerMessageInternal));
      NetworkServer.RegisterHandler((short) 38, new NetworkMessageDelegate(this.OnServerRemovePlayerMessageInternal));
      NetworkServer.RegisterHandler((short) 34, new NetworkMessageDelegate(this.OnServerErrorInternal));
    }

    // overloads to handle different config param cases for starting a stand alone server
    #region StartServer() overloads
    public bool StartServer(ConnectionConfig config, int maxConnections)
    {
      return this.StartServer((MatchInfo) null, config, maxConnections);
    }

    /// <summary>
    /// 
    /// <para>
    /// This starts a new server.
    /// </para>
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// 
    /// <para>
    /// True is the server was started.
    /// </para>
    /// 
    /// </returns>
    public bool StartServer()
    {
      return this.StartServer((MatchInfo) null);
    }

    public bool StartServer(MatchInfo info)
    {
      return this.StartServer(info, (ConnectionConfig) null, -1);
    }

    private bool StartServer(MatchInfo info, ConnectionConfig config, int maxConnections)
    {
      this.OnStartServer();
      if (this.m_RunInBackground)
        Application.runInBackground = true;

      // Check if we have been given custom config info and configure server accordingly if so
      if (this.m_CustomConfig && this.m_ConnectionConfig != null && config == null)
      {
        this.m_ConnectionConfig.Channels.Clear();
        using (List<QosType>.Enumerator enumerator = this.m_Channels.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            int num = (int) this.m_ConnectionConfig.AddChannel(enumerator.Current);
          }
        }
        NetworkServer.Configure(this.m_ConnectionConfig, this.m_MaxConnections);
      }
     
      // register server handlers
      this.RegisterServerMessages();

      NetworkServer.sendPeerInfo = this.m_SendPeerInfo; // check if we are to send peer info also

      // if no custom config has been set and a config param was sent in then use that config
      if (config != null)
        NetworkServer.Configure(config, maxConnections);

      // check the MatchInfo param
      if (info != null)
      {
        if (!NetworkServer.Listen(info, this.m_NetworkPort))
        {
          if (LogFilter.logError)
            Debug.LogError((object) "StartServer listen failed.");
          return false;
        }
      }
      else if (!NetworkServer.Listen(this.m_NetworkPort))
      {
        if (LogFilter.logError)
          Debug.LogError((object) "StartServer listen failed.");
        return false;
      }


      if (LogFilter.logDebug)
        Debug.Log((object) ("NetworkManager StartServer port:" + (object) this.m_NetworkPort));

      this.isNetworkActive = true; // set the flag indicating this object is network active
      // transition to the online scene if its name has been set(must be included in build settings before being allowed to be set)
      if (this.m_OnlineScene != string.Empty && this.m_OnlineScene != Application.loadedLevelName && this.m_OnlineScene != this.m_OfflineScene)
        this.ServerChangeScene(this.m_OnlineScene);
      else
        NetworkServer.SpawnObjects();

      return true;
    }
    #endregion


    // This provided NetManager accounts for a client, server, or host. Register handlers for a client here using the NetworkClient field
    internal void RegisterClientMessages(NetworkClient client)
    {
      client.RegisterHandler((short) 32, new NetworkMessageDelegate(this.OnClientConnectInternal));
      client.RegisterHandler((short) 33, new NetworkMessageDelegate(this.OnClientDisconnectInternal));
      client.RegisterHandler((short) 36, new NetworkMessageDelegate(this.OnClientNotReadyMessageInternal));
      client.RegisterHandler((short) 34, new NetworkMessageDelegate(this.OnClientErrorInternal));
      client.RegisterHandler((short) 39, new NetworkMessageDelegate(this.OnClientSceneInternal));

      // handle the registering of the clients prefab also so it doesnt need to be done manually from separate client code
      if ((UnityEngine.Object) this.m_PlayerPrefab != (UnityEngine.Object) null)
        ClientScene.RegisterPrefab(this.m_PlayerPrefab);
      
      // Also register any prefabs added in the inspector so they are included in the game for the client, no manual client code also needed
      using (List<GameObject>.Enumerator enumerator = this.m_SpawnPrefabs.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          GameObject current = enumerator.Current;
          if ((UnityEngine.Object) current != (UnityEngine.Object) null)
            ClientScene.RegisterPrefab(current);
        }
      }
    }

    // overloads to handle different config param cases for connecting as a Client
    #region StartClient() overloads
    // Connections methods return a NetworkClient object, this method connects according to the endpoint being used
    public NetworkClient StartClient(MatchInfo info, ConnectionConfig config)
    {
      this.matchInfo = info;

      if (this.m_RunInBackground)
        Application.runInBackground = true;

      this.isNetworkActive = true;
      this.client = new NetworkClient();
      this.OnStartClient(this.client);

      // if not using a custom config
      if (config != null)
        this.client.Configure(config, 1);
      else if (this.m_CustomConfig && this.m_ConnectionConfig != null) // else using a custom config setup for our clients
      {
        this.m_ConnectionConfig.Channels.Clear();
        using (List<QosType>.Enumerator enumerator = this.m_Channels.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            int num = (int) this.m_ConnectionConfig.AddChannel(enumerator.Current);
          }
        }
        this.client.Configure(this.m_ConnectionConfig, this.m_MaxConnections);
      }

      this.RegisterClientMessages(this.client); // register client handlers

      // if a match was sent in then connect to that game
      if (this.matchInfo != null)
      {
        if (LogFilter.logDebug)
          Debug.Log((object) ("NetworkManager StartClient match: " + (object) this.matchInfo));
        this.client.Connect(this.matchInfo);
      }
      else if (this.m_EndPoint != null) // if we are connecting to an endpoint used by something like XboxOne, then connect there
      {
        if (LogFilter.logDebug)
          Debug.Log((object) "NetworkManager StartClient using provided SecureTunnel");
        this.client.Connect(this.m_EndPoint);
      }
      else // we are going to local connect otherwise if address has been set
      {
        if (string.IsNullOrEmpty(this.m_NetworkAddress))
        {
          if (LogFilter.logError)
            Debug.LogError((object) "Must set the Network Address field in the manager");
          return (NetworkClient) null;
        }
        if (LogFilter.logDebug)
        {
          object[] objArray = new object[4];
          int index1 = 0;
          string str1 = "NetworkManager StartClient address:";
          objArray[index1] = (object) str1;
          int index2 = 1;
          string str2 = this.m_NetworkAddress;
          objArray[index2] = (object) str2;
          int index3 = 2;
          string str3 = " port:";
          objArray[index3] = (object) str3;
          int index4 = 3;
          // ISSUE: variable of a boxed type
          __Boxed<int> local = (ValueType) this.m_NetworkPort;
          objArray[index4] = (object) local;
          Debug.Log((object) string.Concat(objArray));
        }

        // if using simulator connect that way, otherwise just complete the local network connect
        if (this.m_UseSimulator)
          this.client.ConnectWithSimulator(this.m_NetworkAddress, this.m_NetworkPort, this.m_SimulatedLatency, this.m_PacketLossPercentage);
        else
          this.client.Connect(this.m_NetworkAddress, this.m_NetworkPort);
      }

      NetworkManager.s_address = this.m_NetworkAddress;

      return this.client;
    }

    // StartClient overload that passes info on to other overload
    public NetworkClient StartClient(MatchInfo matchInfo)
    {
      return this.StartClient(matchInfo, (ConnectionConfig) null);
    }

    /// <summary>
    /// 
    /// <para>
    /// This starts a network client. It uses the networkAddress and networkPort properties as the address to connect to.
    /// </para>
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// 
    /// <para>
    /// The client object created.
    /// </para>
    /// 
    /// </returns>
    public NetworkClient StartClient()
    {
      return this.StartClient((MatchInfo) null, (ConnectionConfig) null);
    }
    #endregion

    // overloads to handle different config param cases for starting a Host
    #region StartHost() overloads
    public virtual NetworkClient StartHost(ConnectionConfig config, int maxConnections)
    {
      this.OnStartHost();
      if (!this.StartServer(config, maxConnections))
        return (NetworkClient) null;
      NetworkClient client = this.ConnectLocalClient();
      this.OnStartClient(client);
      return client;
    }

    public virtual NetworkClient StartHost(MatchInfo info)
    {
      this.OnStartHost();
      this.matchInfo = info;
      if (!this.StartServer(info))
        return (NetworkClient) null;
      NetworkClient client = this.ConnectLocalClient();
      this.OnStartClient(client);
      return client;
    }

    /// <summary>
    /// 
    /// <para>
    /// This starts a network "host" - a server and client in the same application.
    /// </para>
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// 
    /// <para>
    /// The client object created - this is a "local client".
    /// </para>
    /// 
    /// </returns>
    public virtual NetworkClient StartHost()
    {
      this.OnStartHost();
      if (!this.StartServer())
        return (NetworkClient) null;
      NetworkClient client = this.ConnectLocalClient();
      this.OnStartClient(client);
      return client;
    }


    private NetworkClient ConnectLocalClient()
    {
      if (LogFilter.logDebug)
        Debug.Log((object) ("NetworkManager StartHost port:" + (object) this.m_NetworkPort));
      this.m_NetworkAddress = "localhost";
      this.client = ClientScene.ConnectLocalServer();
      this.RegisterClientMessages(this.client);
      return this.client;
    }
    #endregion

    #region HLAPI methods from shutdown of Server, Client or Host
    /// <summary>
    /// 
    /// <para>
    /// This stops both the client and the server that the manager is using.
    /// </para>
    /// 
    /// </summary>
    public void StopHost()
    {
      this.OnStopHost();
      this.StopServer();
      this.StopClient();
    }

    /// <summary>
    /// 
    /// <para>
    /// Stops the server that the manager is using.
    /// </para>
    /// 
    /// </summary>
    public void StopServer()
    {
      if (!NetworkServer.active)
        return;
      this.OnStopServer();
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkManager StopServer");
      this.isNetworkActive = false;
      NetworkServer.Shutdown();
      this.StopMatchMaker();
      if (!(this.m_OfflineScene != string.Empty))
        return;
      this.ServerChangeScene(this.m_OfflineScene);
    }

    /// <summary>
    /// 
    /// <para>
    /// Stops the client that the manager is using.
    /// </para>
    /// 
    /// </summary>
    public void StopClient()
    {
      this.OnStopClient();

      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkManager StopClient");

      this.isNetworkActive = false;

      if (this.client != null)
      {
        this.client.Disconnect();
        this.client.Shutdown();
        this.client = (NetworkClient) null;
      }

      this.StopMatchMaker();

      ClientScene.DestroyAllClientObjects(); // cleans up all network objects on this clients game, not just this clients objects

      if (this.m_OfflineScene != string.Empty)
        this.ClientChangeScene(this.m_OfflineScene, false);

      NetworkClient.ShutdownAll();
    }

    #endregion



    public virtual void ServerChangeScene(string newSceneName)
    {
      if (string.IsNullOrEmpty(newSceneName))
      {
        if (!LogFilter.logError)
          return;
        Debug.LogError((object) "ServerChangeScene empty scene name");
      }
      else
      {
        if (LogFilter.logDebug)
          Debug.Log((object) ("ServerChangeScene " + newSceneName));
        // End sending state updates to all clients, clients ready themselves back up once new scene is loaded
        NetworkServer.SetAllClientsNotReady(); 
        NetworkManager.networkSceneName = newSceneName; // change current scene name
        NetworkManager.s_LoadingSceneAsync = Application.LoadLevelAsync(newSceneName); // scene currently being loaded
        // signal all clients about the scene change so callbacks are handled accordingly to change scene
        NetworkServer.SendToAll((short) 39, (MessageBase) new StringMessage(NetworkManager.networkSceneName));
        NetworkManager.s_StartPositionIndex = 0;
        NetworkManager.s_StartPositions.Clear();
      }
    }


    internal void ClientChangeScene(string newSceneName, bool forceReload)
    {
      if (string.IsNullOrEmpty(newSceneName))
      {
        if (!LogFilter.logError)
          return;
        Debug.LogError((object) "ClientChangeScene empty scene name");
      }
      else
      {
        if (LogFilter.logDebug)
          Debug.Log((object) ("ClientChangeScene newSceneName:" + newSceneName + " networkSceneName:" + NetworkManager.networkSceneName));
        if (newSceneName == NetworkManager.networkSceneName && !forceReload)
          return;
        NetworkManager.s_LoadingSceneAsync = Application.LoadLevelAsync(newSceneName);
        NetworkManager.networkSceneName = newSceneName;
      }
    }

    // used in Update to complete loading of a scene 
    private void FinishLoadScene()
    {
      if (this.client == null)
      {
        if (LogFilter.logDebug)
          Debug.LogWarning((object) "FinishLoadScene client is null");
        if (NetworkClient.allClients.Count > 0)
          this.client = NetworkClient.allClients[0];
      }
      if (this.client != null)
      {
        if (NetworkManager.s_ClientReadyConnection != null)
        {
          this.OnClientConnect(NetworkManager.s_ClientReadyConnection);
          NetworkManager.s_ClientReadyConnection = (NetworkConnection) null;
        }
      }
      else if (LogFilter.logDev)
        Debug.Log((object) "FinishLoadScene client is STILL null");

      if (NetworkServer.active)
      {
        NetworkServer.SpawnObjects();
        this.OnServerSceneChanged(NetworkManager.networkSceneName);
      }

      if (!NetworkClient.active || NetworkServer.active)
        ;

      if (!NetworkClient.active)
        return;

      this.RegisterClientMessages(this.client);
      this.OnClientSceneChanged(this.client.connection);
    }


    private void Update()
    {
      // if not loading a scene or has not finished loading a scene then just return
      if (NetworkManager.s_LoadingSceneAsync == null || !NetworkManager.s_LoadingSceneAsync.isDone)
        return;

      if (LogFilter.logDebug)
        Debug.Log((object) ("ClientChangeScene done readyCon:" + (object) NetworkManager.s_ClientReadyConnection));

      this.FinishLoadScene();

      NetworkManager.s_LoadingSceneAsync.allowSceneActivation = true;
      NetworkManager.s_LoadingSceneAsync = (AsyncOperation) null;
    }


    private void OnDestroy()
    {
      if (!LogFilter.logDev)
        return;
      Debug.Log((object) "NetworkManager destroyed");
    }

    /// <summary>
    /// 
    /// <para>
    /// Registers the transform of a game object as a player spawn location.
    /// </para>
    /// 
    /// </summary>
    /// <param name="start">Transform to register.</param>
    public static void RegisterStartPosition(Transform start)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) ("RegisterStartPosition:" + (object) start));
      NetworkManager.s_StartPositions.Add(start);
    }

    /// <summary>
    /// 
    /// <para>
    /// Unregisters the transform of a game object as a player spawn location.
    /// </para>
    /// 
    /// </summary>
    /// <param name="start"/>
    public static void UnRegisterStartPosition(Transform start)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) ("UnRegisterStartPosition:" + (object) start));
      NetworkManager.s_StartPositions.Remove(start);
    }



    #region Server message handlers for the messages registered for. These result in calling the api functions

    // new client has connected to server
    internal void OnServerConnectInternal(NetworkMessage netMsg)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkManager:OnServerConnectInternal");

      netMsg.conn.SetMaxDelay(this.m_MaxDelay);

      if (NetworkManager.networkSceneName != string.Empty && NetworkManager.networkSceneName != this.m_OfflineScene)
      {
        StringMessage stringMessage = new StringMessage(NetworkManager.networkSceneName);
        netMsg.conn.Send((short) 39, (MessageBase) stringMessage);
      }
      this.OnServerConnect(netMsg.conn);
    }

    // a client has disconnected from server
    internal void OnServerDisconnectInternal(NetworkMessage netMsg)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkManager:OnServerDisconnectInternal");
      this.OnServerDisconnect(netMsg.conn);
    }

    // when a client is ready
    internal void OnServerReadyMessageInternal(NetworkMessage netMsg)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkManager:OnServerReadyMessageInternal");
      this.OnServerReady(netMsg.conn);
    }

    internal void OnServerAddPlayerMessageInternal(NetworkMessage netMsg)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkManager:OnServerAddPlayerMessageInternal");
      netMsg.ReadMessage<AddPlayerMessage>(NetworkManager.s_AddPlayerMessage);
      this.OnServerAddPlayer(netMsg.conn, NetworkManager.s_AddPlayerMessage.playerControllerId);
    }

    internal void OnServerRemovePlayerMessageInternal(NetworkMessage netMsg)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkManager:OnServerRemovePlayerMessageInternal");
      netMsg.ReadMessage<RemovePlayerMessage>(NetworkManager.s_RemovePlayerMessage);
      PlayerController playerController;
      netMsg.conn.GetPlayerController(NetworkManager.s_RemovePlayerMessage.playerControllerId, out playerController);
      this.OnServerRemovePlayer(netMsg.conn, playerController);
      netMsg.conn.RemovePlayerController(NetworkManager.s_RemovePlayerMessage.playerControllerId);
    }

    internal void OnServerErrorInternal(NetworkMessage netMsg)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkManager:OnServerErrorInternal");
      netMsg.ReadMessage<ErrorMessage>(NetworkManager.s_ErrorMessage);
      this.OnServerError(netMsg.conn, NetworkManager.s_ErrorMessage.errorCode);
    }
    #endregion


    internal void OnClientConnectInternal(NetworkMessage netMsg)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkManager:OnClientConnectInternal");
      netMsg.conn.SetMaxDelay(this.m_MaxDelay);
      if (string.IsNullOrEmpty(this.m_OnlineScene) || this.m_OnlineScene == this.m_OfflineScene)
        this.OnClientConnect(netMsg.conn);
      else
        NetworkManager.s_ClientReadyConnection = netMsg.conn;
    }

    internal void OnClientDisconnectInternal(NetworkMessage netMsg)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkManager:OnClientDisconnectInternal");
      if (this.m_OfflineScene != string.Empty)
        this.ClientChangeScene(this.m_OfflineScene, false);
      this.OnClientDisconnect(netMsg.conn);
    }

    internal void OnClientNotReadyMessageInternal(NetworkMessage netMsg)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkManager:OnClientNotReadyMessageInternal");
      ClientScene.s_IsReady = false;
      this.OnClientNotReady(netMsg.conn);
    }

    internal void OnClientErrorInternal(NetworkMessage netMsg)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkManager:OnClientErrorInternal");
      netMsg.ReadMessage<ErrorMessage>(NetworkManager.s_ErrorMessage);
      this.OnClientError(netMsg.conn, NetworkManager.s_ErrorMessage.errorCode);
    }

    internal void OnClientSceneInternal(NetworkMessage netMsg)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkManager:OnClientSceneInternal");
      string newSceneName = netMsg.reader.ReadString();
      if (!NetworkClient.active || NetworkServer.active)
        return;
      this.ClientChangeScene(newSceneName, true);
    }

    /// <summary>
    /// 
    /// <para>
    /// Called on the server when a new client connects.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public virtual void OnServerConnect(NetworkConnection conn)
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// Called on the server when a client disconnects.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public virtual void OnServerDisconnect(NetworkConnection conn)
    {
      NetworkServer.DestroyPlayersForConnection(conn);
    }

    /// <summary>
    /// 
    /// <para>
    /// Called on the server when a client is ready.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public virtual void OnServerReady(NetworkConnection conn)
    {
      if (conn.playerControllers.Count == 0 && LogFilter.logDebug)
        Debug.Log((object) "Ready with no player object");
      NetworkServer.SetClientReady(conn);
    }

    /// <summary>
    /// 
    /// <para>
    /// Called on the server when a client adds a new player.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">Connection from client.</param><param name="playerControllerId">Id of the new player.</param>
    public virtual void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
      if ((UnityEngine.Object) this.m_PlayerPrefab == (UnityEngine.Object) null)
      {
        if (!LogFilter.logError)
          return;
        Debug.LogError((object) "The PlayerPrefab is empty on the NetworkManager. Please setup a PlayerPrefab object.");
      }
      else if ((UnityEngine.Object) this.m_PlayerPrefab.GetComponent<NetworkIdentity>() == (UnityEngine.Object) null)
      {
        if (!LogFilter.logError)
          return;
        Debug.LogError((object) "The PlayerPrefab does not have a NetworkIdentity. Please add a NetworkIdentity to the player prefab.");
      }
      else if ((int) playerControllerId < conn.playerControllers.Count && conn.playerControllers[(int) playerControllerId].IsValid && (UnityEngine.Object) conn.playerControllers[(int) playerControllerId].gameObject != (UnityEngine.Object) null)
      {
        if (!LogFilter.logError)
          return;
        Debug.LogError((object) "There is already a player at that playerControllerId for this connections.");
      }
      else
      {
        Transform startPosition = this.GetStartPosition();
        GameObject player = !((UnityEngine.Object) startPosition != (UnityEngine.Object) null) ? (GameObject) UnityEngine.Object.Instantiate((UnityEngine.Object) this.m_PlayerPrefab, Vector3.zero, Quaternion.identity) : (GameObject) UnityEngine.Object.Instantiate((UnityEngine.Object) this.m_PlayerPrefab, startPosition.position, startPosition.rotation);
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// This finds a spawn position based on NetworkStartPosition objects in the scene.
    /// </para>
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// 
    /// <para>
    /// Returns the transform to spawn a player at, or null.
    /// </para>
    /// 
    /// </returns>
    public Transform GetStartPosition()
    {
      if (NetworkManager.s_StartPositions.Count > 0)
      {
        for (int index = NetworkManager.s_StartPositions.Count - 1; index >= 0; --index)
        {
          if ((UnityEngine.Object) NetworkManager.s_StartPositions[index] == (UnityEngine.Object) null)
            NetworkManager.s_StartPositions.RemoveAt(index);
        }
      }
      if (this.m_PlayerSpawnMethod == PlayerSpawnMethod.Random && NetworkManager.s_StartPositions.Count > 0)
      {
        int index = UnityEngine.Random.Range(0, NetworkManager.s_StartPositions.Count);
        return NetworkManager.s_StartPositions[index];
      }
      if (this.m_PlayerSpawnMethod != PlayerSpawnMethod.RoundRobin || NetworkManager.s_StartPositions.Count <= 0)
        return (Transform) null;
      if (NetworkManager.s_StartPositionIndex >= NetworkManager.s_StartPositions.Count)
        NetworkManager.s_StartPositionIndex = 0;
      Transform transform = NetworkManager.s_StartPositions[NetworkManager.s_StartPositionIndex];
      ++NetworkManager.s_StartPositionIndex;
      return transform;
    }

    public virtual void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
      if (!((UnityEngine.Object) player.gameObject != (UnityEngine.Object) null))
        return;
      NetworkServer.Destroy(player.gameObject);
    }

    /// <summary>
    /// 
    /// <para>
    /// Called on the server when a network error occurs for a client connection.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">Connection from client.</param><param name="errorCode">Error code.</param>
    public virtual void OnServerError(NetworkConnection conn, int errorCode)
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
    /// </para>
    /// 
    /// </summary>
    /// <param name="sceneName">The name of the new scene.</param>
    public virtual void OnServerSceneChanged(string sceneName)
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// Called on the client when connected to a server.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">Connection to the server.</param>
    public virtual void OnClientConnect(NetworkConnection conn)
    {
      if (!string.IsNullOrEmpty(this.m_OnlineScene) && !(this.m_OnlineScene == this.m_OfflineScene))
        return;
      ClientScene.Ready(conn);
      if (!this.m_AutoCreatePlayer)
        return;
      ClientScene.AddPlayer((short) 0);
    }

    /// <summary>
    /// 
    /// <para>
    /// Called on clients when disconnected from a server.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">Connection to the server.</param>
    public virtual void OnClientDisconnect(NetworkConnection conn)
    {
      this.StopClient();
    }

    /// <summary>
    /// 
    /// <para>
    /// Called on clients when a network error occurs.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">Connection to a server.</param><param name="errorCode">Error code.</param>
    public virtual void OnClientError(NetworkConnection conn, int errorCode)
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// Called on clients when a servers tells the client it is no longer ready.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">Connection to a server.</param>
    public virtual void OnClientNotReady(NetworkConnection conn)
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
    /// </para>
    /// 
    /// </summary>
    /// <param name="conn">The network connection that the scene change message arrived on.</param>
    public virtual void OnClientSceneChanged(NetworkConnection conn)
    {
      ClientScene.Ready(conn);
      if (!this.m_AutoCreatePlayer)
        return;
      bool flag1 = false;
      if (ClientScene.localPlayers.Count == 0)
        flag1 = true;
      bool flag2 = false;
      using (List<PlayerController>.Enumerator enumerator = ClientScene.localPlayers.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          if ((UnityEngine.Object) enumerator.Current.gameObject != (UnityEngine.Object) null)
          {
            flag2 = true;
            break;
          }
        }
      }
      if (!flag2)
        flag1 = true;
      if (!flag1)
        return;
      ClientScene.AddPlayer((short) 0);
    }

    /// <summary>
    /// 
    /// <para>
    /// This creates a UMatch matchmaker for the NetworkManager.
    /// </para>
    /// 
    /// </summary>
    public void StartMatchMaker()
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkManager StartMatchMaker");
      this.SetMatchHost(this.m_MatchHost, this.m_MatchPort, true);
    }

    /// <summary>
    /// 
    /// <para>
    /// Stops the matchmaker that the NetworkManager is using.
    /// </para>
    /// 
    /// </summary>
    public void StopMatchMaker()
    {
      if ((UnityEngine.Object) this.matchMaker != (UnityEngine.Object) null)
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) this.matchMaker);
        this.matchMaker = (NetworkMatch) null;
      }
      this.matchInfo = (MatchInfo) null;
      this.matches = (List<MatchDesc>) null;
    }

    /// <summary>
    /// 
    /// <para>
    /// This set the address of the matchmaker service.
    /// </para>
    /// 
    /// </summary>
    /// <param name="newHost">Hostname of matchmaker service.</param><param name="port">Port of matchmaker service.</param><param name="https">Protocol used by matchmaker service.</param>
    public void SetMatchHost(string newHost, int port, bool https)
    {
      if ((UnityEngine.Object) this.matchMaker == (UnityEngine.Object) null)
        this.matchMaker = this.gameObject.AddComponent<NetworkMatch>();
      if (newHost == "localhost" || newHost == "127.0.0.1")
        newHost = Environment.MachineName;
      string str1 = "http://";
      if (https)
        str1 = "https://";
      if (LogFilter.logDebug)
        Debug.Log((object) ("SetMatchHost:" + newHost));
      this.m_MatchHost = newHost;
      this.m_MatchPort = port;
      NetworkMatch networkMatch = this.matchMaker;
      object[] objArray = new object[4];
      int index1 = 0;
      string str2 = str1;
      objArray[index1] = (object) str2;
      int index2 = 1;
      string str3 = this.m_MatchHost;
      objArray[index2] = (object) str3;
      int index3 = 2;
      string str4 = ":";
      objArray[index3] = (object) str4;
      int index4 = 3;
      // ISSUE: variable of a boxed type
      __Boxed<int> local = (ValueType) this.m_MatchPort;
      objArray[index4] = (object) local;
      Uri uri = new Uri(string.Concat(objArray));
      networkMatch.baseUri = uri;
    }

    /// <summary>
    /// 
    /// <para>
    /// This hook is invoked when a host is started.
    /// </para>
    /// 
    /// </summary>
    public virtual void OnStartHost()
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This hook is invoked when a server is started - including when a host is started.
    /// </para>
    /// 
    /// </summary>
    public virtual void OnStartServer()
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This is a hook that is invoked when the client is started.
    /// </para>
    /// 
    /// </summary>
    /// <param name="client"/>
    public virtual void OnStartClient(NetworkClient client)
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This hook is called when a server is stopped - including when a host is stopped.
    /// </para>
    /// 
    /// </summary>
    public virtual void OnStopServer()
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This hook is called when a client is stopped.
    /// </para>
    /// 
    /// </summary>
    public virtual void OnStopClient()
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This hook is called when a host is stopped.
    /// </para>
    /// 
    /// </summary>
    public virtual void OnStopHost()
    {
    }

    /// <summary>
    /// 
    /// <para>
    /// This is invoked when a match has been created.
    /// </para>
    /// 
    /// </summary>
    /// <param name="matchInfo">Info about the match that has been created.</param>
    public virtual void OnMatchCreate(CreateMatchResponse matchInfo)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) ("NetworkManager OnMatchCreate " + (object) matchInfo));
      if (matchInfo.success)
      {
        Utility.SetAccessTokenForNetwork(matchInfo.networkId, new NetworkAccessToken(matchInfo.accessTokenString));
        this.StartHost(new MatchInfo(matchInfo));
      }
      else
      {
        if (!LogFilter.logError)
          return;
        Debug.LogError((object) ("Create Failed:" + matchInfo.ToString()));
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// This is invoked when a list of matches is returned from ListMatches().
    /// </para>
    /// 
    /// </summary>
    /// <param name="matchList">A list of available matches.</param>
    public virtual void OnMatchList(ListMatchResponse matchList)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkManager OnMatchList ");
      this.matches = matchList.matches;
    }

    /// <summary>
    /// 
    /// <para>
    /// This is invoked when a match is joined.
    /// </para>
    /// 
    /// </summary>
    /// <param name="matchInfo"/>
    public void OnMatchJoined(JoinMatchResponse matchInfo)
    {
      if (LogFilter.logDebug)
        Debug.Log((object) "NetworkManager OnMatchJoined ");
      if (matchInfo.success)
      {
        Utility.SetAccessTokenForNetwork(matchInfo.networkId, new NetworkAccessToken(matchInfo.accessTokenString));
        this.StartClient(new MatchInfo(matchInfo));
      }
      else
      {
        if (!LogFilter.logError)
          return;
        Debug.LogError((object) ("Join Failed:" + matchInfo.ToString()));
      }
    }
  }
}
