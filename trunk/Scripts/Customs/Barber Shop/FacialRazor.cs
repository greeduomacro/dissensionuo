using System;
using Server;
using System.Collections;
using Server.Targeting;
using Server.Mobiles;
using Server.Items;
using Server.Network;

namespace Server.Items
{
	public class FacialRazor : Item
	{
		[Constructable]
		public FacialRazor() : base( 0xEC4 )
		{
                 Name = "Facial Razor";
		}

		public FacialRazor( Serial serial ) : base( serial )
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
                //goatee
                if (from.FacialHairItemID == 0x2040)
                {
                    Point3D scissorloc = from.Location; 
                    CutWhiskers CutWhiskers = new CutWhiskers();
                    CutWhiskers.Location = scissorloc;
                    CutWhiskers.MoveToWorld(scissorloc, from.Map);

                    from.SendMessage("You shave your Beard.");
                    from.FacialHairItemID = 0; 
                    return;
                }

                // Mustashe
                if (from.FacialHairItemID == 0x2041) 
                {
                    Point3D scissorloc = from.Location;
                    CutWhiskers CutWhiskers = new CutWhiskers();
                    CutWhiskers.Location = scissorloc;
                    CutWhiskers.MoveToWorld(scissorloc, from.Map);

                    from.SendMessage("You shave your Beard.");
                    from.FacialHairItemID = 0;
                    return;
                }                

                // vandyke
                if (from.FacialHairItemID == 0x204D)
                {
                    Point3D scissorloc = from.Location; 
                    CutWhiskers CutWhiskers = new CutWhiskers();
                    CutWhiskers.Location = scissorloc;
                    CutWhiskers.MoveToWorld(scissorloc, from.Map);

                    from.SendMessage("You shave your Beard.");
                    from.FacialHairItemID = 0x2040;
                    return;
                }

		 // shortbeard
                if (from.FacialHairItemID == 0x203F)
                {
                    Point3D scissorloc = from.Location; 
                    CutWhiskers CutWhiskers = new CutWhiskers();
                    CutWhiskers.Location = scissorloc;
                    CutWhiskers.MoveToWorld(scissorloc, from.Map);

                    from.SendMessage("You shave your Beard.");
                    from.FacialHairItemID = 0x2040;
                    return;
                }

		// mediumshortbeard
                if (from.FacialHairItemID == 0x204B)
                {
                    Point3D scissorloc = from.Location; 
                    CutWhiskers CutWhiskers = new CutWhiskers();
                    CutWhiskers.Location = scissorloc;
                    CutWhiskers.MoveToWorld(scissorloc, from.Map);

                    from.SendMessage("You shave your Beard.");
                    from.FacialHairItemID = 0x204D;
                    return;
                }

                if (from.FacialHairItemID == 0)
                {
                    from.SendMessage("You cannot shave your Beard. You have none!");
                    return;
                }

                else
                {
                    from.SendMessage("You cannot shave your Beard. Trim it a bit first.");
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