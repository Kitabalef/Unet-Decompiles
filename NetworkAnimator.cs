// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkAnimator
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using System;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;

namespace UnityEngine.Networking
{
  /// <summary>
  /// 
  /// <para>
  /// A component to synchronize Mecanim animation states for networked objects.
  /// </para>
  /// 
  /// </summary>
  [AddComponentMenu("Network/NetworkAnimator")]
  [RequireComponent(typeof (NetworkIdentity))]
  [RequireComponent(typeof (Animator))]
  public class NetworkAnimator : NetworkBehaviour
  {
    private static AnimationMessage s_AnimationMessage = new AnimationMessage();
    private static AnimationParametersMessage s_AnimationParametersMessage = new AnimationParametersMessage();
    private static AnimationTriggerMessage s_AnimationTriggerMessage = new AnimationTriggerMessage();
    [SerializeField]
    private Animator m_Animator;
    [SerializeField]
    private uint m_ParameterSendBits;
    private int m_AnimationHash;
    private int m_TransitionHash;
    private NetworkWriter m_ParameterWriter;
    private float m_SendTimer;
    public string param0;
    public string param1;
    public string param2;
    public string param3;
    public string param4;
    public string param5;

    /// <summary>
    /// 
    /// <para>
    /// The animator component to synchronize.
    /// </para>
    /// 
    /// </summary>
    public Animator animator
    {
      get
      {
        return this.m_Animator;
      }
      set
      {
        this.m_Animator = value;
        this.ResetParameterOptions();
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Sets whether an animation parameter should be auto sent.
    /// </para>
    /// 
    /// </summary>
    /// <param name="index">Index of the parameter in the Animator.</param><param name="value">The new value.</param>
    public void SetParameterAutoSend(int index, bool value)
    {
      if (value)
        this.m_ParameterSendBits |= (uint) (1 << index);
      else
        this.m_ParameterSendBits &= (uint) ~(1 << index);
    }

    /// <summary>
    /// 
    /// <para>
    /// Gets whether an animation parameter should be auto sent.
    /// </para>
    /// 
    /// </summary>
    /// <param name="index">Index of the parameter in the Animator.</param>
    /// <returns>
    /// 
    /// <para>
    /// True if the parameter should be sent.
    /// </para>
    /// 
    /// </returns>
    public bool GetParameterAutoSend(int index)
    {
      return ((int) this.m_ParameterSendBits & 1 << index) != 0;
    }

    internal void ResetParameterOptions()
    {
      Debug.Log((object) "ResetParameterOptions");
      this.m_ParameterSendBits = 0U;
    }

    private void InitializeAuthority()
    {
      this.m_ParameterWriter = new NetworkWriter();
    }

    public override void OnStartServer()
    {
      if (this.localPlayerAuthority)
        return;
      this.InitializeAuthority();
    }

    public override void OnStartLocalPlayer()
    {
      if (!this.localPlayerAuthority)
        return;
      this.InitializeAuthority();
    }

    private void FixedUpdate()
    {
      if (this.m_ParameterWriter == null)
        return;
      this.CheckSendRate();
      int stateHash;
      float normalizedTime;
      if (!this.CheckAnimStateChanged(out stateHash, out normalizedTime))
        return;
      AnimationMessage animationMessage = new AnimationMessage();
      animationMessage.netId = this.netId;
      animationMessage.stateHash = stateHash;
      animationMessage.normalizedTime = normalizedTime;
      this.m_ParameterWriter.SeekZero();
      this.WriteParameters(this.m_ParameterWriter, false);
      animationMessage.parameters = this.m_ParameterWriter.ToArray();
      if (this.hasAuthority)
      {
        NetworkClient.allClients[0].Send((short) 40, (MessageBase) animationMessage);
      }
      else
      {
        if (!this.isServer || this.localPlayerAuthority)
          return;
        NetworkServer.SendToReady(this.gameObject, (short) 40, (MessageBase) animationMessage);
      }
    }

    private bool CheckAnimStateChanged(out int stateHash, out float normalizedTime)
    {
      stateHash = 0;
      normalizedTime = 0.0f;
      if (this.m_Animator.IsInTransition(0))
      {
        AnimatorTransitionInfo animatorTransitionInfo = this.m_Animator.GetAnimatorTransitionInfo(0);
        if (animatorTransitionInfo.fullPathHash == this.m_TransitionHash)
          return false;
        this.m_TransitionHash = animatorTransitionInfo.fullPathHash;
        this.m_AnimationHash = 0;
        return true;
      }
      AnimatorStateInfo animatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(0);
      if (animatorStateInfo.fullPathHash == this.m_AnimationHash)
        return false;
      if (this.m_AnimationHash != 0)
      {
        stateHash = animatorStateInfo.fullPathHash;
        normalizedTime = animatorStateInfo.normalizedTime;
      }
      this.m_TransitionHash = 0;
      this.m_AnimationHash = animatorStateInfo.fullPathHash;
      return true;
    }

    private void CheckSendRate()
    {
      if ((double) this.GetNetworkSendInterval() == 0.0 || (double) this.m_SendTimer >= (double) Time.time)
        return;
      this.m_SendTimer = Time.time + this.GetNetworkSendInterval();
      AnimationParametersMessage parametersMessage = new AnimationParametersMessage();
      parametersMessage.netId = this.netId;
      this.m_ParameterWriter.SeekZero();
      this.WriteParameters(this.m_ParameterWriter, true);
      parametersMessage.parameters = this.m_ParameterWriter.ToArray();
      if (this.hasAuthority)
      {
        NetworkClient.allClients[0].Send((short) 41, (MessageBase) parametersMessage);
      }
      else
      {
        if (!this.isServer || this.localPlayerAuthority)
          return;
        NetworkServer.SendToReady(this.gameObject, (short) 41, (MessageBase) parametersMessage);
      }
    }

    private void SetSendTrackingParam(string p, int i)
    {
      p = "Sent Param: " + p;
      if (i == 0)
        this.param0 = p;
      if (i == 1)
        this.param1 = p;
      if (i == 2)
        this.param2 = p;
      if (i == 3)
        this.param3 = p;
      if (i == 4)
        this.param4 = p;
      if (i != 5)
        return;
      this.param5 = p;
    }

    private void SetRecvTrackingParam(string p, int i)
    {
      p = "Recv Param: " + p;
      if (i == 0)
        this.param0 = p;
      if (i == 1)
        this.param1 = p;
      if (i == 2)
        this.param2 = p;
      if (i == 3)
        this.param3 = p;
      if (i == 4)
        this.param4 = p;
      if (i != 5)
        return;
      this.param5 = p;
    }

    internal void HandleAnimMsg(AnimationMessage msg, NetworkReader reader)
    {
      if (this.hasAuthority)
        return;
      if (msg.stateHash != 0)
        this.m_Animator.Play(msg.stateHash, 0, msg.normalizedTime);
      this.ReadParameters(reader, false);
    }

    internal void HandleAnimParamsMsg(AnimationParametersMessage msg, NetworkReader reader)
    {
      if (this.hasAuthority)
        return;
      this.ReadParameters(reader, true);
    }

    internal void HandleAnimTriggerMsg(int hash)
    {
      this.m_Animator.SetTrigger(hash);
    }

    private void WriteParameters(NetworkWriter writer, bool autoSend)
    {
      for (int index = 0; index < this.m_Animator.parameters.Length; ++index)
      {
        if (!autoSend || this.GetParameterAutoSend(index))
        {
          AnimatorControllerParameter controllerParameter = this.m_Animator.parameters[index];
          if (controllerParameter.type == AnimatorControllerParameterType.Int)
          {
            writer.WritePackedUInt32((uint) this.m_Animator.GetInteger(controllerParameter.nameHash));
            this.SetSendTrackingParam(controllerParameter.name + ":" + this.m_Animator.GetInteger(controllerParameter.nameHash).ToString(), index);
          }
          if (controllerParameter.type == AnimatorControllerParameterType.Float)
          {
            writer.Write(this.m_Animator.GetFloat(controllerParameter.nameHash));
            this.SetSendTrackingParam(controllerParameter.name + ":" + this.m_Animator.GetFloat(controllerParameter.nameHash).ToString(), index);
          }
          if (controllerParameter.type == AnimatorControllerParameterType.Bool)
          {
            writer.Write(this.m_Animator.GetBool(controllerParameter.nameHash));
            this.SetSendTrackingParam(controllerParameter.name + ":" + this.m_Animator.GetBool(controllerParameter.nameHash).ToString(), index);
          }
        }
      }
    }

    private void ReadParameters(NetworkReader reader, bool autoSend)
    {
      for (int index = 0; index < this.m_Animator.parameters.Length; ++index)
      {
        if (!autoSend || this.GetParameterAutoSend(index))
        {
          AnimatorControllerParameter controllerParameter = this.m_Animator.parameters[index];
          if (controllerParameter.type == AnimatorControllerParameterType.Int)
          {
            int num = (int) reader.ReadPackedUInt32();
            this.m_Animator.SetInteger(controllerParameter.nameHash, num);
            this.SetRecvTrackingParam(controllerParameter.name + ":" + num.ToString(), index);
          }
          if (controllerParameter.type == AnimatorControllerParameterType.Float)
          {
            float num = reader.ReadSingle();
            this.m_Animator.SetFloat(controllerParameter.nameHash, num);
            this.SetRecvTrackingParam(controllerParameter.name + ":" + num.ToString(), index);
          }
          if (controllerParameter.type == AnimatorControllerParameterType.Bool)
          {
            bool flag = reader.ReadBoolean();
            this.m_Animator.SetBool(controllerParameter.nameHash, flag);
            this.SetRecvTrackingParam(controllerParameter.name + ":" + flag.ToString(), index);
          }
        }
      }
    }

    public override bool OnSerialize(NetworkWriter writer, bool forceAll)
    {
      if (!forceAll)
        return false;
      if (this.m_Animator.IsInTransition(0))
      {
        AnimatorStateInfo animatorStateInfo = this.m_Animator.GetNextAnimatorStateInfo(0);
        writer.Write(animatorStateInfo.fullPathHash);
        writer.Write(animatorStateInfo.normalizedTime);
      }
      else
      {
        AnimatorStateInfo animatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(0);
        writer.Write(animatorStateInfo.fullPathHash);
        writer.Write(animatorStateInfo.normalizedTime);
      }
      this.WriteParameters(writer, false);
      return true;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
      if (!initialState)
        return;
      int stateNameHash = reader.ReadInt32();
      float normalizedTime = reader.ReadSingle();
      this.ReadParameters(reader, false);
      this.m_Animator.Play(stateNameHash, 0, normalizedTime);
    }

    /// <summary>
    /// 
    /// <para>
    /// Causes an animation trigger to be invoked for a networked object.
    /// </para>
    /// 
    /// </summary>
    /// <param name="name">Name of trigger.</param><param name="hash">Hash id of trigger (from the Animator).</param>
    public void SetTrigger(string name)
    {
      this.SetTrigger(Animator.StringToHash(name));
    }

    /// <summary>
    /// 
    /// <para>
    /// Causes an animation trigger to be invoked for a networked object.
    /// </para>
    /// 
    /// </summary>
    /// <param name="name">Name of trigger.</param><param name="hash">Hash id of trigger (from the Animator).</param>
    public void SetTrigger(int hash)
    {
      AnimationTriggerMessage animationTriggerMessage = new AnimationTriggerMessage();
      animationTriggerMessage.netId = this.netId;
      animationTriggerMessage.hash = hash;
      if (this.hasAuthority && this.localPlayerAuthority)
      {
        if (NetworkClient.allClients.Count <= 0)
          return;
        NetworkClient networkClient = NetworkClient.allClients[0];
        if (networkClient == null)
          return;
        networkClient.Send((short) 42, (MessageBase) animationTriggerMessage);
      }
      else
      {
        if (!this.isServer || this.localPlayerAuthority)
          return;
        NetworkServer.SendToReady(this.gameObject, (short) 42, (MessageBase) animationTriggerMessage);
      }
    }

    internal static void OnAnimationServerMessage(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<AnimationMessage>(NetworkAnimator.s_AnimationMessage);
      if (LogFilter.logDev)
      {
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "OnAnimationMessage for netId=";
        objArray[index1] = (object) str1;
        int index2 = 1;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local = (ValueType) NetworkAnimator.s_AnimationMessage.netId;
        objArray[index2] = (object) local;
        int index3 = 2;
        string str2 = " conn=";
        objArray[index3] = (object) str2;
        int index4 = 3;
        NetworkConnection networkConnection = netMsg.conn;
        objArray[index4] = (object) networkConnection;
        Debug.Log((object) string.Concat(objArray));
      }
      GameObject localObject = NetworkServer.FindLocalObject(NetworkAnimator.s_AnimationMessage.netId);
      if ((UnityEngine.Object) localObject == (UnityEngine.Object) null)
        return;
      NetworkAnimator component = localObject.GetComponent<NetworkAnimator>();
      if (!((UnityEngine.Object) component != (UnityEngine.Object) null))
        return;
      NetworkReader reader = new NetworkReader(NetworkAnimator.s_AnimationMessage.parameters);
      component.HandleAnimMsg(NetworkAnimator.s_AnimationMessage, reader);
      NetworkServer.SendToReady(localObject, (short) 40, (MessageBase) NetworkAnimator.s_AnimationMessage);
    }

    internal static void OnAnimationParametersServerMessage(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<AnimationParametersMessage>(NetworkAnimator.s_AnimationParametersMessage);
      if (LogFilter.logDev)
      {
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "OnAnimationParametersMessage for netId=";
        objArray[index1] = (object) str1;
        int index2 = 1;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local = (ValueType) NetworkAnimator.s_AnimationParametersMessage.netId;
        objArray[index2] = (object) local;
        int index3 = 2;
        string str2 = " conn=";
        objArray[index3] = (object) str2;
        int index4 = 3;
        NetworkConnection networkConnection = netMsg.conn;
        objArray[index4] = (object) networkConnection;
        Debug.Log((object) string.Concat(objArray));
      }
      GameObject localObject = NetworkServer.FindLocalObject(NetworkAnimator.s_AnimationParametersMessage.netId);
      if ((UnityEngine.Object) localObject == (UnityEngine.Object) null)
        return;
      NetworkAnimator component = localObject.GetComponent<NetworkAnimator>();
      if (!((UnityEngine.Object) component != (UnityEngine.Object) null))
        return;
      NetworkReader reader = new NetworkReader(NetworkAnimator.s_AnimationParametersMessage.parameters);
      component.HandleAnimParamsMsg(NetworkAnimator.s_AnimationParametersMessage, reader);
      NetworkServer.SendToReady(localObject, (short) 41, (MessageBase) NetworkAnimator.s_AnimationParametersMessage);
    }

    internal static void OnAnimationTriggerServerMessage(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<AnimationTriggerMessage>(NetworkAnimator.s_AnimationTriggerMessage);
      if (LogFilter.logDev)
      {
        object[] objArray = new object[4];
        int index1 = 0;
        string str1 = "OnAnimationTriggerMessage for netId=";
        objArray[index1] = (object) str1;
        int index2 = 1;
        // ISSUE: variable of a boxed type
        __Boxed<NetworkInstanceId> local = (ValueType) NetworkAnimator.s_AnimationTriggerMessage.netId;
        objArray[index2] = (object) local;
        int index3 = 2;
        string str2 = " conn=";
        objArray[index3] = (object) str2;
        int index4 = 3;
        NetworkConnection networkConnection = netMsg.conn;
        objArray[index4] = (object) networkConnection;
        Debug.Log((object) string.Concat(objArray));
      }
      GameObject localObject = NetworkServer.FindLocalObject(NetworkAnimator.s_AnimationTriggerMessage.netId);
      if ((UnityEngine.Object) localObject == (UnityEngine.Object) null)
        return;
      NetworkAnimator component = localObject.GetComponent<NetworkAnimator>();
      if (!((UnityEngine.Object) component != (UnityEngine.Object) null))
        return;
      component.HandleAnimTriggerMsg(NetworkAnimator.s_AnimationTriggerMessage.hash);
      NetworkServer.SendToReady(localObject, (short) 42, (MessageBase) NetworkAnimator.s_AnimationTriggerMessage);
    }

    internal static void OnAnimationClientMessage(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<AnimationMessage>(NetworkAnimator.s_AnimationMessage);
      GameObject localObject = ClientScene.FindLocalObject(NetworkAnimator.s_AnimationMessage.netId);
      if ((UnityEngine.Object) localObject == (UnityEngine.Object) null)
        return;
      NetworkAnimator component = localObject.GetComponent<NetworkAnimator>();
      if (!((UnityEngine.Object) component != (UnityEngine.Object) null))
        return;
      NetworkReader reader = new NetworkReader(NetworkAnimator.s_AnimationMessage.parameters);
      component.HandleAnimMsg(NetworkAnimator.s_AnimationMessage, reader);
    }

    internal static void OnAnimationParametersClientMessage(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<AnimationParametersMessage>(NetworkAnimator.s_AnimationParametersMessage);
      GameObject localObject = ClientScene.FindLocalObject(NetworkAnimator.s_AnimationParametersMessage.netId);
      if ((UnityEngine.Object) localObject == (UnityEngine.Object) null)
        return;
      NetworkAnimator component = localObject.GetComponent<NetworkAnimator>();
      if (!((UnityEngine.Object) component != (UnityEngine.Object) null))
        return;
      NetworkReader reader = new NetworkReader(NetworkAnimator.s_AnimationParametersMessage.parameters);
      component.HandleAnimParamsMsg(NetworkAnimator.s_AnimationParametersMessage, reader);
    }

    internal static void OnAnimationTriggerClientMessage(NetworkMessage netMsg)
    {
      netMsg.ReadMessage<AnimationTriggerMessage>(NetworkAnimator.s_AnimationTriggerMessage);
      GameObject localObject = ClientScene.FindLocalObject(NetworkAnimator.s_AnimationTriggerMessage.netId);
      if ((UnityEngine.Object) localObject == (UnityEngine.Object) null)
        return;
      NetworkAnimator component = localObject.GetComponent<NetworkAnimator>();
      if (!((UnityEngine.Object) component != (UnityEngine.Object) null))
        return;
      component.HandleAnimTriggerMsg(NetworkAnimator.s_AnimationTriggerMessage.hash);
    }
  }
}
