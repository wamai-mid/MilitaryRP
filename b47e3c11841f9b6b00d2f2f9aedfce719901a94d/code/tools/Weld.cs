namespace Sandbox.Tools
{
	[Library( "tool_weld", Title = "Weld", Description = "Weld stuff together", Group = "construction" )]
	public partial class WeldTool : BaseTool
	{
		private Prop target;

		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				if(Input.Pressed( InputButton.Attack1 ) && protect.InVehicle(Owner,true))	return;
			  if(Input.Down( InputButton.Attack2 ) && protect.InVehicle(Owner,true))	return;
				if(Input.Pressed( InputButton.Reload ) && protect.InVehicle(Owner,true))	return;
				var startPos = Owner.EyePos;
				var dir = Owner.EyeRot.Forward;

				var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
					.Ignore( Owner )
					.Run();

				if ( !tr.Hit || !tr.Body.IsValid() || !tr.Entity.IsValid() || tr.Entity.IsWorld )
					return;

				if ( tr.Entity.PhysicsGroup == null || tr.Entity.PhysicsGroup.BodyCount > 1 )
					return;

				if ( tr.Entity is not Prop prop )
					return;

				if ( Input.Pressed( InputButton.Attack1 ) )
				{
					if (! protect.SameOwner(Owner,tr.Entity ) )
					return;
					if ( prop.Root is not Prop rootProp )
					{
						return;
					}

					if ( target == rootProp )
						return;

					if ( !target.IsValid() )
					{
						target = rootProp;
					}
					else
					{
						target.Weld( rootProp );
						target = null;
					}
				}
				else if ( Input.Pressed( InputButton.Attack2 ) )
				{
					if (! protect.SameOwner(Owner,tr.Entity ) )
					return;
					prop.Unweld( true );

					Reset();
				}
				else if ( Input.Pressed( InputButton.Reload ) )
				{
					if (! protect.SameOwner(Owner,tr.Entity ) )
					return;
					if ( prop.Root is not Prop rootProp )
					{
						return;
					}

					rootProp.Unweld();

					Reset();
				}
				else
				{
					return;
				}

				CreateHitEffects( tr.EndPos );
			}
		}

		private void Reset()
		{
			target = null;
		}

		public override void Activate()
		{
			base.Activate();

			Reset();
		}

		public override void Deactivate()
		{
			base.Deactivate();

			Reset();
		}
	}
}
