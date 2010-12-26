using System;
using Server;

namespace Server.Items
{
	public class TrammelBarstool : DeerMask
	{


		public override int InitMinHits{ get{ return 255; } }
		public override int InitMaxHits{ get{ return 255; } }
                public override int BaseColdResistance{ get{ return 12; } }

		[Constructable]
		public TrammelBarstool()
		{
			Name = "a trammel barstool";
                        Hue = Utility.RandomList( Utility.RandomRedHue(), Utility.RandomBlueHue(), Utility.RandomGreenHue(), Utility.RandomMetalHue() );

                        LootType = LootType.Blessed;

		}

		public TrammelBarstool( Serial serial ) : base( serial )
		{
		}
		
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 );
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
			switch ( version )
			{
				case 0:
				{
					Resistances.Cold = 0;
					break;
				}
			}
		}
	}
}