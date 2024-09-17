using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using RoR2;

namespace VoidtouchedRework
{

    //This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a small plugin that adds a relatively simple item to the game, and gives you that item whenever you press F2.

    //This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class VoidtouchedRework : BaseUnityPlugin
    {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "OakPrime";
        public const string PluginName = "VoidtouchedRework";
        public const string PluginVersion = "1.0.1";

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            Log.Init(Logger);
            try
            {
                Logger.LogDebug("test");

                /*RoR2.RoR2Application.onLoad += () =>
                {
                    Logger.LogDebug("test2");

                    Logger.LogDebug("eliteCat length : " + EliteCatalog.eliteDefs.Length);
                    foreach (EliteDef def in EliteCatalog.eliteDefs)
                    {
                        Logger.LogDebug(def.name + ": health factor: " + def.healthBoostCoefficient + " damage factor: " + def.damageBoostCoefficient);
                        if (def.name.Equals("edVoid")) {
                            //Logger.LogDebug("Old health factor: " + def.healthBoostCoefficient);
                            //Logger.LogDebug("Old damage factor: " + def.damageBoostCoefficient);

                            //def.healthBoostCoefficient = 1.42f;
                            def.damageBoostCoefficient = 10.0f;
                            //Logger.LogDebug("New health factor: " + def.healthBoostCoefficient);

                            //Logger.LogDebug("New damage factor: " + def.damageBoostCoefficient);

                        }
                    };
                };*/
                IL.RoR2.GlobalEventManager.ProcessHitEnemy += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    c.GotoNext(
                        x => x.MatchLdloc(1),
                        x => x.MatchCallOrCallvirt(out _),
                        x => x.MatchBrfalse(out _),
                        x => x.MatchLdarg(out _)
                    );
                    c.Index += 3;
                    c.Emit(OpCodes.Ldloc_1);
                    c.Emit(OpCodes.Ldloc_2);
                    c.Emit(OpCodes.Ldarg_1);
                    c.EmitDelegate<Action<CharacterBody, CharacterBody, DamageInfo>>((attacker, victim, info) =>
                    {
                        if (attacker.HasBuff(RoR2.DLC1Content.Buffs.EliteVoid) && Util.CheckRoll(info.procCoefficient * 100, attacker.master))
                        {
                            victim.AddTimedBuff(RoR2Content.Buffs.NullifyStack, 8f);
                        }
                    });
                    c.GotoNext(
                         x => x.MatchLdloc(out _),
                         x => x.MatchLdloc(out _),
                         x => x.MatchLdsfld(out _),
                         x => x.MatchCallOrCallvirt(out _)
                    );
                    c.Index--;
                    c.RemoveRange(10);
                };
                IL.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    c.GotoNext(
                        x => x.MatchLdcI4(2),
                        x => x.MatchBge(out _)
                    );
                    c.Remove();
                    c.Emit(OpCodes.Ldc_I4_1);
                };
                On.RoR2.AffixVoidBehavior.OnEnable += (orig, self) =>
                {
                    orig(self);
                    self?.body?.inventory?.GiveItem(RoR2Content.Items.BoostDamage.itemIndex, 3);
                };
                On.RoR2.AffixVoidBehavior.OnDisable += (orig, self) =>
                {
                    orig(self);
                    self?.body?.inventory?.RemoveItem(RoR2Content.Items.BoostDamage.itemIndex, 3);
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message + " - " + e.StackTrace);
            }
        }
    }
}
