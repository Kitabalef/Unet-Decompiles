// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.NetworkTransform
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Networking
{
  /// <summary>
  /// 
  /// <para>
  /// A component to synchronize the position of networked objects.
  /// </para>
  /// 
  /// </summary>
  [AddComponentMenu("Network/NetworkTransform")]
  public class NetworkTransform : NetworkBehaviour
  {
    [SerializeField]
    private float m_SendInterval = 0.1f;
    [SerializeField]
    private NetworkTransform.AxisSyncMode m_SyncRotationAxis = NetworkTransform.AxisSyncMode.AxisXYZ;
    [SerializeField]
    private float m_MovementTheshold = 1.0 / 1000.0;
    [SerializeField]
    private float m_SnapThreshold = 5f;
    [SerializeField]
    private float m_InterpolateRotation = 1f;
    [SerializeField]
    private float m_InterpolateMovement = 1f;
    private bool m_Grounded = true;
    private const float localMovementThreshold = 1E-05f;
    private const float localRotationThreshold = 1E-05f;
    private const float moveAheadRatio = 0.1f;
    [SerializeField]
    private NetworkTransform.TransformSyncMode m_TransformSyncMode;
    [SerializeField]
    private NetworkTransform.CompressionSyncMode m_RotationSyncCompression;
    [SerializeField]
    private bool m_SyncSpin;
    private Rigidbody m_RigidBody3D;
    private Rigidbody2D m_RigidBody2D;
    private CharacterController m_CharacterController;
    private Vector3 m_TargetSyncPosition;
    private Vector3 m_TargetSyncVelocity;
    private Vector3 m_FixedPosDiff;
    private Quaternion m_TargetSyncRotation3D;
    private Vector3 m_TargetSyncAngularVelocity3D;
    private float m_TargetSyncRotation2D;
    private float m_TargetSyncAngularVelocity2D;
    private float m_LastClientSyncTime;
    private float m_LastClientSendTime;
    private Vector3 m_PrevPosition;
    private Quaternion m_PrevRotation;
    private float m_PrevRotation2D;
    private NetworkWriter m_LocalTransformWriter;

    /// <summary>
    /// 
    /// <para>
    /// What method to use to sync the object's position.
    /// </para>
    /// 
    /// </summary>
    public NetworkTransform.TransformSyncMode transformSyncMode
    {
      get
      {
        return this.m_TransformSyncMode;
      }
      set
      {
        this.m_TransformSyncMode = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The sendInterval controls how often state updates are sent for this object.
    /// </para>
    /// 
    /// </summary>
    public float sendInterval
    {
      get
      {
        return this.m_SendInterval;
      }
      set
      {
        this.m_SendInterval = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Which axis should rotation by synchronized for.
    /// </para>
    /// 
    /// </summary>
    public NetworkTransform.AxisSyncMode syncRotationAxis
    {
      get
      {
        return this.m_SyncRotationAxis;
      }
      set
      {
        this.m_SyncRotationAxis = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// How much to compress rotation sync updates.
    /// </para>
    /// 
    /// </summary>
    public NetworkTransform.CompressionSyncMode rotationSyncCompression
    {
      get
      {
        return this.m_RotationSyncCompression;
      }
      set
      {
        this.m_RotationSyncCompression = value;
      }
    }

    public bool syncSpin
    {
      get
      {
        return this.m_SyncSpin;
      }
      set
      {
        this.m_SyncSpin = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The distance that an object can move without sending a movement synchronization update.
    /// </para>
    /// 
    /// </summary>
    public float movementTheshold
    {
      get
      {
        return this.m_MovementTheshold;
      }
      set
      {
        this.m_MovementTheshold = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// If a movement update puts an object further from its current position that this value, it will snap to the position instead of moving smoothly.
    /// </para>
    /// 
    /// </summary>
    public float snapThreshold
    {
      get
      {
        return this.m_SnapThreshold;
      }
      set
      {
        this.m_SnapThreshold = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Enables interpolation of the synchronized rotation.
    /// </para>
    /// 
    /// </summary>
    public float interpolateRotation
    {
      get
      {
        return this.m_InterpolateRotation;
      }
      set
      {
        this.m_InterpolateRotation = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Enables interpolation of the synchronized movement.
    /// </para>
    /// 
    /// </summary>
    public float interpolateMovement
    {
      get
      {
        return this.m_InterpolateMovement;
      }
      set
      {
        this.m_InterpolateMovement = value;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Cached CharacterController.
    /// </para>
    /// 
    /// </summary>
    public CharacterController characterContoller
    {
      get
      {
        return this.m_CharacterController;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Cached Rigidbody.
    /// </para>
    /// 
    /// </summary>
    public Rigidbody rigidbody3D
    {
      get
      {
        return this.m_RigidBody3D;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Cached Rigidbody2D.
    /// </para>
    /// 
    /// </summary>
    public Rigidbody2D rigidbody2D
    {
      get
      {
        return this.m_RigidBody2D;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The most recent time when a movement synchronization packet arrived for this object.
    /// </para>
    /// 
    /// </summary>
    public float lastSyncTime
    {
      get
      {
        return this.m_LastClientSyncTime;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The target position interpolating towards.
    /// </para>
    /// 
    /// </summary>
    public Vector3 targetSyncPosition
    {
      get
      {
        return this.m_TargetSyncPosition;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The velocity send for synchronization.
    /// </para>
    /// 
    /// </summary>
    public Vector3 targetSyncVelocity
    {
      get
      {
        return this.m_TargetSyncVelocity;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The target position interpolating towards.
    /// </para>
    /// 
    /// </summary>
    public Quaternion targetSyncRotation3D
    {
      get
      {
        return this.m_TargetSyncRotation3D;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// The target rotation interpolating towards.
    /// </para>
    /// 
    /// </summary>
    public float targetSyncRotation2D
    {
      get
      {
        return this.m_TargetSyncRotation2D;
      }
    }

    /// <summary>
    /// 
    /// <para>
    /// Tells the NetworkTransform that it is on a surface (this is the default).
    /// </para>
    /// 
    /// </summary>
    public bool grounded
    {
      get
      {
        return this.m_Grounded;
      }
      set
      {
        this.m_Grounded = value;
      }
    }

    private void OnValidate()
    {
      if (this.m_TransformSyncMode < NetworkTransform.TransformSyncMode.SyncNone || this.m_TransformSyncMode > NetworkTransform.TransformSyncMode.SyncCharacterController)
        this.m_TransformSyncMode = NetworkTransform.TransformSyncMode.SyncTransform;
      if ((double) this.m_SendInterval < 0.0)
        this.m_SendInterval = 0.0f;
      if (this.m_SyncRotationAxis < NetworkTransform.AxisSyncMode.None || this.m_SyncRotationAxis > NetworkTransform.AxisSyncMode.AxisXYZ)
        this.m_SyncRotationAxis = NetworkTransform.AxisSyncMode.None;
      if ((double) this.movementTheshold < 0.0)
        this.movementTheshold = 0.0f;
      if ((double) this.snapThreshold < 0.0)
        this.snapThreshold = 0.01f;
      if ((double) this.interpolateRotation < 0.0)
        this.interpolateRotation = 0.01f;
      if ((double) this.interpolateMovement >= 0.0)
        return;
      this.interpolateMovement = 0.01f;
    }

    private void Awake()
    {
      this.m_RigidBody3D = this.GetComponent<Rigidbody>();
      this.m_RigidBody2D = this.GetComponent<Rigidbody2D>();
      this.m_CharacterController = this.GetComponent<CharacterController>();
      this.m_PrevPosition = this.transform.position;
      this.m_PrevRotation = this.transform.rotation;
      if (!this.localPlayerAuthority)
        return;
      this.m_LocalTransformWriter = new NetworkWriter();
    }

    public override bool OnSerialize(NetworkWriter writer, bool initialState)
    {
      if (!initialState)
      {
        if ((int) this.syncVarDirtyBits == 0)
        {
          writer.WritePackedUInt32(0U);
          return false;
        }
        writer.WritePackedUInt32(1U);
      }
      switch (this.transformSyncMode)
      {
        case NetworkTransform.TransformSyncMode.SyncNone:
          return false;
        case NetworkTransform.TransformSyncMode.SyncTransform:
          this.SerializeModeTransform(writer);
          break;
        case NetworkTransform.TransformSyncMode.SyncRigidbody2D:
          this.SerializeMode2D(writer);
          break;
        case NetworkTransform.TransformSyncMode.SyncRigidbody3D:
          this.SerializeMode3D(writer);
          break;
        case NetworkTransform.TransformSyncMode.SyncCharacterController:
          this.SerializeModeCharacterController(writer);
          break;
      }
      return true;
    }

    private void SerializeModeTransform(NetworkWriter writer)
    {
      writer.Write(this.transform.position);
      if (this.m_SyncRotationAxis != NetworkTransform.AxisSyncMode.None)
        this.SerializeRotation3D(writer, this.transform.rotation, this.syncRotationAxis, this.rotationSyncCompression);
      this.m_PrevPosition = this.transform.position;
      this.m_PrevRotation = this.transform.rotation;
    }

    private void SerializeMode3D(NetworkWriter writer)
    {
      if (this.isServer && (double) this.m_LastClientSyncTime != 0.0)
      {
        writer.Write(this.m_TargetSyncPosition);
        this.SerializeVelocity3D(writer, this.m_TargetSyncVelocity, NetworkTransform.CompressionSyncMode.None);
        if (this.syncRotationAxis != NetworkTransform.AxisSyncMode.None)
          this.SerializeRotation3D(writer, this.m_TargetSyncRotation3D, this.syncRotationAxis, this.rotationSyncCompression);
      }
      else
      {
        writer.Write(this.m_RigidBody3D.position);
        this.SerializeVelocity3D(writer, this.m_RigidBody3D.velocity, NetworkTransform.CompressionSyncMode.None);
        if (this.syncRotationAxis != NetworkTransform.AxisSyncMode.None)
          this.SerializeRotation3D(writer, this.m_RigidBody3D.rotation, this.syncRotationAxis, this.rotationSyncCompression);
      }
      if (this.m_SyncSpin)
        this.SerializeSpin3D(writer, this.m_RigidBody3D.angularVelocity, this.syncRotationAxis, this.rotationSyncCompression);
      this.m_PrevPosition = this.m_RigidBody3D.position;
      this.m_PrevRotation = this.transform.rotation;
    }

    private void SerializeModeCharacterController(NetworkWriter writer)
    {
      if (this.isServer && (double) this.m_LastClientSyncTime != 0.0)
      {
        writer.Write(this.m_TargetSyncPosition);
        if (this.syncRotationAxis != NetworkTransform.AxisSyncMode.None)
          this.SerializeRotation3D(writer, this.m_TargetSyncRotation3D, this.syncRotationAxis, this.rotationSyncCompression);
      }
      else
      {
        writer.Write(this.transform.position);
        if (this.syncRotationAxis != NetworkTransform.AxisSyncMode.None)
          this.SerializeRotation3D(writer, this.transform.rotation, this.syncRotationAxis, this.rotationSyncCompression);
      }
      this.m_PrevPosition = this.transform.position;
      this.m_PrevRotation = this.transform.rotation;
    }

    private void SerializeMode2D(NetworkWriter writer)
    {
      if (this.isServer && (double) this.m_LastClientSyncTime != 0.0)
      {
        writer.Write((Vector2) this.m_TargetSyncPosition);
        this.SerializeVelocity2D(writer, (Vector2) this.m_TargetSyncVelocity, NetworkTransform.CompressionSyncMode.None);
        if (this.syncRotationAxis != NetworkTransform.AxisSyncMode.None)
        {
          float rot = this.m_TargetSyncRotation2D % 360f;
          if ((double) rot < 0.0)
            rot += 360f;
          this.SerializeRotation2D(writer, rot, this.rotationSyncCompression);
        }
      }
      else
      {
        writer.Write(this.m_RigidBody2D.position);
        this.SerializeVelocity2D(writer, this.m_RigidBody2D.velocity, NetworkTransform.CompressionSyncMode.None);
        if (this.syncRotationAxis != NetworkTransform.AxisSyncMode.None)
        {
          float rot = this.m_RigidBody2D.rotation % 360f;
          if ((double) rot < 0.0)
            rot += 360f;
          this.SerializeRotation2D(writer, rot, this.rotationSyncCompression);
        }
      }
      if (this.m_SyncSpin)
        this.SerializeSpin2D(writer, this.m_RigidBody2D.angularVelocity, this.rotationSyncCompression);
      this.m_PrevPosition = (Vector3) this.m_RigidBody2D.position;
      this.m_PrevRotation = this.transform.rotation;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
      if (this.isServer && NetworkServer.localClientActive || !initialState && (int) reader.ReadPackedUInt32() == 0 || this.hasAuthority)
        return;
      switch (this.transformSyncMode)
      {
        case NetworkTransform.TransformSyncMode.SyncNone:
          return;
        case NetworkTransform.TransformSyncMode.SyncTransform:
          this.UnserializeModeTransform(reader, initialState);
          break;
        case NetworkTransform.TransformSyncMode.SyncRigidbody2D:
          this.UnserializeMode2D(reader, initialState);
          break;
        case NetworkTransform.TransformSyncMode.SyncRigidbody3D:
          this.UnserializeMode3D(reader, initialState);
          break;
        case NetworkTransform.TransformSyncMode.SyncCharacterController:
          this.UnserializeModeCharacterController(reader, initialState);
          break;
      }
      this.m_LastClientSyncTime = Time.time;
    }

    private void UnserializeModeTransform(NetworkReader reader, bool initialState)
    {
      this.transform.position = reader.ReadVector3();
      if (this.syncRotationAxis == NetworkTransform.AxisSyncMode.None)
        return;
      this.transform.rotation = this.UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
    }

    private void UnserializeMode3D(NetworkReader reader, bool initialState)
    {
      if ((UnityEngine.Object) this.m_RigidBody3D == (UnityEngine.Object) null)
        return;
      this.m_TargetSyncPosition = reader.ReadVector3();
      this.m_TargetSyncVelocity = reader.ReadVector3();
      if (this.syncRotationAxis != NetworkTransform.AxisSyncMode.None)
        this.m_TargetSyncRotation3D = this.UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
      if (this.syncSpin)
        this.m_TargetSyncAngularVelocity3D = this.UnserializeSpin3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
      if (this.isServer && !this.isClient)
      {
        this.m_RigidBody3D.MovePosition(this.m_TargetSyncPosition);
        this.m_RigidBody3D.MoveRotation(this.m_TargetSyncRotation3D);
        this.m_RigidBody3D.velocity = this.m_TargetSyncVelocity;
      }
      else if ((double) this.GetNetworkSendInterval() == 0.0)
      {
        this.m_RigidBody3D.MovePosition(this.m_TargetSyncPosition);
        this.m_RigidBody3D.velocity = this.m_TargetSyncVelocity;
        if (this.syncRotationAxis != NetworkTransform.AxisSyncMode.None)
          this.m_RigidBody3D.MoveRotation(this.m_TargetSyncRotation3D);
        if (!this.syncSpin)
          return;
        this.m_RigidBody3D.angularVelocity = this.m_TargetSyncAngularVelocity3D;
      }
      else
      {
        if ((double) (this.m_RigidBody3D.position - this.m_TargetSyncPosition).magnitude > (double) this.snapThreshold)
        {
          this.m_RigidBody3D.position = this.m_TargetSyncPosition;
          this.m_RigidBody3D.velocity = this.m_TargetSyncVelocity;
        }
        if ((double) this.interpolateRotation == 0.0)
        {
          this.m_RigidBody3D.rotation = this.m_TargetSyncRotation3D;
          if (this.syncSpin)
            this.m_RigidBody3D.angularVelocity = this.m_TargetSyncAngularVelocity3D;
        }
        if ((double) this.m_InterpolateMovement == 0.0)
          this.m_RigidBody3D.position = this.m_TargetSyncPosition;
        if (!initialState)
          return;
        this.m_RigidBody3D.rotation = this.m_TargetSyncRotation3D;
      }
    }

    private void UnserializeMode2D(NetworkReader reader, bool initialState)
    {
      if ((UnityEngine.Object) this.m_RigidBody2D == (UnityEngine.Object) null)
        return;
      this.m_TargetSyncPosition = (Vector3) reader.ReadVector2();
      this.m_TargetSyncVelocity = (Vector3) reader.ReadVector2();
      if (this.syncRotationAxis != NetworkTransform.AxisSyncMode.None)
        this.m_TargetSyncRotation2D = this.UnserializeRotation2D(reader, this.rotationSyncCompression);
      if (this.syncSpin)

        this.m_TargetSyncAngularVelocity2D = this.UnserializeSpin2D(reader, this.rotationSyncCompression);
      if (this.isServer && !this.isClient)
      {
        this.m_RigidBody2D.MovePosition((Vector2) this.m_TargetSyncPosition);
        this.m_RigidBody2D.MoveRotation(this.m_TargetSyncRotation2D);
        this.m_RigidBody2D.velocity = (Vector2) this.m_TargetSyncVelocity;
      }
      else if ((double) this.GetNetworkSendInterval() == 0.0)
      {
        this.m_RigidBody2D.MovePosition((Vector2) this.m_TargetSyncPosition);
        this.m_RigidBody2D.velocity = (Vector2) this.m_TargetSyncVelocity;
        if (this.syncRotationAxis != NetworkTransform.AxisSyncMode.None)
          this.m_RigidBody2D.MoveRotation(this.m_TargetSyncRotation2D);
        if (!this.syncSpin)
          return;
        this.m_RigidBody2D.angularVelocity = this.m_TargetSyncAngularVelocity2D;
      }
      else
      {
        if ((double) (this.m_RigidBody2D.position - (Vector2) this.m_TargetSyncPosition).magnitude > (double) this.snapThreshold)
        {
          this.m_RigidBody2D.position = (Vector2) this.m_TargetSyncPosition;
          this.m_RigidBody2D.velocity = (Vector2) this.m_TargetSyncVelocity;
        }
        if ((double) this.interpolateRotation == 0.0)
        {
          this.m_RigidBody2D.rotation = this.m_TargetSyncRotation2D;
          if (this.syncSpin)
            this.m_RigidBody2D.angularVelocity = this.m_TargetSyncAngularVelocity2D;
        }
        if ((double) this.m_InterpolateMovement == 0.0)
          this.m_RigidBody2D.position = (Vector2) this.m_TargetSyncPosition;
        if (!initialState)
          return;
        this.m_RigidBody2D.rotation = this.m_TargetSyncRotation2D;
      }
    }

    private void UnserializeModeCharacterController(NetworkReader reader, bool initialState)
    {
      if ((UnityEngine.Object) this.m_CharacterController == (UnityEngine.Object) null)
        return;
      this.m_TargetSyncPosition = reader.ReadVector3();
      this.m_FixedPosDiff = (this.m_TargetSyncPosition - this.transform.position) / this.GetNetworkSendInterval() * Time.fixedDeltaTime;
      if (this.syncRotationAxis != NetworkTransform.AxisSyncMode.None)
        this.m_TargetSyncRotation3D = this.UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
      if (this.isServer && !this.isClient)
      {
        this.transform.position = this.m_TargetSyncPosition;
        this.transform.rotation = this.m_TargetSyncRotation3D;
      }
      else if ((double) this.GetNetworkSendInterval() == 0.0)
      {
        this.transform.position = this.m_TargetSyncPosition;
        if (this.syncRotationAxis == NetworkTransform.AxisSyncMode.None)
          return;
        this.transform.rotation = this.m_TargetSyncRotation3D;
      }
      else
      {
        if ((double) (this.transform.position - this.m_TargetSyncPosition).magnitude > (double) this.snapThreshold)
          this.transform.position = this.m_TargetSyncPosition;
        if ((double) this.interpolateRotation == 0.0)
          this.transform.rotation = this.m_TargetSyncRotation3D;
        if ((double) this.m_InterpolateMovement == 0.0)
          this.transform.position = this.m_TargetSyncPosition;
        if (!initialState)
          return;
        this.transform.rotation = this.m_TargetSyncRotation3D;
      }
    }

    private void FixedUpdate()
    {
      if (this.isServer)
        this.FixedUpdateServer();
      if (!this.isClient)
        return;
      this.FixedUpdateClient();
    }

    private void FixedUpdateServer()
    {
      if ((int) this.syncVarDirtyBits != 0 || !NetworkServer.active || (!this.isServer || (double) this.GetNetworkSendInterval() == 0.0) || (double) (this.transform.position - this.m_PrevPosition).magnitude < (double) this.movementTheshold && (double) Quaternion.Angle(this.m_PrevRotation, this.transform.rotation) < (double) this.movementTheshold)
        return;
      this.SetDirtyBit(1U);
    }

    private void FixedUpdateClient()
    {
      if ((double) this.m_LastClientSyncTime == 0.0 || !NetworkServer.active && !NetworkClient.active || (!this.isServer && !this.isClient || ((double) this.GetNetworkSendInterval() == 0.0 || this.hasAuthority)))
        return;
      switch (this.transformSyncMode)
      {
        case NetworkTransform.TransformSyncMode.SyncRigidbody2D:
          this.InterpolateTransformMode2D();
          break;
        case NetworkTransform.TransformSyncMode.SyncRigidbody3D:
          this.InterpolateTransformMode3D();
          break;
        case NetworkTransform.TransformSyncMode.SyncCharacterController:
          this.InterpolateTransformModeCharacterController();
          break;
      }
    }

    private void InterpolateTransformMode3D()
    {
      if ((double) this.m_InterpolateMovement != 0.0)
        this.m_RigidBody3D.velocity = (this.m_TargetSyncPosition - this.m_RigidBody3D.position) * this.m_InterpolateMovement / this.GetNetworkSendInterval();
      if ((double) this.interpolateRotation != 0.0)
        this.m_RigidBody3D.MoveRotation(Quaternion.Slerp(this.m_RigidBody3D.rotation, this.m_TargetSyncRotation3D, Time.fixedDeltaTime * this.interpolateRotation));
      this.m_TargetSyncPosition += this.m_TargetSyncVelocity * Time.fixedDeltaTime * 0.1f;
    }

    private void InterpolateTransformModeCharacterController()
    {
      if (this.m_FixedPosDiff == Vector3.zero)
        return;
      if ((double) this.m_InterpolateMovement != 0.0)
      {
        int num1 = (int) this.m_CharacterController.Move(this.m_FixedPosDiff * this.m_InterpolateMovement);
      }
      if ((double) this.interpolateRotation != 0.0)
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, this.m_TargetSyncRotation3D, (float) ((double) Time.fixedDeltaTime * (double) this.interpolateRotation * 10.0));
      if ((double) Time.time - (double) this.m_LastClientSyncTime <= (double) this.GetNetworkSendInterval())
        return;
      this.m_FixedPosDiff = Vector3.zero;
      int num2 = (int) this.m_CharacterController.Move(this.m_TargetSyncPosition - this.transform.position);
    }

    private void InterpolateTransformMode2D()
    {
      if ((double) this.m_InterpolateMovement != 0.0)
      {
        Vector2 velocity = this.m_RigidBody2D.velocity;
        Vector2 vector2 = ((Vector2) this.m_TargetSyncPosition - this.m_RigidBody2D.position) * this.m_InterpolateMovement / this.GetNetworkSendInterval();
        if (!this.m_Grounded && (double) vector2.y < 0.0)
          vector2.y = velocity.y;
        this.m_RigidBody2D.velocity = vector2;
      }
      if ((double) this.interpolateRotation != 0.0)
      {
        float num1 = this.m_RigidBody2D.rotation % 360f;
        if ((double) num1 < 0.0)
        {
          float num2 = num1 + 360f;
        }
        this.m_RigidBody2D.MoveRotation(Quaternion.Slerp(this.transform.rotation, Quaternion.Euler(0.0f, 0.0f, this.m_TargetSyncRotation2D), Time.fixedDeltaTime * this.interpolateRotation / this.GetNetworkSendInterval()).eulerAngles.z);
        this.m_TargetSyncRotation2D += (float) ((double) this.m_TargetSyncAngularVelocity2D * (double) Time.fixedDeltaTime * 0.100000001490116);
      }
      this.m_TargetSyncPosition += this.m_TargetSyncVelocity * Time.fixedDeltaTime * 0.1f;
    }

    private void Update()
    {
      if (!this.hasAuthority || !this.localPlayerAuthority || (NetworkServer.active || (double) Time.time - (double) this.m_LastClientSendTime <= (double) this.GetNetworkSendInterval()))
        return;
      this.SendTransform();
      this.m_LastClientSendTime = Time.time;
    }

    private bool HasMoved()
    {
      return (!((UnityEngine.Object) this.m_RigidBody3D != (UnityEngine.Object) null) ? (!((UnityEngine.Object) this.m_RigidBody2D != (UnityEngine.Object) null) ? (double) (this.transform.position - this.m_PrevPosition).magnitude : (double) (this.m_RigidBody2D.position - (Vector2) this.m_PrevPosition).magnitude) : (double) (this.m_RigidBody3D.position - this.m_PrevPosition).magnitude) > 9.99999974737875E-06 || (!((UnityEngine.Object) this.m_RigidBody3D != (UnityEngine.Object) null) ? (!((UnityEngine.Object) this.m_RigidBody2D != (UnityEngine.Object) null) ? (double) Quaternion.Angle(this.transform.rotation, this.m_PrevRotation) : (double) Math.Abs(this.m_RigidBody2D.rotation - this.m_PrevRotation2D)) : (double) Quaternion.Angle(this.m_RigidBody3D.rotation, this.m_PrevRotation)) > 9.99999974737875E-06;
    }

    [Client]
    private void SendTransform()
    {
      if (!this.HasMoved())
        return;
      this.m_LocalTransformWriter.StartMessage((short) 6);
      this.m_LocalTransformWriter.Write(this.netId);
      switch (this.transformSyncMode)
      {
        case NetworkTransform.TransformSyncMode.SyncNone:
          return;
        case NetworkTransform.TransformSyncMode.SyncTransform:
          this.SerializeModeTransform(this.m_LocalTransformWriter);
          break;
        case NetworkTransform.TransformSyncMode.SyncRigidbody2D:
          this.SerializeMode2D(this.m_LocalTransformWriter);
          break;
        case NetworkTransform.TransformSyncMode.SyncRigidbody3D:
          this.SerializeMode3D(this.m_LocalTransformWriter);
          break;
        case NetworkTransform.TransformSyncMode.SyncCharacterController:
          this.SerializeModeCharacterController(this.m_LocalTransformWriter);
          break;
      }
      if ((UnityEngine.Object) this.m_RigidBody3D != (UnityEngine.Object) null)
      {
        this.m_PrevPosition = this.m_RigidBody3D.position;
        this.m_PrevRotation = this.m_RigidBody3D.rotation;
      }
      else if ((UnityEngine.Object) this.m_RigidBody2D != (UnityEngine.Object) null)
      {
        this.m_PrevPosition = (Vector3) this.m_RigidBody2D.position;
        this.m_PrevRotation2D = this.m_RigidBody2D.rotation;
      }
      else
      {
        this.m_PrevPosition = this.transform.position;
        this.m_PrevRotation = this.transform.rotation;
      }
      this.m_LocalTransformWriter.FinishMessage();
      NetworkClient.allClients[0].SendWriter(this.m_LocalTransformWriter, this.GetNetworkChannel());
    }

    public static void HandleTransform(NetworkMessage netMsg)
    {
      NetworkInstanceId netId = netMsg.reader.ReadNetworkId();
      GameObject localObject = NetworkServer.FindLocalObject(netId);
      if ((UnityEngine.Object) localObject == (UnityEngine.Object) null)
        return;
      NetworkTransform component = localObject.GetComponent<NetworkTransform>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        return;
      using (List<PlayerController>.Enumerator enumerator = netMsg.conn.playerControllers.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          if (enumerator.Current.unetView.netId == netId)
          {
            switch (component.transformSyncMode)
            {
              case NetworkTransform.TransformSyncMode.SyncNone:
                return;
              case NetworkTransform.TransformSyncMode.SyncTransform:
                component.UnserializeModeTransform(netMsg.reader, false);
                break;
              case NetworkTransform.TransformSyncMode.SyncRigidbody2D:
                component.UnserializeMode2D(netMsg.reader, false);
                break;
              case NetworkTransform.TransformSyncMode.SyncRigidbody3D:
                component.UnserializeMode3D(netMsg.reader, false);
                break;
              case NetworkTransform.TransformSyncMode.SyncCharacterController:
                component.UnserializeModeCharacterController(netMsg.reader, false);
                break;
            }
            component.m_LastClientSyncTime = Time.time;
            return;
          }
        }
      }
      if (!LogFilter.logWarn)
        return;
      Debug.LogWarning((object) ("HandleTransform netId:" + (object) netId + " is not for a valid player"));
    }

    private void WriteAngle(NetworkWriter writer, float angle, NetworkTransform.CompressionSyncMode compression)
    {
      switch (compression)
      {
        case NetworkTransform.CompressionSyncMode.None:
          writer.Write(angle);
          break;
        case NetworkTransform.CompressionSyncMode.Low:
          writer.Write((short) angle);
          break;
        case NetworkTransform.CompressionSyncMode.High:
          writer.Write((short) angle);
          break;
      }
    }

    private float ReadAngle(NetworkReader reader, NetworkTransform.CompressionSyncMode compression)
    {
      switch (compression)
      {
        case NetworkTransform.CompressionSyncMode.None:
          return reader.ReadSingle();
        case NetworkTransform.CompressionSyncMode.Low:
          return (float) reader.ReadInt16();
        case NetworkTransform.CompressionSyncMode.High:
          return (float) reader.ReadInt16();
        default:
          return 0.0f;
      }
    }

    public void SerializeVelocity3D(NetworkWriter writer, Vector3 velocity, NetworkTransform.CompressionSyncMode compression)
    {
      writer.Write(velocity);
    }

    public void SerializeVelocity2D(NetworkWriter writer, Vector2 velocity, NetworkTransform.CompressionSyncMode compression)
    {
      writer.Write(velocity);
    }

    public void SerializeRotation3D(NetworkWriter writer, Quaternion rot, NetworkTransform.AxisSyncMode mode, NetworkTransform.CompressionSyncMode compression)
    {
      switch (mode)
      {
        case NetworkTransform.AxisSyncMode.AxisX:
          this.WriteAngle(writer, rot.eulerAngles.x, compression);
          break;
        case NetworkTransform.AxisSyncMode.AxisY:
          this.WriteAngle(writer, rot.eulerAngles.y, compression);
          break;
        case NetworkTransform.AxisSyncMode.AxisZ:
          this.WriteAngle(writer, rot.eulerAngles.z, compression);
          break;
        case NetworkTransform.AxisSyncMode.AxisXY:
          this.WriteAngle(writer, rot.eulerAngles.x, compression);
          this.WriteAngle(writer, rot.eulerAngles.y, compression);
          break;
        case NetworkTransform.AxisSyncMode.AxisXZ:
          this.WriteAngle(writer, rot.eulerAngles.x, compression);
          this.WriteAngle(writer, rot.eulerAngles.z, compression);
          break;
        case NetworkTransform.AxisSyncMode.AxisYZ:
          this.WriteAngle(writer, rot.eulerAngles.y, compression);
          this.WriteAngle(writer, rot.eulerAngles.z, compression);
          break;
        case NetworkTransform.AxisSyncMode.AxisXYZ:
          this.WriteAngle(writer, rot.eulerAngles.x, compression);
          this.WriteAngle(writer, rot.eulerAngles.y, compression);
          this.WriteAngle(writer, rot.eulerAngles.z, compression);
          break;
      }
    }

    public void SerializeRotation2D(NetworkWriter writer, float rot, NetworkTransform.CompressionSyncMode compression)
    {
      this.WriteAngle(writer, rot, compression);
    }

    public void SerializeSpin3D(NetworkWriter writer, Vector3 angularVelocity, NetworkTransform.AxisSyncMode mode, NetworkTransform.CompressionSyncMode compression)
    {
      switch (mode)
      {
        case NetworkTransform.AxisSyncMode.AxisX:
          this.WriteAngle(writer, angularVelocity.x, compression);
          break;
        case NetworkTransform.AxisSyncMode.AxisY:
          this.WriteAngle(writer, angularVelocity.y, compression);
          break;
        case NetworkTransform.AxisSyncMode.AxisZ:
          this.WriteAngle(writer, angularVelocity.z, compression);
          break;
        case NetworkTransform.AxisSyncMode.AxisXY:
          this.WriteAngle(writer, angularVelocity.x, compression);
          this.WriteAngle(writer, angularVelocity.y, compression);
          break;
        case NetworkTransform.AxisSyncMode.AxisXZ:
          this.WriteAngle(writer, angularVelocity.x, compression);
          this.WriteAngle(writer, angularVelocity.z, compression);
          break;
        case NetworkTransform.AxisSyncMode.AxisYZ:
          this.WriteAngle(writer, angularVelocity.y, compression);
          this.WriteAngle(writer, angularVelocity.z, compression);
          break;
        case NetworkTransform.AxisSyncMode.AxisXYZ:
          this.WriteAngle(writer, angularVelocity.x, compression);
          this.WriteAngle(writer, angularVelocity.y, compression);
          this.WriteAngle(writer, angularVelocity.z, compression);
          break;
      }
    }

    public void SerializeSpin2D(NetworkWriter writer, float angularVelocity, NetworkTransform.CompressionSyncMode compression)
    {
      this.WriteAngle(writer, angularVelocity, compression);
    }

    public Vector3 UnserializeVelocity3D(NetworkReader reader, NetworkTransform.CompressionSyncMode compression)
    {
      return reader.ReadVector3();
    }

    public Vector3 UnserializeVelocity2D(NetworkReader reader, NetworkTransform.CompressionSyncMode compression)
    {
      return (Vector3) reader.ReadVector2();
    }

    public Quaternion UnserializeRotation3D(NetworkReader reader, NetworkTransform.AxisSyncMode mode, NetworkTransform.CompressionSyncMode compression)
    {
      Quaternion identity = Quaternion.identity;
      Vector3 zero = Vector3.zero;
      switch (mode)
      {
        case NetworkTransform.AxisSyncMode.AxisX:
          zero.Set(this.ReadAngle(reader, compression), 0.0f, 0.0f);
          identity.eulerAngles = zero;
          break;
        case NetworkTransform.AxisSyncMode.AxisY:
          zero.Set(0.0f, this.ReadAngle(reader, compression), 0.0f);
          identity.eulerAngles = zero;
          break;
        case NetworkTransform.AxisSyncMode.AxisZ:
          zero.Set(0.0f, 0.0f, this.ReadAngle(reader, compression));
          identity.eulerAngles = zero;
          break;
        case NetworkTransform.AxisSyncMode.AxisXY:
          zero.Set(this.ReadAngle(reader, compression), this.ReadAngle(reader, compression), 0.0f);
          identity.eulerAngles = zero;
          break;
        case NetworkTransform.AxisSyncMode.AxisXZ:
          zero.Set(this.ReadAngle(reader, compression), 0.0f, this.ReadAngle(reader, compression));
          identity.eulerAngles = zero;
          break;
        case NetworkTransform.AxisSyncMode.AxisYZ:
          zero.Set(0.0f, this.ReadAngle(reader, compression), this.ReadAngle(reader, compression));
          identity.eulerAngles = zero;
          break;
        case NetworkTransform.AxisSyncMode.AxisXYZ:
          zero.Set(this.ReadAngle(reader, compression), this.ReadAngle(reader, compression), this.ReadAngle(reader, compression));
          identity.eulerAngles = zero;
          break;
      }
      return identity;
    }

    public float UnserializeRotation2D(NetworkReader reader, NetworkTransform.CompressionSyncMode compression)
    {
      return this.ReadAngle(reader, compression);
    }

    public Vector3 UnserializeSpin3D(NetworkReader reader, NetworkTransform.AxisSyncMode mode, NetworkTransform.CompressionSyncMode compression)
    {
      Vector3 zero = Vector3.zero;
      switch (mode)
      {
        case NetworkTransform.AxisSyncMode.AxisX:
          zero.Set(this.ReadAngle(reader, compression), 0.0f, 0.0f);
          break;
        case NetworkTransform.AxisSyncMode.AxisY:
          zero.Set(0.0f, this.ReadAngle(reader, compression), 0.0f);
          break;
        case NetworkTransform.AxisSyncMode.AxisZ:
          zero.Set(0.0f, 0.0f, this.ReadAngle(reader, compression));
          break;
        case NetworkTransform.AxisSyncMode.AxisXY:
          zero.Set(this.ReadAngle(reader, compression), this.ReadAngle(reader, compression), 0.0f);
          break;
        case NetworkTransform.AxisSyncMode.AxisXZ:
          zero.Set(this.ReadAngle(reader, compression), 0.0f, this.ReadAngle(reader, compression));
          break;
        case NetworkTransform.AxisSyncMode.AxisYZ:
          zero.Set(0.0f, this.ReadAngle(reader, compression), this.ReadAngle(reader, compression));
          break;
        case NetworkTransform.AxisSyncMode.AxisXYZ:
          zero.Set(this.ReadAngle(reader, compression), this.ReadAngle(reader, compression), this.ReadAngle(reader, compression));
          break;
      }
      return zero;
    }

    public float UnserializeSpin2D(NetworkReader reader, NetworkTransform.CompressionSyncMode compression)
    {
      return this.ReadAngle(reader, compression);
    }

    public override int GetNetworkChannel()
    {
      return 1;
    }

    public override float GetNetworkSendInterval()
    {
      return this.m_SendInterval;
    }

    /// <summary>
    /// 
    /// <para>
    /// How to synchronize an object's position.
    /// </para>
    /// 
    /// </summary>
    public enum TransformSyncMode
    {
      SyncNone,
      SyncTransform,
      SyncRigidbody2D,
      SyncRigidbody3D,
      SyncCharacterController,
    }

    /// <summary>
    /// 
    /// <para>
    /// An axis or set of axis.
    /// </para>
    /// 
    /// </summary>
    public enum AxisSyncMode
    {
      None,
      AxisX,
      AxisY,
      AxisZ,
      AxisXY,
      AxisXZ,
      AxisYZ,
      AxisXYZ,
    }

    /// <summary>
    /// 
    /// <para>
    /// How much to compress sync data.
    /// </para>
    /// 
    /// </summary>
    public enum CompressionSyncMode
    {
      None,
      Low,
      High,
    }
  }
}
