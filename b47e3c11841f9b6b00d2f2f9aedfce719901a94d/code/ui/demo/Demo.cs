using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.UI.Construct;


namespace Sandbox.UI
{ 
	public class Demo : Panel
	{
		public Label DemoT;
		public Demo()
		{
			this.StyleSheet.Load( "/ui/demo/Demo.scss" );
			DemoT = Add.Label( "NEW GAMEMODE RELEASED ! VISIT \"XNRP\" IF YOU WANT :)", "DEMOT" );
			DemoT.AddClass( "GLASS" );
		}
		
	}
}
