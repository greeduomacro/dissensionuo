using System;
using Server.Mobiles;
using Server.Items;
using Server.Spells;

namespace Server.Mobiles
{
	public class EtherealHiryu : EtherealMount
	{
		[Constructable]
		public EtherealHiryu() : base( 0x276A, 0x3E94 )
		{
			Name = "Ethereal Hiryu Statuette";
		}

		public EtherealHiryu( Serial serial ) : base( serial )
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

			if ( Name == "an ethereal hiryu" )
				Name = null;
		}
	}
}
