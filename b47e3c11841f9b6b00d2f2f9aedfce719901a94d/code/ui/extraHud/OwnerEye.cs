using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class OwnerEye : Panel
{

	public Label Owner;

	public OwnerEye()
	{
		AddClass( "OWNER_EYE" );
		Owner = Add.Label( "", "value" );
	}

	public override void Tick()
	{
		var res = "";
		var player = Local.Pawn as SandboxPlayer;
		if ( player != null )
		{
			if ( player.Vehicle as Entity == null )
			{
				var eyePos = player.EyePos;
				var eyeRot = player.EyeRot;

				var tr = Trace.Ray( eyePos, eyePos + eyeRot.Forward * 5000 ).Radius( 2 ).Ignore( player ).EntitiesOnly().Run();
				if ( tr.Entity as Entity != null )
				{
					var ent = tr.Entity as Entity;
					var cl = ent.ClassInfo.Name;
					var owner = ent.Owner as SandboxPlayer;
					if ( owner == null ) res = "World";
					else res = "Type : " + cl + "\nOwner : " + owner.GetClientOwner().Name;
					Owner.Text = res;
					SetClass( "hide", false );
					return;
				}
			}
		}
		SetClass( "hide", true );
	}

}
