// Created by Peoharen

using System;
using System.Collections;

namespace Server
{
    public class EnhancementTimer : Timer
    {
        private int m_Duration;
        private readonly ArrayList AL = new ArrayList();
        private readonly Mobile m_Mobile;
        private readonly string m_Title;

        public EnhancementTimer(Mobile m, int duration, string title, params object[] args)
            : base(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
        {
            if (args.Length < 1 || (args.Length%2) != 0)
                throw new Exception("EnhancementTimer: args.length must be an even number greater than 0");

            Enhancement.AddMobile(m, title);
            m_Mobile = m;
            m_Title = title;
            m_Duration = duration;

            AosAttribute att;
            AosWeaponAttribute weapon;
            AosArmorAttribute armor;
            SAAbsorptionAttribute absorb;
            var number = 0;

            for (var i = 0; i < args.Length - 1; i += 2)
            {
                if (!(args[i + 1] is int))
                    throw new Exception("EnhancementTimer: The second value must be an integer");

                number = (int) args[i + 1];

                if (args[i] is AosAttribute)
                {
                    att = (AosAttribute) args[i];
                    Enhancement.SetValue(m, att, (Enhancement.GetValue(m, att) + number), m_Title);
                    AL.Add(att);
                    AL.Add(number);
                }
                else if (args[i] is AosWeaponAttribute)
                {
                    weapon = (AosWeaponAttribute) args[i];
                    Enhancement.SetValue(m, weapon, (Enhancement.GetValue(m, weapon) + number), m_Title);
                    AL.Add(weapon);
                    AL.Add(number);
                }
                else if (args[i] is AosArmorAttribute)
                {
                    armor = (AosArmorAttribute) args[i];
                    Enhancement.SetValue(m, armor, (Enhancement.GetValue(m, armor) + number), m_Title);
                    AL.Add(armor);
                    AL.Add(number);
                }
                else if (args[i] is SAAbsorptionAttribute)
                {
                    absorb = (SAAbsorptionAttribute) args[i];
                    Enhancement.SetValue(m, absorb, (Enhancement.GetValue(m, absorb) + number), m_Title);
                    AL.Add(absorb);
                    AL.Add(number);
                }
            }
        }

        public void End()
        {
            if (Enhancement.EnhancementList.ContainsKey(m_Mobile))
            {
                AosAttribute att;
                AosWeaponAttribute weapon;
                AosArmorAttribute armor;
                SAAbsorptionAttribute absorb;
                var number = 0;

                for (var i = 0; i < AL.Count - 1; i += 2)
                {
                    number = (int) AL[i + 1];

                    if (AL[i] is AosAttribute)
                    {
                        att = (AosAttribute) AL[i];
                        Enhancement.SetValue(m_Mobile, att, (Enhancement.GetValue(m_Mobile, att) - number), m_Title);
                    }
                    else if (AL[i] is AosWeaponAttribute)
                    {
                        weapon = (AosWeaponAttribute) AL[i];
                        Enhancement.SetValue(m_Mobile, weapon, (Enhancement.GetValue(m_Mobile, weapon) - number),
                            m_Title);
                    }
                    else if (AL[i] is AosArmorAttribute)
                    {
                        armor = (AosArmorAttribute) AL[i];
                        Enhancement.SetValue(m_Mobile, armor, (Enhancement.GetValue(m_Mobile, armor) - number), m_Title);
                    }
                    else if (AL[i] is SAAbsorptionAttribute)
                    {
                        absorb = (SAAbsorptionAttribute) AL[i];
                        Enhancement.SetValue(m_Mobile, absorb, (Enhancement.GetValue(m_Mobile, absorb) - number),
                            m_Title);
                    }
                }
            }

            Stop();
        }

        protected override void OnTick()
        {
            m_Duration--;

            if (m_Mobile == null)
                Stop();

            if (m_Duration < 0)
            {
                End();
            }
        }
    }
}