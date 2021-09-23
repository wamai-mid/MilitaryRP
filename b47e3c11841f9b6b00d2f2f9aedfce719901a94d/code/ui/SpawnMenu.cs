
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;

[Library]
public partial class SpawnMenu : Panel
{
	public static SpawnMenu Instance;
	readonly Panel toollist;

	public SpawnMenu()
	{
		Instance = this;

		StyleSheet.Load( "/ui/SpawnMenu.scss" );

		var left = Add.Panel( "left" );
		{
			var tabs = left.AddChild<ButtonGroup>();
			tabs.AddClass( "tabs" );
			tabs.AddClass( "theme" );

			var body = left.Add.Panel( "body" );
			body.AddClass( "theme" );

			{
				var props = body.AddChild<SpawnList>();
				tabs.SelectedButton = tabs.AddButtonActive( "📦 Props", ( b ) => props.SetClass( "active", b ) );

				var ents = body.AddChild<EntityList>();
				tabs.AddButtonActive( "🚀 Entities", ( b ) => ents.SetClass( "active", b ) );

				var drp_ents = body.AddChild<DRPEntityList>();
				tabs.AddButtonActive( "👮 DARKRP Entities", ( b ) => drp_ents.SetClass( "active", b ) );

				var weapons = body.AddChild<WeaponsList>();
				tabs.AddButtonActive( "🔫 Weapons", ( b ) => weapons.SetClass( "active", b ) );

				var vehicles = body.AddChild<VehiclesList>();
				tabs.AddButtonActive( "🚗 Vehicles", ( b ) => vehicles.SetClass( "active", b ) );

				var seat = body.AddChild<SeatEntityList>();
				seat.AddClass("wip");
				tabs.AddButtonActive( "🪑 Seat [WIP]", ( b ) => seat.SetClass( "active", b ) ).AddClass("wip");

			}
		}

		var right = Add.Panel( "right" );
		{
			var tabs = right.Add.Panel( "tabs" );
			{
				tabs.Add.Button( "Outils" ).AddClass( "active" );
				tabs.Add.Button( "Utilitaire" );
			}
			tabs.AddClass( "theme" );
			var body = right.Add.Panel( "body" );
			{
				toollist = body.Add.Panel( "toollist" );
				{
					RebuildToolList();
				}
				body.Add.Panel( "inspector" );
			}
			body.AddClass( "theme" );
		}

	}

	void RebuildToolList()
	{
		toollist.DeleteChildren( true );

		foreach ( var entry in Library.GetAllAttributes<Sandbox.Tools.BaseTool>() )
		{
			if ( entry.Title == "BaseTool" )
				continue;

			var button = toollist.Add.Button( entry.Title );
			button.SetClass( "active", entry.Name == ConsoleSystem.GetValue( "tool_current" ) );

			button.AddEventListener( "onclick", () =>
			{
				ConsoleSystem.Run( "tool_current", entry.Name );
				ConsoleSystem.Run( "inventory_current", "weapon_tool" );

				foreach ( var child in toollist.Children )
					child.SetClass( "active", child == button );
			} );
		}
	}

	public override void Tick()
	{
		base.Tick();

		Parent.SetClass( "spawnmenuopen", Input.Down( InputButton.Menu ) );
	}

	public override void OnHotloaded()
	{
		base.OnHotloaded();

		RebuildToolList();
	}
}
