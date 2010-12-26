using System;
using System.Collections;
using System.Collections.Generic;
using Server.Multis;
using Server.Mobiles;
using Server.Network;
using Server.ContextMenus;
using Server.Spells;
using Server.Targeting;
using Server.Misc;
using Server.Items;


namespace Server.Items
{
	public class ArmorDurabilityEnhancementGem : Item
	{
		[Constructable]
		public ArmorDurabilityEnhancementGem() : this( 1 )
		{
		}

		[Constructable]
		public ArmorDurabilityEnhancementGem( int amount ) : base( 0xF13 )
		{
			Weight = 1.0;
			Stackable = false;
			Name = "an armor durability enhancement gem";
			Amount = amount;
			Hue = 1157;
		}
		
		public override void OnDoubleClick( Mobile from ) 
		{
			PlayerMobile pm = from as PlayerMobile;
		
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}

		        else if( from.InRange( this.GetWorldLocation(), 1 ) ) 
		        {
				
					from.SendMessage( "Select the item to enhance." );
					from.Target = new InternalTarget( this );
				
		        } 

		        else 
		        { 
		        	from.SendLocalizedMessage( 500446 ); // That is too far away. 
		        } 
		} 
		
		private class InternalTarget : Target 
		{
			private ArmorDurabilityEnhancementGem m_ArmorDurabilityEnhancementGem;

			public InternalTarget( ArmorDurabilityEnhancementGem runeaug ) : base( 1, false, TargetFlags.None )
			{
				m_ArmorDurabilityEnhancementGem = runeaug;
			}

		 	protected override void OnTarget( Mobile from, object targeted ) 
		 	{ 
				
			    	if ( targeted is BaseArmor ) 
					{ 
			       		BaseArmor Armor = targeted as BaseArmor; 

						if ( !from.InRange( ((Item)targeted).GetWorldLocation(), 1 ) ) 
						{ 
			          		from.SendLocalizedMessage( 500446 ); // That is too far away. 
		       			}

						else if (( ((Item)targeted).Parent != null ) && ( ((Item)targeted).Parent is Mobile ) ) 
			       		{ 
			          		from.SendMessage( "You cannot enhance that in it's current location." ); 
		       			}

						else
		       			{
							int DestroyChance = Utility.Random( 3 );

							if ( DestroyChance > 0 ) // Success
							{
								if ( Armor.Durability == ArmorDurabilityLevel.Regular )
								{
									Armor.Durability = ArmorDurabilityLevel.Durable;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The durability level of your armor has been enhanced." );
									m_ArmorDurabilityEnhancementGem.Delete();
									return;
								}
							
								if ( Armor.Durability == ArmorDurabilityLevel.Durable )
								{
									Armor.Durability = ArmorDurabilityLevel.Substantial;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The durability level of your armor has been enhanced." );
									m_ArmorDurabilityEnhancementGem.Delete();
									return;
								}
							
								if ( Armor.Durability == ArmorDurabilityLevel.Substantial )
								{
									Armor.Durability = ArmorDurabilityLevel.Massive;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The durability level of your armor has been enhanced." );
									m_ArmorDurabilityEnhancementGem.Delete();
									return;
								}
							
								if ( Armor.Durability == ArmorDurabilityLevel.Massive )
								{
									Armor.Durability = ArmorDurabilityLevel.Fortified;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The durability level of your armor has been enhanced." );
									m_ArmorDurabilityEnhancementGem.Delete();
									return;
								}
							
								if ( Armor.Durability == ArmorDurabilityLevel.Fortified )
								{
									Armor.Durability = ArmorDurabilityLevel.Indestructible;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The durability level of your armor has been enhanced." );
									m_ArmorDurabilityEnhancementGem.Delete();
									return;
								}
							
								if ( Armor.Durability == ArmorDurabilityLevel.Indestructible )
								{												 
									from.SendMessage( "This armor is already at full durability level." );
									return;
								}
							}	

							else // Fail
							{
								from.SendMessage( "You have failed to enhance the armor!" );
								from.SendMessage( "The armor is damaged beyond repair!" );
								from.PlaySound( 42 );
								Armor.Delete();
								m_ArmorDurabilityEnhancementGem.Delete();
							}
					
						}
					}
					
					else 
					{
		       			from.SendMessage( "You cannot enhance that." );
		    		} 
					
		  	}
		
		}

		public ArmorDurabilityEnhancementGem( Serial serial ) : base( serial )
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
		}
	}
}