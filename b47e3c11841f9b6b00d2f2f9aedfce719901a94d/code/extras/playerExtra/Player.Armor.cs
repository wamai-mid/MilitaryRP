using Sandbox;
using System;
partial class SandboxPlayer
{
	[Net] public float Armor { get; set; }

	public void ArmorInit()
	{
		Armor = 50.0f;
	}

}