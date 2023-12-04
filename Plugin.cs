using Aki.Reflection.Patching;
using BepInEx;
using System.Reflection;
using EFT.Animations;
using BepInEx.Configuration;

namespace AimPunchTweaker
{
    [BepInPlugin("com.creamcheese.apt", "CreamCheese-AimPunchTweaker", "1.0.0")]

    public class AimPunchPatches : BaseUnityPlugin
    {

        public static ConfigEntry<float> aimPunchIntensity { get; set; }
        public static ConfigEntry<bool> aimPunchBlur { get; set; }

        class AimPunchBlurPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(FastBlur).GetMethod("Hit", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void Postfix(FastBlur __instance, float power, ref int ____blurCount, ref float ___float_2)
            {
                if (aimPunchBlur.Value == true) { return; }
                ____blurCount = 0;
                ___float_2 = 0; // Removes the blur, I'm assuming. Tarkov's spaghetti code is so hard to read.
            }
        }

        class AimPunchMagnitudePatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(ForceEffector).GetMethod("Process", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void Postfix(ForceEffector __instance, float deltaTime)
            {
                __instance.WiggleMagnitude = 6.0f * aimPunchIntensity.Value;
            }
        }

        void Awake()
        {
            initConfig();
            new AimPunchBlurPatch().Enable();
            new AimPunchMagnitudePatch().Enable();
        }

        private void initConfig()
        {
            aimPunchBlur = Config.Bind<bool>("Aim Punch", "Aim Punch Blur", false, new ConfigDescription("Enables or disables the blur when shot."));
            aimPunchIntensity = Config.Bind<float>("Aim Punch", "Aim Punch Intensity", 1.0f, new ConfigDescription("Changes the intensity of aim punch. This is multiplicative.", new AcceptableValueRange<float>(0f, 1.0f)));
        }
    }
    
}
