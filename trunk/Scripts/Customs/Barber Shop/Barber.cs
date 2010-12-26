/*Created by Shai'Tan Malkier*/

using System;
using System.Collections;
using Server;

namespace Server.Mobiles
{
	public class Barber : BaseVendor
	{
		private ArrayList m_SBInfos = new ArrayList();
		protected override ArrayList SBInfos{ get { return m_SBInfos; } }

		[Constructable]
		public Barber() : base( "the barber" )
		{
		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add( new SBBarber() );
		}

		public override void InitOutfit()
		{
			base.InitOutfit();

			AddItem( new Server.Items.FullApron() );
		}

		public Barber( Serial serial ) : base( serial )
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