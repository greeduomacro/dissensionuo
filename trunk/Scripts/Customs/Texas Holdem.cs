// Texas Hold'Em Poker

using System;
using System.Net;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using Server.Network;
using Server.Items;
using Server.Mobiles;
using Server.Gumps;
using Server.Targeting;
using Server.Prompts;
using Server.ContextMenus;
using Server.Misc;

namespace Server.Items
{
	public enum HandType
	{
		HT_HICARD,
		HT_ONEPAIR,
		HT_TWOPAIR,
		HT_THREE,
		HT_STRAIGHT,
		HT_FLUSH,
		HT_FULLHOUSE,
		HT_FOUR,
		HT_SFLUSH,
		HT_RFLUSH
	}
	public enum GumpType
	{
		Cards=0x1,
		Info=0x2,
		Buttons=0x4,
		All=0x7
	}
	public enum GameType
	{
		Normal, // First to join sets the min bet
		Casino, // min bet is set by GM and remains constant
		Tourney, // has a fixed buy-in set by GM. Not ready for use.
		Test // fixed buy in not taken from bank
	}
	public enum JoinMode
	{
		Private, // New players must be invited to join
		Public, // Anyone can join
		GM_run // New players must be invited by staff
	}

	public class PokerGame : Item
	{
		private class PlayerInfo // Info needed per-player
		{
			public PlayerMobile pm;
			public int AmountBet, BetThisRound, UsableGold;
			public PokerHand Hand=new PokerHand();
			public bool Active, Quitting, AllIn, Folded;
			public int NeedRefresh;
			public IPAddress Address;
		}

		public const int MAX_PLAYERS = 8;
		public const int USE_RANGE = 6;
		public const int WATCH_RANGE = 12;

		private static ArrayList AllGames;

		ArrayList m_Invited=new ArrayList();
		ArrayList m_Observers=new ArrayList();
		int ObsNeedRefresh;
		PlayerInfo[] m_Players;
		int m_NumPlayers;
		int m_BettingRound, m_Dealer, m_Turn;
		int m_SubRounds, m_BetThisRound, m_FirstTurn;
		int m_SinceLastBet;
		int[] m_BoardCards=new int[5];
		int m_MinBet=100;
		int m_Pot=0;
		HuryPokerTimer m_Timer;
		int m_FoldedRound;
		int m_Rake=0;
		int m_TimerDelay=20;
		int m_HighestCall;
		PokerBoard m_Board;

		GameType m_Type=GameType.Casino;
		int m_BuyIn=1000;
		JoinMode m_JoinMode=JoinMode.Public;

		[CommandProperty( AccessLevel.GameMaster )]
		public PokerBoard ScoreBoard{get{return m_Board;} set{m_Board=value;}}

		[CommandProperty( AccessLevel.GameMaster )]
		public GameType Game_Mode{get{return m_Type;} set{if(m_NumPlayers==0) m_Type=value;}}

		[CommandProperty( AccessLevel.GameMaster )]
		public int TimerDelay{
			get{return m_TimerDelay;}
			set{
				m_TimerDelay=value >= 10 ? value : 10;
				m_Timer.Stop();
				m_Timer = new HuryPokerTimer(this);
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int Rake{get{return m_Rake;} set{m_Rake=value;}}

		[CommandProperty( AccessLevel.GameMaster )]
		public int Buy_In{get{return m_BuyIn;} set{m_BuyIn=value;}}

		[CommandProperty( AccessLevel.GameMaster )]
		public int Min_Bet{get{return m_MinBet;} set{m_MinBet=value;}}

		[CommandProperty( AccessLevel.GameMaster )]
		public JoinMode Join_Mode{get{return m_JoinMode;} set{m_JoinMode=value;}}

		[Constructable]
		public PokerGame() : base( 4775 )
		{
			Weight = 1.0;
			Name = "Texas Hold'em (Double Click To Play)";
			m_FoldedRound = -1;
			m_BettingRound = 0;
			m_Timer = new HuryPokerTimer(this);
			if(AllGames == null)
				AllGames = new ArrayList();
			if(!AllGames.Contains(this)) AllGames.Add(this);
		}

		public PokerGame( Serial serial ) : base( serial )
		{
			m_Timer = new HuryPokerTimer(this);
			if(AllGames == null)
				AllGames = new ArrayList();
			if(!AllGames.Contains(this)) AllGames.Add(this);
		}
		public override void OnDelete()
		{
			AllGames.Remove(this);
			base.OnDelete();
		}
		public override void OnDoubleClick( Mobile from )
		{
			if ( !from.InRange( this.GetWorldLocation(), WATCH_RANGE ) )
				return;

			PlayerMobile pm=from as PlayerMobile;
			if(pm==null) return;
			
			int pn=PlayerNum(pm);
			if(pn != -1)
			{
				m_Players[pn].Quitting = false;
				if(m_MinBet > 0)
					InvalidateGump(pn, GumpType.All);
			}
			else if(!m_Observers.Contains(pm) && !GlobalIsPlayer(pm))
			{
				GlobalUnObserve(pm);
				m_Observers.Add(pm);
				SendObserverGump(pm);
			}
			else
				SendObserverGump(pm);
			RefreshAllGumps();
		}
		public bool DupeIP(Mobile from)
		{
			if(from.NetState == null) return false;
			for(int i=0; i<m_NumPlayers; i++)
				if(m_Players[i].Address.Equals(from.NetState.Address)) return true;
			return false;
		}
		/*public override void GetContextMenuEntries( Mobile from, ArrayList list )
		{
			list.Add( new JoinEntry( from, this ) );
			base.GetContextMenuEntries( from, list );
		}*/
		private class JoinEntry : ContextMenuEntry
		{
			private Mobile m_From;
			private PokerGame m_Game;

			public JoinEntry( Mobile from, PokerGame Game ) : base( 6116, PokerGame.USE_RANGE )
			{
				m_From = from;
				m_Game = Game;
			}

			public override void OnClick()
			{
				m_Game.JoinRequest(m_From);
			}
		}

		public bool JoinRequest(Mobile from)
		{
			if(!(from is PlayerMobile) || from.NetState == null)
				return false;
			if(m_NumPlayers == 0 && m_Type == GameType.Normal)
			{
				if(AddPlayer((PlayerMobile)from))
				{
					m_Players[0].pm.SendMessage("Please set a minimum bet value (At least 10):");
					m_Players[0].pm.Prompt = new MinBetPrompt(this);
				}
				return true;
			}
			else if(m_MinBet <= 0 && m_NumPlayers > 0)
				from.SendMessage("This game needs a minimum bet to be set.");
			else if(GlobalIsPlayer((PlayerMobile)from))
				from.SendMessage("You are already playing poker!");
			else if( !from.Alive )
				from.SendMessage("A ghost playing cards would just be too creepy.");
			else if(m_JoinMode != JoinMode.Public && !m_Invited.Contains(from) && from.AccessLevel < AccessLevel.GameMaster)
				from.SendMessage("You must be invited by a player before you may join.");
			else if(Banker.GetBalance( from ) < m_MinBet)
				from.SendMessage("You havn't enough gold to place one bet!");
			else if(m_Type == GameType.Tourney && Banker.GetBalance( from ) < m_BuyIn)
				from.SendMessage("You cannot afford the buy in!");
			else if(m_NumPlayers >= MAX_PLAYERS)
				from.SendMessage("This table is full.");
			else if(DupeIP(from))
				from.SendMessage("Multiclienting and poker just don't mix.");
			else
			{
				AddPlayer((PlayerMobile)from);
				InvalidateGump(-1, GumpType.Info);
				RefreshAllGumps();
				return true;
			}
			return false;
		}
		//  No attempt is made to save the entire state of the game, since
		//  it would be impracticle to make players wait for everyone to
		//  log back in after a server restart. Instead, everyone is simply
		//  refunded whatever gold they had put into the game.
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.WriteEncodedInt( (int) 0 );

			writer.Write( m_Board );
			writer.WriteEncodedInt( (int) m_Rake );
			writer.WriteEncodedInt( (int) m_Type );
			writer.WriteEncodedInt( (int) m_JoinMode );
			writer.WriteEncodedInt( (int) m_MinBet );
			writer.WriteEncodedInt( (int) m_BuyIn );
			if(m_Type != GameType.Test && ( m_Pot > 0 || m_Type == GameType.Tourney ))
			{
				writer.Write( m_NumPlayers );
				for(int i=0; i<m_NumPlayers; i++)
				{
					int g=0;
					if(m_Pot > 0) g+=m_Players[i].AmountBet;
					if(m_Type == GameType.Tourney) g+=m_Players[i].UsableGold;
					writer.Write( (Mobile)m_Players[i].pm );
					writer.Write( g );
				}
			}
			else
				writer.Write( 0 );

			if(m_Timer.Running)
			{
				Timer.DelayCall(TimeSpan.FromSeconds(0), new TimerStateCallback( StartTimer ), m_Timer);
				m_Timer.Stop();
			}
		}
		public static void StartTimer(Object obj)
		{
			((HuryPokerTimer)obj).ReStart();
		}
		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadEncodedInt();
			switch(version)
			{
			case 1:
				goto case 0;
			case 0:
				m_Board = (PokerBoard)reader.ReadItem();
				m_Rake = reader.ReadEncodedInt();
				m_Type = (GameType)reader.ReadEncodedInt();
				m_JoinMode = (JoinMode)reader.ReadEncodedInt();
				m_MinBet = reader.ReadEncodedInt();
				m_BuyIn = reader.ReadEncodedInt();
				if(m_Type == GameType.Normal) m_MinBet=0;
				int n=reader.ReadInt();
				if(n>0)
				{
					ArrayList al = new ArrayList();
					for(int i=0; i<n; i++)
					{
						al.Add(reader.ReadMobile());
						al.Add(reader.ReadInt());
					}
					Timer.DelayCall(TimeSpan.FromSeconds(0), new TimerStateCallback( PayBack ), al);
				}
				break;
			}
		}
		public static void PayBack(Object obj)
		{
			ArrayList al = (ArrayList)obj;
			for(int i=0; i<al.Count; i+=2)
				Banker.Deposit((Mobile)al[i], (int)al[i+1]);
		}

		private static bool GlobalIsPlayer(Mobile from)
		{
			foreach(PokerGame game in AllGames)
				if(game.IsPlayer(from))
					return true;
			return false;
		}
		private static void GlobalUnObserve(Mobile from)
		{
			foreach(PokerGame game in AllGames)
				while(game.m_Observers.Contains(from)) game.m_Observers.Remove(from);
		}

		public void MsgPlayers(String str)
		{
			//debug Console.WriteLine(str);
			for(int i=0; i<m_NumPlayers; i++) m_Players[i].pm.SendMessage(str);
			foreach(PlayerMobile pm in m_Observers) pm.SendMessage(str);
		}
		private bool IsPlayer(Mobile from)
		{
			if(m_Players==null) return false;
			for(int i=0; i<m_NumPlayers; i++)
				if(m_Players[i].pm==from) return true;
			return false;
		}
		private int PlayerNum(PlayerMobile pm)
		{
			if(m_Players==null) return -1;
			for(int i=0; i<m_NumPlayers; i++)
				if(m_Players[i].pm==pm) return i;
			return -1;
		}
		public int ActivePlayers()
		{
			int ret=0;
			for(int i=0; i<m_NumPlayers; i++)if(m_Players[i].Active) ret++;
			return ret;
		}
		private void RemovePlayer(int i)
		{
			if( i<0 || i>=m_NumPlayers ) return;
			MsgPlayers(String.Format("{0} has left the game.", m_Players[i].pm.Name));
			m_Players[i].pm.Send( new CloseGump( Gump.GetTypeID(  typeof( CardsGump) ), 0 ) );
			m_Players[i].pm.Send( new CloseGump( Gump.GetTypeID(  typeof( InfoGump) ), 0 ) );
			m_Players[i].pm.Send( new CloseGump( Gump.GetTypeID(  typeof( ButtonGump) ), 0 ) );
			if(m_Type == GameType.Tourney)
				Banker.Deposit(m_Players[i].pm, m_Players[i].UsableGold);
			if(m_Players[i].pm.NetState != null && m_Players[i].pm.InRange(GetWorldLocation(), WATCH_RANGE))
			{
				m_Observers.Add(m_Players[i].pm);
				SendObserverGump(m_Players[i].pm);
			}
			m_NumPlayers--;

			for(int j=i; j<m_NumPlayers; j++) m_Players[j]=m_Players[j+1];
			m_Players[m_NumPlayers]=null;
			if(m_NumPlayers==0)
			{
				m_BettingRound = 0;
				m_Players=null;
				if(m_Type == GameType.Normal) m_MinBet=0;
				m_Timer.Stop();
			}
		}
		private void RemovePlayer(PlayerMobile pm)
		{
			for(int i=0; i<m_NumPlayers; i++)
				if(m_Players[i].pm==pm)
					RemovePlayer(i);
		}

		private bool AddPlayer(PlayerMobile pm)
		{
			if(IsPlayer(pm) || m_NumPlayers >= MAX_PLAYERS) return false;
			if(m_Type == GameType.Tourney && !Banker.Withdraw( pm, m_BuyIn )) return false;
			GlobalUnObserve(pm);
			MsgPlayers(String.Format("{0} has joined the game.", pm.Name));
			if(m_Players == null) m_Players=new PlayerInfo[MAX_PLAYERS];
			m_Players[m_NumPlayers]=new PlayerInfo();
			m_Players[m_NumPlayers].pm=pm;
			m_Players[m_NumPlayers].AmountBet=0;
			m_Players[m_NumPlayers].BetThisRound=0;
			m_Players[m_NumPlayers].Active=false;
			m_Players[m_NumPlayers].Address = pm.NetState.Address;
			if(m_Type == GameType.Tourney || m_Type == GameType.Test) 
				m_Players[m_NumPlayers].UsableGold=m_BuyIn;
			else
				m_Players[m_NumPlayers].UsableGold=GetBalance(m_NumPlayers);
			m_Players[m_NumPlayers].Hand[0]=-1;
			InvalidateGump(-1, GumpType.Info);
			m_NumPlayers++;
			InvalidateGump(m_NumPlayers-1, GumpType.All);
			return true;
		}

		private void InvalidateGump( int i, GumpType gt )
		{
			if(i==-1)
			{
				for(i=0; i<m_NumPlayers; i++)
					m_Players[i].NeedRefresh |= (int)gt;

				ObsNeedRefresh |= (int)gt;
			}
			else
			{
				if(i<0 || i>=m_NumPlayers) return;
				m_Players[i].NeedRefresh |= (int)gt;
			}
		}

		public void FullRefresh()
		{
			for(int i=0; i<m_NumPlayers; i++)
			{
				/*FullClose(m_Players[i].pm, typeof( CardsGump));
				FullClose(m_Players[i].pm, typeof( InfoGump));
				FullClose(m_Players[i].pm, typeof( ButtonGump));*/
				//FullClose( m_Players[i].pm );
				
				m_Players[i].pm.CloseGump( typeof(CardsGump) );
				m_Players[i].pm.CloseGump( typeof(InfoGump) );
				m_Players[i].pm.CloseGump( typeof(ButtonGump) );
				
				m_Players[i].NeedRefresh=0;
				if(!m_Players[i].pm.InRange( this.GetWorldLocation(), WATCH_RANGE ))
					continue;
					
				m_Players[i].pm.SendGump(new CardsGump(this, i));
				m_Players[i].pm.SendGump(new InfoGump(this, i));
				m_Players[i].pm.SendGump(new ButtonGump(this, i, m_Players[i].pm));
			}

			for(int i=0; i<m_Observers.Count; )
			{
				PlayerMobile pm=(m_Observers[i] as PlayerMobile);
				/*FullClose(pm, typeof( CardsGump));
				FullClose(pm, typeof( InfoGump));
				FullClose(pm, typeof( ButtonGump));*/
				FullClose( pm );
				if(pm == null || pm.NetState == null || !pm.InRange( this.GetWorldLocation(), WATCH_RANGE ))
					m_Observers.RemoveAt(i);
				else
				{
					pm.SendGump(new CardsGump(this, -1));
					pm.SendGump(new InfoGump(this, -1));
					pm.SendGump(new ButtonGump(this, -1, pm));
					i++;
				}
			}
		}
		public static void FullClose( Mobile pm/*, Type type*/ )
		{
			/*NetState ns = pm.NetState;
			if(ns==null) 
				return;*/
				
			pm.CloseAllGumps();

			/*bool contains = false;
			//GumpCollection gumps = ns.Gumps;
			//FIXED by Carve
			List<Gump> gumps = new List<Gump>( ns.Gumps );

			for ( int i = 0; i < gumps.Count; )
			{
				pm.CloseAllGumps();
				if (i == null) {
					return;
				} else {
					if( i < gumps.Count )
					{
						if( gumps[i].GetType() == type )
						{
							Console.WriteLine("Your gump is numbered: {0}", i);
							contains=true;
							ns.RemoveGump( i );
						}
						else i++;
					}
				}
			}*/

			//if(contains) ns.Send( new CloseGump( Gump.GetTypeID( type ), 0 ) );
		}

		public static int CountGumps( Mobile mob, Type type )
		{
			NetState ns = mob.NetState;

			if ( ns != null )
			{
				int contains = 0;
				//GumpCollection gumps = ns.Gumps;
				//FIXED by Carve
				List<Gump> gumps = new List<Gump>( ns.Gumps );

				for ( int i = 0; i < gumps.Count; ++i )
						if( gumps[i].GetType() == type ) contains++;

				return contains;
			}
			else
			{
				return 0;
			}
		}
		private void RefreshAllGumps()
		{
			for(int i=0; i<m_NumPlayers; i++)
			{
				if((m_Players[i].NeedRefresh & (int)GumpType.Cards) != 0)
					m_Players[i].pm.Send( new CloseGump( Gump.GetTypeID(  typeof( CardsGump) ), 0 ) );
				if((m_Players[i].NeedRefresh & (int)GumpType.Info) != 0)
					m_Players[i].pm.Send( new CloseGump( Gump.GetTypeID(  typeof( InfoGump) ), 0 ) );
				if((m_Players[i].NeedRefresh & (int)GumpType.Buttons) != 0)
					m_Players[i].pm.Send( new CloseGump( Gump.GetTypeID(  typeof( ButtonGump) ), 0 ) );
			}
			for(int i=0; i<m_NumPlayers; i++)
			{
				if(!m_Players[i].pm.InRange( this.GetWorldLocation(), WATCH_RANGE ))
				{
					m_Players[i].NeedRefresh=0;
					continue;
				}
				if((m_Players[i].NeedRefresh & (int)GumpType.Cards) != 0)
					m_Players[i].pm.SendGump(new CardsGump(this, i));
				if((m_Players[i].NeedRefresh & (int)GumpType.Info) != 0)
					m_Players[i].pm.SendGump(new InfoGump(this, i));
				if((m_Players[i].NeedRefresh & (int)GumpType.Buttons) != 0)
					m_Players[i].pm.SendGump(new ButtonGump(this, i, m_Players[i].pm));
				m_Players[i].NeedRefresh=0;
			}

			for(int i=0; i<m_Observers.Count; )
			{
				PlayerMobile pm=(m_Observers[i] as PlayerMobile);
				if(pm == null || !pm.InRange( this.GetWorldLocation(), WATCH_RANGE ))
				{
					pm.Send( new CloseGump( Gump.GetTypeID(  typeof( CardsGump) ), 0 ) );
					pm.Send( new CloseGump( Gump.GetTypeID(  typeof( InfoGump) ), 0 ) );
					pm.Send( new CloseGump( Gump.GetTypeID(  typeof( ButtonGump) ), 0 ) );
					m_Observers.RemoveAt(i);
				}
				else i++;
			}
			foreach(PlayerMobile pm in m_Observers)
			{
				if((ObsNeedRefresh & (int)GumpType.Cards) != 0)
				{
					pm.Send( new CloseGump( Gump.GetTypeID(  typeof( CardsGump) ), 0 ) );
					pm.SendGump(new CardsGump(this, -1));
				}
				if((ObsNeedRefresh & (int)GumpType.Info) != 0)
				{
					pm.Send( new CloseGump( Gump.GetTypeID(  typeof( InfoGump) ), 0 ) );
					pm.SendGump(new InfoGump(this, -1));
				}
			}
			ObsNeedRefresh=0;
		}
		public void SendObserverGump(PlayerMobile pm)
		{
			pm.Send( new CloseGump( Gump.GetTypeID(  typeof( CardsGump) ), 0 ) );
			pm.Send( new CloseGump( Gump.GetTypeID(  typeof( InfoGump) ), 0 ) );
			pm.Send( new CloseGump( Gump.GetTypeID(  typeof( ButtonGump) ), 0 ) );
			pm.SendGump(new CardsGump(this, -1));
			pm.SendGump(new InfoGump(this, -1));
			pm.SendGump(new ButtonGump(this, -1, pm));
		}
		public int GetBalance(int i)
		{
			if(m_Type==GameType.Test || m_Type==GameType.Tourney)
				return m_Players[i].UsableGold;
			else
				return Banker.GetBalance( m_Players[i].pm );
		}
		private void WinGold(PlayerInfo pi, int amount)
		{
			if(m_Type != GameType.Tourney && m_Type != GameType.Test)
			{
//				Banker.Deposit(pi.pm, amount);
				int total = amount;
				while (total > 0)
				{
					if (total > 999000000)
						amount = 999000000;
					else
						amount = total;
					total -= amount;
					
					BankCheck cheque = new BankCheck(amount);
					BankBox box = pi.pm.FindBankNoCreate();
					if (!box.TryDropItem(pi.pm, cheque, false))
						pi.pm.AddToBackpack(cheque);
					else
						pi.UsableGold += amount;
				}
			}
		}
		private bool UseGold(int player, int amount)
		{
			if(m_Type != GameType.Tourney && m_Type != GameType.Test)
			{
				if(!Banker.Withdraw(m_Players[player].pm, amount))
					return false;
				m_Players[player].UsableGold -= amount;
				return true;
			}
			if(m_Players[player].UsableGold < amount)
				return false;
			m_Players[player].UsableGold -= amount;
			return true;
		}
		private bool PlaceBet(int player, int amount)
		{
			if(!UseGold(player, amount))
				return false;
			m_Pot+=amount;
			m_Players[player].AmountBet += amount;
			m_Players[player].BetThisRound += amount;
			return true;
		}

		private void StartNewHand()
		{
			int i=0;
			while(i<m_NumPlayers)
			{
				m_Players[i].UsableGold=GetBalance(i);
				if(m_Players[i].Quitting || m_Players[i].UsableGold < m_MinBet || !m_Players[i].pm.Alive)
					RemovePlayer(i);
				else
				{
					m_Players[i].Active=true;
					m_Players[i].AllIn=false;
					m_Players[i].Folded=false;
					m_Players[i].AmountBet=0;
					i++;
				}
			}
			m_BettingRound = 0;
			InvalidateGump(-1, GumpType.All);
			if(m_NumPlayers >= 2)
				NextRound();
		}
		private bool DoBlinds()
		{
			for(int i=0; i<m_NumPlayers; i++) // Boot out the deadbeats
				if(!m_Players[i].Active || GetBalance(i)<m_MinBet)
					RemovePlayer(i);

			if(m_NumPlayers < 2) return false;

			if(m_NumPlayers == 2)
			{
				PlaceBet((m_Dealer+1)%m_NumPlayers, m_MinBet);
				m_Turn = m_Dealer;
			}
			else
			{
				PlaceBet((m_Dealer+1)%m_NumPlayers, m_MinBet/2);
				PlaceBet((m_Dealer+2)%m_NumPlayers, m_MinBet);
				m_Turn = (m_Dealer+3)%m_NumPlayers;
			}
			return true;
		}
		private void Deal()
		{
			int[] deck=new int[52];
			int i, p;
			//shuffle
			for(i=0; i<52; i++) deck[i]=i;
			for(i=0; i<52; i++)
			{
				p=Utility.RandomMinMax(0, 51);
				int t=deck[p];
				deck[p]=deck[i];
				deck[i]=t;
			}
			//  Deal
			p=0;
			// Every player gets 2 hole cards
			for(i=0; i<m_NumPlayers; i++)
				if(m_Players[i].Active)
				{
					m_Players[i].Hand[0]=deck[p++];
					m_Players[i].Hand[1]=deck[p++];
				}
				else
					m_Players[i].Hand[0]=-1;

			//  Every player's hand also gets copies of the board cards
			//  for easier scoring
			for(int c=0; c<5; c++)
			{
				m_BoardCards[c]=deck[p];
				for(i=0; i<m_NumPlayers; i++)
					m_Players[i].Hand[2+c]=deck[p];
				p++;
			}
		/*	m_BoardCards[0]=0;
			m_BoardCards[1]=1;
			m_BoardCards[2]=4;
			m_BoardCards[3]=5;
			m_BoardCards[4]=6;
			for(int c=0; c<5; c++) for(i=0; i<m_NumPlayers; i++)m_Players[i].Hand[2+c]=m_BoardCards[c];
			m_Players[0].Hand[0]=2;
			m_Players[0].Hand[1]=3;*/
		}
		private void AwardPot()
		{
			ArrayList PotPlayers=new ArrayList();
			ArrayList winners=new ArrayList();
			int i;
			int LastLimit=0;
			String ResultStr = "";
			int RSlen = 0;

			while(m_Pot > 0)
			{
				int ThisLimit=0x7fffffff;

				//  Find the lowest all-in amount not yet checked
				for(i=0; i<m_NumPlayers; i++)
					if(m_Players[i].Active && m_Players[i].AmountBet > LastLimit && m_Players[i].AmountBet < ThisLimit)
						ThisLimit = m_Players[i].AmountBet;

				int ThisPot = 0;
				int UnRakeable=0;
				for(i=0; i<m_NumPlayers; i++)
				{
					if(m_Players[i].AmountBet > LastLimit)
					{
						int UnRake=Math.Min(ThisLimit, m_Players[i].AmountBet);	// Any gold in this range greater
						UnRake-=Math.Max(LastLimit, m_HighestCall);				// than the highest call is exempt
						if(UnRake > 0) UnRakeable += UnRake;					// from the rake
						if(m_Players[i].AmountBet > ThisLimit)
							ThisPot+=ThisLimit-LastLimit;
						else
							ThisPot+=m_Players[i].AmountBet-LastLimit;
					}
				}

				int ThisRake = m_Rake;
				int ThisPayout = ThisPot - ThisRake;

				//debug Console.WriteLine(String.Format("{0} ThisPot", ThisPot));
				//debug Console.WriteLine(String.Format("{0} ThisRake", ThisRake));
				//debug Console.WriteLine(String.Format("{0} ThisLimit", ThisLimit));
				//debug Console.WriteLine(String.Format("{0} m_Pot", m_Pot));
				//debug Console.WriteLine(String.Format("{0} ThisPayout", ThisPayout));
				//debug if(UnRakeable > 0) Console.WriteLine(String.Format("{0} UnRakeable", UnRakeable));

				//  Anyone who bet at least this much can win this pot
				for(i=0; i<m_NumPlayers; i++)
					if(m_Players[i].Active && m_Players[i].AmountBet >= ThisLimit) PotPlayers.Add(m_Players[i]);

				PlayerInfo winner=null;
				//  Find a winner
				foreach(PlayerInfo pi in PotPlayers)
					if(winner == null || pi.Hand > winner.Hand) winner = pi;

				// Find any ties
				foreach(PlayerInfo pi in PotPlayers)
					if(pi.Hand == winner.Hand) winners.Add(pi);

				String WinnerStr=null;
				m_Pot -= ThisPot;

				if(m_Board != null)
				{
					if(m_Board.Deleted) {m_Board = null;}
					else m_Board.AddGold(ThisRake/2);
				}
				i=0;
				foreach(PlayerInfo pi in winners)
				{
					//build a string of winner names, i.e. "Tom, Dick and Harry"
					if(i==0) WinnerStr=pi.pm.Name;
					else if(i==winners.Count-1) WinnerStr += " and " + pi.pm.Name;
					else WinnerStr += ", " + pi.pm.Name;

					WinGold(pi, ThisPayout / winners.Count);
					i++;
				}

				if(LastLimit == 0) ResultStr += String.Format("{0} {1} a pot of {2} gold.<br>", WinnerStr, (winners.Count > 1) ? "split" : "won", ThisPot);
				else ResultStr += String.Format("{0} {1} a side pot of {2}.<br>", WinnerStr, (winners.Count > 1) ? "split" : "won", ThisPot);
				RSlen++;

				PotPlayers.Clear();
				winners.Clear();
				LastLimit = ThisLimit;
			}
			
			if(m_FoldedRound==-1)
			{
				ResultStr+= "Board: ";
				for(i=0; i<5; i++)
					ResultStr+= PokerHand.CardStr(m_BoardCards[i]) + " ";
				ResultStr+= "<br>";
				for(i=0; i<m_NumPlayers; i++) if(m_Players[i].Active)
				{
					ResultStr+=m_Players[i].pm.Name + ": " + 
						PokerHand.CardStr(m_Players[i].Hand[0]) + " " +
						PokerHand.CardStr(m_Players[i].Hand[1]) + " " +
						m_Players[i].Hand.ToString() + "<br>";
				}
			}
			for(i=0; i<m_NumPlayers; i++)
			{
				//m_Players[i].pm.Send( new CloseGump( Gump.GetTypeID(  typeof( ResultGump) ), 0 ) );
				//FullClose( m_Players[i].pm, typeof( ResultGump) );
				FullClose( m_Players[i].pm );
				m_Players[i].pm.SendGump(new ResultGump(ResultStr));
			}
			foreach(PlayerMobile pm in m_Observers)
			{
				//pm.Send( new CloseGump( Gump.GetTypeID(  typeof( ResultGump) ), 0 ) );
				/*FullClose( pm, typeof( ResultGump) );*/
				FullClose( pm );
				pm.SendGump(new ResultGump(ResultStr));
			}
		}
		//  Advances to a new betting round
		private void NextRound()
		{
			int i;

			m_BettingRound++;
			m_SinceLastBet=0;
			m_SubRounds=0;
			m_BetThisRound = 0;
			for(i=0; i<m_NumPlayers; i++)
				m_Players[i].BetThisRound=0;

			//Do some setup if this is the first round
			if(m_BettingRound == 1)
			{
				m_FoldedRound=-1;
				m_Dealer=(m_Dealer+1)%m_NumPlayers;
				if(!DoBlinds())
				{
					InvalidateGump(-1, GumpType.Info);
					m_BettingRound=0;
					return;
				}
				Deal();
				m_BetThisRound = m_MinBet;
				m_HighestCall=m_MinBet;
				m_FirstTurn=m_Turn;
			}

			// Analyze each players hand to tell them what they have.
			int UpCards, round=(m_FoldedRound==-1) ? m_BettingRound : m_FoldedRound;
			if(round == 1)		UpCards=0;
			else if(round == 2)	UpCards=3;
			else if(round == 3)	UpCards=4;
			else				UpCards=5;
			for(i=0; i<m_NumPlayers; i++) if(m_Players[i].Hand[0]>=0) m_Players[i].Hand.Analyze(UpCards+2);

			m_Timer.Stop();

			if(m_BettingRound < 5) //  Setup for a regular betting round
			{
				m_Timer.Start(TimerDelay, true);
				m_Turn=m_FirstTurn;
				while(m_Players[m_Turn].AllIn || !m_Players[m_Turn].Active)
					m_Turn=(m_Turn+1)%m_NumPlayers;
				InvalidateGump(m_Turn, GumpType.Buttons );
			}

			if(m_BettingRound == 5) //  Award the pot if the hand has ended
			{
				// Send any active hands to the jackpot board
				if(m_Board != null) for(i=0; i<m_NumPlayers; i++)
				{
					if(m_Players[i].Active)
						m_Board.TestHand(m_Players[i].Hand, UpCards, m_Players[i].pm);
				}
				AwardPot();
				Timer.DelayCall(TimeSpan.FromSeconds(0), new TimerStateCallback( StartNewHandHack ), this);
				StartNewHand();
			}
			else InvalidateGump(-1, GumpType.Info | GumpType.Cards );
		}
		public static void StartNewHandHack(Object obj)
		{
			((PokerGame)obj).FullRefresh();
		}

		//  Advance turn to next active player
		private void NextTurn()
		{
			int CanRaise=0, MustCall=0, Active=0;
			for(int i=0; i<m_NumPlayers; i++)
			{
				if(m_Players[i].Active)
				{
					Active++;
					if(!m_Players[i].AllIn)
					{
						CanRaise++;
						if(m_Players[i].BetThisRound < m_BetThisRound) MustCall++;
					}
				}
			}

			//  If there are one or no players who can raise, and no one needs
			//  to call, end the hand now. If all but one folded, set m_FoldedRound
			//  so we don't show more board cards or the winner's hand
			if(CanRaise<=1 && MustCall == 0)
			{
				if(Active == 1) m_FoldedRound=m_BettingRound;
				m_BettingRound=4;
				NextRound();
				InvalidateGump(-1, GumpType.All );
				return;
			}

			//  Advance m_Turn around the table, skipping inactive or all in players.
			do
			{
				m_Turn=(m_Turn+1)%m_NumPlayers;
				if(m_Turn==m_FirstTurn) m_SubRounds++;
				m_SinceLastBet++;
			} while(!m_Players[m_Turn].Active || m_Players[m_Turn].AllIn);

			// If all active players have checked or called, advance to next round
			if(m_SinceLastBet>=m_NumPlayers)
			{
				NextRound();
				return;
			}
			InvalidateGump(-1, GumpType.Info );
			InvalidateGump(m_Turn, GumpType.Buttons );
			m_Timer.Stop();
			m_Timer.Start(TimerDelay, true);
		}
		public void OverheadMessage( string text )
		{
			Packet p = new AsciiMessage( Serial, ItemID, MessageType.Regular, 0x3b2, 3, Name, text );
			for(int i=0; i<m_NumPlayers; i++)
			{
				NetState state=m_Players[i].pm.NetState;
				if(state != null) state.Send( p );
			}
		}
		public void OverheadMessage( string text, int i )
		{
			Packet p = new AsciiMessage( Serial, ItemID, MessageType.Regular, 0x3b2, 3, Name, text );
			NetState state=m_Players[i].pm.NetState;
			if(state != null) state.Send( p );
		}

		public void Fold(int i)
		{
			m_Players[i].Folded=true; m_Players[i].Active=false;
			MsgPlayers(String.Format("{0} folds.", m_Players[i].pm.Name));
			InvalidateGump(-1, GumpType.Info );
			InvalidateGump(m_Turn, GumpType.Buttons );
		}

		private class MinBetPrompt : Prompt
		{
			private PokerGame m_Game;

			public MinBetPrompt( PokerGame Game )
			{
				m_Game=Game;
			}

			public override void OnResponse( Mobile from, string text )
			{
				int bet;
				try
				{
					bet = Convert.ToInt32( text );
				}
				catch
				{
					bet=-1;
				}
				if( bet < 10 )
				{
					from.SendMessage("The minimum bet must be a number greater than 10.");
					bet=-1;
				}
				if( Banker.GetBalance(from) < bet )
				{
					from.SendMessage("You must have enough gold in the bank to cover at least one bet.");
					bet=-1;
				}
				if( m_Game.m_Type != GameType.Normal )
					bet=-1;
				if( bet <= 0 )
					m_Game.RemovePlayer(from as PlayerMobile);
				else
					m_Game.m_MinBet=bet;

				m_Game.InvalidateGump(-1, GumpType.All );
				m_Game.RefreshAllGumps();
			}

			public override void OnCancel( Mobile from )
			{
				m_Game.RemovePlayer(from as PlayerMobile);
				m_Game.RefreshAllGumps();
			}
		}

		private class HuryPokerTimer : Timer
		{
			private PokerGame m_Game;
			private int m_Time;
			private int m_delay;
			private bool m_countdown;

			public HuryPokerTimer( PokerGame game ) : base( TimeSpan.FromSeconds( game.m_TimerDelay - 10 ),TimeSpan.FromSeconds( 1.0 ))
			{
				m_Game=game;
				Priority = TimerPriority.OneSecond;
			}


			public void ReStart()
			{
				Start(m_delay, m_countdown);
			}
			public void Start(int delay, bool countdown)
			{
				if(Running) return;
				Priority = TimerPriority.OneSecond;
				if(countdown)
				{
					delay = delay-10;
					if(delay < 0)
					{
						m_Time = -delay;
						delay = 0;
					}
					else
						m_Time = 10;
				}
				else m_Time = 0;
				this.Delay = TimeSpan.FromSeconds( delay );
				base.Start();
				m_delay = delay;
				m_countdown = countdown;
			}
			protected override void OnTick()
			{
				if(m_Time>0)
				{
					if(m_Game.m_BettingRound > 0 && m_Game.m_BettingRound < 5)
						m_Game.OverheadMessage( m_Time.ToString(), m_Game.m_Turn );
					else
						m_Game.OverheadMessage( m_Time.ToString() );
					m_Time--;
					Priority = TimerPriority.FiftyMS;
					return;
				}
				Stop();
				if(m_Game.m_BettingRound==0 && m_Game.m_NumPlayers>=2)
				{
					for(int i=0; i<m_Game.m_NumPlayers;)
					{
						if(m_Game.m_Players[i].Active)
							i++;
						else
							m_Game.RemovePlayer(i);
					}
					if(m_Game.m_NumPlayers>=2)
						m_Game.NextRound();
				}
				else
				{
					PlayerInfo pi=m_Game.m_Players[m_Game.m_Turn];
					m_Game.Fold(m_Game.m_Turn);
					if(pi.pm.NetState == null || !pi.pm.InRange( m_Game.GetWorldLocation(), PokerGame.USE_RANGE)) pi.Quitting=true;
					m_Game.NextTurn();							
				}
				m_Game.RefreshAllGumps();
			}
		}

		void call(int Playern)
		{
			PlayerInfo pi=m_Players[Playern];
			if(m_BetThisRound == 0)
			{
				MsgPlayers(String.Format("{0} checks.", pi.pm.Name));
				pi.pm.Say("Check.");
			}
			else
			{
				if(PlaceBet(Playern, m_BetThisRound - pi.BetThisRound))
				{
					MsgPlayers(String.Format("{0} calls.", pi.pm.Name));
					pi.pm.Say("Call.");
				}
				else
				{
					PlaceBet(Playern, GetBalance(Playern));
					MsgPlayers(String.Format("{0} calls.", pi.pm.Name));
					MsgPlayers(String.Format("{0} is all-in.", pi.pm.Name));
					pi.AllIn=true;
					pi.pm.Say("All in!");
				}
				if(pi.AmountBet > m_HighestCall) m_HighestCall = pi.AmountBet;
			}
		}
		void bet(int Playern, int raise)
		{
			PlayerInfo pi=m_Players[Playern];
			int ToCall = m_BetThisRound - pi.BetThisRound;
			if(!PlaceBet(Playern, ToCall + raise))
			{
				raise = GetBalance(Playern) - ToCall;
				if(raise <= 0)
				{
					call(Playern);
					return;
				}
				if(!PlaceBet(Playern, ToCall + raise))
				{
					Console.WriteLine("can't place any bet... this should never happen.");
					return;
				}
			}
			m_HighestCall = pi.AmountBet - raise;

			m_BetThisRound+=raise;
			if(m_BetThisRound == 0)
				MsgPlayers(String.Format("{0} bets {1} gold.", pi.pm.Name, raise));
			else
				MsgPlayers(String.Format("{0} raises {1} gold.", pi.pm.Name, raise));								
			if(pi.UsableGold == 0)
			{
				MsgPlayers(String.Format("{0} is all-in.", pi.pm.Name));
				pi.AllIn=true;
			}
			m_SinceLastBet=0;
		}

		public class CardsGump : Gump
		{
			private const int GUMP_BORDER = 14;
			private const int CARD_BORDER = 8;
			private const int CARD_WIDTH = 100;
			private const int CARD_HEIGHT = 150;
			private const int CARD_SPACING = 30;
			private const int CARD_GAP = 20;
			private const int ALPHA_HEIGHT = CARD_BORDER*2 + CARD_HEIGHT;

			private static readonly int[] posvals=new int[]{
				0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 
				0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 
				0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 
				0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 
				0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 
				0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 
				1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 
				1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 
				1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 
			};

			private PokerGame m_Game;
			private int m_Playern;
			private PlayerInfo pi;

			public CardsGump(PokerGame Game, int Playern) : base(5, 50)
			{
				m_Game=Game;
				m_Playern=Playern;
				if(Playern >= 0) pi=m_Game.m_Players[Playern];
				//  if Playern is -1 this is an observer

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				int ALPHA_WIDTH = CARD_BORDER*2 + CARD_WIDTH + CARD_SPACING*4;
				if(Playern >= 0 && pi.Hand[0]>-1) ALPHA_WIDTH += CARD_WIDTH + CARD_SPACING + CARD_GAP;
				this.AddPage(0);
				this.AddBackground(0, 0, ALPHA_WIDTH+GUMP_BORDER*2, ALPHA_HEIGHT+GUMP_BORDER*2, 9270);

				this.AddAlphaRegion(GUMP_BORDER, GUMP_BORDER, ALPHA_WIDTH, ALPHA_HEIGHT);
				int i;

				if(m_Game.m_BettingRound == 0)
				{
					AddLabel(GUMP_BORDER + CARD_BORDER , GUMP_BORDER + CARD_BORDER, 0x480, "Waiting for the game to start...");
				}
				else
				{
					int nextx = GUMP_BORDER + CARD_BORDER;
					int UpCards, round=(m_Game.m_FoldedRound==-1) ? m_Game.m_BettingRound : m_Game.m_FoldedRound;
					if(round == 1)		UpCards=0;
					else if(round == 2)	UpCards=3;
					else if(round == 3)	UpCards=4;
					else				UpCards=5;

					if(Playern >= 0 && pi.Hand[0]>-1)
					{
						ShowCard(nextx, GUMP_BORDER + CARD_BORDER, pi.Hand[0], false);
						nextx+=CARD_SPACING;
						ShowCard(nextx, GUMP_BORDER + CARD_BORDER, pi.Hand[1], true);
						nextx+=CARD_WIDTH + CARD_GAP;
					}
					for(i=0; i<UpCards; i++)
					{
						ShowCard(nextx, GUMP_BORDER + CARD_BORDER, m_Game.m_BoardCards[i], i==UpCards-1);
						nextx+=CARD_SPACING;
					}

				}
			}
			private void ShowCard(int x, int y, int card, bool ShowAll)
			{
				AddBackground(x, y, CARD_WIDTH, CARD_HEIGHT, 9350);
				int val = card/4;
				int suit = card%4;
				int htmlhue = suit<2 ? 0x101010 : 0xd01010;
				string SuitStr=String.Format( "<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", htmlhue, PokerHand.SuitChar[suit]);
				string CornerStr = String.Format( "<center><BASEFONT COLOR=#{0:X6}>{1}<br>{2}</BASEFONT></center>", htmlhue, PokerHand.ValStr(val), PokerHand.SuitChar[suit]);

				AddHtml(x+6, y+3, 20, 40, CornerStr, false, false);

				if(!ShowAll) return;

				AddHtml(x+76, y+112, 20, 40, CornerStr, false, false);

				int i;
				switch(val)
				{
					default:
						int s=val*21;
						for(i=0; i<21; i++) if(posvals[s+i]==1) 
							AddHtml( x+28+12*(i%3), y+21+15*(i/3), 20, 20, SuitStr, false, false );
						break;
					case 9:
						AddItem(x+35, y+50, 8425);
						break;
					case 10:
						AddItem(x+35, y+50, 8455);
						break;
					case 11:
						AddItem(x+35, y+50, 9640);
						break;
					case 12:
						AddItem(x+35, y+50, 8406);
						break;
				}
			}
			public override void OnResponse( NetState sender, RelayInfo info )
			{
				if(m_Playern>=0)
					m_Playern=m_Game.PlayerNum((PlayerMobile)sender.Mobile);
				if(m_Playern<0)
					return;
				m_Game.InvalidateGump(m_Playern, GumpType.Cards);
				m_Game.RefreshAllGumps();
			}
		}

		public class InfoGump : Gump
		{
			private const int GUMP_BORDER=14;
			private const int ENTRY_HEIGHT = 18;
			private const int ENTRY_WIDTH = 300;
			private const int ENTRY_BORDER = 3;

			private const int ALPHA_WIDTH = ENTRY_WIDTH + ENTRY_BORDER*2;
			private const int ALPHA_HEIGHT = ENTRY_HEIGHT*11 + ENTRY_BORDER*2;

			private PokerGame m_Game;
			private int m_Playern;
			private PlayerInfo pi;

			public InfoGump(PokerGame Game, int Playern) : base(5, 245)
			{
				m_Game=Game;
				m_Playern=Playern;
				if(Playern >= 0) pi=m_Game.m_Players[Playern];
				//  if Playern is -1 this is an observer

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				this.AddPage(0);
				this.AddBackground(0, 0, ALPHA_WIDTH+GUMP_BORDER*2, ALPHA_HEIGHT+GUMP_BORDER*2, 9260);
				this.AddImageTiled( GUMP_BORDER, GUMP_BORDER, ALPHA_WIDTH, ALPHA_HEIGHT, 2624 );
				this.AddAlphaRegion(GUMP_BORDER, GUMP_BORDER, ALPHA_WIDTH, ALPHA_HEIGHT);
				int i;
				int nexty = GUMP_BORDER + ENTRY_BORDER;
				int startx = GUMP_BORDER + ENTRY_BORDER;

				AddLabel(startx+16, nexty, 0x480, "Name");
				AddLabel(startx+150, nexty, 0x480, "Total Bet");
				AddLabel(startx+220, nexty, 0x480, "Available");

				nexty+=ENTRY_HEIGHT;

				for(i=0; i<m_Game.m_NumPlayers; i++)
				{
					if(i==m_Game.m_Dealer) AddImage(startx, nexty, 2087);
					int hue;
					if(m_Game.m_Players[i].Active) {if(i==m_Game.m_Turn) hue=0x36; else hue=0x480;}
					else hue=997;
					AddLabel(startx+16, nexty, hue, m_Game.m_Players[i].pm.Name);
					AddLabel(startx+150, nexty, 0x480, m_Game.m_Players[i].AmountBet.ToString());
					AddLabel(startx+220, nexty, 0x480, m_Game.m_Players[i].UsableGold.ToString());
					nexty+=ENTRY_HEIGHT;
				}
				nexty = ENTRY_HEIGHT * 10;

				if(m_Game.m_BettingRound>0)
				{
					AddLabel( startx, nexty, 0x480, String.Format("Pot: {0}", m_Game.m_Pot));
					nexty+=ENTRY_HEIGHT;
					if(Playern >= 0 && pi.Hand[0]>-1)
						AddLabel( startx, nexty, 0x480, String.Format("You have {0}.", pi.Hand.LongString()));
				}

			}
			public override void OnResponse( NetState sender, RelayInfo info )
			{
				if(m_Playern>=0)
					m_Playern=m_Game.PlayerNum((PlayerMobile)sender.Mobile);
				if(m_Playern<0)
					return;
				m_Game.InvalidateGump(m_Playern, GumpType.Info);
				m_Game.RefreshAllGumps();
			}
		}
		public class ResultGump : Gump
		{
			public ResultGump(String text) : base(539, 444)
			{
				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				this.AddPage(0);
				AddAlphaRegion(0, 0, 300, 200);
				AddHtml(0, 0, 300, 200, text, true, true);
			}
		}
		public class ButtonGump : Gump
		{
			private const int GUMP_BORDER=10;
			private const int BUTTON_WIDTH = 150;
			private const int BUTTON_HEIGHT = 21;

			private PokerGame m_Game;
			private int m_Playern;
			private PlayerInfo pi;

			private void AddButtonEntry(ArrayList list, int buttonID, string text )
			{
				list.Add(buttonID);list.Add(text);				
			}

			public ButtonGump(PokerGame Game, int Playern, Mobile from) : base(339, 244)
			{
				m_Game=Game;
				m_Playern=Playern;
				if(Playern >= 0) pi=m_Game.m_Players[Playern];
				//  if Playern is -1 this is an observer

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				ArrayList Buttons = new ArrayList();

				if(Playern >= 0) 
				{
					if(m_Game.m_BettingRound>0)
					{
						if(m_Game.m_Turn==Playern)
						{
							AddButtonEntry( Buttons, 3, "Fold" );
							if(m_Game.m_BetThisRound == 0)
							{
								AddButtonEntry( Buttons, 4, "Check" );
								if(m_Game.m_SubRounds < 3) AddButtonEntry( Buttons, 5, "Bet" );
							}
							else
							{
								AddButtonEntry( Buttons, 4, String.Format("Call ({0})", m_Game.m_BetThisRound - pi.BetThisRound) );
								if(m_Game.m_SubRounds < 3) AddButtonEntry( Buttons, 5, "Raise" );
							}
						}
					}
					else if(!pi.Active)
						AddButtonEntry( Buttons, 6, "Ready" );
					if(m_Game.m_JoinMode == JoinMode.Private || (from.AccessLevel >= AccessLevel.GameMaster && m_Game.m_JoinMode == JoinMode.GM_run))
						AddButtonEntry( Buttons, 1, "Invite" );
					if(pi.Quitting)
						AddButtonEntry( Buttons, 2, "Cancel Quit" );
					else
						AddButtonEntry( Buttons, 2, "Quit" );
				}
				else
				{
					if(from.AccessLevel >= AccessLevel.GameMaster && m_Game.m_JoinMode == JoinMode.GM_run) AddButtonEntry( Buttons, 1, "Invite" );
					AddButtonEntry( Buttons, 7, "Join" );
					AddButtonEntry( Buttons, 8, "Close" );
				}

				AddPage(0);
				AddBackground(0, 0, BUTTON_WIDTH+GUMP_BORDER*2, BUTTON_HEIGHT*(Buttons.Count/2)+GUMP_BORDER*2, 9200);
				AddImageTiled( GUMP_BORDER, GUMP_BORDER, BUTTON_WIDTH, BUTTON_HEIGHT*(Buttons.Count/2), 2624 );

				int ButtonX = GUMP_BORDER;
				int ButtonY = GUMP_BORDER;
				int i=0;
				while(i<Buttons.Count)
				{
					int buttonID=(int)Buttons[i];
					String text=(String)Buttons[i+1];
					AddAlphaRegion(ButtonX, ButtonY, BUTTON_WIDTH, BUTTON_HEIGHT);
					AddButton( ButtonX, ButtonY, 4005, 4007, buttonID, GumpButtonType.Reply, 0 );
					AddLabel( ButtonX + 35, ButtonY, 0x480, text);
					if(buttonID == 5)
					{
						AddBackground(ButtonX+70, ButtonY, 75, 20, 9300);
						AddTextEntry( ButtonX+74, ButtonY, 75, 20, 0x480, 0, null );
					}
					i+=2;
					ButtonY+=BUTTON_HEIGHT;
				}

				// this.AddAlphaRegion(GUMP_BORDER, GUMP_BORDER, ALPHA_WIDTH, ALPHA_HEIGHT);

			}
			public override void OnResponse( NetState sender, RelayInfo info )
			{
				if(m_Playern>=0)
				{
					m_Playern=m_Game.PlayerNum((PlayerMobile)sender.Mobile);
					if(m_Playern>=0)
					{
						pi=m_Game.m_Players[m_Playern];
						pi.UsableGold=m_Game.GetBalance(m_Playern);
					}
					if(sender.Mobile.AccessLevel == AccessLevel.Player && !pi.pm.InRange( m_Game.GetWorldLocation(), PokerGame.USE_RANGE) && info.ButtonID != 0 && info.ButtonID != 2)
					{
						pi.pm.SendMessage("You find it hard to play when unable to see the cards.");
						m_Game.InvalidateGump(m_Playern, GumpType.Buttons);
						m_Game.RefreshAllGumps();
						return;
					}
				}
				if(m_Playern<0 && info.ButtonID!=7 && !(sender.Mobile.AccessLevel > AccessLevel.Player && info.ButtonID==1))
				{
					sender.Mobile.Send( new CloseGump( Gump.GetTypeID(  typeof(CardsGump) ), 0 ) );
					sender.Mobile.Send( new CloseGump( Gump.GetTypeID(  typeof(InfoGump) ), 0 ) );
					sender.Mobile.Send( new CloseGump( Gump.GetTypeID(  typeof(ButtonGump) ), 0 ) );
					while(m_Game.m_Observers.Contains(sender.Mobile)) m_Game.m_Observers.Remove(sender.Mobile);
					return;
				}
				switch(info.ButtonID)
				{
					case 0:	//close
						if((pi.Quitting && m_Game.m_BettingRound==5))
							goto case 2;
						break;
					case 1: //invite
						sender.Mobile.BeginTarget( -1, false, TargetFlags.None, new TargetCallback( OnInviteTarget ) );
						break;
					case 2: //quit
						if(m_Game.m_BettingRound == 0)
						{
							m_Game.RemovePlayer(m_Playern);
							m_Game.InvalidateGump(-1, GumpType.Info);
						}
						else
						{
							pi.Quitting=!pi.Quitting;
						}
						break;
					case 3: //fold
						if(m_Game.m_Turn != m_Playern)
							break;
						m_Game.Fold(m_Playern);
						m_Game.NextTurn();
						break;
						//goto nextturn;
					case 4: //check/call
						if(m_Game.m_Turn != m_Playern)
							break;
						m_Game.call(m_Playern);
						m_Game.NextTurn();
						break;
						//goto nextturn;
					case 5: //bet/raise
						if(m_Game.m_Turn != m_Playern)
							break;
						int bet=Utility.ToInt32( info.GetTextEntry( 0 ).Text );
						if(bet < m_Game.m_MinBet)
						{
							pi.pm.SendMessage(String.Format("Bets and raises must be at least {0} gold.", m_Game.m_MinBet));
							break;
						}
						m_Game.bet(m_Playern, bet);
						m_Game.NextTurn();
						break;
						//goto nextturn;
					case 6: //ready
						if(m_Game.m_BettingRound!=0) break;
						pi.Active=true;
						pi.Quitting=false;

						int readyplayers=0;
						for(int i=0; i<m_Game.m_NumPlayers; i++)
							if(m_Game.m_Players[i].Active)
								readyplayers++;

						if(readyplayers>=2)
						{
							if(readyplayers == m_Game.m_NumPlayers)
								m_Game.StartNewHand();
							else
								m_Game.m_Timer.Start(m_Game.TimerDelay, false);
						}
						m_Game.InvalidateGump(-1, GumpType.Info);
						break;
					case 7: //join
						if(m_Game.JoinRequest(sender.Mobile))
						{
							m_Game.RefreshAllGumps();
							return;
						}
						else
						{
							sender.Mobile.SendGump(new ButtonGump(m_Game, -1, sender.Mobile));
							return;
						}
					nextturn:
						m_Game.NextTurn();
						break;
				}
				m_Game.InvalidateGump(m_Playern, GumpType.Buttons);
				m_Game.RefreshAllGumps();
			}
			public void OnInviteTarget( Mobile from, object obj )
			{
				Mobile m=obj as Mobile;
				PlayerMobile pm=obj as PlayerMobile;
				if(m == null)
					from.SendMessage( "It can't seem to grasp the concept of game." );
				else if ( from == m && from.AccessLevel < AccessLevel.GameMaster )
					from.SendLocalizedMessage( 502054 ); // That's a silly thing to do.
				else if ( !m.Player && m.Body.IsHuman )
					m.SayTo( from, 1005443 ); // Nay, I would rather stay here and watch a nail rust.
				else if ( !m.Player )
					from.SendLocalizedMessage( 1005444 ); // The creature ignores your offer.
				else if (pm==null)
					return;
				else if ( m_Game.IsPlayer(pm) )
					from.SendMessage( "That person is already playing!" );
				else if ( m_Game.m_Invited.Contains(pm) )
					from.SendMessage("They have already been invited to join.");
				else
				{
					m_Game.m_Invited.Add(pm);
					m_Game.MsgPlayers(String.Format("{0} has invited {1} to join the game.", from.Name, pm.Name));
					pm.SendMessage(String.Format("{0} has invited you to join his poker game.", from.Name));
				}
			}
		}
	}
	public class PokerHand
	{
		public int[] m_Cards = new int[7];
		public int[] m_TieBreak = new int[5];
		public HandType m_Type;

		public static readonly string[] SuitChar = new string[]{"\u2660", "\u2663", "\u2665", "\u25c6"};
		public static string ValStr(int val)
		{
			switch(val)
			{
				case 9:
					return "J";
				case 10:
					return "Q";
				case 11:
					return "K";
				case 12:
					return "A";
				default:
					return(val+2).ToString();
			}
		}
		public static string ValStrFull(int val)
		{
			switch(val)
			{
				case 9:
					return "Jack";
				case 10:
					return "Queen";
				case 11:
					return "King";
				case 12:
					return "Ace";
				default:
					return(val+2).ToString();
			}
		}
		public override String ToString()
		{
			switch(m_Type)
			{
			case HandType.HT_HICARD: return "a high card";
			case HandType.HT_ONEPAIR: return "a pair";
			case HandType.HT_TWOPAIR: return "two pair";
			case HandType.HT_THREE: return "three of a kind";
			case HandType.HT_STRAIGHT: return "a straight";
			case HandType.HT_FLUSH: return "a flush";
			case HandType.HT_FULLHOUSE: return "a full house";
			case HandType.HT_FOUR: return "four of a kind";
			case HandType.HT_SFLUSH: return "a straight flush";
			case HandType.HT_RFLUSH: return "a royal flush";
			}
			return "?";
		}
		public String LongString()
		{
			switch(m_Type)
			{
			case HandType.HT_HICARD: return ((m_TieBreak[0]==12) ? "an " : "a ") + ValStrFull(m_TieBreak[0]) + " high";
			case HandType.HT_ONEPAIR: return "a pair of " + ValStrFull(m_TieBreak[0]) + "s";
			case HandType.HT_TWOPAIR: return "two pair: " + ValStrFull(m_TieBreak[0]) + "s and " + ValStrFull(m_TieBreak[1]) + "s";
			case HandType.HT_THREE: return "three " + ValStrFull(m_TieBreak[0]) + "s";
			case HandType.HT_STRAIGHT: return ((m_TieBreak[0]==12) ? "an " : "a ") + ValStrFull(m_TieBreak[0]) + "-high straight";
			case HandType.HT_FLUSH: return ((m_TieBreak[0]==12) ? "an " : "a ") + ValStrFull(m_TieBreak[0]) + "-high flush";
			case HandType.HT_FULLHOUSE: return "a full house: " + ValStrFull(m_TieBreak[0]) + "s over " + ValStrFull(m_TieBreak[1]) + "s";
			case HandType.HT_FOUR: return "four " + ValStrFull(m_TieBreak[0]) + "s";
			case HandType.HT_SFLUSH: return ((m_TieBreak[0]==12) ? "an " : "a ") + ValStrFull(m_TieBreak[0]) + "-high straight flush";
			case HandType.HT_RFLUSH: return "a royal flush";
			}
			return "?";
		}
		public int Type {
			get{
				return (int)m_Type;
			}
		}
		public int this[int index] {
			get{ return m_Cards[index];}
			set{ m_Cards[index] = value;}
		}
		public static String CardStr(int i) // a short string of val and suit representing card i
		{
			return ValStr(i/4)+SuitChar[i%4];
		}
																			
		//  Starting here is code to analyze a hand's value. I will set m_Type
		//  to the kind of hand, i.e. HT_STRAIGHT, HT_FULLHOUSE, etc... It will
		//  also set the m_TieBreak values, which are card values(no suits) which
		//  will break a tie. These are set differently for each hand type. With
		//  a highcard, for example, tie break values will be the best 5 cards in
		//  order starting with the best. With a full house, they will be:
		//  <val of best 3, val of best 2, 0,0,0>. To compare hands, first m_Type
		//  is compared, and if tied, the first value in m_TieBreak that differs
		//  determines the winner. This code has been tested to death.

		private int TakeHighest(ref int[] cards, int lookat)
		{
			int hp=-1, hc=-1;
			for(int i=0; i<lookat; i++)
				if(cards[i]>hc)
				{
					hp=i;
					hc=cards[i];
				}
			cards[hp]=0;
			return hc/4;
		}
		private int TakeHighest(ref int[] cards, int lookat, int n, ref int[] cv)
		{
			int hc=-1;
			for(int i=0; i<lookat; i++)
			{
				if(cards[i]>hc && cv[cards[i]/4]==n)
					hc=cards[i];
			}
			int hv=hc/4;
			for(int i=0; i<lookat; i++)
			{
				if(cards[i]/4==hv) cards[i]=0;
			}
			cv[hv]-=n;
			return hc/4;
		}
		public void Analyze(int lookat)
		{
			// maxs = highest number of cards in one suit. 5 or more = flush
			// straight_high = high card of straight, or -1 if no straight
			// maxv = highest number of cards of one value, i.e. maxv of 4 means 4 of a kind
			// pairs = number of card values appearing 2 or more times in a hand.
			int i, maxv=0, maxs=0, pairs=0, straight_high=-1, flush_suit=-1;
			int[] cv=new int[]{0,0,0,0,0,0,0,0,0,0,0,0,0}, cs=new int[]{0,0,0,0};
			int[] thand=new int[7];
			for(i=0; i<5; i++)
				m_TieBreak[i]=0;

			for(i=0; i<lookat; i++)
			{
				thand[i]=m_Cards[i];
				int v=m_Cards[i]/4, s=m_Cards[i]%4;
				cs[s]++;
				cv[v]++;
				maxv=Math.Max(maxv, cv[v]);
				if(cv[v]==2) pairs++;
			}
			for(i=0; i<4; i++)
				if(cs[i]>maxs)
				{
					maxs=cs[i];
					flush_suit=i;
				}
			i=0;
			while(i<13)
			{
				int st=0, AddAce=0;
				for(; i<13 && cv[i]==0; i++);
				if(i==0 && cv[12]>0) AddAce=1;
				for(st=0; i+st<13 && cv[i+st]!=0; st++);
				if(st+AddAce>=5) straight_high=i+st-1;
				i+=st;
			}

			if(maxs>=5 && straight_high>=0)
			{
				bool sflush = true;
				for(i=straight_high-4; i<=straight_high; i++)
				{
					bool HasCard=false;
					if(i==-1)
					{
						for(int n=0; n<lookat && !HasCard; n++)
						{
							if(m_Cards[n]/4 == 12 && m_Cards[n]%4 == flush_suit)
								HasCard=true;
						}
						if(HasCard)
							continue;
						else
						{
							sflush = false;
							break;
						}
					}
					for(int n=0; n<lookat && !HasCard; n++)
					{
						if(m_Cards[n]/4 == i && m_Cards[n]%4 == flush_suit)
							HasCard=true;
					}
					if(!HasCard)
					{
						sflush = false;
						break;
					}
				}
				if(sflush)
				{					
					if(straight_high==12)
					{
						m_Type=HandType.HT_RFLUSH;
						return;
					}
					m_Type=HandType.HT_SFLUSH;
					m_TieBreak[0]=straight_high;
					return;
				}
			}
			if(maxv>=4)
			{
				m_Type=HandType.HT_FOUR;
				m_TieBreak[0]=TakeHighest(ref thand, lookat, 4, ref cv);
				m_TieBreak[1]=TakeHighest(ref thand, lookat);
				return;
			}
			if(maxv>=3 && pairs>=2)
			{
				m_Type=HandType.HT_FULLHOUSE;
				m_TieBreak[0]=TakeHighest(ref thand, lookat, 3, ref cv);
				m_TieBreak[1]=TakeHighest(ref thand, lookat, 2, ref cv);
				return;
			}
			if(maxs>=5)
			{
				m_Type=HandType.HT_FLUSH;
				for(i=0;i<lookat;i++) if(thand[i]%4 != flush_suit) thand[i]=0;
				for(i=0;i<5;i++)m_TieBreak[i]=TakeHighest(ref thand, lookat);
				return;
			}
			if(straight_high>=0)
			{
				m_Type=HandType.HT_STRAIGHT;
				m_TieBreak[0]=straight_high;
				return;

			}
			if(maxv==3)
			{
				m_Type=HandType.HT_THREE;
				m_TieBreak[0]=TakeHighest(ref thand, lookat, 3, ref cv);
				for(i=0;i<2;i++)m_TieBreak[i+1]=TakeHighest(ref thand, lookat);
				return;
			}
			if(pairs>=2)
			{
				m_Type=HandType.HT_TWOPAIR;
				m_TieBreak[0]=TakeHighest(ref thand, lookat, 2, ref cv);
				m_TieBreak[1]=TakeHighest(ref thand, lookat, 2, ref cv);
				m_TieBreak[2]=TakeHighest(ref thand, lookat);
				return;
			}
			if(pairs==1)
			{
				m_Type=HandType.HT_ONEPAIR;
				m_TieBreak[0]=TakeHighest(ref thand, lookat, 2, ref cv);
				for(i=0;i<3;i++)m_TieBreak[i+1]=TakeHighest(ref thand, lookat);
				return;
			}
			m_Type=HandType.HT_HICARD;
			for(i=0;i<5;i++)m_TieBreak[i]=TakeHighest(ref thand, lookat);
		}
		public static bool operator < (PokerHand a, PokerHand b)
		{
			if(a.Type < b.Type) return true;
			if(a.Type > b.Type) return false;
			for(int i=0; i<5; i++)
			{
				if(a.m_TieBreak[i] < b.m_TieBreak[i]) return true;
				if(a.m_TieBreak[i] > b.m_TieBreak[i]) return false;
			}
			return false;
		}
		public static bool operator > (PokerHand a, PokerHand b)
		{
			if(a.Type < b.Type) return false;
			if(a.Type > b.Type) return true;
			for(int i=0; i<5; i++)
			{
				if(a.m_TieBreak[i] < b.m_TieBreak[i]) return false;
				if(a.m_TieBreak[i] > b.m_TieBreak[i]) return true;
			}
			return false;
		}
		public static bool operator == (PokerHand a, PokerHand b)
		{
			if(a.Type != b.Type) return false;
			for(int i=0; i<5; i++)
				if(a.m_TieBreak[i] != b.m_TieBreak[i]) return false;
			return true;
		}
		public static bool operator != (PokerHand a, PokerHand b) {return !(a==b);}
		public override int GetHashCode() {return Type;} //compiler bitches if this isn't here
		public override bool Equals(object o)
		{
			if(!(o is PokerHand)) return false;
			return this==(PokerHand)o;
		}
	}



	[Flipable( 0x1E5E, 0x1E5F )]
	public class PokerBoard : Item
	{
		PokerHand m_BestHand=new PokerHand();
		ArrayList m_BestFrom=new ArrayList();
		bool m_HasBeenSet=false, m_AutoPayout=false;
		TimeSpan m_PayoutTime=TimeSpan.Zero;
		int m_Jackpot;
		String m_Text;
		int m_UpCards=0;
		Timer m_PayoutTimer;

		[CommandProperty( AccessLevel.GameMaster )]
		public int Jackpot{get{return m_Jackpot;} set{m_Jackpot=value;}}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool AutoPayout{get{return m_AutoPayout;} set{m_AutoPayout=value;UpdateTimer();}}

		[CommandProperty( AccessLevel.GameMaster )]
		public TimeSpan PayoutTime{get{return m_PayoutTime;} set{m_PayoutTime=value;UpdateTimer();}}

		[Constructable]
		public PokerBoard() : base( 0x1e5e )
		{
			Weight = 1;
			UpdateText();
		}
		public PokerBoard( Serial serial ) : base( serial )
		{
		}
		public override void Serialize( GenericWriter writer )
		{
			int i;
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 2 ); // version
			writer.Write(m_AutoPayout);
			writer.Write(m_PayoutTime);
			writer.WriteEncodedInt( m_UpCards );
			writer.Write( m_HasBeenSet );
			writer.WriteMobileList( m_BestFrom );
			writer.Write(m_Jackpot);
			for(i=0; i<7; i++) writer.WriteEncodedInt(m_BestHand[i]);
		}
		public override void Deserialize( GenericReader reader )
		{
			int i;
			base.Deserialize( reader );
			int version = reader.ReadEncodedInt();

			switch(version)
			{
			case 2:
				m_AutoPayout = reader.ReadBool();
				m_PayoutTime = reader.ReadTimeSpan();
				goto case 1;
			case 1:
				m_UpCards = reader.ReadEncodedInt();
				if(m_UpCards==0) m_UpCards=5;
				m_HasBeenSet = reader.ReadBool();
				m_BestFrom = reader.ReadMobileList();
				m_Jackpot = reader.ReadInt();
				for(i=0; i<7; i++) m_BestHand[i] = reader.ReadEncodedInt();
				break;
			case 0:
				m_HasBeenSet = reader.ReadBool();
				m_BestFrom = reader.ReadMobileList();
				m_Jackpot = reader.ReadInt();
				for(i=0; i<7; i++) m_BestHand[i] = reader.ReadEncodedInt();
				for(i=0; i<5; i++) m_BestHand.m_TieBreak[i] = reader.ReadEncodedInt();
				m_UpCards=5;
				break;
			}
			m_BestHand.Analyze(2+m_UpCards);
			UpdateText();
			UpdateTimer();
		}
		public void UpdateTimer()
		{
			if(m_PayoutTimer != null)
			{
				m_PayoutTimer.Stop();
				m_PayoutTimer=null;
			}
			if(!m_AutoPayout) return;
			TimeSpan togo = m_PayoutTime - (DateTime.Now).TimeOfDay;
			if(togo < TimeSpan.Zero) togo += TimeSpan.FromDays( 1 );
			m_PayoutTimer = Timer.DelayCall( togo, new TimerCallback(Payout) );
			m_PayoutTimer.Priority = TimerPriority.OneMinute;
//			Console.WriteLine(togo.ToString());
		}
		public override void OnDoubleClick( Mobile from )
		{
			NetState state=from.NetState;
			if(state != null)
			{
				Packet p = new AsciiMessage( Serial, ItemID, MessageType.Regular, 0x3b2, 3, Name, m_Text );
				state.Send( p );
			}
		}
		public void UpdateText()
		{
			if(!m_HasBeenSet)
			{
				m_Text = "No one has played a hand yet!";
				return;
			}
			m_Text = "The jackpot is " + m_Jackpot.ToString() + " gold. ";
			
			int i=0;
			foreach(Mobile mob in m_BestFrom)
			{
				if(i==0) m_Text+=mob.Name;
				else if(i==m_BestFrom.Count-1) m_Text += " and " + mob.Name;
				else m_Text += ", " + mob.Name;
				i++;
			}
			if(m_BestFrom.Count == 1)
				m_Text += " leads with ";
			else
				m_Text += " lead with ";
			m_Text += m_BestHand.LongString();
		}
		public void TestHand(PokerHand hand, int UpCards, Mobile from)
		{
			if( m_HasBeenSet && (hand < m_BestHand) ) return;
			if( !m_HasBeenSet || hand > m_BestHand)
			{
				m_BestFrom.Clear();
				for(int i=0; i<7; i++)
				{
					m_BestHand[i] = hand[i];
				}
				m_BestHand.Analyze(2+UpCards);
				m_UpCards=UpCards;
			}
			if(!m_BestFrom.Contains(from)) m_BestFrom.Add(from);
			m_HasBeenSet=true;
			UpdateText();
		}
		public void AddGold(int ammount)
		{
			m_Jackpot+=ammount;
			UpdateText();
		}
		/*public override void GetContextMenuEntries( Mobile from, ArrayList list )
		{
			if(from.AccessLevel >= AccessLevel.GameMaster) list.Add( new PayoutEntry( this ) );
			base.GetContextMenuEntries( from, list );
		}*/
		public void Payout()
		{
			UpdateTimer();
			if(!m_HasBeenSet || m_Jackpot <= 0) return;
			int i=0;
			String Message="";
			foreach(Mobile mob in m_BestFrom)
			{
				Banker.Deposit(mob, m_Jackpot/m_BestFrom.Count);
				if(i==0) Message+=mob.Name;
				else if(i==m_BestFrom.Count-1) Message += " and " + mob.Name;
				else Message += ", " + mob.Name;
				i++;
			}
			Message += " won the poker jackpot of " + m_Jackpot.ToString() + " gold with ";
			Message += m_BestHand.LongString() + "!";
			foreach ( NetState state in NetState.Instances )
			{
				Mobile m = state.Mobile;
				if ( m != null )
					m.SendMessage( 0x482, Message );
			}
			m_Jackpot = 0;
			m_BestFrom.Clear();
			m_HasBeenSet=false;
			UpdateText();
		}
		private class PayoutEntry : ContextMenuEntry
		{
			private PokerBoard m_Board;
			public PayoutEntry( PokerBoard Board ) : base( 188, -1 )
			{
				m_Board = Board;
			}
			public override void OnClick()
			{
				m_Board.Payout();
			}
		}
	}
}