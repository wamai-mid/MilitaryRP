using Sandbox;
using Sandbox.UI;
namespace Sandbox
{
	public partial class Protect
	{

		public void SendError( Player sbp, string name, string message, bool showMessage )
		{
			if ( !showMessage ) return;
			if ( sbp == null ) return;
			ChatBox.AddChatEntry( To.Single( sbp ), name, message );
		}

		public bool SameOwner( Player owner, Entity entity, bool IgnoreWorld = false, bool showMessage = true )
		{
			var eowner = entity.Owner;
			if ( entity is WorldEntity && IgnoreWorld == true ) return true;
			var isOwner = (entity.Owner == owner);
			if ( !isOwner ) SendError( owner, "Owner Error", "This entity is not yours", showMessage );
			return isOwner;
		}
		public bool InVehicle( Player player, bool showMessage = false )
		{
			if ( player is SandboxPlayer p){
				if ( p == null ) return false;
				var isDriving = player.Tags.Has( "driving" );
				if ( isDriving )
				{
					if ( p.timeBetweenErrorVehicle > 2.0f )
					{
						var name = "Vehicle";
						if(p.Vehicle is Entity vehicle){
							name = vehicle.ClassInfo.Name.Contains("ent_seat_") ? "Seat" : name;
						}
						SendError( player, name, "You can't do that in " + name.ToLower(), showMessage );
						p.timeBetweenErrorVehicle = 0.0f;
					}
				}
				return isDriving;
			}
			return false;
		}

		public bool NeedRestrictPVPWeapon( Player player, Weapon weapon, bool showMessage = true )
		{
			if(player is SandboxPlayer sbp){
				if ( sbp == null ) return false;
				if ( sbp.GameMode == (int)SandboxPlayer.GM.BUILD )
				{
					if ( weapon != null )
					{
						weapon.Remove();
						SendError( sbp, "Warning :", "You can't spawn (" + weapon.ClassInfo.Name + ") in this mode, switch to PVP :)", showMessage );
						sbp.Inventory.SetActiveSlot( 0, false );
						return true;
					}
				}
			}
			return false;
		}


	}
}