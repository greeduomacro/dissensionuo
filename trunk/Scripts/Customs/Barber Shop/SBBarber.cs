/*Created by Shai'Tan Malkier*/

using System;
using System.Collections;
using Server.Items;

namespace Server.Mobiles
{
	public class SBBarber : SBInfo
	{
		private ArrayList m_BuyInfo = new InternalBuyInfo();
		private IShopSellInfo m_SellInfo = new InternalSellInfo();

		public SBBarber()
		{
		}

		public override IShopSellInfo SellInfo { get { return m_SellInfo; } }
		public override ArrayList BuyInfo { get { return m_BuyInfo; } }

		public class InternalBuyInfo : ArrayList
		{
			public InternalBuyInfo()
			{
                // for hair
				Add( new GenericBuyInfo( typeof( Brush ), 6, 10, 0x1372, 0 ) );
				Add( new GenericBuyInfo( typeof( BarberScissors ), 21, 10, 0xDFC, 0 ) );
				Add( new GenericBuyInfo( typeof( Razor ), 17, 20, 0xEC4, 0 ) );
				Add( new GenericBuyInfo( typeof( HairGrowthElixir ), 8, 20, 0xE26, 0 ) );
                
                // for facial hair
                Add(new GenericBuyInfo(typeof(FacialTrimmers), 21, 10, 0xDFC, 0));
                Add(new GenericBuyInfo(typeof(FacialRazor), 17, 20, 0xEC4, 0));
                Add(new GenericBuyInfo(typeof(BeardGrowthElixir), 8, 20, 0xE26, 0));
                Add(new GenericBuyInfo(typeof(MustasheGrowthElixir), 8, 20, 0xE26, 0));
			}
		}

		public class InternalSellInfo : GenericSellInfo
		{
			public InternalSellInfo()
			{
				Add( typeof( Brush ), 2 );
				Add( typeof( Razor ), 3 );
				Add( typeof( BarberScissors ), 8 );
				Add( typeof( HairGrowthElixir ), 3 );

                Add(typeof(FacialTrimmers), 2);
                Add(typeof(FacialRazor), 3);
                Add(typeof(BeardGrowthElixir), 8);
                Add(typeof(MustasheGrowthElixir), 3);
			}
		}
	}
}