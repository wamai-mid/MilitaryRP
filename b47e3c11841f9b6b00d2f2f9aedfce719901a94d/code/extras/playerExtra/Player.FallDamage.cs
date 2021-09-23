using Sandbox;
using System;
partial class SandboxPlayer
{

  private TimeSince timeBetweenTwoFall;

  public bool TouchGround()
  {
    var p  = Position;
    var vd = Vector3.Down;
    return Trace.Ray(p, p + vd * 20).Radius(1).Ignore(this).Run().Hit;
  }

  public bool HasNotNoClip()
  {
    return !Controller.HasTag("noclip");
  }

  public void PlaySoundFall()
  {
    var predictDeath = this.Health - 10.0f <= 0;
    var fallSound = "xnbox_" + (predictDeath ? "death" : "fall") + "0"; 
    var rndFall = new Random().Next(1,predictDeath ? 2 : 5);
    Log.Info(fallSound + rndFall);
    Sound.FromEntity(fallSound + rndFall, this);
  }

	public void TakeFallDamage()
	{
    if(LifeState != LifeState.Alive) return;
		var v = Velocity;
    var d = Rotation.Down;
    var vd = (v*d).z;
    if( timeBetweenTwoFall > 0.02f && vd >= 590 && TouchGround() && HasNotNoClip()) {
      var damage = new DamageInfo(){
        Position = Position,
        Damage = 10.0f
      };
      PlaySoundFall();
      TakeDamage(damage);
      timeBetweenTwoFall = 0;
    }
	}

}