using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class PlayMode : Panel
{
	public Label Label;
	public PlayMode()
	{
		Label = Add.Label( "100", "value" );
	}

	public override void Tick()
	{
		var player = Local.Pawn as SandboxPlayer;
		if ( player == null ) return;
		Label.Text = $"{(player.GMICON())}";
	}
}
