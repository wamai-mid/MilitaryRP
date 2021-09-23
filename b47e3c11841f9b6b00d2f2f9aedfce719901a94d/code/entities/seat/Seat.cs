using Sandbox;
using System.Collections.Generic;

[Library( "ent_seat_base", Title = "SeatBase", Spawnable = false )]
public partial class Seat : Prop, IUse
{  private Vector3 OutPosition => Vector3.Forward * 120;
  public Vector3 SitOffset => Vector3.Up * 5 + Vector3.Forward * 10;

  protected SeatPlace[] SeatPlaces { get; set; }
  public string ModelName { get; set;} = "models/citizen_props/chair02.vmdl";

  private Dictionary<int,SeatPlace> Places;

  public void InitPlaces()
  {
    Places = new Dictionary<int, SeatPlace>(); 
    for (int i = 1; i <= SeatPlaces.Length; i++)
    {
      var seatplace = SeatPlaces[i-1];
      seatplace.player = null;
      if(seatplace.SitOffset == null) seatplace.SitOffset = SitOffset;
      if(seatplace.OutPosition == null) seatplace.OutPosition = OutPosition;
      Places.Add(i-1,seatplace);
    }
  }

  public void FreePlace(Player player)
  {
    foreach(KeyValuePair<int, SeatPlace> p in Places) {
      if(player == p.Value.player) {
          Places[p.Key].player = null;
      }
    }
  }

  public int getFreePlace()
  {
    foreach(KeyValuePair<int, SeatPlace> p in Places) {
       if(p.Value.player == null) return p.Key;
    }
    return -1;
  }
  public override void Spawn()
  {
    base.Spawn();
    SetModel(ModelName);
    InitPlaces();
    SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false);
    EnableSelfCollisions = false;
  }

  private bool takePlace( SandboxPlayer player, int Index)
  {
    if(SeatPlaces[Index].player != null) return OutPlayer(player);
    SeatPlaces[Index].player = player;
    return true;
  }

  private int playerTakePlace( SandboxPlayer player )
  {
    foreach(KeyValuePair<int, SeatPlace> p in Places) {
       if(p.Value.player == player) return p.Key;
    }
    return -1;
  }

  private bool OutPlayer( SandboxPlayer player )
  {
    player.Tags.Remove("driving");
    player.LocalPosition = OutPosition;
    FreePlace(player);
    player.Vehicle = null;
		player.VehicleController = null;
		player.VehicleAnimator = null;
		player.Parent = null;
		player.PhysicsBody.Enabled = true;
    return false;
  }

  private bool InPlayer( SandboxPlayer player, bool changePlace = false )
  {
      var freePlace = getFreePlace();
      if(freePlace == -1) return false;
      if(changePlace == true) { FreePlace(player); }
      if(takePlace(player,freePlace) == false)  return false;
      var place = SeatPlaces[freePlace];

      if(changePlace == false){
        player.VehicleAnimator = new CarAnimator();
        player.VehicleController = new SeatController();
      }

      var LocalPosition = place.SitOffset;
      player.Tags.Add("driving");
      player.LocalRotation = Rotation.Identity;
			player.LocalScale = 1;
      player.Vehicle = this;
      player.Parent = this;
      player.PhysicsBody.Enabled = false;
			player.LocalPosition = LocalPosition;

      return true;
  }

  public override void Simulate( Client owner )
	{
		if ( owner == null ) return;
    if ( !IsServer ) return;
    using ( Prediction.Off() ) {
      if ( Input.Pressed( InputButton.Use )) { if ( owner.Pawn is SandboxPlayer player) if(!player.IsUseDisabled()) { OutPlayer(player); return; } }
      if ( Input.Pressed( InputButton.Reload )) { if ( owner.Pawn is SandboxPlayer player) if(!player.IsUseDisabled()) { Log.Info("ici");InPlayer(player,true);} }
    }
  }

  public bool OnUse( Entity user )
  {
    if ( user is SandboxPlayer player && player.Vehicle == null) { InPlayer(player); }
    return true;
  }

  public bool IsUsable( Entity user )
	{
    return user is SandboxPlayer player && player.Vehicle == null;
  }


}