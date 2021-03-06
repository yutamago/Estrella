﻿using System;
using System.Collections.Generic;
using Estrella.FiestaLib.Networking;
using Estrella.Util;
using Estrella.Zone.Handlers;

namespace Estrella.Zone.Game
{
    public abstract class MapObject
    {
        public MapObject()
        {
            IsAttackable = true;
            SelectedBy = new List<ZoneCharacter>();
        }

        ~MapObject()
        {
            SelectedBy.Clear();
        }

        public bool IsAdded { get; set; }
        public bool IsAttackable { get; set; }

        public bool IsDead
        {
            get { return HP == 0; }
        }


        public Map Map { get; set; }
        public Sector MapSector { get; set; }
        public Vector2 Position { get; set; }
        public byte Rotation { get; set; }
        public ushort MapObjectID { get; set; }

        public virtual uint HP { get; set; }
        public virtual uint MaxHP { get; set; }
        public virtual uint SP { get; set; }
        public virtual uint MaxSP { get; set; }

        public List<ZoneCharacter> SelectedBy { get; private set; }

        public ushort UpdateCounter
        {
            get { return ++statUpdateCounter; }
        }

        // HP/SP update counter thingy
        private ushort statUpdateCounter;
        public static readonly TimeSpan HpSpUpdateInterval = TimeSpan.FromSeconds(3);
        protected DateTime lastHpSpUpdate = DateTime.Now;

        public virtual void Attack(MapObject victim)
        {
            if (victim != null && !victim.IsAttackable) return;
        }

        public virtual void AttackSkill(ushort skillid, MapObject victim)
        {
            if (victim != null && !victim.IsAttackable) return;
        }

        public virtual void AttackSkillAoE(ushort skillid, uint x, uint y)
        {
        }

        public virtual void Revive(bool totally = false)
        {
            if (totally)
            {
                HP = MaxHP;
                SP = MaxSP;
            }
            else
            {
                // Note - Why not take e.g. 10% of your MaxHp?
                // HP = MaxHP * 0.1;
                HP = 50;
            }
        }

        public virtual void Damage(MapObject bully, uint amount, bool isSP = false)
        {
            if (isSP)
            {
                if (SP < amount) SP = 0;
                else SP -= amount;
            }
            else
            {
                if (HP < amount) HP = 0;
                else HP -= amount;
            }

            if (bully == null)
            {
                if (this is ZoneCharacter)
                {
                    var character = this as ZoneCharacter;
                    if (isSP)
                        Handler9.SendUpdateSP(character);
                    else
                        Handler9.SendUpdateHP(character);
                }
            }
            else
            {
                if (this is Mob && ((Mob) this).AttackingSequence == null)
                {
                    Attack(bully);
                }
                else if (this is ZoneCharacter && !((ZoneCharacter) this).IsAttacking)
                {
                    Attack(bully);
                }
            }
        }

        public abstract void Update(DateTime date);
        public abstract Packet Spawn();

        // Event trigger
        protected virtual void OnHpSpChanged()
        {
            HpSpChanged?.Invoke(this, new EventArgs());
        }

        // Event-Variables
        public event EventHandler<EventArgs> HpSpChanged;
    }
}