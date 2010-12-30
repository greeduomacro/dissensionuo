//Remove From Factions Deed
//By Tresdni aka DxMonkey of Ultima Eclipse
//This deed will remove you from factions.


using System;
using Server.Network;
using Server.Prompts;
using Server.Items;
using Server.Mobiles;
using Server.Factions;

namespace Server.Items
{
	public class FactionKickDeed : Item
	{
		
		[Constructable]
		public FactionKickDeed() : base( 0x14F0 )
		{
			Weight = 1.0;
			Name = "Remove From Factions Deed";
			Hue = 39;  //This is red, change it to whatever you want.
			LootType = LootType.Blessed;
		}

		public FactionKickDeed( Serial serial ) : base( serial )
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
			LootType = LootType.Blessed;

			int version = reader.ReadInt();
		}
		
	
	
		public override void OnDoubleClick( Mobile from )
		{
			PlayerState pl = PlayerState.Find( (Mobile) from );

						if ( pl != null )
						{
							pl.Faction.RemoveMember( from );

							from.SendMessage( "You have been kicked from your faction." );
							Delete();
							return;
						}
						from.SendMessage( "You are not in a faction." );
						return;
		}	
	}	

}	