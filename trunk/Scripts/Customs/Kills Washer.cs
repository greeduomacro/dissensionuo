  //----------------------------------------------------------------------------------//
 // Created by Vano. Email: vano2006uo@mail.ru      //
//---------------------------------------------------------------------------------//
using System;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Multis;

namespace Server.Items
{
	public class KillsWasher : Item
	{
		[Constructable]
		public KillsWasher() : base( 0x1852 )
		{
			Name = "Kills Washer";
			Weight = 1.0;
			//LootType = LootType.Blessed;
			Hue = 1153;
		}

		public KillsWasher( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); //version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}

			else
			{

				if ( from.Kills == 0 )
				{
					from.SendMessage( 0x32, "You have no kills..." );
				}
				else
				{
					from.Kills = 0;
					from.SendMessage( 0x32, "All of your kills have been reset." );

						this.Delete();
					
				}
			}
		}
	}
}