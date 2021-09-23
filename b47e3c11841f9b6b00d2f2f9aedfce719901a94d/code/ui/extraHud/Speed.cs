using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Speed : Panel
{
	public Label Label;

	public Speed()
	{
			Label = Add.Label( "0 KH/H", "value" );
	}

	public override void Tick()
	{
		var player = Local.Pawn as SandboxPlayer;
		if ( player == null ) return;
		RemoveClass( "stop" );
		RemoveClass( "walk" );
		RemoveClass( "run" );
		AddClass(player.IsStopped ? "stop" : (player.IsWalk ? "walk" : "run"));
		var speed = player.Speed.CeilToInt();
		var rspeed = (speed >= 100 ? "" : (speed >= 10 ? "0" : "00")) + speed + " KM/H";
		Label.Text = $"{rspeed}";
	}
}
