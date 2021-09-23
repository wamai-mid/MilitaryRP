using Sandbox;
using System;
using System.Collections.Generic;

[Library( "ent_car_ferrari", Title = "Ferrari", Spawnable = true )]
public partial class FerrariEntity : Prop, IUse
{
  	[ConVar.Replicated( "debug_car" )]
	public static bool debug_car { get; set; } = false;

	private FerrariWheel frontLeft;
	private FerrariWheel frontRight;
	private FerrariWheel backLeft;
	private FerrariWheel backRight;

	private float frontLeftDistance;
	private float frontRightDistance;
	private float backLeftDistance;
	private float backRightDistance;

	private bool frontWheelsOnGround;
	private bool backWheelsOnGround;
	private float accelerateDirection;

	[Net] private float WheelSpeed { get; set; }
	[Net] private float TurnDirection { get; set; }
	[Net] private float AccelerationTilt { get; set; }
	[Net] private float TurnLean { get; set; }

	[Net]
	public float MovementSpeed { get; private set; }

	private struct InputState
	{
		public float throttle;
		public float turning;
		public float breaking;
		public bool airControl;

		public void Reset()
		{
			throttle = 0;
			turning = 0;
			breaking = 0;
			airControl = false;
		}
	}

	private InputState currentInput;

	public FerrariEntity()
	{
		frontLeft = new FerrariWheel( this );
		frontRight = new FerrariWheel( this );
		backLeft = new FerrariWheel( this );
		backRight = new FerrariWheel( this );
	}

	private Player driver;

	private ModelEntity chassis_axle_rear;
	private ModelEntity chassis_axle_front;
	private ModelEntity wheel0;
	private ModelEntity wheel1;
	private ModelEntity wheel2;
	private ModelEntity wheel3;

	private readonly List<ModelEntity> clientModels = new();
	public override void Spawn()
	{
		base.Spawn();
		var mdl = "models/cars/Ferrari/ferrari";
		SetModel( mdl );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
		SetInteractsExclude( CollisionLayer.Player );

		var trigger = new ModelEntity
		{
			Parent = this,
			Position = Position,
			Rotation = Rotation,
			EnableTouch = true,
			CollisionGroup = CollisionGroup.Trigger,
			Transmit = TransmitType.Never
		};

		trigger.SetModel( mdl );
		trigger.SetupPhysicsFromModel( PhysicsMotionType.Keyframed, false );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		{
			chassis_axle_front = new ModelEntity();
			chassis_axle_front.SetModel( "entities/modular_vehicle/chassis_axle_front.vmdl" );
			chassis_axle_front.Transform = Transform;
			chassis_axle_front.Parent = this;
			chassis_axle_front.LocalPosition = new Vector3( 2.1f, 0, 0.60f ) * 33.0f;
			clientModels.Add( chassis_axle_front );

			{
				wheel0 = new ModelEntity();
				wheel0.SetModel( "models/cars/Ferrari/ferrari_wheel" );
				wheel0.SetParent( chassis_axle_front, "Wheel_Steer_R", new Transform( Vector3.OneX * (-0.6f * 40), Rotation.From( 0, 180, 0 ) ) );
				clientModels.Add( wheel0 );
			}

			{
				wheel1 = new ModelEntity();
				wheel1.SetModel( "models/cars/Ferrari/ferrari_wheel" );
				wheel1.SetParent( chassis_axle_front, "Wheel_Steer_L", new Transform( Vector3.OneX * (0.6f * 40), Rotation.From( 0, 0, 0 ) ) );
				clientModels.Add( wheel1 );
			}

			{
				var chassis_steering = new ModelEntity();
				chassis_steering.SetModel( "entities/modular_vehicle/chassis_steering.vmdl" );
				chassis_steering.SetParent( chassis_axle_front, "Axle_front_Center", new Transform( Vector3.Zero, Rotation.From( -90, 180, 0 ) ) );
				clientModels.Add( chassis_steering );
			}
		}

		{
			chassis_axle_rear = new ModelEntity();
			chassis_axle_rear.SetModel( "entities/modular_vehicle/chassis_axle_rear.vmdl" );
			chassis_axle_rear.Transform = Transform;
			chassis_axle_rear.Parent = this;
			chassis_axle_rear.LocalPosition = new Vector3( -2.64f, 0, 0.59f ) * 34.0f;
			clientModels.Add( chassis_axle_rear );

			{
				var chassis_transmission = new ModelEntity();
				chassis_transmission.SetModel( "entities/modular_vehicle/chassis_transmission.vmdl" );
				chassis_transmission.SetParent( chassis_axle_rear, "Axle_Rear_Center", new Transform( Vector3.Zero, Rotation.From( -90, 180, 0 ) ) );
				clientModels.Add( chassis_transmission );
			}

			{
				wheel2 = new ModelEntity();
				wheel2.SetModel( "models/cars/Ferrari/ferrari_wheel" );
				wheel2.SetParent( chassis_axle_rear, "Axle_Rear_Center", new Transform( Vector3.Left * (1.2f * 40), Rotation.From( 0, 90, 0 ) ) );
				clientModels.Add( wheel2 );
			}

			{
				wheel3 = new ModelEntity();
				wheel3.SetModel( "models/cars/Ferrari/ferrari_wheel" );
				wheel3.SetParent( chassis_axle_rear, "Axle_Rear_Center", new Transform( Vector3.Right * (1.2f * 40), Rotation.From( 0, -90, 0 ) ) );
				clientModels.Add( wheel3 );
			}
    }

	}

 	private void RemoveDriver( SandboxPlayer player )
	{
		driver = null;
		player.Vehicle = null;
		player.VehicleController = null;
		player.VehicleCamera = null;
		player.Tags.Remove( "driving" );

		ResetInput();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( driver is SandboxPlayer player )
		{
			RemoveDriver( player );
		}

		foreach ( var model in clientModels )
		{
			model?.Delete();
		}

		clientModels.Clear();
	}

	public void ResetInput()
	{
		currentInput.Reset();
	}

	[Event.Tick.Server]
	protected void Tick()
	{
		if ( driver is SandboxPlayer player )
		{
			if ( player.LifeState != LifeState.Alive || player.Vehicle != this )
			{
				RemoveDriver( player );
			}
		}
	}

	public override void Simulate( Client owner )
	{
		if ( owner == null ) return;
		if ( !IsServer ) return;

		using ( Prediction.Off() )
		{
			currentInput.Reset();

			if ( Input.Pressed( InputButton.Use ) )
			{
				if ( owner.Pawn is SandboxPlayer player )
				{
					RemoveDriver( player );

					return;
				}
			}

			currentInput.throttle = (Input.Down( InputButton.Forward ) ? 1 : 0) + (Input.Down( InputButton.Back ) ? -1 : 0);
			currentInput.turning = (Input.Down( InputButton.Left ) ? 1 : 0) + (Input.Down( InputButton.Right ) ? -1 : 0);
			currentInput.breaking = (Input.Down( InputButton.Jump ) ? 1 : 0);
			currentInput.airControl = Input.Down( InputButton.Duck );
		}
	}

	[Event.Physics.PreStep]
	public void OnPrePhysicsStep()
	{
		if ( !IsServer )
			return;

		var body = PhysicsBody;
		if ( !body.IsValid() )
			return;

		var dt = Time.Delta;

		body.DragEnabled = false;
		body.LinearDamping = 0;
		body.AngularDamping = (backWheelsOnGround && frontWheelsOnGround) ? 0 : 0.5f;
		body.GravityScale = (backWheelsOnGround && frontWheelsOnGround) ? 0 : 1;

		var rotation = body.Rotation;

		accelerateDirection = currentInput.throttle.Clamp( -1, 1 ) * (1.0f - currentInput.breaking);
		TurnDirection = TurnDirection.LerpTo( currentInput.turning.Clamp( -1, 1 ), 1.0f - MathF.Pow( 0.0075f, dt ) );

		float targetTilt = 0;
		float targetLean = 0;

		var localVelocity = rotation.Inverse * body.Velocity;

		if ( backWheelsOnGround || frontWheelsOnGround )
		{
			var forwardSpeed = MathF.Abs( localVelocity.x );
			var speedFraction = MathF.Min( forwardSpeed / 500.0f, 1 );

			targetTilt = accelerateDirection.Clamp( -1.0f, 1.0f );
			targetLean = speedFraction * TurnDirection;
		}

		AccelerationTilt = AccelerationTilt.LerpTo( targetTilt, 1.0f - MathF.Pow( 0.01f, dt ) );
		TurnLean = TurnLean.LerpTo( targetLean, 1.0f - MathF.Pow( 0.01f, dt ) );

		if ( backWheelsOnGround )
		{
			var forwardSpeed = MathF.Abs( localVelocity.x );
			var speedFactor = 1.0f - (forwardSpeed / 5000.0f).Clamp( 0.0f, 1.0f );
			var acceleration = speedFactor * (accelerateDirection < 0.0f ? 500.0f : 1000.0f) * accelerateDirection * dt;
			body.Velocity += rotation * new Vector3( acceleration, 0, 0 );
		}

		RaycastWheels( rotation, true, out frontWheelsOnGround, out backWheelsOnGround, dt );
		var onGround = frontWheelsOnGround || backWheelsOnGround;

		if ( frontWheelsOnGround && backWheelsOnGround )
		{
			body.Velocity += PhysicsWorld.Gravity * dt;
		}

		if ( onGround )
		{
			float forwardDamping = 0.2f;
			body.Velocity = VelocityDamping( body.Velocity, rotation, new Vector3( forwardDamping.LerpTo( 0.99f, currentInput.breaking ), 1.0f, 0.0f ), dt );

			localVelocity = rotation.Inverse * body.Velocity;
			WheelSpeed = localVelocity.x;
			var turnAmount = frontWheelsOnGround ? (MathF.Sign( localVelocity.x ) * 25.0f * CalculateTurnFactor( TurnDirection, MathF.Abs( localVelocity.x ) ) * dt) : 0.0f;

			body.AngularVelocity += rotation * new Vector3( 0, 0, turnAmount );
			body.AngularVelocity = VelocityDamping( body.AngularVelocity, rotation, new Vector3( 0, 0, 0.999f ), dt );
		}

		localVelocity = rotation.Inverse * body.Velocity;
		MovementSpeed = localVelocity.x;
	}

	private static float CalculateTurnFactor( float direction, float speed )
	{
		var turnFactor = MathF.Min( speed / 500.0f, 1 );
		var yawSpeedFactor = 1.0f - (speed / 1000.0f).Clamp( 0, 0.5f );

		return direction * turnFactor * yawSpeedFactor;
	}

	private static Vector3 VelocityDamping( Vector3 velocity, Rotation rotation, Vector3 damping, float dt )
	{
		var localVelocity = rotation.Inverse * velocity;
		var dampingPow = new Vector3( MathF.Pow( 1.0f - damping.x, dt ), MathF.Pow( 1.0f - damping.y, dt ), MathF.Pow( 1.0f - damping.z, dt ) );
		return rotation * (localVelocity * dampingPow);
	}

	private void RaycastWheels( Rotation rotation, bool doPhysics, out bool frontWheels, out bool backWheels, float dt )
	{
		float forward = 90;
		float right = 50;

		var frontLeftPos = rotation.Forward * forward + rotation.Right * right + rotation.Up * 20;
		var frontRightPos = rotation.Forward * forward - rotation.Right * right + rotation.Up * 20;
		var backLeftPos = -rotation.Forward * forward + rotation.Right * right + rotation.Up * 20;
		var backRightPos = -rotation.Forward * forward - rotation.Right * right + rotation.Up * 20;

		var tiltAmount = AccelerationTilt * 2.5f;
		var leanAmount = TurnLean * 2.5f;

		float length = 30.0f;

		frontWheels =
			frontLeft.Raycast( length + tiltAmount - leanAmount, doPhysics, frontLeftPos * Scale, ref frontLeftDistance, dt ) |
			frontRight.Raycast( length + tiltAmount + leanAmount, doPhysics, frontRightPos * Scale, ref frontRightDistance, dt );

		backWheels =
			backLeft.Raycast( length - tiltAmount - leanAmount, doPhysics, backLeftPos * Scale, ref backLeftDistance, dt ) |
			backRight.Raycast( length - tiltAmount + leanAmount, doPhysics, backRightPos * Scale, ref backRightDistance, dt );
	}

	float wheelAngle = 0.0f;
	float wheelRevolute = 0.0f;

	[Event.Frame]
	public void OnFrame()
	{
		wheelAngle = wheelAngle.LerpTo( CalculateTurnFactor( TurnDirection, Math.Abs( WheelSpeed ) ), 1.0f - MathF.Pow( 0.01f, Time.Delta ) );
		wheelRevolute += (WheelSpeed / (14.0f * Scale)).RadianToDegree() * Time.Delta;

		var wheelRotRight = Rotation.From( -wheelAngle * 70, 180, -wheelRevolute );
		var wheelRotLeft = Rotation.From( wheelAngle * 70, 0, wheelRevolute );
		var wheelRotBackRight = Rotation.From( 0, 90, -wheelRevolute );
		var wheelRotBackLeft = Rotation.From( 0, -90, wheelRevolute );

		RaycastWheels( Rotation, false, out _, out _, Time.Delta );

		float frontOffset = 20.0f - Math.Min( frontLeftDistance, frontRightDistance );
		float backOffset = 20.0f - Math.Min( backLeftDistance, backRightDistance );

		chassis_axle_front.SetBoneTransform( "Axle_front_Center", new Transform( Vector3.Up * frontOffset ), false );
		chassis_axle_rear.SetBoneTransform( "Axle_Rear_Center", new Transform( Vector3.Up * backOffset ), false );

		wheel0.LocalRotation = wheelRotRight;
		wheel1.LocalRotation = wheelRotLeft;
		wheel2.LocalRotation = wheelRotBackRight;
		wheel3.LocalRotation = wheelRotBackLeft;
	}

	public bool OnUse( Entity user )
	{
		if ( user is SandboxPlayer player && player.Vehicle == null )
		{
			player.Vehicle = this;
			player.VehicleController = new FerrariController();
			player.VehicleCamera = new FerrariCamera();
			player.Tags.Add( "driving" );
			driver = player;
		}

		return true;
	}

	public bool IsUsable( Entity user )
	{
		return driver == null;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( !IsServer )
			return;

		var body = PhysicsBody;
		if ( !body.IsValid() )
			return;

		if ( other != driver && other is Player player )
		{
			var speed = body.Velocity.Length;
			var forceOrigin = Position + Rotation.Down * Rand.Float( 20, 30 );
			var velocity = (player.Position - forceOrigin).Normal * speed;
			var angularVelocity = body.AngularVelocity;

			OnPhysicsCollision( new CollisionEventData
			{
				Entity = player,
				Pos = player.Position + Vector3.Up * 50,
				Velocity = velocity,
				PreVelocity = velocity,
				PostVelocity = velocity,
				PreAngularVelocity = angularVelocity,
				Speed = speed,
			} );
		}
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if ( !IsServer )
			return;

		var propData = GetModelPropData();

		var minImpactSpeed = propData.MinImpactDamageSpeed;
		if ( minImpactSpeed <= 0.0f ) minImpactSpeed = 500;

		var impactDmg = propData.ImpactDamage;
		if ( impactDmg <= 0.0f ) impactDmg = 10;

		var speed = eventData.Speed;

		if ( speed > minImpactSpeed )
		{
			if ( eventData.Entity.IsValid() && eventData.Entity != this )
			{
				var damage = speed / minImpactSpeed * impactDmg * 1.2f;
				eventData.Entity.TakeDamage( DamageInfo.Generic( damage )
					.WithFlag( DamageFlags.PhysicsImpact )
					.WithAttacker( driver != null ? driver : this, driver != null ? this : null )
					.WithPosition( eventData.Pos )
					.WithForce( eventData.PreVelocity ) );
			}
		}

		if ( eventData.Entity is SandboxPlayer player && player.Vehicle == null )
		{
			if ( player.LifeState == LifeState.Dead )
			{
				Particles.Create( "particles/impact.flesh.bloodpuff-big.vpcf", eventData.Pos );
				Particles.Create( "particles/impact.flesh-big.vpcf", eventData.Pos );
				PlaySound( "kersplat" );
			}
		}
	}
}
