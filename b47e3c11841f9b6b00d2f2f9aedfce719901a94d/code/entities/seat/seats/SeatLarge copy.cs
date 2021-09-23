using Sandbox;
using System.Collections.Generic;

[Library( "ent_seat_xl", Title = "SeatXL", Spawnable = true )]
public partial class SeatXL : Seat
{

  public SeatPlace[] ListPlaces()
  {
    return new SeatPlace[]{
      new SeatPlace(){ SitOffset = Vector3.Up * 5 + Vector3.Forward * 10 + Vector3.Left * 45},
      new SeatPlace(){ SitOffset = Vector3.Up * 5 + Vector3.Forward * 10 + Vector3.Left * 15},
      new SeatPlace(){ SitOffset = Vector3.Up * 20 + Vector3.Forward * 10 + Vector3.Left * 0},
      new SeatPlace(){ SitOffset = Vector3.Up * 5 + Vector3.Forward * 10 + Vector3.Left * -15},
      new SeatPlace(){ SitOffset = Vector3.Up * 5 + Vector3.Forward * 10 + Vector3.Left * -45},
    };
  }

  public override void Spawn()
  {
    SeatPlaces = ListPlaces();
    base.Spawn();
    SetModel("models/citizen_props/chair04blackleather.vmdl");
  }
}