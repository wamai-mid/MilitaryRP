using Sandbox;
public class SeatController : PawnController
{
	public override void FrameSimulate()
	{
		base.FrameSimulate();

		Simulate();
	}

	public override void Simulate()
	{
		var seat = Pawn.Parent as Seat;
		seat.Simulate( Client );

		EyeRot = Input.Rotation;
		EyePosLocal = Vector3.Up * (64 - 10) * seat.Scale;
		Velocity = seat.Velocity;

		SetTag( "noclip" );
		SetTag( "sitting" );
	}
}
