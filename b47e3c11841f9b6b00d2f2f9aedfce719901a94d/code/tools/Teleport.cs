using Sandbox;
using Sandbox.UI;

namespace Sandbox.Tools
{
	[Library( "tool_teleport", Title = "Teleport", Description = "Attack 1 : Select Prop\nAttack 2 : Teleport Prop\nReload : Reset Prop", Group = "construction" )]
	public partial class TeleportTool : BaseTool
	{


    private Prop PropTeleport;

    public TraceResult Target()
    {
        var startPos = Owner.EyePos;
				var dir = Owner.EyeRot.Forward;
        return Trace.Ray( startPos, startPos + dir * MaxTraceDistance ).Ignore( Owner ).HitLayer( CollisionLayer.Debris ).Run();
    }

    public Prop Get()
    {
        var tr = Target();
        if ( !tr.Hit || !tr.Entity.IsValid()) return null;
        var ent = tr.Entity as Prop;
        if ( ent == null || ent.IsWorld || (!protect.SameOwner(Owner,ent)) ) return null;
        var particle = Particles.Create( "particles/physgun_freeze.vpcf" );
				particle.SetPosition( 0, ent.Position );
        return ent;
    }

    public void Move()
    {
        var tr = Target();
        var pos = tr.EndPos;
        PropTeleport.Position = pos;
    }


    public void SendMessageWrong()
    {
      var sbp = Owner as Player;
      if(sbp == null) return;
      ChatBox.AddChatEntry(To.Single(sbp),"Teleport", "This Prop is Freeze,You can't move Freeze Prop.");
    }

    public bool isFreeze()
    {
      if(PropTeleport == null) return true;
      
      var physicsGroup = PropTeleport.PhysicsGroup;
		  if ( physicsGroup == null ) return true;
      var hasPropFreeze = false;

		  for ( int i = 0; i < physicsGroup.BodyCount; ++i )
		  {
			  var body = physicsGroup.GetBody( i );
			  if ( body.IsValid() && body.BodyType == PhysicsBodyType.Static ) { hasPropFreeze = true; }
		  }
      if(hasPropFreeze == true) SendMessageWrong();

      return hasPropFreeze;
    }

		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
        if(Input.Pressed( InputButton.Attack1 ) && protect.InVehicle(Owner,true))	return;
			  if(Input.Down( InputButton.Attack2 ) && protect.InVehicle(Owner,true))	return;
				if(Input.Pressed( InputButton.Reload ) && protect.InVehicle(Owner,true))	return;

        if ( Input.Pressed( InputButton.Attack1 )) { var p = Get(); if(p != null) PropTeleport = p;};
        if ( Input.Pressed( InputButton.Attack2) && ( PropTeleport != null ) && (!isFreeze()) ) Move();
        if ( Input.Pressed( InputButton.Reload)) PropTeleport = null;
			}
		}
	}
}
