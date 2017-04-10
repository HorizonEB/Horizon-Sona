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

        private static Menu StartMenu, ComboMenu, DrawingsMenu, ActivatorMenu;

        public static Spell.Active _Q;
        public static Spell.Active _W;
        public static Spell.Active _E;
        public static Spell.Skillshot _R;
        public static Spell.Skillshot _FlashR;
        public static Spell.Skillshot _Flash;
        public static Spell.Targeted _Ignite;

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
            Chat.Print("The Horizon Sona - Loaded,");

            _Q = new Spell.Active(SpellSlot.Q, 825);
            _W = new Spell.Active(SpellSlot.W, 1000);
            _E = new Spell.Active(SpellSlot.E, 425);
            _R = new Spell.Skillshot(SpellSlot.R, 900, SkillShotType.Circular, 250, 2400, 140);
            _R.AllowedCollisionCount = int.MaxValue;
            _Flash = new Spell.Skillshot(Player.Instance.GetSpellSlotFromName("summonerflash"), 425, SkillShotType.Linear);
            _Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            _FlashR = new Spell.Skillshot(SpellSlot.R, 1050, SkillShotType.Circular, 250, 2400, 140);
            StartMenu = MainMenu.AddMenu("The Horizon Sona", "The Horizon Sona");
            ComboMenu = StartMenu.AddSubMenu("Combo", "Combo");
            DrawingsMenu = StartMenu.AddSubMenu("Drawings", "Drawings");
            ActivatorMenu = StartMenu.AddSubMenu("Activator", "Activator");


            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddLabel("Tick for enable/disable spells in Combo");
            ComboMenu.Add("UseQ", new CheckBox("Use [Q]"));
            ComboMenu.AddLabel("W Settings");
            ComboMenu.Add("UseW", new CheckBox("Use [W]"));
            ComboMenu.Add("HPW", new Slider("Ally Minimum Health  Percentage %{0} Use W ", 70, 1));
            ComboMenu.AddLabel("E Settings");
            ComboMenu.Add("UseE", new CheckBox("Use [E]"));
            ComboMenu.Add("HPE", new Slider(" Minimum Health  Percentage %{0} Use E ", 30, 1));
        

            foreach (var y in EntityManager.Heroes.Enemies)
            {
                ComboMenu.Add("EnemyRR" + y.Hero, new CheckBox(y.ChampionName + " Use R "));
            }
            ComboMenu.Add("UseR", new CheckBox("Use [R]"));
            ComboMenu.Add("FlashR", new KeyBind("Use Flash + R", false, KeyBind.BindTypes.HoldActive, 'T'));

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
            Drawing.OnDraw += Drawing_OnDraw;

        }

        private static void Game_OnTick(EventArgs args)
        {
            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Combo:
                    Combo();
                    break;
                case Orbwalker.ActiveModes.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();

            }
            Activator();
            if (ComboMenu["FlashR"].Cast<KeyBind>().CurrentValue)
            FlashR();

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
                if (target.IsInRange(_Player, _Q.Range) && _Q.IsReady() && _Player.Distance(target) > 125)

                {
                    _Q.Cast();
                }



            }
            if (ComboMenu["UseW"].Cast<CheckBox>().CurrentValue)
            {
                if (target.IsInRange(_Player, _W.Range) && _W.IsReady())
                {

                    _W.Cast();



                }
            }
            if (ComboMenu["UseE"].Cast<CheckBox>().CurrentValue)
            {
                if (target.IsInRange(_Player, _E.Range) && _E.IsReady())
                  
                    {

                        _E.Cast(target);


                    }
            }
            if (ComboMenu["UseR"].Cast<CheckBox>().CurrentValue)
            {

                    if (target.IsInRange(_Player, _R.Range))
                    {

                        _R.Cast(target);



                    }
            }





        }

        public static void Activator()
        {
            var target = TargetSelector.GetTarget(_Ignite.Range, DamageType.True);
            if (_Ignite != null && ActivatorMenu["IGNI"].Cast<CheckBox>().CurrentValue && _Ignite.IsReady())
            {
                if (target.Health + target.AttackShield <
                    _Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite))
                {
                    _Ignite.Cast(target);
                }
            }
        }
        public static void FlashR()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            var target = TargetSelector.GetTarget(_FlashR.Range, DamageType.Magical);
            if (target.IsValidTarget(_FlashR.Range))
            {
                var Flashh = EloBuddy.Player.Instance.ServerPosition.Extend(target.ServerPosition, _Flash.Range);

                if (_Flash.IsReady() && target.IsValidTarget() && _R.IsReady())
                {
                    _Flash.Cast(Flashh.To3DWorld());
                    _FlashR.Cast(target.Position);
                }
            }
        }


    }
}
