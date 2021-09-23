using Sandbox;
using System;
partial class SandboxPlayer
{
	[Net] public float Speed { get; set; }
	[Net] public bool IsStopped { get; set; }
	[Net] public bool IsWalk { get; set; }

	public void SpeedInit()
	{
		Speed = 0.0f;
		IsStopped = false;
		IsWalk = false;
	}

	public void SpeedCalculate()
	{
		if ( IsServer )
		{
			var tspeed = ((Velocity.Length / 100) * 1.60934);
			Speed = (float)tspeed;
			IsStopped = tspeed == 0 ? true : false;
			IsWalk = IsStopped ? false : (tspeed > 4 ? false : true);
		}
	}


}