
/* ****************************************************
SkillStone.cs
Created By: Poolmanjim
Last Updated: 07/11/2021
VersioN: 0.9.2 Beta
GitHub Link: https://github.com/poolmanjim/SkillStone

DESCRIPTION
For ServUO - Skill Stone can be used to add to a player's current skills.

INSTALLATION INSTRUCTIONS
1. Copy SkillStone.cs into your ServUO scripts folder. 
2. In game use [add skillstone to add the skill stone.

CUSTOMIZATION INSTRUCTIONS
1. Locate "public SkillStone() : base(0x1870)" in the script.
2. Modify the lines for SkillPoints and SkillMaxLevel in the code block to be the values you want.
3. Save the script. 

LICENSE
MIT License (see below). However, here are my desires beyond what the license states. 
1. Leave the credit section above in tact.
2. Redistribute at will.
3. By license, you are allowed to sell this. I ask kindly, that you avoid doing that. I want this to build the community, not one person.
4. Do not intentionally include this in a world-ending AI. 

Copyright (c) 2021 Poolmanjim

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*********
CHANGE LOG
    v0.9.2
        Fixed bug causing crash if the stone was used, server rebooted, and it was used again.

**************************************************** */
using System;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
    public class SkillStone : Item
    {
        private int m_SkillPoints; // Total number of available skill points
        private double m_SkillMaxLevel; // Max any given skill can be set to using the stone.
        private PlayerMobile m_AssignedPlayer;

        [CommandProperty(AccessLevel.GameMaster)]
        public int SkillPoints
        {
            get { return m_SkillPoints; }
            set { m_SkillPoints = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double SkillMaxLevel 
        {
            get  { return m_SkillMaxLevel; }
            set { m_SkillMaxLevel = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public PlayerMobile AssignedPlayer 
        {
            get  { return m_AssignedPlayer; }
            set { m_AssignedPlayer = value; }
        }

        [Constructable]
        public SkillStone() : base(0x1870)
        {
            Name = "Skill Stone";
            Hue = 2704;
            LootType = LootType.Blessed;
            Movable = false;
            SkillPoints = 12000; // Change this to change how many points are available on the stone by default. 1 point = 0.1 skill.
            SkillMaxLevel = 100; // Change this to change the maximum value the stone can set a skill to.
        }

        public SkillStone(int skillPoints, double skillMaxLevel) : base( 0x1870 )
        {
            Name = "Skill Stone";
            Hue = 2704;
            LootType = LootType.Blessed;
            Movable = false;
            SkillPoints = skillPoints; 
            SkillMaxLevel = skillMaxLevel; 
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.CloseGump(typeof(SetSkillsGump));

            if (from.Backpack == null )
            {
                from.SendMessage(" This must be in your backpack to function.");
                return;
            }

            if( this.AssignedPlayer == null )
            {
                this.AssignedPlayer = (PlayerMobile)from;
                this.Name = String.Format("{0}'s {1}",from.Name,this.Name);
                from.SendMessage( "The skill stone has been assigned to you!" );
            }
            else if( this.AssignedPlayer != (PlayerMobile)from )
            {
                from.SendMessage( "This skill stone does not belong to you!" );
            }
            else
            {
                if( from.AccessLevel >= AccessLevel.GameMaster )
                    from.SendMessage( "Debug: Stone is working." );
            }

            if( this.AssignedPlayer == (PlayerMobile)from )
                from.SendGump(new SetSkillsGump(from, this, SkillInfo.Table, null));
        }

        public SkillStone(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version 
            writer.Write((int)m_SkillPoints);
            writer.Write((int)m_SkillMaxLevel);  
            writer.Write((PlayerMobile)m_AssignedPlayer);     
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    m_SkillPoints = reader.ReadInt();
                    m_SkillMaxLevel = reader.ReadInt();
                    m_AssignedPlayer = (PlayerMobile)reader.ReadMobile();
                    break;
            }
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add( String.Format("Points Remaining: {0}",m_SkillPoints)); // Add the points to the display.
        }

    }

    public class SetSkillsGump : Gump
    {
        private readonly Mobile m_Player;
        private readonly SkillStone m_SkillStone;

        private const int LabelColor = 0x7FFF;

        public SetSkillsGump(Mobile player, SkillStone stone, SkillInfo[] skills, object notice) : base(0, 0)
        {
            m_Player = player;
            m_SkillStone = stone;

            string skillsRemainingHTML = String.Format("<CENTER><BASEFONT COLOR=#FFFF00>Remaining Skill Points: </BASEFONT><BASEFONT COLOR=#CCCCCC>{0}</BASEFONT> <BASEFONT COLOR=#FFFF00>Stone Max Skill Level: </BASEFONT><BASEFONT COLOR=#CCCCCC>{1}</BASEFONT></CENTER>",stone.SkillPoints, stone.SkillMaxLevel);

            AddPage(0);
            AddBackground(0, 0, 550, 550, 2620);// Stone BG: x,y,w,h,imageId
            AddHtml( 0, 15, 550, 25, "<CENTER><BASEFONT COLOR=#FFFF00>Skill Stone</BASEFONT></CENTER>", false, false );
            AddHtml( 0, 30, 550, 25, "<CENTER><BASEFONT COLOR=#FFFF00>Created By: Poolmanjim</BASEFONT></CENTER>", false, false );
            AddHtml( 0, 45, 550, 25, "<CENTER><BASEFONT COLOR=#FFFF00>Version 0.9.2 Beta</BASEFONT></CENTER>", false, false );
            AddHtml( 0, 60, 550, 25, skillsRemainingHTML, false, false );
            AddHtml( 50, 90, 400, 60, "<BASEFONT COLOR=#CCCCCC>1 point = 0.1 skill. Click the number area and enter a value up to the maximum skill level supported. Click the button next to the field to set your skill. </BASEFONT>",false, false );

            // AddAlphaRegion(10, 70, 125, 460); 

            for( int skillCount = 0; skillCount < skills.Length; ++skillCount )
            {
                int skillsPerPage = 15;
                int index = (skillCount % skillsPerPage);

                int stdOriginY = 165;
                int standardOffY = 25; 
                int offsetY = stdOriginY + ( index * standardOffY );

                String skillLabel = String.Format( "{0}. {1}", skillCount.ToString(), skills[skillCount].Name );
                String playerSkillBase = player.Skills[skillCount].Base.ToString();

                if (index == 0)
                {
                    if (skillCount > 0)
                    {
                        AddButton(425, 470, 4005, 4007, 0, GumpButtonType.Page, (skillCount / skillsPerPage) + 1);
                        AddHtmlLocalized(460, 470, 100, 18, 1044045, LabelColor, false, false); // NEXT PAGE
                    }

                    AddPage((skillCount / skillsPerPage) + 1);

                    if (skillCount > 0)
                    {
                        AddButton(425, 495, 4014, 4015, 0, GumpButtonType.Page, skillCount / skillsPerPage);
                        AddHtmlLocalized(460, 495, 100, 18, 1044044, LabelColor, false, false); // PREV PAGE
                    }
                }

                // Skill List
                AddLabel( 150,  stdOriginY + (index * 20), 1153, skillLabel );
                AddTextEntry ( 300, stdOriginY + (index * 20), 50, 25, 1153, skillCount+1, playerSkillBase );
                //AddAlphaRegion( 295, 70 + (index * 20), 50, 15); 
                AddButton(355, stdOriginY + (index * 20), 4005, 4007, skillCount+1, GumpButtonType.Reply, 0); // x, y, off click , on click, button type
            }

            // Close Button at Bottom          
            AddButton(425, 520, 4005, 4007, 0x2, GumpButtonType.Reply, 0); // x, y, off click , on click, button type
            AddHtmlLocalized(460, 520, 140, 25, 1011012, false, false); // CANCEL
        }
        public override void OnResponse(NetState state, RelayInfo info)
        {
            switch( info.ButtonID )
            {
                case 0: // Cancel
                    return;
                default:
                    foreach( TextRelay textEntry in info.TextEntries )
                    {
                        double newSkillValue;
                        double skillDiff;
                        Skill targetSkill;

                        targetSkill = m_Player.Skills[textEntry.EntryID-1];
                        
                        try
                        {
                            newSkillValue = Convert.ToDouble( textEntry.Text );
                            
                        }
                        catch ( System.Exception )
                        {
                            newSkillValue = 0;
                            m_Player.SendMessage( 2117, String.Format("You cannot set your skill {0} to a value that is not a number.",targetSkill.Name) );
                        }

                        skillDiff =  newSkillValue - targetSkill.Base;

                        // Rules out some weird conditions with adding.
                        if( skillDiff < 0 )
                        {
                            skillDiff = 0;
                        }

                        // If the skill is equal to the existing or is just 0, ignore it. 
                        if( (targetSkill.Base == newSkillValue) || newSkillValue == 0 ) // Skill is equal to target
                        {
                            // Literally do nothing, we don't particularly care in this case so we ignore it. This is the default action.
                        }
                        else
                        {
                            if( newSkillValue > m_SkillStone.SkillMaxLevel )
                            {
                                m_Player.SendMessage( 2117, String.Format("You cannot set your skill that high. Your stone is locked to a max skill of {0}",m_SkillStone.SkillMaxLevel ) );
                            }
                            else if( targetSkill.Base >= newSkillValue ) // Skill is currently higher
                            {
                                m_Player.SendMessage( 2117, String.Format( "Your ability in the skill in {0} skill is higher than that. Current: {1}.", targetSkill.Name, targetSkill.Base) );
                            }
                            else if( targetSkill.Cap < newSkillValue ) // Skill is at Skill Cap
                            {
                                m_Player.SendMessage( 2117, String.Format( "You cannot set your skill in {0} any higher. Current: {1}/{2}. Use a PowerScroll to go higher.", targetSkill.Name, targetSkill.Base, targetSkill.Cap ) );
                                m_Player.SendMessage( newSkillValue.ToString() );
                            }
                            else if( m_Player.Skills.Cap  < ((m_Player.SkillsTotal ) + (skillDiff * 10)) ) // Skill max
                            {
                                m_Player.SendMessage( 2117, "You cannot set your skill any higher. You are at the skill cap of the server.");
                            }
                            else if( (newSkillValue * 10) > m_SkillStone.SkillPoints )
                            {
                                m_Player.SendMessage( 2117, String.Format("Your skill stone lacks the points to raise your skill that high. Points remaining: {0}.", m_SkillStone.SkillPoints) );
                            }
                            else
                            {
                                targetSkill.Base = newSkillValue;
                                m_SkillStone.SkillPoints -= (int)(skillDiff * 10);
                                m_Player.SendMessage( String.Format("Set your {0} skill to {1}. The stone's power drains. You have {2} points remaining.", targetSkill.Name, newSkillValue, m_SkillStone.SkillPoints.ToString() ) );
                            }
                        }
                    }
                    break;
            }

            if( m_SkillStone.SkillPoints <= 0 )
            {
                m_SkillStone.Delete(); // Remove it when it is empty.
            }
            else
            {
                m_Player.SendGump(new SetSkillsGump(m_Player, m_SkillStone, SkillInfo.Table, null));
            }
        }
    }
}