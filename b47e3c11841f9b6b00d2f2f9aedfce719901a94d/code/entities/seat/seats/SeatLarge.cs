using Sandbox;
using System.Collections.Generic;

[Library( "ent_seat_large", Title = "SeatLarge", Spawnable = true )]
public partial class SeatLarge : Seat
{

  public SeatPlace[] ListPlaces()
  {
    return new SeatPlace[]{
      new SeatPlace(){ SitOffset = Vector3.Up * 5 + Vector3.Forward * 10 + Vector3.Left * 15},
      new SeatPlace(){ SitOffset = Vector3.Up * 20 + Vector3.Forward * 10 + Vector3.Left * -15},
    };
  }

  public override void Spawn()
  {
    SeatPlaces = ListPlaces();
    base.Spawn();
    SetModel("models/citizen_props/chair03.vmdl");
  }
}