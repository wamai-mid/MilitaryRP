using Sandbox;
using System;
using System.Collections.Generic;

partial class SandboxPlayer
{
	[Net] public bool ModeLock { get; set; }
	[Net] public int ModeLockWait { get; set; }
	[Net] public int GameMode { get; set; }
	public enum GM { BUILD = 1, PVP = 2 }

	public string GMICON()
	{
		switch(GameMode)
		{
			case 1: return "🏗️";
			case 2: return "⚔️";
			default: return "⌛";
		}
	}

	public void GameModeInit()
	{
		GameMode = 1;
		base.Spawn();
	}

	public void RespawnRules()
	{
		Inventory.DeleteContents();
		Inventory.Add( new Flashlight() );
		if ( GameMode == (int)GM.BUILD )
		{
			Inventory.Add( new GravGun() );
			Inventory.Add( new Tool() );
			Inventory.Add( new PhysGun(), true );
		}
		else if ( GameMode == (int)GM.PVP )
		{
			Inventory.Add( new SMG() );
			Inventory.Add( new Shotgun() );
			Inventory.Add( new Pistol(), true );
		}
	}

	public async void TempLockModeInc( int i )
	{
		if ( i == 0 ) { ModeLockWait = 0; return; }
		await Task.Delay( 1000 );
		i--;
		ModeLockWait = i;
		TempLockModeInc( i );
		return;
	}
	public async void TempLockMode()
	{
		var waitS = 20;
		ModeLockWait = waitS;
		TempLockModeInc( waitS );
		ModeLock = true;
		await Task.Delay( waitS * 1000 );
		ModeLock = false;
	}


	[ServerCmd( "pvp_mode" )]

	public static void SetPVPMode()
	{
		var GMPVP = (int)GM.PVP;
		var target = ConsoleSystem.Caller.Pawn as SandboxPlayer;
		if ( target == null || (target.GameMode == GMPVP) ) return;
		if ( target.ModeLock ) return;
		target.GameMode = GMPVP;
		target.TempLockMode();
		target.Respawn();
		return;
	}

	[ServerCmd( "build_mode" )]

	public static void SetBUILDMode()
	{
		var GMBUILD = (int)GM.BUILD;
		var target = ConsoleSystem.Caller.Pawn as SandboxPlayer;
		if ( target == null || (target.GameMode == GMBUILD) ) return;
		if ( target.ModeLock ) return;
		target.GameMode = GMBUILD;
		target.TempLockMode();
		target.Respawn();
		return;
	}

};