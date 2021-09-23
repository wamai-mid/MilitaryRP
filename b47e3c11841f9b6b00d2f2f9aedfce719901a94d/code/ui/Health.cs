using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Health : Panel
{
	public Label Label;

	public Health()
	{
		Label = Add.Label( "100", "value" );
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;

		var h = player.Health.CeilToInt();
		var bh = (h >= 100 ? "" : (h >= 10 ? "0" : "00")) + h;

		Label.Text = $"💖 {bh}";
	}
}
