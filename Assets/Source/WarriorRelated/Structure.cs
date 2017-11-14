using System.Collections.Generic;

namespace Warrior
{
    public class Limb
    {
        public string name;
        public bool isVital;
        public bool severed;
        public float hp;
        public float armorPercent;

        private WarriorController warrior;

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="hp"></param>
        /// <param name="armor">percent of damage reflected</param>
        /// <param name="isVital"></param>
        /// <param name="wa"></param>
        public Limb(string name, float hp, float armor, bool isVital, WarriorController wa = null)
        {
            warrior = wa;
            this.name = name;
            this.hp = hp;
            armorPercent = armor;
            this.isVital = isVital;
        }

        public void GetDamage(float dmg)
        {
            hp -= dmg;
            warrior.TakeDamage(dmg);
            if (hp < 0) severed = true;
        }

        public Limb Copy(WarriorController warrior)
        {
            return new Limb(name, hp, armorPercent, isVital, warrior);
        }
    }

    public class StructureOfWarrior
    {
        public float blood;
        public List<Limb> str;
    }
}
