using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Network;
using Server.Prompts;
using Server.Multis;
using Server.Targeting;
using System.Net;
using Server.Accounting;
using Server.Commands;

namespace Server.Gumps
{
	public class FindHouseGump : Gump
	{
		private const int GreenHue = 0x40;
		private const int RedHue = 0x20;

		public static void Initialize()
		{
			CommandSystem.Register( "FindHouse", AccessLevel.GameMaster, new CommandEventHandler( FindHouse_OnCommand ) );
		}

		[Usage( "FindHouse" )]
		[Description( "Finds all Houses in the world." )]
		public static void FindHouse_OnCommand( CommandEventArgs e )
		{
			ArrayList list = new ArrayList();

			foreach ( Item item in World.Items.Values )
			{
				if ( item is BaseHouse )

				{
				BaseHouse House = item as BaseHouse;

				list.Add( House );
 					
				}
			}
			e.Mobile.SendGump( new FindHouseGump( e.Mobile, list, 1 ) );
		}

		private ArrayList m_List;
		private int m_DefaultIndex;
		private int m_Page;
		private Mobile m_From;

		public void AddBlackAlpha( int x, int y, int width, int height )
		{
			AddImageTiled( x, y, width, height, 2624 );
			AddAlphaRegion( x, y, width, height );
		}

		public FindHouseGump( Mobile from, ArrayList list, int page ) : base( 50, 40 )
		{
			from.CloseGump( typeof( FindHouseGump ) );

			int Houses = 0;
			m_Page = page;
			m_From = from;
			int pageCount = 0;
			m_List = list;

			AddPage( 0 );

			AddBackground( 0, 0, 645, 325, 3500 );

			AddBlackAlpha( 20, 20, 604, 277 );

			if ( m_List == null )
			{
				return;
			}
			else
			{
				Houses = list.Count;

				if ( list.Count % 12 == 0 )
				{
					pageCount = (list.Count / 12);
				}
				else
				{
					pageCount = (list.Count / 12) + 1;
				}
			}

			AddLabelCropped( 32, 20, 100, 20, 1152, "House Name" );
			AddLabelCropped( 132, 20, 120, 20, 1152, "Owner" );
			AddLabelCropped( 285, 20, 120, 20, 1152, "Account" );
			AddLabelCropped( 415, 20, 120, 20, 1152, "Location" );
			AddLabel( 27, 298, 32, String.Format( "Made By Sidious    Version 1.0.1                       There are {0} houses in the world.", Houses ));

			if ( page > 1 )
				AddButton( 573, 22, 0x15E3, 0x15E7, 1, GumpButtonType.Reply, 0 );
			else
				AddImage( 573, 22, 0x25EA );

			if ( pageCount > page )
				AddButton( 590, 22, 0x15E1, 0x15E5, 2, GumpButtonType.Reply, 0 );
			else
				AddImage( 590, 22, 0x25E6 );

			if ( m_List.Count == 0 )
				AddLabel( 180, 115, 1152, "There are no Houses in world" );

			if ( page == pageCount )
			{
				for ( int i = (page * 12) -12; i < Houses; ++i )
					AddDetails( i );
			}
			else
			{
				for ( int i = (page * 12) -12; i < page * 12; ++ i )
					AddDetails( i );
			}
		}

		private void AddDetails( int index )
		{	try{
			if ( index < m_List.Count )
			{	
                        		int btn;
				int row;
				btn = (index) + 101;
				row = index % 12;
//				bool online;
				BaseHouse House = m_List[index] as BaseHouse;
				Account a = House.Owner.Account as Account;

				AddLabel(32, 46 +(row * 20), 1152, String.Format( "{0}", House.Sign.Name ));
				AddLabel(132, 46 +(row * 20), 1152, String.Format( "{0}", House.Owner.Name ));
				AddLabel(415, 46 +(row * 20), 1152, String.Format( "{0} {1}", House.GetWorldLocation(), House.Map));

				AddButton( 585, 51 +(row * 20), 2437, 2438, btn, GumpButtonType.Reply, 0 );
		if ( House == null )
			{
				Console.WriteLine("No Houses In Shard...");
				return;
			}
		else if ( House.Owner == null )
				AddLabel( 285, 46 +(row * 20), RedHue, String.Format( "UnOwned" ));
		else if ( a.Banned )
				AddLabel( 285, 46 +(row * 20), RedHue, String.Format( "{0} ( Banned )", House.Owner.Account ));
		else if ( House.Owner.NetState == null )
				AddLabel( 285, 46 +(row * 20), RedHue, String.Format( "{0}", House.Owner.Account ));
		else if ( House.Owner.NetState != null )
				AddLabel( 285, 46 +(row * 20), GreenHue, String.Format( "{0}", House.Owner.Account ));
		else
				AddLabel( 285, 46 +(row * 20), RedHue, String.Format( "{0}", House.Owner.Account ));
				}
			}
				catch {}
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile from = state.Mobile;

			int buttonID = info.ButtonID;
			if ( buttonID == 2 )
			{
				m_Page ++;
				from.CloseGump( typeof( FindHouseGump ) );
				from.SendGump( new FindHouseGump( from, m_List, m_Page ) );
			}
			if ( buttonID == 1 )
			{
				m_Page --;
				from.CloseGump( typeof( FindHouseGump ) );
				from.SendGump( new FindHouseGump( from, m_List, m_Page ) );
			}
			if ( buttonID > 100 )
			{
				int index = buttonID - 101;
				BaseHouse House = m_List[index] as BaseHouse;
				Point3D xyz = House.GetWorldLocation();
				int x = xyz.X;
				int y = xyz.Y;
				int z = xyz.Z + 10;

				Point3D dest = new Point3D( x, y, z );
				from.MoveToWorld( dest, House.Map );
				
			}
		}
	}
}