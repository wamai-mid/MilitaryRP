using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.UI.Tests;
using System.Linq;

[Library]
public partial class WeaponsList : Panel
{
	VirtualScrollPanel Canvas;

	public bool Filter(Sandbox.LibraryAttribute x)
	{
		if (!x.Spawnable) return false;
    if (!x.Name.StartsWith("weapon_")) return false;
		return true;
	}

	public WeaponsList()
	{
		AddClass( "spawnpage" );
		AddChild( out Canvas, "canvas" );

		Canvas.Layout.AutoColumns = true;
		Canvas.Layout.ItemSize = new Vector2( 100, 100 );
		Canvas.OnCreateCell = ( cell, data ) =>
		{
			var entry = (LibraryAttribute)data;
			var btn = cell.Add.Button( entry.Title );
			btn.AddClass( "icon" );
			btn.AddEventListener( "onclick", () => ConsoleSystem.Run( "spawn_entity", entry.Name ) );
			btn.Style.Background = new PanelBackground
			{
				Texture = Texture.Load( $"/entity/{entry.Name}.png", false )
			};
		};

		var ents = Library.GetAllAttributes<Weapon>().Where( x => Filter(x) ).OrderBy( x => x.Title ).ToArray();

		foreach ( var entry in ents )
		{
			Canvas.AddItem( entry );
		}
	}
}
