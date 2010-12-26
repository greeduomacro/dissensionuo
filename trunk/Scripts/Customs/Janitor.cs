/*Scripted by  _____         
*	  		   \_   \___ ___ 
*			    / /\/ __/ _ \
*		     /\/ /_| (_|  __/
*			 \____/ \___\___|
*/
using System;
using Server.Items;
using Server.Multis;
using Server.Mobiles;
using Server.Network;
using System.Collections;
using Server.ContextMenus;
using System.Collections.Generic;

namespace Server.Mobiles 
{ 
   	[CorpseName( "a person's corpse" )] 
   	public class Janitor : BaseCreature 
   	{ 
		private static bool m_Spoak;
		string[] JanitorSpeech = new string[]
		{
	        "How many times have you been murdered today?  I bet those healers are wore out!",
	        "Did you vote today? I don't want to keep this place clean for nothing.",
            "Silly bank sitters!  This isn't Trammel!",
            "Why aren't you out killing people or monsters?"
            
		};
        private JanitorChest m_JanitorChest;
		private DateTime m_NextPickup; 
        [CommandProperty(AccessLevel.GameMaster)]
        public JanitorChest janitorchest { get { return m_JanitorChest; } set { m_JanitorChest = value; } } 
      	[Constructable] 
      	public Janitor() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 ) 
      	{ 
	      	if ( m_Timer == null )
			{
				m_Timer = new CreateTimer( this );
				m_Timer.Start();
			}
			m_Spoak = true;
	      	if ( this.Female = Utility.RandomBool() )
			{
				Body = 0x191;
				Name = NameList.RandomName( "female" );
				AddItem( new Skirt(Utility.RandomNeutralHue()) );
				AddItem( new ShepherdsCrook());
				HairItemID = Utility.RandomList
				( 
					8253, 8252, 
					8265, 8262,
					8261
				);
			}
			else
			{
				Body = 0x190;
				Name = NameList.RandomName( "male" );
				AddItem( new LongPants(Utility.RandomNeutralHue()) );
				AddItem( new GnarledStaff());
				FacialHairItemID = Utility.RandomList
				( 
					8268, 8267, 
					8257, 8269,
					0
				);
				FacialHairHue = Utility.RandomNeutralHue();
				HairItemID = Utility.RandomList
				( 
					8263, 8261, 
					8253, 8252,
					0
				);
			}
            Blessed = true;
			Hue = Utility.RandomSkinHue();
			HairHue = Utility.RandomSkinHue();  
			Title = "The Janitor";
            
            FancyShirt FS = new FancyShirt();
            FS.Hue = Utility.RandomNeutralHue();
			AddItem(FS);
			
            Boots SL = new Boots();
            SL.Hue = Utility.RandomNeutralHue();
			AddItem(SL);
 		}
 		public override void OnMovement( Mobile from, Point3D oldLocation )
		{
			if( m_Spoak == false )
			{
				if ( from.InRange( this, 3 ) && from is PlayerMobile)
				{
					if (Utility.RandomDouble() < 0.50) //50%
            		{
                		switch (Utility.Random(1))
                		{
                   			case 0: SayStuff( JanitorSpeech, this );break;
               			}
           			}
					
					m_Spoak = true;
					SpamTimer timer = new SpamTimer();
					timer.Start();
				}
			}
		}		
		private class SpamTimer : Timer
		{
			public SpamTimer() : base( TimeSpan.FromSeconds( 30 ) )
			{
			}
			protected override void OnTick()
			{
				m_Spoak = false;
			}
		}
		private static void SayStuff( string[] speak, Mobile from )
		{
			from.Say( speak[Utility.Random( speak.Length )] );
		}
      	public override void OnThink() 
        { 
        	base.OnThink(); 
	        if( this.m_JanitorChest == null )
	        {
				this.Say("I Need A JanitorChest please add one and [props Me to It.");
	   			return;
   			} 
       		if ( DateTime.Now < m_NextPickup ) 
	   			return; 
       		m_NextPickup = DateTime.Now + TimeSpan.FromSeconds( 2.5 + (2.5 * Utility.RandomDouble()) ); 
       		ArrayList LFBox = new ArrayList(); 
       		foreach ( Item item in this.GetItemsInRange( 10 ) ) 
       		{ 
	       		if ( item.Movable ) 
	       		{
            		LFBox.Add(item);
            		if (Utility.RandomDouble() < 0.05) //5%
            		{
                		switch (Utility.Random(2))
                		{
                   			case 0: this.Say("Oh, Look more litter."); break;
                    		case 1: this.Say("Whats with you people and losing stuff."); break;
                		}
            		} 
        		}
       		} 	
/*********************************************************************             
 *	         YOU CAN SET UP WHAT HE DOES NOT PICK UP HERE		     *					
 *********************************************************************/	
       		Type[] DoNotPickUp = new Type[]
	   		{ 
//		   		typeof(ItemName), 
//		   		typeof(ItemName), 
//		   		typeof(ItemName), 
//		   		typeof(ItemName), 
//		   		typeof(ItemName)
		   	};
/*************************************************************************		   	  
* Delete the // in front of what u entered and make sure the , are right *
**************************************************************************/ 	
       		bool LFBoxIt = true; 
       		for (int i = 0; i < LFBox.Count; i++) 
       		{ 
         		for (int j = 0; j < DoNotPickUp.Length; j++) 
           		{ 
               		if ( (LFBox[i]).GetType() == DoNotPickUp[j] ) 
                  		LFBoxIt = false; 
               	} 
            		if (LFBoxIt)
               			m_JanitorChest.DropItem(((Item)LFBox[i])); 
            			LFBoxIt = true; 
            } 
        }        
      	public Janitor( Serial serial ) : base( serial ) 
      	{ 
      	} 
      	public override void Serialize( GenericWriter writer ) 
      	{ 
      		base.Serialize( writer ); 
      		writer.Write( (int) 0 ); 
            writer.Write(m_JanitorChest);
      	} 
   		public override void Deserialize( GenericReader reader ) 
   		{ 
     		base.Deserialize( reader ); 
      		int version = reader.ReadInt(); 
            m_JanitorChest = reader.ReadItem() as JanitorChest;
   		}
   		public void MakeBox(Janitor from)
		{
			Map map = from.Map;
			Point3D loc = from.Location;
			if ( map == null )
			{return;}
			JanitorChest J = new JanitorChest();
			J.MoveToWorld( loc, map );
			from.m_JanitorChest = J;
			if ( m_Timer != null )
				m_Timer.Stop();
		}
		private Timer m_Timer;
		private class CreateTimer : Timer
		{
			private Janitor m_Janitor;
			public CreateTimer( Janitor j ) : base( TimeSpan.FromSeconds( 0.02 ) )
			{
				m_Janitor = j;
			}
			protected override void OnTick()
			{
				m_Janitor.MakeBox(m_Janitor);
				m_Janitor.Say("I've brought this to help make my job easier.");
				SpamTimer timer = new SpamTimer();
				timer.Start();
			}
		} 
   	} 
}
namespace Server.Items
{
	public class JanitorChest : MetalChest
	{
		public override int DefaultMaxWeight{ get{ return 0; } }
		public override bool IsDecoContainer
		{
			get{ return false; }
		}
		[Constructable]
        public JanitorChest() : base()
        {
            Name = "Lost & Found";
            Movable = false;
            Hue = 1360;
            if ( m_Timer == null )
			{
				m_Timer = new EmptyTimer( this );
				m_Timer.Start();
			}
        }
		public JanitorChest( Serial serial ) : base( serial )
		{
		}
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}
		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			if ( m_Timer == null )
			{
				m_Timer = new EmptyTimer( this );
				m_Timer.Start();
			}
		}
		public void Empty()
		{
			List<Item> items = this.Items;
			if ( items.Count > 0 )
			{
				for ( int i = items.Count - 1; i >= 0; --i )
				{
					if ( i >= items.Count )
						continue;
					items[i].Delete();
				}
			}
			if ( m_Timer != null )
				m_Timer.Stop();
				m_Timer = new EmptyTimer( this );
				m_Timer.Start();
		}
		private Timer m_Timer;
		private class EmptyTimer : Timer
		{
			private JanitorChest m_JanitorChest;																				
			public EmptyTimer( JanitorChest chest ) : base( TimeSpan.FromHours( 24.0 ) )//Sets how long in which to wait befor deleteing all items inside lost and found
			{																			
				m_JanitorChest = chest;
			}
			protected override void OnTick()
			{
				m_JanitorChest.Empty();
				m_JanitorChest.PublicOverheadMessage( Network.MessageType.Regular, 0x3B2, 501479, "" );
			}
		}
	}
}