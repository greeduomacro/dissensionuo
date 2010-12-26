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
	public class ProtectionLevelEnhancementGem : Item
	{
		[Constructable]
		public ProtectionLevelEnhancementGem() : this( 1 )
		{
		}

		[Constructable]
		public ProtectionLevelEnhancementGem( int amount ) : base( 0xF13 )
		{
			Weight = 1.0;
			Stackable = false;
			Name = "an armor protection enhancement gem";
			Amount = amount;
			Hue = 1260;
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
			private ProtectionLevelEnhancementGem m_ProtectionLevelEnhancementGem;

			public InternalTarget( ProtectionLevelEnhancementGem runeaug ) : base( 1, false, TargetFlags.None )
			{
				m_ProtectionLevelEnhancementGem = runeaug;
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
								if ( Armor.ProtectionLevel == ArmorProtectionLevel.Regular )
								{
									Armor.ProtectionLevel = ArmorProtectionLevel.Defense;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The protection level of your armor has been enhanced." );
									m_ProtectionLevelEnhancementGem.Delete();
									return;
								}
							
								if ( Armor.ProtectionLevel == ArmorProtectionLevel.Defense )
								{
									Armor.ProtectionLevel = ArmorProtectionLevel.Guarding;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The protection level of your armor has been enhanced." );
									m_ProtectionLevelEnhancementGem.Delete();
									return;
								}
							
								if ( Armor.ProtectionLevel == ArmorProtectionLevel.Guarding )
								{
									Armor.ProtectionLevel = ArmorProtectionLevel.Hardening;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The protection level of your armor has been enhanced." );
									m_ProtectionLevelEnhancementGem.Delete();
									return;
								}
							
								if ( Armor.ProtectionLevel == ArmorProtectionLevel.Hardening )
								{
									Armor.ProtectionLevel = ArmorProtectionLevel.Fortification;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The protection level of your armor has been enhanced." );
									m_ProtectionLevelEnhancementGem.Delete();
									return;
								}
							
								if ( Armor.ProtectionLevel == ArmorProtectionLevel.Fortification )
								{
									Armor.ProtectionLevel = ArmorProtectionLevel.Invulnerability;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The protection level of your armor has been enhanced." );
									m_ProtectionLevelEnhancementGem.Delete();
									return;
								}
							
								if ( Armor.ProtectionLevel == ArmorProtectionLevel.Invulnerability )
								{												 
									from.SendMessage( "This armor is already at full protection level." );
									return;
								}
							}	

							else // Fail
							{
								from.SendMessage( "You have failed to enhance the armor!" );
								from.SendMessage( "The armor is damaged beyond repair!" );
								from.PlaySound( 42 );
								Armor.Delete();
								m_ProtectionLevelEnhancementGem.Delete();
							}
					
						}
					}
					
					else 
					{
		       			from.SendMessage( "You cannot enhance that." );
		    		} 
					
		  	}
		
		}

		public ProtectionLevelEnhancementGem( Serial serial ) : base( serial )
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