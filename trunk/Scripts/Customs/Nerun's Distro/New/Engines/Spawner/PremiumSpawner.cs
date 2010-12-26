///////////////////////////
//       By Nerun        //
//    Engine v5.2.2      //
///////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Server;
using Server.Commands;
using Server.Items;
using Server.Network;
using CPA = Server.CommandPropertyAttribute;

namespace Server.Mobiles
{
	public class PremiumSpawner : Item
	{
		private int m_Team;
		private int m_HomeRange;          // = old SpawnRange
		private int m_WalkingRange = -1;  // = old HomeRange
		private int m_SpawnID = 1;
		private int m_Count;
		private int m_SubCountA;
		private int m_SubCountB;
		private int m_SubCountC;
		private int m_SubCountD;
		private int m_SubCountE;
		private TimeSpan m_MinDelay;
		private TimeSpan m_MaxDelay;
		private List<string> m_CreaturesName;
		private List<IEntity> m_Creatures;
		private List<string> m_SubSpawnerA;
		private List<IEntity> m_SubCreaturesA;
		private List<string> m_SubSpawnerB;
		private List<IEntity> m_SubCreaturesB;
		private List<string> m_SubSpawnerC;
		private List<IEntity> m_SubCreaturesC;
		private List<string> m_SubSpawnerD;
		private List<IEntity> m_SubCreaturesD;
		private List<string> m_SubSpawnerE;
		private List<IEntity> m_SubCreaturesE;
		private DateTime m_End;
		private InternalTimer m_Timer;
		private bool m_Running;
		private bool m_Group;
		private WayPoint m_WayPoint;

		public bool IsFull{ get{ return ( m_Creatures != null && m_Creatures.Count >= m_Count ); } }
		public bool IsFulla{ get{ return ( m_SubCreaturesA != null && m_SubCreaturesA.Count >= m_SubCountA ); } }
		public bool IsFullb{ get{ return ( m_SubCreaturesB != null && m_SubCreaturesB.Count >= m_SubCountB ); } }
		public bool IsFullc{ get{ return ( m_SubCreaturesC != null && m_SubCreaturesC.Count >= m_SubCountC ); } }
		public bool IsFulld{ get{ return ( m_SubCreaturesD != null && m_SubCreaturesD.Count >= m_SubCountD ); } }
		public bool IsFulle{ get{ return ( m_SubCreaturesE != null && m_SubCreaturesE.Count >= m_SubCountE ); } }
		
		public List<string> CreaturesName
		{
			get { return m_CreaturesName; }
			set
			{
				m_CreaturesName = value;
				if ( m_CreaturesName.Count < 1 )
					Stop();

				InvalidateProperties();
			}
		}

		public List<string> SubSpawnerA
		{
			get { return m_SubSpawnerA; }
			set
			{
				m_SubSpawnerA = value;
				if ( m_SubSpawnerA.Count < 1 )
					Stop();

				InvalidateProperties();
			}
		}

		public List<string> SubSpawnerB
		{
			get { return m_SubSpawnerB; }
			set
			{
				m_SubSpawnerB = value;
				if ( m_SubSpawnerB.Count < 1 )
					Stop();

				InvalidateProperties();
			}
		}

		public List<string> SubSpawnerC
		{
			get { return m_SubSpawnerC; }
			set
			{
				m_SubSpawnerC = value;
				if ( m_SubSpawnerC.Count < 1 )
					Stop();

				InvalidateProperties();
			}
		}

		public List<string> SubSpawnerD
		{
			get { return m_SubSpawnerD; }
			set
			{
				m_SubSpawnerD = value;
				if ( m_SubSpawnerD.Count < 1 )
					Stop();

				InvalidateProperties();
			}
		}

		public List<string> SubSpawnerE
		{
			get { return m_SubSpawnerE; }
			set
			{
				m_SubSpawnerE = value;
				if ( m_SubSpawnerE.Count < 1 )
					Stop();

				InvalidateProperties();
			}
		}

		public virtual int CreaturesNameCount { get { return m_CreaturesName.Count; } }
		public virtual int SubSpawnerACount { get { return m_SubSpawnerA.Count; } }
		public virtual int SubSpawnerBCount { get { return m_SubSpawnerB.Count; } }
		public virtual int SubSpawnerCCount { get { return m_SubSpawnerC.Count; } }
		public virtual int SubSpawnerDCount { get { return m_SubSpawnerD.Count; } }
		public virtual int SubSpawnerECount { get { return m_SubSpawnerE.Count; } }

		public override void OnAfterDuped( Item newItem )
		{
			PremiumSpawner s = newItem as PremiumSpawner;

			if ( s == null )
				return;

			s.m_CreaturesName = new List<string>( m_CreaturesName );
			s.m_SubSpawnerA = new List<string>( m_SubSpawnerA );
			s.m_SubSpawnerB = new List<string>( m_SubSpawnerB );
			s.m_SubSpawnerC = new List<string>( m_SubSpawnerC );
			s.m_SubSpawnerD = new List<string>( m_SubSpawnerD );
			s.m_SubSpawnerE = new List<string>( m_SubSpawnerE );
		}
		
		[CommandProperty( AccessLevel.GameMaster )]
		public int Count
		{
			get { return m_Count; }
			set { m_Count = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int CountA
		{
			get { return m_SubCountA; }
			set { m_SubCountA = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int CountB
		{
			get { return m_SubCountB; }
			set { m_SubCountB = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int CountC
		{
			get { return m_SubCountC; }
			set { m_SubCountC = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int CountD
		{
			get { return m_SubCountD; }
			set { m_SubCountD = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int CountE
		{
			get { return m_SubCountE; }
			set { m_SubCountE = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public WayPoint WayPoint
		{
			get
			{
				return m_WayPoint;
			}
			set
			{
				m_WayPoint = value;
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Running
		{
			get { return m_Running; }
			set
			{
				if ( value )
					Start();
				else
					Stop();

				InvalidateProperties();
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int HomeRange
		{
			get { return m_HomeRange; }
			set { m_HomeRange = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )] 
		public int WalkingRange 
		{ 
		   get { return m_WalkingRange; } 
		   set { m_WalkingRange = value; InvalidateProperties(); } 
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int SpawnID
		{
			get { return m_SpawnID; }
			set { m_SpawnID = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int Team
		{
			get { return m_Team; }
			set { m_Team = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public TimeSpan MinDelay
		{
			get { return m_MinDelay; }
			set { m_MinDelay = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public TimeSpan MaxDelay
		{
			get { return m_MaxDelay; }
			set { m_MaxDelay = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public TimeSpan NextSpawn
		{
			get
			{
				if ( m_Running )
					return m_End - DateTime.Now;
				else
					return TimeSpan.FromSeconds( 0 );
			}
			set
			{
				Start();
				DoTimer( value );
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Group
		{
			get { return m_Group; }
			set { m_Group = value; InvalidateProperties(); }
		}

		[Constructable]
		public PremiumSpawner( int amount, int subamountA, int subamountB, int subamountC, int subamountD, int subamountE, int spawnid, int minDelay, int maxDelay, int team, int homeRange, int walkingRange, string creatureName, string subSpawnerA, string subSpawnerB, string subSpawnerC, string subSpawnerD, string subSpawnerE ) : base( 0x1f13 )
		{
			List<string> creaturesName = new List<string>();
			creaturesName.Add( creatureName );

			List<string> subSpawnerAA = new List<string>();
			creaturesName.Add( subSpawnerA );

			List<string> subSpawnerBB = new List<string>();
			creaturesName.Add( subSpawnerB );

			List<string> subSpawnerCC = new List<string>();
			creaturesName.Add( subSpawnerC );

			List<string> subSpawnerDD = new List<string>();
			creaturesName.Add( subSpawnerD );

			List<string> subSpawnerEE = new List<string>();
			creaturesName.Add( subSpawnerE );

			InitSpawn( amount, subamountA, subamountB, subamountC, subamountD, subamountE, spawnid, TimeSpan.FromMinutes( minDelay ), TimeSpan.FromMinutes( maxDelay ), team, homeRange, walkingRange, creaturesName, subSpawnerAA, subSpawnerBB, subSpawnerCC, subSpawnerDD, subSpawnerEE );
		}

		[Constructable]
		public PremiumSpawner( string creatureName ) : base( 0x1f13 )
		{
			List<string> creaturesName = new List<string>();
			creaturesName.Add( creatureName );

			List<string> subSpawnerAA = new List<string>();
			List<string> subSpawnerBB = new List<string>();
			List<string> subSpawnerCC = new List<string>();
			List<string> subSpawnerDD = new List<string>();
			List<string> subSpawnerEE = new List<string>();

			InitSpawn( 1, 0, 0, 0, 0, 0, 1, TimeSpan.FromMinutes( 5 ), TimeSpan.FromMinutes( 10 ), 0, 4, -1, creaturesName, subSpawnerAA, subSpawnerBB, subSpawnerCC, subSpawnerDD, subSpawnerEE );
		}

		[Constructable]
		public PremiumSpawner() : base( 0x1f13 )
		{
			List<string> creaturesName = new List<string>();

			List<string> subSpawnerAA = new List<string>();
			List<string> subSpawnerBB = new List<string>();
			List<string> subSpawnerCC = new List<string>();
			List<string> subSpawnerDD = new List<string>();
			List<string> subSpawnerEE = new List<string>();

			InitSpawn( 1, 0, 0, 0, 0, 0, 1, TimeSpan.FromMinutes( 5 ), TimeSpan.FromMinutes( 10 ), 0, 4, -1, creaturesName, subSpawnerAA, subSpawnerBB, subSpawnerCC, subSpawnerDD, subSpawnerEE );
		}

		public PremiumSpawner( int amount, int subamountA, int subamountB, int subamountC, int subamountD, int subamountE, int spawnid, TimeSpan minDelay, TimeSpan maxDelay, int team, int homeRange, int walkingRange, List<string> creaturesName, List<string> subSpawnerAA, List<string> subSpawnerBB, List<string> subSpawnerCC, List<string> subSpawnerDD, List<string> subSpawnerEE )
			: base( 0x1f13 )
		{
			InitSpawn( amount, subamountA, subamountB, subamountC, subamountD, subamountE, spawnid, minDelay, maxDelay, team, homeRange, walkingRange, creaturesName, subSpawnerAA, subSpawnerBB, subSpawnerCC, subSpawnerDD, subSpawnerEE );
		}

		public override string DefaultName
		{
			get { return "PremiumSpawner"; }
		}

		public void InitSpawn( int amount, int subamountA, int subamountB, int subamountC, int subamountD, int subamountE, int SpawnID, TimeSpan minDelay, TimeSpan maxDelay, int team, int homeRange, int walkingRange, List<string> creaturesName, List<string> subSpawnerAA, List<string> subSpawnerBB, List<string> subSpawnerCC, List<string> subSpawnerDD, List<string> subSpawnerEE )
		{
			Name = "PremiumSpawner";
			m_SpawnID = SpawnID;
			Visible = false;
			Movable = false;
			m_Running = true;
			m_Group = false;
			m_MinDelay = minDelay;
			m_MaxDelay = maxDelay;
			m_Count = amount;
			m_SubCountA = subamountA;
			m_SubCountB = subamountB;
			m_SubCountC = subamountC;
			m_SubCountD = subamountD;
			m_SubCountE = subamountE;
			m_Team = team;
			m_HomeRange = homeRange;
			m_WalkingRange = walkingRange;
			m_CreaturesName = creaturesName;
			m_SubSpawnerA = subSpawnerAA;
			m_SubSpawnerB = subSpawnerBB;
			m_SubSpawnerC = subSpawnerCC;
			m_SubSpawnerD = subSpawnerDD;
			m_SubSpawnerE = subSpawnerEE;
			m_Creatures = new List<IEntity>();
			m_SubCreaturesA = new List<IEntity>();
			m_SubCreaturesB = new List<IEntity>();
			m_SubCreaturesC = new List<IEntity>();
			m_SubCreaturesD = new List<IEntity>();
			m_SubCreaturesE = new List<IEntity>();
			DoTimer( TimeSpan.FromSeconds( 1 ) );
		}
			
		public PremiumSpawner( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from.AccessLevel < AccessLevel.GameMaster )
				return;

			PremiumSpawnerGump g = new PremiumSpawnerGump( this );
			from.SendGump( g );
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( m_Running )
			{
				list.Add( 1060742 ); // active

				list.Add( 1060656, m_Count.ToString() );
				list.Add( 1061169, m_HomeRange.ToString() );
				list.Add( 1060658, "walking range\t{0}", m_WalkingRange );

				list.Add( 1060663, "SpawnID\t{0}", m_SpawnID.ToString() );

//				list.Add( 1060659, "group\t{0}", m_Group );
//				list.Add( 1060660, "team\t{0}", m_Team );
				list.Add( 1060661, "speed\t{0} to {1}", m_MinDelay, m_MaxDelay );

				for ( int i = 0; i < 2 && i < m_CreaturesName.Count; ++i )
					list.Add( 1060662 + i, "{0}\t{1}", m_CreaturesName[i], CountCreatures( m_CreaturesName[i] ) );
			}
			else
			{
				list.Add( 1060743 ); // inactive
			}
		}

		public override void OnSingleClick( Mobile from )
		{
			base.OnSingleClick( from );

			if ( m_Running )
				LabelTo( from, "[Running]" );
			else
				LabelTo( from, "[Off]" );
		}

		public void Start()
		{
			if ( !m_Running )
			{
				if ( m_CreaturesName.Count > 0 || m_SubSpawnerA.Count > 0 || m_SubSpawnerB.Count > 0 || m_SubSpawnerC.Count > 0 || m_SubSpawnerD.Count > 0 || m_SubSpawnerE.Count > 0 )
				{
					m_Running = true;
					DoTimer();
				}
			}
		}

		public void Stop()
		{
			if ( m_Running )
			{
				m_Timer.Stop();
				m_Running = false;
			}
		}

		public static string ParseType( string s )
		{
			return s.Split( null, 2 )[0];
		}

		public void Defrag()
		{
			bool removed = false;

			for ( int i = 0; i < m_Creatures.Count; ++i )
			{
				IEntity e = m_Creatures[i];

				if ( e is Item )
				{
					Item item = (Item)e;

					if ( item.Deleted || item.Parent != null )
					{
						m_Creatures.RemoveAt( i );
						--i;
						removed = true;
					}
				}
				else if ( e is Mobile )
				{
					Mobile m = (Mobile)e;

					if ( m.Deleted )
					{
						m_Creatures.RemoveAt( i );
						--i;
						removed = true;
					}
					else if ( m is BaseCreature )
					{
						BaseCreature bc = (BaseCreature)m;
						if ( bc.Controlled || bc.IsStabled )
						{
							m_Creatures.RemoveAt( i );
							--i;
							removed = true;
						}
					}
				}
				else
				{
					m_Creatures.RemoveAt( i );
					--i;
					removed = true;
				}
			}

			for ( int i = 0; i < m_SubCreaturesA.Count; ++i )
			{
				IEntity e = m_SubCreaturesA[i];

				if ( e is Item )
				{
					Item item = (Item)e;

					if ( item.Deleted || item.Parent != null )
					{
						m_SubCreaturesA.RemoveAt( i );
						--i;
						removed = true;
					}
				}
				else if ( e is Mobile )
				{
					Mobile m = (Mobile)e;

					if ( m.Deleted )
					{
						m_SubCreaturesA.RemoveAt( i );
						--i;
						removed = true;
					}
					else if ( m is BaseCreature )
					{
						BaseCreature bc = (BaseCreature)m;
						if ( bc.Controlled || bc.IsStabled )
						{
							m_SubCreaturesA.RemoveAt( i );
							--i;
							removed = true;
						}
					}
				}
				else
				{
					m_SubCreaturesA.RemoveAt( i );
					--i;
					removed = true;
				}
			}

			for ( int i = 0; i < m_SubCreaturesB.Count; ++i )
			{
				IEntity e = m_SubCreaturesB[i];

				if ( e is Item )
				{
					Item item = (Item)e;

					if ( item.Deleted || item.Parent != null )
					{
						m_SubCreaturesB.RemoveAt( i );
						--i;
						removed = true;
					}
				}
				else if ( e is Mobile )
				{
					Mobile m = (Mobile)e;

					if ( m.Deleted )
					{
						m_SubCreaturesB.RemoveAt( i );
						--i;
						removed = true;
					}
					else if ( m is BaseCreature )
					{
						BaseCreature bc = (BaseCreature)m;
						if ( bc.Controlled || bc.IsStabled )
						{
							m_SubCreaturesB.RemoveAt( i );
							--i;
							removed = true;
						}
					}
				}
				else
				{
					m_SubCreaturesB.RemoveAt( i );
					--i;
					removed = true;
				}
			}

			for ( int i = 0; i < m_SubCreaturesC.Count; ++i )
			{
				IEntity e = m_SubCreaturesC[i];

				if ( e is Item )
				{
					Item item = (Item)e;

					if ( item.Deleted || item.Parent != null )
					{
						m_SubCreaturesC.RemoveAt( i );
						--i;
						removed = true;
					}
				}
				else if ( e is Mobile )
				{
					Mobile m = (Mobile)e;

					if ( m.Deleted )
					{
						m_SubCreaturesC.RemoveAt( i );
						--i;
						removed = true;
					}
					else if ( m is BaseCreature )
					{
						BaseCreature bc = (BaseCreature)m;
						if ( bc.Controlled || bc.IsStabled )
						{
							m_SubCreaturesC.RemoveAt( i );
							--i;
							removed = true;
						}
					}
				}
				else
				{
					m_SubCreaturesC.RemoveAt( i );
					--i;
					removed = true;
				}
			}

			for ( int i = 0; i < m_SubCreaturesD.Count; ++i )
			{
				IEntity e = m_SubCreaturesD[i];

				if ( e is Item )
				{
					Item item = (Item)e;

					if ( item.Deleted || item.Parent != null )
					{
						m_SubCreaturesD.RemoveAt( i );
						--i;
						removed = true;
					}
				}
				else if ( e is Mobile )
				{
					Mobile m = (Mobile)e;

					if ( m.Deleted )
					{
						m_SubCreaturesD.RemoveAt( i );
						--i;
						removed = true;
					}
					else if ( m is BaseCreature )
					{
						BaseCreature bc = (BaseCreature)m;
						if ( bc.Controlled || bc.IsStabled )
						{
							m_SubCreaturesD.RemoveAt( i );
							--i;
							removed = true;
						}
					}
				}
				else
				{
					m_SubCreaturesD.RemoveAt( i );
					--i;
					removed = true;
				}
			}

			for ( int i = 0; i < m_SubCreaturesE.Count; ++i )
			{
				IEntity e = m_SubCreaturesE[i];

				if ( e is Item )
				{
					Item item = (Item)e;

					if ( item.Deleted || item.Parent != null )
					{
						m_SubCreaturesE.RemoveAt( i );
						--i;
						removed = true;
					}
				}
				else if ( e is Mobile )
				{
					Mobile m = (Mobile)e;

					if ( m.Deleted )
					{
						m_SubCreaturesE.RemoveAt( i );
						--i;
						removed = true;
					}
					else if ( m is BaseCreature )
					{
						BaseCreature bc = (BaseCreature)m;
						if ( bc.Controlled || bc.IsStabled )
						{
							m_SubCreaturesE.RemoveAt( i );
							--i;
							removed = true;
						}
					}
				}
				else
				{
					m_SubCreaturesE.RemoveAt( i );
					--i;
					removed = true;
				}
			}

			if ( removed )
				InvalidateProperties();
		}

		public void OnTick()
		{
			DoTimer();

			if ( m_Group )
			{
				Defrag();

				if  ( m_Creatures.Count == 0 || m_SubCreaturesA.Count == 0 || m_SubCreaturesB.Count == 0 || m_SubCreaturesC.Count == 0 || m_SubCreaturesD.Count == 0 || m_SubCreaturesE.Count == 0 )
				{
					Respawn();
				}
				else
				{
					return;
				}
			}
			else
			{
				Spawn();
			}
		}
		
		public void Respawn()
		{
			RemoveCreatures();
			RemoveCreaturesA();
			RemoveCreaturesB();
			RemoveCreaturesC();
			RemoveCreaturesD();
			RemoveCreaturesE();

			for ( int i = 0; i < m_Count; i++ )
				Spawn();

			for ( int i = 0; i < m_SubCountA; i++ )
				SpawnXA();

			for ( int i = 0; i < m_SubCountB; i++ )
				SpawnXB();

			for ( int i = 0; i < m_SubCountC; i++ )
				SpawnXC();

			for ( int i = 0; i < m_SubCountD; i++ )
				SpawnXD();

			for ( int i = 0; i < m_SubCountE; i++ )
				SpawnXE();
		}
		
		public void Spawn()
		{
			if ( CreaturesNameCount > 0 )
				Spawn( Utility.Random( CreaturesNameCount ) );
		}

		public void SpawnXA()
		{
			if ( SubSpawnerACount > 0 )
				SpawnA( Utility.Random( SubSpawnerACount ) );
		}

		public void SpawnXB()
		{
			if ( SubSpawnerBCount > 0 )
				SpawnB( Utility.Random( SubSpawnerBCount ) );
		}

		public void SpawnXC()
		{
			if ( SubSpawnerCCount > 0 )
				SpawnC( Utility.Random( SubSpawnerCCount ) );
		}

		public void SpawnXD()
		{
			if ( SubSpawnerDCount > 0 )
				SpawnD( Utility.Random( SubSpawnerDCount ) );
		}

		public void SpawnXE()
		{
			if ( SubSpawnerECount > 0 )
				SpawnE( Utility.Random( SubSpawnerECount ) );
		}
		
		public void Spawn( string creatureName )
		{
			for ( int i = 0; i < m_CreaturesName.Count; i++ )
			{
				if ( m_CreaturesName[i] == creatureName )
				{
					Spawn( i );
					break;
				}
			}
		}

		public void SpawnA( string subSpawnerA )
		{
			for ( int i = 0; i < m_SubSpawnerA.Count; i++ )
			{
				if ( (string)m_SubSpawnerA[i] == subSpawnerA )
				{
					SpawnA( i );
					break;
				}
			}
		}

		public void SpawnB( string subSpawnerB )
		{
			for ( int i = 0; i < m_SubSpawnerB.Count; i++ )
			{
				if ( (string)m_SubSpawnerB[i] == subSpawnerB )
				{
					SpawnB( i );
					break;
				}
			}
		}

		public void SpawnC( string subSpawnerC )
		{
			for ( int i = 0; i < m_SubSpawnerC.Count; i++ )
			{
				if ( (string)m_SubSpawnerC[i] == subSpawnerC )
				{
					SpawnC( i );
					break;
				}
			}
		}

		public void SpawnD( string subSpawnerD )
		{
			for ( int i = 0; i < m_SubSpawnerD.Count; i++ )
			{
				if ( (string)m_SubSpawnerD[i] == subSpawnerD )
				{
					SpawnD( i );
					break;
				}
			}
		}

		public void SpawnE( string subSpawnerE )
		{
			for ( int i = 0; i < m_SubSpawnerE.Count; i++ )
			{
				if ( (string)m_SubSpawnerE[i] == subSpawnerE )
				{
					SpawnE( i );
					break;
				}
			}
		}

		protected virtual IEntity CreateSpawnedObject( int index )
		{
			if ( index >= m_CreaturesName.Count )
				return null;

			Type type = ScriptCompiler.FindTypeByName( ParseType( m_CreaturesName[index] ) );

			if ( type != null )
			{
				try
				{
					return Build( CommandSystem.Split( m_CreaturesName[index] ) );
				}
				catch
				{
				}
			}

			return null;
		}

		protected virtual IEntity CreateSpawnedObjectA( int index )
		{
			if ( index >= m_SubSpawnerA.Count )
				return null;

			Type type = ScriptCompiler.FindTypeByName( ParseType( m_SubSpawnerA[index] ) );

			if ( type != null )
			{
				try
				{
					return Build( CommandSystem.Split( m_SubSpawnerA[index] ) );
				}
				catch
				{
				}
			}

			return null;
		}

		protected virtual IEntity CreateSpawnedObjectB( int index )
		{
			if ( index >= m_SubSpawnerB.Count )
				return null;

			Type type = ScriptCompiler.FindTypeByName( ParseType( m_SubSpawnerB[index] ) );

			if ( type != null )
			{
				try
				{
					return Build( CommandSystem.Split( m_SubSpawnerB[index] ) );
				}
				catch
				{
				}
			}

			return null;
		}

		protected virtual IEntity CreateSpawnedObjectC( int index )
		{
			if ( index >= m_SubSpawnerC.Count )
				return null;

			Type type = ScriptCompiler.FindTypeByName( ParseType( m_SubSpawnerC[index] ) );

			if ( type != null )
			{
				try
				{
					return Build( CommandSystem.Split( m_SubSpawnerC[index] ) );
				}
				catch
				{
				}
			}

			return null;
		}

		protected virtual IEntity CreateSpawnedObjectD( int index )
		{
			if ( index >= m_SubSpawnerD.Count )
				return null;

			Type type = ScriptCompiler.FindTypeByName( ParseType( m_SubSpawnerD[index] ) );

			if ( type != null )
			{
				try
				{
					return Build( CommandSystem.Split( m_SubSpawnerD[index] ) );
				}
				catch
				{
				}
			}

			return null;
		}

		protected virtual IEntity CreateSpawnedObjectE( int index )
		{
			if ( index >= m_SubSpawnerE.Count )
				return null;

			Type type = ScriptCompiler.FindTypeByName( ParseType( m_SubSpawnerE[index] ) );

			if ( type != null )
			{
				try
				{
					return Build( CommandSystem.Split( m_SubSpawnerE[index] ) );
				}
				catch
				{
				}
			}

			return null;
		}

		public static IEntity Build( string[] args )
		{
			string name = args[0];

			Add.FixArgs( ref args );

			string[,] props = null;

			for ( int i = 0; i < args.Length; ++i )
			{
				if ( Insensitive.Equals( args[i], "set" ) )
				{
					int remains = args.Length - i - 1;

					if ( remains >= 2 )
					{
						props = new string[remains / 2, 2];

						remains /= 2;

						for ( int j = 0; j < remains; ++j )
						{
							props[j, 0] = args[i + (j * 2) + 1];
							props[j, 1] = args[i + (j * 2) + 2];
						}

						Add.FixSetString( ref args, i );
					}

					break;
				}
			}

			Type type = ScriptCompiler.FindTypeByName( name );

			if ( !Add.IsEntity( type ) ) {
				return null;
			}

			PropertyInfo[] realProps = null;

			if ( props != null )
			{
				realProps = new PropertyInfo[props.GetLength( 0 )];

				PropertyInfo[] allProps = type.GetProperties( BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public );

				for ( int i = 0; i < realProps.Length; ++i )
				{
					PropertyInfo thisProp = null;

					string propName = props[i, 0];

					for ( int j = 0; thisProp == null && j < allProps.Length; ++j )
					{
						if ( Insensitive.Equals( propName, allProps[j].Name ) )
							thisProp = allProps[j];
					}

					if ( thisProp != null )
					{
						CPA attr = Properties.GetCPA( thisProp );

						if ( attr != null && AccessLevel.GameMaster >= attr.WriteLevel && thisProp.CanWrite && !attr.ReadOnly )
							realProps[i] = thisProp;
					}
				}
			}

			ConstructorInfo[] ctors = type.GetConstructors();

			for ( int i = 0; i < ctors.Length; ++i )
			{
				ConstructorInfo ctor = ctors[i];

				if ( !Add.IsConstructable( ctor, AccessLevel.GameMaster ) )
					continue;

				ParameterInfo[] paramList = ctor.GetParameters();

				if ( args.Length == paramList.Length )
				{
					object[] paramValues = Add.ParseValues( paramList, args );

					if ( paramValues == null )
						continue;

					object built = ctor.Invoke( paramValues );

					if ( built != null && realProps != null )
					{
						for ( int j = 0; j < realProps.Length; ++j )
						{
							if ( realProps[j] == null )
								continue;

							string result = Properties.InternalSetValue( built, realProps[j], props[j, 1] );
						}
					}

					return (IEntity)built;
				}
			}

			return null;
		}

		public void Spawn( int index )
		{
			Map map = Map;

			if ( map == null || map == Map.Internal || CreaturesNameCount == 0 || index >= CreaturesNameCount || Parent != null )
				return;

			Defrag();

			if ( m_Creatures.Count >= m_Count )
				return;


			IEntity ent = CreateSpawnedObject( index );

			if ( ent is Mobile )
			{
				Mobile m = (Mobile)ent;

				m_Creatures.Add( m );
				

				Point3D loc = ( m is BaseVendor ? this.Location : GetSpawnPosition() );

				m.OnBeforeSpawn( loc, map );
				InvalidateProperties();


				m.MoveToWorld( loc, map );

				if ( m is BaseCreature )
				{
					BaseCreature c = (BaseCreature)m;
					
					if( m_WalkingRange >= 0 )
						c.RangeHome = m_WalkingRange;
					else
						c.RangeHome = m_HomeRange;

					c.CurrentWayPoint = m_WayPoint;

					if ( m_Team > 0 )
						c.Team = m_Team;

					c.Home = this.Location;
				}

				m.OnAfterSpawn();
			}
			else if ( ent is Item )
			{
				Item item = (Item)ent;

				m_Creatures.Add( item );

				Point3D loc = GetSpawnPosition();

				item.OnBeforeSpawn( loc, map );
				InvalidateProperties();

				item.MoveToWorld( loc, map );

				item.OnAfterSpawn();
			}
		}

		public void SpawnA( int index )
		{
			Map map = Map;

			if ( map == null || map == Map.Internal || SubSpawnerACount == 0 || index >= SubSpawnerACount || Parent != null )
				return;

			Defrag();

			if ( m_SubSpawnerA.Count >= m_SubCountA )
				return;


			IEntity ent = CreateSpawnedObjectA( index );

			if ( ent is Mobile )
			{
				Mobile m = (Mobile)ent;

				m_SubCreaturesA.Add( m );
				

				Point3D loc = ( m is BaseVendor ? this.Location : GetSpawnPosition() );

				m.OnBeforeSpawn( loc, map );
				InvalidateProperties();


				m.MoveToWorld( loc, map );

				if ( m is BaseCreature )
				{
					BaseCreature c = (BaseCreature)m;
					
					if( m_WalkingRange >= 0 )
						c.RangeHome = m_WalkingRange;
					else
						c.RangeHome = m_HomeRange;

					c.CurrentWayPoint = m_WayPoint;

					if ( m_Team > 0 )
						c.Team = m_Team;

					c.Home = this.Location;
				}

				m.OnAfterSpawn();
			}
			else if ( ent is Item )
			{
				Item item = (Item)ent;

				m_SubCreaturesA.Add( item );

				Point3D loc = GetSpawnPosition();

				item.OnBeforeSpawn( loc, map );
				InvalidateProperties();

				item.MoveToWorld( loc, map );

				item.OnAfterSpawn();
			}
		}

		public void SpawnB( int index )
		{
			Map map = Map;

			if ( map == null || map == Map.Internal || SubSpawnerBCount == 0 || index >= SubSpawnerBCount || Parent != null )
				return;

			Defrag();

			if ( m_SubSpawnerB.Count >= m_SubCountB )
				return;


			IEntity ent = CreateSpawnedObjectB( index );

			if ( ent is Mobile )
			{
				Mobile m = (Mobile)ent;

				m_SubCreaturesB.Add( m );
				

				Point3D loc = ( m is BaseVendor ? this.Location : GetSpawnPosition() );

				m.OnBeforeSpawn( loc, map );
				InvalidateProperties();


				m.MoveToWorld( loc, map );

				if ( m is BaseCreature )
				{
					BaseCreature c = (BaseCreature)m;
					
					if( m_WalkingRange >= 0 )
						c.RangeHome = m_WalkingRange;
					else
						c.RangeHome = m_HomeRange;

					c.CurrentWayPoint = m_WayPoint;

					if ( m_Team > 0 )
						c.Team = m_Team;

					c.Home = this.Location;
				}

				m.OnAfterSpawn();
			}
			else if ( ent is Item )
			{
				Item item = (Item)ent;

				m_SubCreaturesB.Add( item );

				Point3D loc = GetSpawnPosition();

				item.OnBeforeSpawn( loc, map );
				InvalidateProperties();

				item.MoveToWorld( loc, map );

				item.OnAfterSpawn();
			}
		}

		public void SpawnC( int index )
		{
			Map map = Map;

			if ( map == null || map == Map.Internal || SubSpawnerCCount == 0 || index >= SubSpawnerCCount || Parent != null )
				return;

			Defrag();

			if ( m_SubSpawnerC.Count >= m_SubCountC )
				return;


			IEntity ent = CreateSpawnedObjectC( index );

			if ( ent is Mobile )
			{
				Mobile m = (Mobile)ent;

				m_SubCreaturesC.Add( m );
				

				Point3D loc = ( m is BaseVendor ? this.Location : GetSpawnPosition() );

				m.OnBeforeSpawn( loc, map );
				InvalidateProperties();


				m.MoveToWorld( loc, map );

				if ( m is BaseCreature )
				{
					BaseCreature c = (BaseCreature)m;
					
					if( m_WalkingRange >= 0 )
						c.RangeHome = m_WalkingRange;
					else
						c.RangeHome = m_HomeRange;

					c.CurrentWayPoint = m_WayPoint;

					if ( m_Team > 0 )
						c.Team = m_Team;

					c.Home = this.Location;
				}

				m.OnAfterSpawn();
			}
			else if ( ent is Item )
			{
				Item item = (Item)ent;

				m_SubCreaturesC.Add( item );

				Point3D loc = GetSpawnPosition();

				item.OnBeforeSpawn( loc, map );
				InvalidateProperties();

				item.MoveToWorld( loc, map );

				item.OnAfterSpawn();
			}
		}

		public void SpawnD( int index )
		{
			Map map = Map;

			if ( map == null || map == Map.Internal || SubSpawnerDCount == 0 || index >= SubSpawnerDCount || Parent != null )
				return;

			Defrag();

			if ( m_SubSpawnerD.Count >= m_SubCountD )
				return;


			IEntity ent = CreateSpawnedObjectD( index );

			if ( ent is Mobile )
			{
				Mobile m = (Mobile)ent;

				m_SubCreaturesD.Add( m );
				

				Point3D loc = ( m is BaseVendor ? this.Location : GetSpawnPosition() );

				m.OnBeforeSpawn( loc, map );
				InvalidateProperties();


				m.MoveToWorld( loc, map );

				if ( m is BaseCreature )
				{
					BaseCreature c = (BaseCreature)m;
					
					if( m_WalkingRange >= 0 )
						c.RangeHome = m_WalkingRange;
					else
						c.RangeHome = m_HomeRange;

					c.CurrentWayPoint = m_WayPoint;

					if ( m_Team > 0 )
						c.Team = m_Team;

					c.Home = this.Location;
				}

				m.OnAfterSpawn();
			}
			else if ( ent is Item )
			{
				Item item = (Item)ent;

				m_SubCreaturesD.Add( item );

				Point3D loc = GetSpawnPosition();

				item.OnBeforeSpawn( loc, map );
				InvalidateProperties();

				item.MoveToWorld( loc, map );

				item.OnAfterSpawn();
			}
		}

		public void SpawnE( int index )
		{
			Map map = Map;

			if ( map == null || map == Map.Internal || SubSpawnerECount == 0 || index >= SubSpawnerECount || Parent != null )
				return;

			Defrag();

			if ( m_SubSpawnerE.Count >= m_SubCountE )
				return;


			IEntity ent = CreateSpawnedObjectE( index );

			if ( ent is Mobile )
			{
				Mobile m = (Mobile)ent;

				m_SubCreaturesE.Add( m );
				

				Point3D loc = ( m is BaseVendor ? this.Location : GetSpawnPosition() );

				m.OnBeforeSpawn( loc, map );
				InvalidateProperties();


				m.MoveToWorld( loc, map );

				if ( m is BaseCreature )
				{
					BaseCreature c = (BaseCreature)m;
					
					if( m_WalkingRange >= 0 )
						c.RangeHome = m_WalkingRange;
					else
						c.RangeHome = m_HomeRange;

					c.CurrentWayPoint = m_WayPoint;

					if ( m_Team > 0 )
						c.Team = m_Team;

					c.Home = this.Location;
				}

				m.OnAfterSpawn();
			}
			else if ( ent is Item )
			{
				Item item = (Item)ent;

				m_SubCreaturesE.Add( item );

				Point3D loc = GetSpawnPosition();

				item.OnBeforeSpawn( loc, map );
				InvalidateProperties();

				item.MoveToWorld( loc, map );

				item.OnAfterSpawn();
			}
		}

		public Point3D GetSpawnPosition()
		{
			Map map = Map;

			if ( map == null )
				return Location;

			// Try 10 times to find a Spawnable location.
			for ( int i = 0; i < 10; i++ )
			{
				int x, y;

				if ( m_HomeRange > 0 ) {
					x = Location.X + (Utility.Random( (m_HomeRange * 2) + 1 ) - m_HomeRange);
					y = Location.Y + (Utility.Random( (m_HomeRange * 2) + 1 ) - m_HomeRange);
				} else {
					x = Location.X;
					y = Location.Y;
				}

				int z = Map.GetAverageZ( x, y );

				if ( Map.CanSpawnMobile( new Point2D( x, y ), this.Z ) )
					return new Point3D( x, y, this.Z );
				else if ( Map.CanSpawnMobile( new Point2D( x, y ), z ) )
					return new Point3D( x, y, z );
			}

			return this.Location;
		}

		public void DoTimer()
		{
			if ( !m_Running )
				return;

			int minSeconds = (int)m_MinDelay.TotalSeconds;
			int maxSeconds = (int)m_MaxDelay.TotalSeconds;

			TimeSpan delay = TimeSpan.FromSeconds( Utility.RandomMinMax( minSeconds, maxSeconds ) );
			DoTimer( delay );
		}

		public void DoTimer( TimeSpan delay )
		{
			if ( !m_Running )
				return;

			m_End = DateTime.Now + delay;

			if ( m_Timer != null )
				m_Timer.Stop();

			m_Timer = new InternalTimer( this, delay );
			m_Timer.Start();
		}

		private class InternalTimer : Timer
		{
			private PremiumSpawner m_PremiumSpawner;

			public InternalTimer( PremiumSpawner spawner, TimeSpan delay ) : base( delay )
			{
				if ( spawner.IsFull || spawner.IsFulla || spawner.IsFullb || spawner.IsFullc || spawner.IsFulld || spawner.IsFulle )
					Priority = TimerPriority.FiveSeconds;
				else
					Priority = TimerPriority.OneSecond;

				m_PremiumSpawner = spawner;
			}

			protected override void OnTick()
			{
				if ( m_PremiumSpawner != null )
					if ( !m_PremiumSpawner.Deleted )
						m_PremiumSpawner.OnTick();
			}
		}

		public int CountCreatures( string creatureName )
		{
			Defrag();

			int count = 0;

			for ( int i = 0; i < m_Creatures.Count; ++i )
				if ( Insensitive.Equals( creatureName, m_Creatures[i].GetType().Name ) )
					++count;

			return count;
		}

		public int CountCreaturesA( string subSpawnerA )
		{
			Defrag();

			int count = 0;

			for ( int i = 0; i < m_SubCreaturesA.Count; ++i )
				if ( Insensitive.Equals( subSpawnerA, m_SubCreaturesA[i].GetType().Name ) )
					++count;

			return count;
		}

		public int CountCreaturesB( string subSpawnerB )
		{
			Defrag();

			int count = 0;

			for ( int i = 0; i < m_SubCreaturesB.Count; ++i )
				if ( Insensitive.Equals( subSpawnerB, m_SubCreaturesB[i].GetType().Name ) )
					++count;

			return count;
		}

		public int CountCreaturesC( string subSpawnerC )
		{
			Defrag();

			int count = 0;

			for ( int i = 0; i < m_SubCreaturesC.Count; ++i )
				if ( Insensitive.Equals( subSpawnerC, m_SubCreaturesC[i].GetType().Name ) )
					++count;

			return count;
		}

		public int CountCreaturesD( string subSpawnerD )
		{
			Defrag();

			int count = 0;

			for ( int i = 0; i < m_SubCreaturesD.Count; ++i )
				if ( Insensitive.Equals( subSpawnerD, m_SubCreaturesD[i].GetType().Name ) )
					++count;

			return count;
		}

		public int CountCreaturesE( string subSpawnerE )
		{
			Defrag();

			int count = 0;

			for ( int i = 0; i < m_SubCreaturesE.Count; ++i )
				if ( Insensitive.Equals( subSpawnerE, m_SubCreaturesE[i].GetType().Name ) )
					++count;

			return count;
		}

		public void RemoveCreatures( string creatureName )
		{
			Defrag();

			for ( int i = 0; i < m_Creatures.Count; ++i )
			{
				IEntity e = m_Creatures[i];

				if ( Insensitive.Equals( creatureName, e.GetType().Name ) )
						e.Delete();
			}

			InvalidateProperties();
		}

		public void RemoveCreaturesA( string subSpawnerA )
		{
			Defrag();

			for ( int i = 0; i < m_SubCreaturesA.Count; ++i )
			{
				IEntity e = m_SubCreaturesA[i];

				if ( Insensitive.Equals( subSpawnerA, e.GetType().Name ) )
						e.Delete();
			}

			InvalidateProperties();
		}

		public void RemoveCreaturesB( string subSpawnerB )
		{
			Defrag();

			for ( int i = 0; i < m_SubCreaturesB.Count; ++i )
			{
				IEntity e = m_SubCreaturesB[i];

				if ( Insensitive.Equals( subSpawnerB, e.GetType().Name ) )
						e.Delete();
			}

			InvalidateProperties();
		}

		public void RemoveCreaturesC( string subSpawnerC )
		{
			Defrag();

			for ( int i = 0; i < m_SubCreaturesC.Count; ++i )
			{
				IEntity e = m_SubCreaturesC[i];

				if ( Insensitive.Equals( subSpawnerC, e.GetType().Name ) )
						e.Delete();
			}

			InvalidateProperties();
		}

		public void RemoveCreaturesD( string subSpawnerD )
		{
			Defrag();

			for ( int i = 0; i < m_SubCreaturesD.Count; ++i )
			{
				IEntity e = m_SubCreaturesD[i];

				if ( Insensitive.Equals( subSpawnerD, e.GetType().Name ) )
						e.Delete();
			}

			InvalidateProperties();
		}

		public void RemoveCreaturesE( string subSpawnerE )
		{
			Defrag();

			for ( int i = 0; i < m_SubCreaturesE.Count; ++i )
			{
				IEntity e = m_SubCreaturesE[i];

				if ( Insensitive.Equals( subSpawnerE, e.GetType().Name ) )
						e.Delete();
			}

			InvalidateProperties();
		}
		
		public void RemoveCreatures()
		{
			Defrag();

			for ( int i = 0; i < m_Creatures.Count; ++i )
				m_Creatures[i].Delete();

			InvalidateProperties();
		}

		public void RemoveCreaturesA()
		{
			Defrag();

			for ( int i = 0; i < m_SubCreaturesA.Count; ++i )
				m_SubCreaturesA[i].Delete();

			InvalidateProperties();
		}

		public void RemoveCreaturesB()
		{
			Defrag();

			for ( int i = 0; i < m_SubCreaturesB.Count; ++i )
				m_SubCreaturesB[i].Delete();

			InvalidateProperties();
		}

		public void RemoveCreaturesC()
		{
			Defrag();

			for ( int i = 0; i < m_SubCreaturesC.Count; ++i )
				m_SubCreaturesC[i].Delete();

			InvalidateProperties();
		}

		public void RemoveCreaturesD()
		{
			Defrag();

			for ( int i = 0; i < m_SubCreaturesD.Count; ++i )
				m_SubCreaturesD[i].Delete();

			InvalidateProperties();
		}

		public void RemoveCreaturesE()
		{
			Defrag();

			for ( int i = 0; i < m_SubCreaturesE.Count; ++i )
				m_SubCreaturesE[i].Delete();

			InvalidateProperties();
		}

		public void BringToHome()
		{
			Defrag();

			for ( int i = 0; i < m_Creatures.Count; ++i )
			{
				IEntity e = m_Creatures[i];

				if ( e is Mobile )
				{
					Mobile m = (Mobile)e;

					m.MoveToWorld( Location, Map );
				}
				else if ( e is Item )
				{
					Item item = (Item)e;

					item.MoveToWorld( Location, Map );
				}
			}

			for ( int i = 0; i < m_SubCreaturesA.Count; ++i )
			{
				object o = m_SubCreaturesA[i];

				if ( o is Mobile )
				{
					Mobile m = (Mobile)o;

					m.MoveToWorld( Location, Map );
				}
				else if ( o is Item )
				{
					Item item = (Item)o;

					item.MoveToWorld( Location, Map );
				}
			}

			for ( int i = 0; i < m_SubCreaturesB.Count; ++i )
			{
				object o = m_SubCreaturesB[i];

				if ( o is Mobile )
				{
					Mobile m = (Mobile)o;

					m.MoveToWorld( Location, Map );
				}
				else if ( o is Item )
				{
					Item item = (Item)o;

					item.MoveToWorld( Location, Map );
				}
			}

			for ( int i = 0; i < m_SubCreaturesC.Count; ++i )
			{
				object o = m_SubCreaturesC[i];

				if ( o is Mobile )
				{
					Mobile m = (Mobile)o;

					m.MoveToWorld( Location, Map );
				}
				else if ( o is Item )
				{
					Item item = (Item)o;

					item.MoveToWorld( Location, Map );
				}
			}

			for ( int i = 0; i < m_SubCreaturesD.Count; ++i )
			{
				object o = m_SubCreaturesD[i];

				if ( o is Mobile )
				{
					Mobile m = (Mobile)o;

					m.MoveToWorld( Location, Map );
				}
				else if ( o is Item )
				{
					Item item = (Item)o;

					item.MoveToWorld( Location, Map );
				}
			}

			for ( int i = 0; i < m_SubCreaturesE.Count; ++i )
			{
				object o = m_SubCreaturesE[i];

				if ( o is Mobile )
				{
					Mobile m = (Mobile)o;

					m.MoveToWorld( Location, Map );
				}
				else if ( o is Item )
				{
					Item item = (Item)o;

					item.MoveToWorld( Location, Map );
				}
			}

		}

		public override void OnDelete()
		{
			base.OnDelete();

			RemoveCreatures();
			RemoveCreaturesA();
			RemoveCreaturesB();
			RemoveCreaturesC();
			RemoveCreaturesD();
			RemoveCreaturesE();
			if ( m_Timer != null )
				m_Timer.Stop();
		}
		
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 4 ); // version
			writer.Write( m_WalkingRange );

			writer.Write( m_SpawnID );
			writer.Write( m_SubCountA );
			writer.Write( m_SubCountB );
			writer.Write( m_SubCountC );
			writer.Write( m_SubCountD );
			writer.Write( m_SubCountE );

			writer.Write( m_WayPoint );

			writer.Write( m_Group );

			writer.Write( m_MinDelay );
			writer.Write( m_MaxDelay );
			writer.Write( m_Count );
			writer.Write( m_Team );
			writer.Write( m_HomeRange );
			writer.Write( m_Running );
			
			if ( m_Running )
				writer.WriteDeltaTime( m_End );

			writer.Write( m_CreaturesName.Count );

			for ( int i = 0; i < m_CreaturesName.Count; ++i )
				writer.Write( m_CreaturesName[i] );

			writer.Write( m_SubSpawnerA.Count );

			for ( int i = 0; i < m_SubSpawnerA.Count; ++i )
				writer.Write( (string)m_SubSpawnerA[i] );

			writer.Write( m_SubSpawnerB.Count );

			for ( int i = 0; i < m_SubSpawnerB.Count; ++i )
				writer.Write( (string)m_SubSpawnerB[i] );

			writer.Write( m_SubSpawnerC.Count );

			for ( int i = 0; i < m_SubSpawnerC.Count; ++i )
				writer.Write( (string)m_SubSpawnerC[i] );

			writer.Write( m_SubSpawnerD.Count );

			for ( int i = 0; i < m_SubSpawnerD.Count; ++i )
				writer.Write( (string)m_SubSpawnerD[i] );

			writer.Write( m_SubSpawnerE.Count );

			for ( int i = 0; i < m_SubSpawnerE.Count; ++i )
				writer.Write( (string)m_SubSpawnerE[i] );

			writer.Write( m_Creatures.Count );

			for ( int i = 0; i < m_Creatures.Count; ++i )
			{
				IEntity e = m_Creatures[i];

				if ( e is Item )
					writer.Write( (Item)e );
				else if ( e is Mobile )
					writer.Write( (Mobile)e );
				else
					writer.Write( Serial.MinusOne );
			}

			writer.Write( m_SubCreaturesA.Count );

			for ( int i = 0; i < m_SubCreaturesA.Count; ++i )
			{
				IEntity e = m_SubCreaturesA[i];

				if ( e is Item )
					writer.Write( (Item)e );
				else if ( e is Mobile )
					writer.Write( (Mobile)e );
				else
					writer.Write( Serial.MinusOne );
			}

			writer.Write( m_SubCreaturesB.Count );

			for ( int i = 0; i < m_SubCreaturesB.Count; ++i )
			{
				IEntity e = m_SubCreaturesB[i];

				if ( e is Item )
					writer.Write( (Item)e );
				else if ( e is Mobile )
					writer.Write( (Mobile)e );
				else
					writer.Write( Serial.MinusOne );
			}

			writer.Write( m_SubCreaturesC.Count );

			for ( int i = 0; i < m_SubCreaturesC.Count; ++i )
			{
				IEntity e = m_SubCreaturesC[i];

				if ( e is Item )
					writer.Write( (Item)e );
				else if ( e is Mobile )
					writer.Write( (Mobile)e );
				else
					writer.Write( Serial.MinusOne );
			}

			writer.Write( m_SubCreaturesD.Count );

			for ( int i = 0; i < m_SubCreaturesD.Count; ++i )
			{
				IEntity e = m_SubCreaturesD[i];

				if ( e is Item )
					writer.Write( (Item)e );
				else if ( e is Mobile )
					writer.Write( (Mobile)e );
				else
					writer.Write( Serial.MinusOne );
			}

			writer.Write( m_SubCreaturesE.Count );

			for ( int i = 0; i < m_SubCreaturesE.Count; ++i )
			{
				IEntity e = m_SubCreaturesE[i];

				if ( e is Item )
					writer.Write( (Item)e );
				else if ( e is Mobile )
					writer.Write( (Mobile)e );
				else
					writer.Write( Serial.MinusOne );
			}

		}

		private static WarnTimer m_WarnTimer;

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 4:
				{
					m_WalkingRange = reader.ReadInt();

					goto case 3;
				}
				case 3:
				case 2:
				{
					m_WayPoint = reader.ReadItem() as WayPoint;

					goto case 1;
				}

				case 1:
				{
					m_Group = reader.ReadBool();
					
					goto case 0;
				}

				case 0:
				{
					m_SpawnID = reader.ReadInt();
					m_SubCountA = reader.ReadInt();
					m_SubCountB = reader.ReadInt();
					m_SubCountC = reader.ReadInt();
					m_SubCountD = reader.ReadInt();
					m_SubCountE = reader.ReadInt();

					m_MinDelay = reader.ReadTimeSpan();
					m_MaxDelay = reader.ReadTimeSpan();
					m_Count = reader.ReadInt();
					m_Team = reader.ReadInt();
					m_HomeRange = reader.ReadInt();
					m_Running = reader.ReadBool();

					TimeSpan ts = TimeSpan.Zero;

					if ( m_Running )
						ts = reader.ReadDeltaTime() - DateTime.Now;
					
					int size = reader.ReadInt();
					m_CreaturesName = new List<string>( size );
					for ( int i = 0; i < size; ++i )
					{
						string creatureString = reader.ReadString();

						m_CreaturesName.Add( creatureString );
						string typeName = ParseType( creatureString );

						if ( ScriptCompiler.FindTypeByName( typeName ) == null )
						{
							if ( m_WarnTimer == null )
								m_WarnTimer = new WarnTimer();

							m_WarnTimer.Add( Location, Map, typeName );
						}
					}

					int sizeA = reader.ReadInt();
					m_SubSpawnerA = new List<string>( sizeA );
					for ( int i = 0; i < sizeA; ++i )
					{
						string creatureString = reader.ReadString();

						m_SubSpawnerA.Add( creatureString );
						string typeName = ParseType( creatureString );

						if ( ScriptCompiler.FindTypeByName( typeName ) == null )
						{
							if ( m_WarnTimer == null )
								m_WarnTimer = new WarnTimer();

							m_WarnTimer.Add( Location, Map, typeName );
						}
					}

					int sizeB = reader.ReadInt();
					m_SubSpawnerB = new List<string>( sizeB );
					for ( int i = 0; i < sizeB; ++i )
					{
						string creatureString = reader.ReadString();

						m_SubSpawnerB.Add( creatureString );
						string typeName = ParseType( creatureString );

						if ( ScriptCompiler.FindTypeByName( typeName ) == null )
						{
							if ( m_WarnTimer == null )
								m_WarnTimer = new WarnTimer();

							m_WarnTimer.Add( Location, Map, typeName );
						}
					}

					int sizeC = reader.ReadInt();
					m_SubSpawnerC = new List<string>( sizeC );
					for ( int i = 0; i < sizeC; ++i )
					{
						string creatureString = reader.ReadString();

						m_SubSpawnerC.Add( creatureString );
						string typeName = ParseType( creatureString );

						if ( ScriptCompiler.FindTypeByName( typeName ) == null )
						{
							if ( m_WarnTimer == null )
								m_WarnTimer = new WarnTimer();

							m_WarnTimer.Add( Location, Map, typeName );
						}
					}

					int sizeD = reader.ReadInt();
					m_SubSpawnerD = new List<string>( sizeD );
					for ( int i = 0; i < sizeD; ++i )
					{
						string creatureString = reader.ReadString();

						m_SubSpawnerD.Add( creatureString );
						string typeName = ParseType( creatureString );

						if ( ScriptCompiler.FindTypeByName( typeName ) == null )
						{
							if ( m_WarnTimer == null )
								m_WarnTimer = new WarnTimer();

							m_WarnTimer.Add( Location, Map, typeName );
						}
					}

					int sizeE = reader.ReadInt();
					m_SubSpawnerE = new List<string>( sizeE );
					for ( int i = 0; i < sizeE; ++i )
					{
						string creatureString = reader.ReadString();

						m_SubSpawnerE.Add( creatureString );
						string typeName = ParseType( creatureString );

						if ( ScriptCompiler.FindTypeByName( typeName ) == null )
						{
							if ( m_WarnTimer == null )
								m_WarnTimer = new WarnTimer();

							m_WarnTimer.Add( Location, Map, typeName );
						}
					}

					int count = reader.ReadInt();
					m_Creatures = new List<IEntity>( count );
					for ( int i = 0; i < count; ++i )
					{
						IEntity e = World.FindEntity( reader.ReadInt() );

						if ( e != null )
							m_Creatures.Add( e );
					}

					int countA = reader.ReadInt();
					m_SubCreaturesA = new List<IEntity>( countA );
					for ( int i = 0; i < countA; ++i )
					{
						IEntity e = World.FindEntity( reader.ReadInt() );

						if ( e != null )
							m_SubCreaturesA.Add( e );
					}

					int countB = reader.ReadInt();
					m_SubCreaturesB = new List<IEntity>( countB );
					for ( int i = 0; i < countB; ++i )
					{
						IEntity e = World.FindEntity( reader.ReadInt() );

						if ( e != null )
							m_SubCreaturesB.Add( e );
					}

					int countC = reader.ReadInt();
					m_SubCreaturesC = new List<IEntity>( countC );
					for ( int i = 0; i < countC; ++i )
					{
						IEntity e = World.FindEntity( reader.ReadInt() );

						if ( e != null )
							m_SubCreaturesC.Add( e );
					}

					int countD = reader.ReadInt();
					m_SubCreaturesD = new List<IEntity>( countD );
					for ( int i = 0; i < countD; ++i )
					{
						IEntity e = World.FindEntity( reader.ReadInt() );

						if ( e != null )
							m_SubCreaturesD.Add( e );
					}

					int countE = reader.ReadInt();
					m_SubCreaturesE = new List<IEntity>( countE );
					for ( int i = 0; i < countE; ++i )
					{
						IEntity e = World.FindEntity( reader.ReadInt() );

						if ( e != null )
							m_SubCreaturesE.Add( e );
					}

					if ( m_Running )
						DoTimer( ts );

					break;
				}
			}

			if ( version < 3 && Weight == 0 )
				Weight = -1;
		}

		private class WarnTimer : Timer
		{
			private List<WarnEntry> m_List;

			private class WarnEntry
			{
				public Point3D m_Point;
				public Map m_Map;
				public string m_Name;

				public WarnEntry( Point3D p, Map map, string name )
				{
					m_Point = p;
					m_Map = map;
					m_Name = name;
				}
			}

			public WarnTimer() : base( TimeSpan.FromSeconds( 1.0 ) )
			{
				m_List = new List<WarnEntry>();
				Start();
			}

			public void Add( Point3D p, Map map, string name )
			{
				m_List.Add( new WarnEntry( p, map, name ) );
			}

			protected override void OnTick()
			{
				try
				{
					Console.WriteLine( "Warning: {0} bad spawns detected, logged: 'PremiumBadspawn.log'", m_List.Count );

					using ( StreamWriter op = new StreamWriter( "PremiumBbadspawn.log", true ) )
					{
						op.WriteLine( "# Bad spawns : {0}", DateTime.Now );
						op.WriteLine( "# Format: X Y Z F Name" );
						op.WriteLine();

						foreach ( WarnEntry e in m_List )
							op.WriteLine( "{0}\t{1}\t{2}\t{3}\t{4}", e.m_Point.X, e.m_Point.Y, e.m_Point.Z, e.m_Map, e.m_Name );

						op.WriteLine();
						op.WriteLine();
					}
				}
				catch
				{
				}
			}
		}
	}
}