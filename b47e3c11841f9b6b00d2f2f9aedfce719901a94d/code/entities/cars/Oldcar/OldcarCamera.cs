using Sandbox;
using System;

public class OldcarCamera : Camera
{
	protected virtual float MinFov => 80.0f;
	protected virtual float MaxFov => 100.0f;
	protected virtual float MaxFovSpeed => 1000.0f;
	protected virtual float FovSmoothingSpeed => 4.0f;
	protected virtual float OrbitCooldown => 0.6f;
	protected virtual float OrbitSmoothingSpeed => 25.0f;
	protected virtual float OrbitReturnSmoothingSpeed => 3.0f;
	protected virtual float MinOrbitPitch => -25.0f;
	protected virtual float MaxOrbitPitch => 70.0f;
	protected virtual float FixedOrbitPitch => 10.0f;
	protected virtual float OrbitHeight => 60.0f;
	protected virtual float OrbitDistance => 250.0f;
	protected virtual float MaxOrbitReturnSpeed => 100.0f;

	private bool orbitEnabled;
	private TimeSince timeSinceOrbit;
	private Angles orbitAngles;
	private Rotation orbitYawRot;
	private Rotation orbitPitchRot;
	private float currentFov;

	public override void Activated()
	{
		var pawn = Local.Pawn;
		if ( pawn == null ) return;

		orbitEnabled = false;
		timeSinceOrbit = 0.0f;
		orbitAngles = Angles.Zero;
		orbitYawRot = Rotation.Identity;
		orbitPitchRot = Rotation.Identity;
		currentFov = MinFov;
	}

	public override void Update()
	{
		var pawn = Local.Pawn;
		if ( pawn == null ) return;

		var car = (pawn as SandboxPlayer)?.Vehicle as OldcarEntity;
		if ( !car.IsValid() ) return;

		Viewer = null;

		if ( orbitEnabled && timeSinceOrbit > OrbitCooldown )
		{
			orbitEnabled = false;
		}

		var speed = car.IsValid() ? car.MovementSpeed : 0.0f;
		var speedAbs = Math.Abs( speed );

		var carPos = car.Position;
		var carRot = car.Rotation;

		if ( orbitEnabled )
		{
			var slerpAmount = Time.Delta * OrbitSmoothingSpeed;

			orbitYawRot = Rotation.Slerp( orbitYawRot, Rotation.From( 0.0f, orbitAngles.yaw, 0.0f ), slerpAmount );
			orbitPitchRot = Rotation.Slerp( orbitPitchRot, Rotation.From( orbitAngles.pitch, 0.0f, 0.0f ), slerpAmount );
		}
		else
		{
			var targetPitch = FixedOrbitPitch.Clamp( MinOrbitPitch, MaxOrbitPitch );
			var targetYaw = speed < 0.0f ? carRot.Yaw() + 180.0f : carRot.Yaw();

			var slerpAmount = MaxOrbitReturnSpeed > 0.0f ? Time.Delta * (speedAbs / MaxOrbitReturnSpeed).Clamp( 0.0f, OrbitReturnSmoothingSpeed ) : 1.0f;

			orbitYawRot = Rotation.Slerp( orbitYawRot, Rotation.From( 0.0f, targetYaw, 0.0f ), slerpAmount );
			orbitPitchRot = Rotation.Slerp( orbitPitchRot, Rotation.From( targetPitch, 0.0f, 0.0f ), slerpAmount );

			orbitAngles.pitch = orbitPitchRot.Pitch();
			orbitAngles.yaw = orbitYawRot.Yaw();
			orbitAngles = orbitAngles.Normal;
		}

		Rot = orbitYawRot * orbitPitchRot;

		var startPos = carPos + carRot.Up * (OrbitHeight * car.Scale);
		var targetPos = startPos + Rot.Backward * (OrbitDistance * car.Scale);

		var tr = Trace.Ray( startPos, targetPos )
			.Ignore( car )
			.Radius( 8.0f )
			.WorldOnly()
			.Run();

		Pos = tr.EndPos;

		currentFov = MaxFovSpeed > 0.0f ? currentFov.LerpTo( MinFov.LerpTo( MaxFov, speedAbs / MaxFovSpeed ), Time.Delta * FovSmoothingSpeed ) : MaxFov;
		FieldOfView = currentFov;
	}

	public override void BuildInput( InputBuilder input )
	{
		base.BuildInput( input );

		var pawn = Local.Pawn;
		if ( pawn == null ) return;

		if ( (Math.Abs( input.AnalogLook.pitch ) + Math.Abs( input.AnalogLook.yaw )) > 0.0f )
		{
			if ( !orbitEnabled )
			{
				orbitAngles = Rot.Angles();
				orbitAngles = orbitAngles.Normal;

				orbitYawRot = Rotation.From( 0.0f, orbitAngles.yaw, 0.0f );
				orbitPitchRot = Rotation.From( orbitAngles.pitch, 0.0f, 0.0f );
			}

			orbitEnabled = true;
			timeSinceOrbit = 0.0f;

			orbitAngles.yaw += input.AnalogLook.yaw;
			orbitAngles.pitch += input.AnalogLook.pitch;
			orbitAngles = orbitAngles.Normal;
			orbitAngles.pitch = orbitAngles.pitch.Clamp( MinOrbitPitch, MaxOrbitPitch );
		}

		input.ViewAngles = orbitAngles.WithYaw( orbitAngles.yaw );
		input.ViewAngles = input.ViewAngles.Normal;
	}
}