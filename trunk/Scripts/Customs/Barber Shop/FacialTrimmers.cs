using System;
using Server;
using System.Collections;
using Server.Targeting;
using Server.Mobiles;
using Server.Items;
using Server.Network;

namespace Server.Items
{
	public class FacialTrimmers : Item
	{
		[Constructable]
		public FacialTrimmers() : base( 0xDFC )
		{
			Name = "Facial Trimmers";
		}

		public FacialTrimmers( Serial serial ) : base( serial )
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
                // goatee, mustashe, vandyke, shortbeard, mediumshortbeard
                if (from.FacialHairItemID == 0x2040 || from.FacialHairItemID == 0x2041 || from.FacialHairItemID == 0x204D || from.FacialHairItemID == 0x203F || from.FacialHairItemID == 0x204B)
                {
                    from.SendMessage("You cannot cut your Beard shorter. Try use a razor on it.");
                    return;
                }

		  //mediumlongbeard
                if (from.FacialHairItemID == 0x204C) 
                {
                    Point3D scissorloc = from.Location; 
                    CutWhiskers CutWhiskers = new CutWhiskers();
                    CutWhiskers.Location = scissorloc;
                    CutWhiskers.MoveToWorld(scissorloc, from.Map);

                    from.SendMessage("You cut your Beard.");
                    from.FacialHairItemID = 0x204B;
                    from.PlaySound(0x249);
                    return;
                }    

		 //longbeard
                if (from.FacialHairItemID == 0x203E) 
                {
                    Point3D scissorloc = from.Location; 
                    CutWhiskers CutWhiskers = new CutWhiskers();
                    CutWhiskers.Location = scissorloc;
                    CutWhiskers.MoveToWorld(scissorloc, from.Map);

                    from.SendMessage("You cut your Beard.");
                    from.FacialHairItemID = 0x203F;
                    from.PlaySound(0x249);
                    return;
                }                                                  

                else
                {
                    from.SendMessage("You cannot cut your Beard shorter. There is none!");
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