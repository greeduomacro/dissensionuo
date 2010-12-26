using System;
using Server;

namespace Server.Items
{
	//[Flipable(0xDFC,0xDFD)]
	public class CutWhiskers : Item
	{
		[Constructable]
		public CutWhiskers() : base( 0xDFE )
		{
            	Movable = true;
		Name = "Cut Whiskers";
		}

		public CutWhiskers( Serial serial ) : base( serial )
		{

		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}