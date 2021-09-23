using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Death : Panel
{

	public Death()
	{
	}

	public override void Tick()
	{
		var player = Local.Pawn as SandboxPlayer;
		if ( player == null ) return;
    SetClass("dead",player.LifeState == LifeState.Dead);
	}
}
