using System;
using Server;
using System.Collections;
using Server.Targeting;
using Server.Mobiles;
using Server.Items;
using Server.Network;

namespace Server.Items
{
	public class MustasheGrowthElixir : Item
	{
		[Constructable]
		public MustasheGrowthElixir() : base( 0xE26 )
		{
            Name = "Mustashe growth elixir";
		}

		public MustasheGrowthElixir( Serial serial ) : base( serial )
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
                // none - goatee
                if (from.FacialHairItemID == 0)
                {
                    Delete();
                    from.SendMessage("You use the elixir on your lip.");
                    from.FacialHairItemID = 0x2041; 
                    return;
                }

                //goatee - vandyke
                if (from.FacialHairItemID == 0x2040) 
                {
                    Delete();
                    from.SendMessage("You use the elixir on your lip.");
                    from.FacialHairItemID = 0x204D;
                    return;
                }                

                //shortbeard
                if (from.FacialHairItemID == 0x203F) 
                {
                    Delete();
                    from.SendMessage("You use the elixir on your lip.");
                    from.FacialHairItemID = 0x204B; 
                    return;
                }

		 //longbeard
                if (from.FacialHairItemID == 0x203E) 
                {
                    Delete();
                    from.SendMessage("You use the elixir on your lip.");
                    from.FacialHairItemID = 0x204C; 
                    return;
                }
                else
                {
                    
                    from.SendMessage("Your Mustashe cant get any longer!");
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