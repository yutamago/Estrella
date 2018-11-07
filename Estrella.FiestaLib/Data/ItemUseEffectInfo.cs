using System.Collections.Generic;
using System.Data;

namespace Estrella.FiestaLib.Data
{
    public sealed class ItemUseEffectInfo
    {
        public ItemUseEffectInfo()
        {
            Effects = new List<ItemEffect>();
        }

        public ushort ID { get; set; }
        public string AbState { get; private set; }
        public List<ItemEffect> Effects { get; private set; }

        public static ItemUseEffectInfo Load(DataRow row, out string inxName)
        {
            var info = new ItemUseEffectInfo();
            inxName = (string) row["ItemIndex"];

            var typeA = (ItemUseEffectType) (uint) row["UseEffectA"];
            if (typeA != ItemUseEffectType.None)
            {
                var effect = new ItemEffect();
                effect.Type = typeA;
                effect.Value = (uint) row["UseValueA"];
                info.Effects.Add(effect);
            }

            var typeB = (ItemUseEffectType) (uint) row["UseEffectB"];
            if (typeB != ItemUseEffectType.None)
            {
                var effect = new ItemEffect();
                effect.Type = typeB;
                effect.Value = (uint) row["UseValueB"];
                info.Effects.Add(effect);
            }

            var typeC = (ItemUseEffectType) (uint) row["UseEffectC"];
            if (typeC != ItemUseEffectType.None)
            {
                var effect = new ItemEffect();
                effect.Type = typeC;
                effect.Value = (uint) row["UseValueC"];
                info.Effects.Add(effect);
            }

            info.AbState = (string) row["UseAbStateName"];
            return info;
        }
    }
}