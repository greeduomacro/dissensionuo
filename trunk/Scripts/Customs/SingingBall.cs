using System;
using Server;


namespace Server.Items
{
	public class SingingBall : Item
	{
        public override int LabelNumber { get { return 1041245; } } // Singing Ball

	
		[Constructable]
		public SingingBall() : base( 0xE2E)
		{
			Weight = 1.0;					
				
		}

		public SingingBall( Serial serial ) : base( serial )
		{
		}

        public override bool HandlesOnMovement { get{ return true; } }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (IsLockedDown && Utility.InRange(m.Location, this.Location, 2) && !Utility.InRange(oldLocation, this.Location, 2) && m.AccessLevel == AccessLevel.Player)
            {
                Effects.PlaySound(this.Location, this.Map, Utility.RandomMinMax(0, 1338));
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

