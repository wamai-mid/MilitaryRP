using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Dev : Panel
{

	public Dev()
	{
    AddClass("DEV_PANEL");
		Add.Label( "X&BOX DEV VERSION", "value" ).AddClass("DEV1");
    Add.Label( "- SLOT 0 KEY TO OPEN GAMEMODE MENU", "value" ).AddClass("DEV2");
	}
}
