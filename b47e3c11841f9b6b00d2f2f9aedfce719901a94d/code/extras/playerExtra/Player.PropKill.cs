using Sandbox;
using System;

partial class SandboxPlayer
{
	[Net] public string[] AntiPK { get; } = { "prop_physics" }; // Todo Conf. File
	[Net] public bool PropKillEnabled { get; set; }
	[Net] public bool PKLockSwitch { get; set; }
	[Net] public int PKLockWait { get; set; }


	public void PropKillInit()
	{
		PropKillEnabled = false;
	}

	public bool AntiPropKill( DamageInfo info )
	{
		if ( info.Attacker == null ) return false;
		if ( Array.Exists( AntiPK, element => element == info.Attacker.ToString() ) && PropKillEnabled == false ) return true;
		return false;
	}

	public async void TempPKLockSwitchInc( int i )
	{
		if ( i == 0 ) { PKLockWait = 0; return; }
		await Task.Delay( 1000 );
		i--;
		PKLockWait = i;
		TempPKLockSwitchInc( i );
		return;
	}

	public async void TempPKLockSwitch()
	{
		var waitS = 20;
		PKLockWait = waitS;
		TempPKLockSwitchInc( waitS );
		PKLockSwitch = true;
		await Task.Delay( waitS * 1000 );
		PKLockSwitch = false;
	}



	[ServerCmd( "propkill_on" )]
	public static void SetPropKillModeOn()
	{
		var target = ConsoleSystem.Caller.Pawn as SandboxPlayer;
		if ( target == null || target.PropKillEnabled ) return;
		if ( target.PKLockSwitch == true ) return;
		target.PropKillEnabled = true;
		target.TempPKLockSwitch();
		return;
	}


	[ServerCmd( "propkill_off" )]

	public static void SetPropKillModeOff()
	{
		var target = ConsoleSystem.Caller.Pawn as SandboxPlayer;
		if ( target == null || (!target.PropKillEnabled) ) return;
		if ( target.PKLockSwitch == true ) return;
		target.PropKillEnabled = false;
		target.TempPKLockSwitch();
		return;
	}

	[ServerCmd( "propkill_switch" )]

	public static void SetPropKillModeSwitch()
	{
		var target = ConsoleSystem.Caller.Pawn as SandboxPlayer;
		if ( target == null ) return;
		if ( target.PKLockSwitch == true ) return;
		target.PropKillEnabled = !target.PropKillEnabled;
		target.TempPKLockSwitch();
		return;
	}



}