using System;
using Server;
using System.Collections;
using Server.Targeting;
using Server.Mobiles;
using Server.Items;
using Server.Network;

namespace Server.Items
{
	public class BeardGrowthElixir : Item
	{
		[Constructable]
		public BeardGrowthElixir() : base( 0xE26 )
		{
            Name = "Beard growth elixir";
		}

		public BeardGrowthElixir( Serial serial ) : base( serial )
		{
		}

        public override void OnDoubleClick(Mobile from)
        {

            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                return;
            }

            else
            {
                // none 
                if (from.FacialHairItemID == 0)
                {
                    Delete();
                    from.SendMessage("You use the elixir on your chin.");
                    from.FacialHairItemID = 0x2040; 
                    return;
                }

                //goatee
                if (from.FacialHairItemID == 0x2040) 
                {
                    Delete();
                    from.SendMessage("You use the elixir on your chin.");
                    from.FacialHairItemID = 0x203F;
                    return;
                }                

                //shortbeard
                if (from.FacialHairItemID == 0x203F) 
                {
                    Delete();
                    from.SendMessage("You use the elixir on your chin.");
                    from.FacialHairItemID = 0x203E; 
                    return;
                }

		//mustashe
                if (from.FacialHairItemID == 0x2041) 
                {
                    Delete();
                    from.SendMessage("You use the elixir on your chin.");
                    from.FacialHairItemID = 0x204D; 
                    return;
                }

		//vandyke
                if (from.FacialHairItemID == 0x204D) 
                {
                    Delete();
                    from.SendMessage("You use the elixir on your chin.");
                    from.FacialHairItemID = 0x204B; 
                    return;
                }
		
		//mediumshortbeard
                if (from.FacialHairItemID == 0x204B) 
                {
                    Delete();
                    from.SendMessage("You use the elixir on your chin.");
                    from.FacialHairItemID = 0x204C; 
                    return;
                }

                else
                {
                    
                    from.SendMessage("Your Beard cant get any longer!");
                    return;
                }
            }
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