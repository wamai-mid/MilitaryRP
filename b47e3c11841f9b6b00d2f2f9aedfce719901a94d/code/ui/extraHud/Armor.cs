using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Armor : Panel
{
	public Label Label;

	public Armor()
	{
		Label = Add.Label( "100", "value" );
	}

	public override void Tick()
	{
		var player = Local.Pawn as SandboxPlayer;
		if ( player == null ) return;

		var a = player.Armor.CeilToInt();
		var ba= (a >= 100 ? "" : (a >= 10 ? "0" : "00")) + a;

		Label.Text = $"🛡️  {ba}";
	}
}
