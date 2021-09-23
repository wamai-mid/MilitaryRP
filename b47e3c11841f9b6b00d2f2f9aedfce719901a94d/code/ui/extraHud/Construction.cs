using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Construction : Panel
{
	public Image C;
	public Construction()
	{
		C = Add.Image( "materials/debug/construction.png", "" );
    C.AddClass("CONSTRUCTION");
	}
}
