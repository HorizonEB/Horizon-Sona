using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace The_Horizon_Sona
{
    class Program
    {
        private static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static Menu StartMenu, ComboMenu, DrawingsMenu, ActivatorMenu, AHarrasMenu;

        public static Spell.Active _Q;
        public static Spell.Active _W;
        public static Spell.Active _E;
        public static Spell.Skillshot _R;
        public static Spell.Skillshot _FlashR;
        public static Spell.Targeted _Ignite;
        public static Spell.Targeted _Flash = new Spell.Targeted(ReturnSlot("summonerflash"), 425);
        public static SpellSlot ReturnSlot(string Name)
        {
            return Player.Instance.GetSpellSlotFromName(Name);
        }
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;

        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Sona"))
            {
                return;
            }
            Chat.Print("The Horizon Sona - Loaded !");
            _Q = new Spell.Active(SpellSlot.Q, 825);
            _W = new Spell.Active(SpellSlot.W, 1000);
            _E = new Spell.Active(SpellSlot.E, 425);
            _R = new Spell.Skillshot(SpellSlot.R, 900, SkillShotType.Linear, 250, 2400, 140);
            _R.AllowedCollisionCount = int.MaxValue;
            _Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            _FlashR = new Spell.Skillshot(SpellSlot.R, 1050, SkillShotType.Linear, 250, 2400, 140);
            StartMenu = MainMenu.AddMenu("The Horizon Sona", "The Horizon Sona");
            ComboMenu = StartMenu.AddSubMenu("Combo", "Combo");
            AHarrasMenu = StartMenu.AddSubMenu("Auto Harras", "Auto Harras");
            DrawingsMenu = StartMenu.AddSubMenu("Drawings", "Drawings");
            ActivatorMenu = StartMenu.AddSubMenu("Activator", "Activator");



            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddLabel("Q Settings");
            ComboMenu.Add("UseQ", new CheckBox("Use [Q]"));
            ComboMenu.AddLabel("W Settings");
            ComboMenu.Add("UseW", new CheckBox("Use [W]"));
            ComboMenu.AddSeparator(1);
            ComboMenu.Add("HPW", new Slider("Ally Minimum Health  Percentage %{0} Use W ", 60, 1));
            ComboMenu.AddSeparator(1);
            ComboMenu.Add("HPWS", new Slider("Sona Minimum Health  Percentage %{0} Use W ", 80, 1));
            ComboMenu.AddSeparator(1);
            ComboMenu.AddLabel("E Settings");
            ComboMenu.Add("UseE", new CheckBox("Use [E] in combo", false));
            ComboMenu.Add("UseEA", new CheckBox("Use [E] for ally"));
            ComboMenu.Add("HPE", new Slider(" Ally Minimum Health  Percentage %{0} Use E ", 30, 1));


            ComboMenu.AddLabel("R Settings");
            ComboMenu.Add("UseR", new CheckBox("Use [R] in combo", false));
            ComboMenu.AddSeparator(1);
            ComboMenu.AddLabel("Use for cast Flash + R To enemy ");
            ComboMenu.Add("FlashR", new KeyBind("Use Flash + R", false, KeyBind.BindTypes.HoldActive, 'T'));
            ComboMenu.AddSeparator(1);
            ComboMenu.AddLabel("Use for cast Ultimate to enemy by Prediction " );
            ComboMenu.Add("UltA", new KeyBind("Use Ultimate Assistance", false, KeyBind.BindTypes.HoldActive, 'R'));
            ComboMenu.Add("UseUA", new CheckBox("Tick for enable/disable Ultimate Assistance"));

            AHarrasMenu.AddGroupLabel("Auto Harras Settings");
            AHarrasMenu.AddLabel("Tick for enable/disable auto harras with Q when enemy is in Range");
            AHarrasMenu.Add("AHQ", new CheckBox("- Use [Q] For Auto Harras"));
            AHarrasMenu.Add("AHQM", new Slider(" Minimum Mana  Percentage %{0} Use [Q] ", 65, 1));


            DrawingsMenu.AddGroupLabel("Drawing Settings");
            DrawingsMenu.AddLabel("Tick for enable/disable Draw Spell Range");
            DrawingsMenu.Add("DQ", new CheckBox("- Draw [Q] range"));
            DrawingsMenu.AddSeparator(0);
            DrawingsMenu.Add("DW", new CheckBox("- Draw [W] range"));
            DrawingsMenu.AddSeparator(0);
            DrawingsMenu.Add("DE", new CheckBox("- Draw [E] range"));
            DrawingsMenu.AddSeparator(0);
            DrawingsMenu.Add("DR", new CheckBox("- Draw [R] range"));


            ActivatorMenu.AddGroupLabel("Activator Settings");
            ActivatorMenu.AddLabel("Use Summoner Spell");
            ActivatorMenu.Add("IGNI", new CheckBox("- Use Ignite if enemy is killable"));
            ActivatorMenu.AddSeparator(0);
            ActivatorMenu.AddSeparator(1);



            Game.OnTick += Game_OnTick;
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

        }


        private static void Game_OnUpdate(EventArgs args)
        {


            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
         //   if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.LastHit))
      //      {
            //    Lasthit();
       //     }
            Activator();
            Heal();
            HealSelf();
            Run();
            AHarras();




        }

        private static void Game_OnTick(EventArgs args)
        {

            if (ComboMenu["FlashR"].Cast<KeyBind>().CurrentValue)
            {
                FlashR();
            }

            if (ComboMenu["UltA"].Cast<KeyBind>().CurrentValue)
            {
                UltA();
            }


        }


        private static void Drawing_OnDraw(EventArgs args)
        {
            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Physical);
            if (DrawingsMenu["DQ"].Cast<CheckBox>().CurrentValue && _Q.IsLearned)
            {
                Circle.Draw(_Q.IsReady() ? Color.White : Color.Red, _Q.Range, _Player);
            }
            if (DrawingsMenu["DW"].Cast<CheckBox>().CurrentValue && _W.IsLearned)
            {
                Circle.Draw(_W.IsReady() ? Color.White : Color.Red, _W.Range, _Player);
            }
            if (DrawingsMenu["DE"].Cast<CheckBox>().CurrentValue && _E.IsLearned)
            {
                Circle.Draw(_E.IsReady() ? Color.White : Color.Red, _E.Range, _Player);
            }
            if (DrawingsMenu["DR"].Cast<CheckBox>().CurrentValue && _R.IsLearned)
            {
                Circle.Draw(_R.IsReady() ? Color.White : Color.Red, _R.Range, _Player);
            }

        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);

            if (target == null)
                return;
            if (ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue)
            {
                if (!target.IsInRange(_Player, _Q.Range) && _Q.IsReady())
                    return;
                {
                    _Q.Cast();
                }



            }
            if (ComboMenu["UseE"].Cast<CheckBox>().CurrentValue)
            {
                if (!target.IsInRange(_Player, _E.Range) && _E.IsReady())
                    return;
                  
                    {

                        _E.Cast(target);


                    }
            }
            if (ComboMenu["UseR"].Cast<CheckBox>().CurrentValue)
            {
                var Rpred = _R.GetPrediction(target);

                if (Rpred.HitChance >= HitChance.High && target.IsValidTarget(_R.Range))
                    if (!target.IsInRange(_Player, _R.Range) && _R.IsReady())
                    return;
                    {

                        _R.Cast(Rpred.CastPosition);



                    }
            }





        }

        public static void Heal()
        {
            foreach (var allies in EntityManager.Heroes.Allies.Where(y => y.HealthPercent < ComboMenu["HPW"].Cast<Slider>().CurrentValue && ComboMenu["UseW"].Cast<CheckBox>().CurrentValue))
            {
                _W.Cast(allies);
            }
            

        }

        public static void HealSelf()
        {
            foreach (var HealSelf in EntityManager.Heroes.Allies.Where(y => y.HealthPercent < ComboMenu["HPWS"].Cast<Slider>().CurrentValue && ComboMenu["UseW"].Cast<CheckBox>().CurrentValue))
            {
                _W.Cast(HealSelf);
            }


        }
        public static void Run()
        {
            foreach (var Run in EntityManager.Heroes.Allies.Where(y => _Player.HealthPercent < ComboMenu["HPE"].Cast<Slider>().CurrentValue && ComboMenu["UseEA"].Cast<CheckBox>().CurrentValue))
            {
                _W.Cast(Run);
            }


        }

        public static void AHarras()
        {

     //       foreach (var HQ in EntityManager.Heroes.Enemies.Where(y => y.ManaPercent > AHarrasMenu["AHQM"].Cast<Slider>().CurrentValue && AHarrasMenu["AHQ"].Cast<CheckBox>().CurrentValue))
              var target = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);

                if (target == null)
                return;
            {
                if (_Player.ManaPercent > AHarrasMenu["AHQM"].Cast<Slider>().CurrentValue && AHarrasMenu["AHQ"].Cast<CheckBox>().CurrentValue)

                {
                    _Q.Cast(target);
                }
            }



            


        }

    
        public static void UltA()
        {

            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                var target = TargetSelector.GetTarget(_R.Range, DamageType.Magical);
                if (target.IsValidTarget(_R.Range))
                    if (!target.IsInRange(_Player, _R.Range))
                        return;
                {
                    if (target.IsValidTarget() && _R.IsReady() && ComboMenu["UseUA"].Cast<CheckBox>().CurrentValue)
                    {
                        _R.Cast(target);
                    }
                }
            }
        }

        public static void Activator()
        {
            var target = TargetSelector.GetTarget(_Ignite.Range, DamageType.True);
            if (target == null)
                return;
            if (ActivatorMenu["IGNI"].Cast<CheckBox>().CurrentValue && _Ignite.IsReady() && target.IsValidTarget())

            {
                if (target.Health + target.AttackShield <
                    _Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite))
                {
                    _Ignite.Cast(target);
                }
            }
        }
        /* credits for wladi0*/
        public static void FlashR()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            var target = TargetSelector.GetTarget(_FlashR.Range, DamageType.Magical);
            if (target.IsValidTarget(_FlashR.Range))
                if (!target.IsInRange(_Player, _FlashR.Range))
                return;
            {
                var Flashh = EloBuddy.Player.Instance.ServerPosition.Extend(target.ServerPosition, _Flash.Range);

                if (_Flash.IsReady() && target.IsValidTarget() && _R.IsReady())
                {
                    _Flash.Cast(Flashh.To3DWorld());
                    _FlashR.Cast(target);
                }
            }
        }


    }
}
