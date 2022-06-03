﻿using System;
using HarmonyLib;
using ReskinMod.Skins;
using UnityEngine;

namespace ReskinMod.Patches.Villagers
{
    [HarmonyPatch(typeof(Villager), "Awake")]
    static class Villager_Reskin_Patch
    {
        static void Postfix(Villager __instance)
        {
             SkinCollection skinCollection = SkinCollection.GetSkinCollection(__instance.GetType().Name.ToLower(), true);
             if(skinCollection != null)
            {
                Skin character = skinCollection.GetSkin(SkinType.Character, 0);
                Skin characterArmed = skinCollection.GetSkin(SkinType.Character, 1);
                Skin gun = skinCollection.GetSkin(SkinType.Gun, 0);

                if (character != null)
                {
                    __instance.disarmedGunMaterial.mainTexture = character.texture;
                    if (!__instance.hasGun)
                    {
                        SpriteSM sprite = __instance.gameObject.GetComponent<SpriteSM>();
                        sprite.meshRender.sharedMaterial.SetTexture("_MainTex", character.texture);
                    }
                }
                if (gun != null)
                {
                    __instance.gunSprite.GetComponent<Renderer>().material.mainTexture = gun.texture;
                    __instance.gunSprite.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", gun.texture);
                }
                if (characterArmed != null)
                {
                    Traverse.Create(__instance).Field("armedMaterial").GetValue<Material>().mainTexture = characterArmed.texture;
                    if(__instance.hasGun)
                    {
                        SpriteSM sprite = __instance.gameObject.GetComponent<SpriteSM>();
                        sprite.meshRender.sharedMaterial.SetTexture("_MainTex", characterArmed.texture);
                    }
                }
            }
        }
    }
}
