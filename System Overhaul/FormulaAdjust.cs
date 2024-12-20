// Project:         System Overhaul for DFU
// Copyright:       Copyright (C) 2024 ClonedPuffin
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          ClonedPuffin

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using System;
using DaggerfallWorkshop.Game.Items;
using DaggerfallConnect;
using static DaggerfallWorkshop.Game.Formulas.FormulaHelper;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;


namespace SystemOverhaul
{
    public class FormulaAdjust: MonoBehaviour
    {
        static Mod mod;

        static readonly PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;

        static bool newAP = true;
        static bool newHP = true;
        static bool newToHit = true;
        static bool newHealingRate = true;
        static bool newMagicResist = true;
        static bool newHPBonus = true;
        static bool useEnduranceBonus;
        static bool newWeaponToHitMultiplier = true;
        static bool newAttack = true;
        static bool governingAttributes = true;
        private static bool adjustSP = true;
        static bool allowCrits = true;
        static bool materialReduction = true;
        static bool newEnemyDamageMod =true;
        static bool hitAdjustments = true;
        static float extraStartHealthMultiplier = 2;
        static float weaponToHitMultiplier = 1;
        static float materialDamageMultiplier = 2;
        static int apGoal = 180;
        static int apLoss = 2;
        static int currentSkill = 0;
        static int baseHealthAmount = 30;
       
        static int toHitPlayer = -25;
        static int toHitMonster = 25;
        static int baseSP = 0;
        static float playerCritDamage = 1;
        static float playerCritToHit = 1;
        static float enemyCritDamage = 1;
        static float enemyCritToHit = 1;
        static float spellPointMultiplier = 1.5f;
        

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<FormulaAdjust>();
            mod.IsReady = true;    

            ModSettings();
        }
        
        #region Settings
        static void ModSettings() 
        {
            ModSettings settings = mod.GetSettings();
            //AP on level
            newAP = settings.GetValue<bool>("Features", "AttributePointOnLevel");
            apGoal = settings.GetValue<int>("AttributePointsOnLevel", "AttributePointsGoal");
            apLoss = settings.GetValue<int>("AttributePointsOnLevel", "AttributePointsLoss");
            //New starting health
            newHP = settings.GetValue<bool>("Features", "StartingHealth");
            baseHealthAmount = settings.GetValue<int>("StartingHealth", "BaseHealth");
            useEnduranceBonus = settings.GetValue<bool>("StartingHealth", "EnduranceBonus");
            extraStartHealthMultiplier = settings.GetValue<float>("StartingHealth", "BonusMultiplier");
            //new attack
            newAttack = settings.GetValue<bool>("Features", "AttackCalculation");
            allowCrits = settings.GetValue<bool>("AttackCalculation", "AllowCrits");
            playerCritDamage = settings.GetValue<float>("AttackCalculation", "PlayerCritDamageMultiplier");
            playerCritToHit =settings.GetValue<float>("AttackCalculation", "PlayerHitChanceMultiplier");
            enemyCritDamage =settings.GetValue<float>("AttackCalculation", "EnemyCritDamageMultiplier");
            enemyCritToHit =settings.GetValue<float>("AttackCalculation", "EnemyHitChanceMultiplier");
            materialReduction = settings.GetValue<bool>("AttackCalculation", "MaterialDamageReduction");
            materialDamageMultiplier = settings.GetValue<float>("AttackCalculation", "MaterialDamageMultiplier");
            newEnemyDamageMod = settings.GetValue<bool>("AttackCalculation", "MoreEnemyDamageModifications");
            //To Hit Adjustments
            hitAdjustments = settings.GetValue<bool>("Features", "ToHitAdjustments");
            toHitPlayer = settings.GetValue<int>("Features", "BonusChanceToHitPlayer");
            toHitMonster = settings.GetValue<int>("Features", "BonusChanceToHitMonster");
            //Weapon to hit
            newWeaponToHitMultiplier = settings.GetValue<bool>("Features", "WeaponToHitMultiplier");
            weaponToHitMultiplier = settings.GetValue<float>("WeaponToHitMultiplier", "MaterialToHitMultiplier"); 
            //Adjust SP
            adjustSP = settings.GetValue<bool>("Features", "AdjustSpellPoints");
            baseSP = settings.GetValue<int>("AdjustSpellPoints", "BaseSpellPoints"); 
            spellPointMultiplier = settings.GetValue<float>("AdjustSpellPoints", "SpellPointsMultiplier");
            
            //Other
            newHealingRate = settings.GetValue<bool>("Features", "EnduranceHealingRate");
            newHPBonus = settings.GetValue<bool>("Features", "EnduranceHealthBonus");           
            newToHit = settings.GetValue<bool>("Features", "AgilityToHitMod");
            newMagicResist = settings.GetValue<bool>("Features", "WillpowerMagicResist");
            governingAttributes = settings.GetValue<bool>("Features", "GoverningAttributes");


        }
        #endregion
        #region Start
        void Start()
        {
            //Geometric attribute points
            if (newAP)
            {
                RegisterOverride<Func<int>>(mod, "BonusPool", () =>
                {
                    float apLevel = Mathf.Round(apGoal * 0.95f * Mathf.Pow(0.05f, playerEntity.Level));
                    Debug.Log(apLevel);

                    int minBonusPool = (int)Mathf.Max(apLevel-apLoss, 1);        // The minimum number of free points to allocate on level up
                    int maxBonusPool = (int)Mathf.Max(apLevel, 1);    // The maximum number of free points to allocate on level up

                    // Roll bonus pool for player to distribute
                    // Using maxBonusPool + 1 for inclusive range
                    UnityEngine.Random.InitState(Time.frameCount);
                    return UnityEngine.Random.Range(minBonusPool, maxBonusPool + 1);                    
                });

            }
            //More starting health
            if (newHP)
            {
                RegisterOverride<Func<PlayerEntity, int>>(mod, "RollMaxHealth", (player) => {
                    int baseHealth = baseHealthAmount;
                    int hpBonus = useEnduranceBonus ? FormulaHelper.HitPointsModifier(player.Stats.PermanentEndurance) : 0;
                    int maxHealth = (int)(baseHealth + extraStartHealthMultiplier * ( player.Career.HitPointsPerLevel + hpBonus ));

                    for (int i = 1; i < player.Level; i++)
                    {
                        maxHealth += CalculateHitPointsPerLevelUp(player);
                    }

                    return maxHealth;
                });
            }
            //SpellPoints
            if (adjustSP)
            {
                RegisterOverride<Func<int, float, int>>(mod, "SpellPoints", (intelligence, multiplier) =>
                {
                    return baseSP + (int)Mathf.Floor((float)intelligence * multiplier * spellPointMultiplier);
                });
            }
            //Agility
            if (newToHit)
            {
                RegisterOverride<Func<int, int>>(mod, "ToHitModifier", (agility) => {
                    return 2 * ((int)Mathf.Floor((float)agility / 5f) - 10);
                });
                RegisterOverride<Func<DaggerfallEntity, DaggerfallEntity, int>>(mod, "CalculateStatDiffsToHit", (attacker, target) => {
                    
                    int chanceToHitMod = 0;

                    // Apply luck modifier.
                    chanceToHitMod += (attacker.Stats.LiveLuck - target.Stats.LiveLuck) / 5;

                    // Apply agility modifier.
                    chanceToHitMod += 2 * (attacker.Stats.LiveAgility - target.Stats.LiveAgility) / 5;

                    return chanceToHitMod;
                });
            }
            //Healing Rate
            if (newHealingRate)
            {
                RegisterOverride<Func<int, int>>(mod, "HealingRateModifier", (endurance) => {
                    return (int)Mathf.Floor((float)endurance / 5f) - 10;
                });
            }
            //Magic resist
            if (newMagicResist)
            {
                RegisterOverride<Func<int, int>>(mod, "MagicResist", (willpower) => {
                    return (int)Mathf.Floor((float)willpower / 5f) - 10;
                });
            }
            //Endurance Bonus
            if (newHPBonus)
            {
                //from -10 to 10 or (hitpoints per level)/2 if higher than 10
                RegisterOverride<Func<int, int>>(mod, "HitPointsModifier", (endurance) => {
                    return (int)Mathf.Floor(((float)endurance / 50f - 1f) * Mathf.Max((float)playerEntity.Career.HitPointsPerLevel / 2f, 10f));
                });
            }
            
            //Weapon to hit
            if (newWeaponToHitMultiplier)
            {
                RegisterOverride<Func<DaggerfallUnityItem, int>>(mod, "CalculateWeaponToHit"
                , (weapon) => {
                    return (int)Mathf.Floor(weapon.GetWeaponMaterialModifier() * weaponToHitMultiplier);
                });
            }
            //Attack Roll calculations
            if (newAttack)
            {
                NewAttack();
            }
            
            //Use governing attributes
            if (governingAttributes)
            {
                RegisterOverride<Func<int, int, float, int, int>>(mod, "CalculateSkillUsesForAdvancement"
                , (skillValue, skillAdvancementMultiplier, careerAdvancementMultiplier, level) => {
                    //get stat relaed to skillID wrap at playerEntity.SkillUses.Length
                    DFCareer.Stats skillStat = DaggerfallSkills.GetPrimaryStat(currentSkill);
                    currentSkill = ( currentSkill + 1 ) % playerEntity.SkillUses.Length;

                    float statModifier = 50f / playerEntity.Stats.GetLiveStatValue(skillStat);
                    double levelMod = Math.Pow(1.04, level);
                    return (int)Math.Floor((statModifier * skillValue * skillAdvancementMultiplier * careerAdvancementMultiplier * levelMod * 2 / 5) + 1);
                });
            }
            //Adjustments to Hit
            if (hitAdjustments)
                RegisterOverride<Func<DaggerfallEntity, DaggerfallEntity, int>>(mod, "CalculateAdjustmentsToHit" 
                , (attacker, target) => {
                    PlayerEntity player = GameManager.Instance.PlayerEntity;
                    EnemyEntity AITarget = target as EnemyEntity;

                    int chanceToHitMod = 0;

                    // Apply hit mod from character biography
                    if (target == player)
                    {
                        chanceToHitMod -= player.BiographyAvoidHitMod;
                        chanceToHitMod += toHitPlayer;
                    }

                    // Apply monster modifier.
                    if ((target != player) && (AITarget.EntityType == EntityTypes.EnemyMonster))
                    {
                        chanceToHitMod += toHitMonster;
                    }

                    // DF Chronicles says -60 is applied at the end, but it actually seems to be -50.
                    chanceToHitMod -= 50;
                    
                    return chanceToHitMod;
                });
        }
        #endregion

        #region Methods Attack
        private void NewAttack()
        {
            //damage
            RegisterOverride<Func<DaggerfallEntity, DaggerfallEntity, bool, int, DaggerfallUnityItem, int>>(mod, "CalculateAttackDamage"
            , (attacker, target, isEnemyFacingAwayFromPlayer, weaponAnimTime, weapon) => {
                bool isPlayer = attacker == playerEntity;
                EnemyEntity AIAttacker = attacker as EnemyEntity;
                short skillID = (short)DFCareer.Skills.HandToHand;
                int damage = 0;
                //Select higest damage source
                weapon = WeaponSelect(AIAttacker, weapon);
                // Apply mods
                ToHitAndDamageMods attackMods = new ToHitAndDamageMods
                {
                    damageMod = 0,
                    toHitMod = 0
                };
                attackMods = isPlayer ? ApplyPlayerMods(attacker, isEnemyFacingAwayFromPlayer, weapon) : attackMods;

                if (weapon == null)
                {             
                    attackMods.toHitMod += attacker.Skills.GetLiveSkillValue(skillID);
                    //Damage
                    damage = DealDamageNoWeapon(attacker, target, attackMods, isPlayer);
                }
                else
                {
                    skillID = weapon.GetWeaponSkillIDAsShort();
                    attackMods.toHitMod += attacker.Skills.GetLiveSkillValue(skillID);

                    //Weapon to Hit
                    attackMods.toHitMod += CalculateWeaponToHit(weapon);
                    // Mod hook for adjusting final hit chance mod and adding new elements to calculation. (no-op in DFU)
                    attackMods.toHitMod += AdjustWeaponHitChanceMod(attacker, target, attackMods.toHitMod, weaponAnimTime, weapon);
                    //Damage
                    damage = DealDamage(attacker, target, weaponAnimTime, weapon, attackMods);
                }
                ApplyRingOfNamira(attacker, target, damage);
                return damage;
            });

            RegisterOverride<Func<DaggerfallEntity, DaggerfallEntity, int, int, DaggerfallUnityItem, int>>(mod, "CalculateWeaponAttackDamage"
            , (attacker, target, damageModifier, weaponAnimTime, weapon) => {

                int damage = UnityEngine.Random.Range(weapon.GetBaseDamageMin(), weapon.GetBaseDamageMax() + 1) + damageModifier;

                damage = newEnemyDamageMod ? RedoneEnemyDamageMods(target, damage, weapon) : OriginalEnemyDamageMods(target, damage, weapon);
                // TODO: Apply strength bonus from Mace of Molag Bal

                // Apply strength modifier
                damage += DamageModifier(attacker.Stats.LiveStrength);

                // Apply material modifier.
                // The in-game display in Daggerfall of weapon damages with material modifiers is incorrect. The material modifier is half of what the display suggests.
                damage += (int)(weapon.GetWeaponMaterialModifier() * materialDamageMultiplier);
                if (damage < 1)
                    damage = 0;

                damage += GetBonusOrPenaltyByEnemyType(attacker, target);

                // Mod hook for adjusting final weapon damage. (no-op in DFU)
                damage = AdjustWeaponAttackDamage(attacker, target, damage, weaponAnimTime, weapon);

                return damage;   
            });
            
        }
        private int RedoneEnemyDamageMods(DaggerfallEntity target, int damage, DaggerfallUnityItem weapon) 
        {
            if (target != playerEntity)
            {
                //changed to all undead
                if (GetEnemyEntityEnemyGroup(target as EnemyEntity) ==  DFCareer.EnemyGroups.Undead)
                {
                    // Apply silver weapon damage modifier for Undead
                    // Arena applies a silver weapon damage bonus for undead enemies, which is probably where this comes from.
                    if (weapon.NativeMaterialValue == (int)WeaponMaterialTypes.Silver)
                        damage *= 2;

                    //reduced weapon damage to ghost and wraith if NOT silver or magic item
                    if ((target as EnemyEntity).CareerIndex == (int)MonsterCareers.Ghost || (target as EnemyEntity).CareerIndex == (int)MonsterCareers.Wraith)
                    {
                        if (weapon.NativeMaterialValue != (int)WeaponMaterialTypes.Silver || !weapon.IsEnchanted)
                            damage /= 3;
                    } 
                        
                    // Apply edged-weapon damage modifier for Skeletal Warrior 
                    if ((weapon.flags & 0x10) == 0)
                        damage /= 2;    
                }

            }
            return damage;
        }

        private int OriginalEnemyDamageMods(DaggerfallEntity target, int damage, DaggerfallUnityItem weapon)
        {
            if ((target as EnemyEntity).CareerIndex == (int)MonsterCareers.SkeletalWarrior)
            {
                // Apply edged-weapon damage modifier for Skeletal Warrior
                if ((weapon.flags & 0x10) == 0)
                    damage /= 2;

                // Apply silver weapon damage modifier for Skeletal Warrior
                // Arena applies a silver weapon damage bonus for undead enemies, which is probably where this comes from.
                if (weapon.NativeMaterialValue == (int)WeaponMaterialTypes.Silver)
                    damage *= 2;
            }
            return damage;
        }
        private DaggerfallUnityItem WeaponSelect(EnemyEntity attacker, DaggerfallUnityItem weapon)
        {           
            if (attacker != null && weapon != null)
            {
                int weaponAverage = (weapon.GetBaseDamageMin() + weapon.GetBaseDamageMax()) / 2;
                int noWeaponAverage = (attacker.MobileEnemy.MinDamage + attacker.MobileEnemy.MaxDamage) / 2;

                if (noWeaponAverage > weaponAverage)
                {
                    // Use hand-to-hand
                    weapon = null;
                }
            }
            return weapon;
        }

        //soften requirements
        private float MaterialCheck(DaggerfallEntity target, DaggerfallUnityItem weapon, bool isPlayer) 
        {
            // If the attacker is using a weapon, check if the material is high enough to damage the target
            float damageRed = 1;
            int compareMaterial = (int)target.MinMetalToHit - weapon.NativeMaterialValue;
            if (compareMaterial > 0)
            {
                    if (materialReduction)
                    {
                        damageRed = 3f / ( 4f * compareMaterial );
                    } 
                    else
                    {
                        damageRed = 0; 
                    }                      
                    if (isPlayer)
                    {
                        DaggerfallUI.Instance.PopupMessage(TextManager.Instance.GetLocalizedText("materialIneffective"));
                    }
            }
            return damageRed;
        }

        //apply player specific toHit and damage
        private ToHitAndDamageMods ApplyPlayerMods(DaggerfallEntity attacker, bool isEnemyFacingAwayFromPlayer, DaggerfallUnityItem weapon)
        {
            ToHitAndDamageMods mods = new ToHitAndDamageMods();

            ToHitAndDamageMods mods1 = CalculateSwingModifiers(GameManager.Instance.WeaponManager.ScreenWeapon);
            ToHitAndDamageMods mods2 = CalculateProficiencyModifiers(attacker, weapon);
            ToHitAndDamageMods mods3 = CalculateRacialModifiers(attacker, weapon, playerEntity);
            int modsBC = CalculateBackstabChance(playerEntity, null, isEnemyFacingAwayFromPlayer);
            
            mods.damageMod = CalculateBackstabDamage(mods1.damageMod+mods2.damageMod+mods3.damageMod, modsBC);
            mods.toHitMod = mods1.toHitMod+mods2.toHitMod+mods3.toHitMod;
            
            return mods;
                                        
        }

        //weapon normal damage
        private int DealDamage(DaggerfallEntity attacker, DaggerfallEntity target, int weaponAnimTime, DaggerfallUnityItem weapon, ToHitAndDamageMods attackMods)
        {
            int struckBodyPart = CalculateStruckBodyPart();
            if (CalculateSuccessfulHit(attacker, target, attackMods.toHitMod, struckBodyPart))
            {
                attackMods.damageMod += CalculateWeaponAttackDamage(attacker, target, attackMods.damageMod, weaponAnimTime, weapon);
                //Crit
                attackMods.damageMod += CalculateSuccessfulCrit(attacker, target, weapon);
                //resist from armor and endurance
                Debug.Log(attacker.Name + " attack before resist " + attackMods.damageMod);
                //player has a base ac of +100
                attackMods.damageMod += CalculateResistance(target, struckBodyPart);
                // Handle poisoned weapons
                if (weapon.poisonType != Poisons.None)
                {
                    InflictPoison(attacker, target, weapon.poisonType, false);
                    weapon.poisonType = Poisons.None;
                }

                //Damage reduction from too low material
                float materialRed = MaterialCheck( target, weapon, attacker == playerEntity);
                attackMods.damageMod = (int)Mathf.Floor(attackMods.damageMod * materialRed);

                if (attackMods.damageMod <= 0)
                    attackMods.damageMod = 1;
                Debug.Log(attacker.Name + " attack for " + attackMods.damageMod);
            }
            else
            {
                Debug.Log(attacker.Name + " missed " + target.Name);
            }
            DamageEquipment(attacker, target, attackMods.damageMod, weapon, struckBodyPart);
            return attackMods.damageMod;
        }

        //no weapon normal damage
        private int DealDamageNoWeapon(DaggerfallEntity attacker, DaggerfallEntity target, ToHitAndDamageMods attackMods, bool isPlayer)
        {
            EnemyEntity AIAttacker = attacker as EnemyEntity;
            int struckBodyPart = CalculateStruckBodyPart();
            int damage = attackMods.damageMod;
            if (isPlayer || AIAttacker.EntityType == EntityTypes.EnemyClass)
            {
                if (CalculateSuccessfulHit(attacker, target, attackMods.toHitMod, struckBodyPart))
                {
                    damage += CalculateHandToHandAttackDamage(attacker, target, attackMods.damageMod, isPlayer);
                    damage += CalculateSuccessfulCrit(attacker, target, null);
                    //resist from armor and endurance
                    damage += CalculateResistance(target, struckBodyPart);
                    if (damage <= 0)
                        damage = 1;
                }
                else
                {
                    Debug.Log(attacker.Name + " missed " + target.Name);
                }
            }
            else // attacker is a monster
            {
                // Handle multiple attacks by AI
                int minBaseDamage = 0;
                int maxBaseDamage = 0;
                int attackNumber = 0;

                while (attackNumber < 3) // Classic supports up to 5 attacks but no monster has more than 3
                {
                    if (attackNumber == 0)
                    {
                        minBaseDamage = AIAttacker.MobileEnemy.MinDamage;
                        maxBaseDamage = AIAttacker.MobileEnemy.MaxDamage;
                    }
                    else if (attackNumber == 1)
                    {
                        minBaseDamage = AIAttacker.MobileEnemy.MinDamage2;
                        maxBaseDamage = AIAttacker.MobileEnemy.MaxDamage2;
                    }
                    else if (attackNumber == 2)
                    {
                        minBaseDamage = AIAttacker.MobileEnemy.MinDamage3;
                        maxBaseDamage = AIAttacker.MobileEnemy.MaxDamage3;
                    }

                    int hitDamage = 0;
                    if (minBaseDamage > 0 && CalculateSuccessfulHit(attacker, target, attackMods.toHitMod, struckBodyPart))
                    {
                        hitDamage = UnityEngine.Random.Range(minBaseDamage, maxBaseDamage + 1);
                        // Apply special monster attack effects
                        if (hitDamage > 0)
                            OnMonsterHit(AIAttacker, target, hitDamage);

                        damage += hitDamage;
                    }
                        else
                    {
                        Debug.Log(attacker.Name + " missed " + target.Name);
                    }

                    // Apply bonus damage only when monster has actually hit, or they will accumulate bonus damage even for missed attacks and zero-damage attacks
                    if (hitDamage > 0)
                    {
                        // monsters get strength damage now too
                        damage += DamageModifier(attacker.Stats.LiveStrength);

                        damage += GetBonusOrPenaltyByEnemyType(attacker, target);
                        damage += CalculateSuccessfulCrit(attacker, target, null);
                        damage += CalculateResistance(target, struckBodyPart);
                        if (damage < 0)
                            damage = 1;
                        Debug.Log(attacker.Name + " attack for " + damage);
                    }
                    ++attackNumber;
                }
            }
            DamageEquipment(attacker, target, attackMods.damageMod, null, struckBodyPart);
            return damage;
        }
        //resistance calc from AC and END
        private int CalculateResistance(DaggerfallEntity target, int struckBodyPart)
        {
                //player has a base ac of +100 enemies are AC *5
                int armorResist = target != playerEntity ? (int) Mathf.Floor(CalculateArmorToHit(target, struckBodyPart) / 10f )
                                : (int) Mathf.Floor( ( CalculateArmorToHit(target, struckBodyPart) - 100 ) / 10f );

                Debug.Log(target.Name + " armor Resist " + armorResist);
                int enduranceResist = (int)Mathf.Floor(( target.Stats.LiveEndurance - 50 ) / 10f);
                Debug.Log(target.Name + " endurance Resist " + enduranceResist);
                return armorResist - enduranceResist;
        }
        //checks if critical and returns damage
        private int CalculateSuccessfulCrit(DaggerfallEntity attacker, DaggerfallEntity target, DaggerfallUnityItem weapon)
        {
            if (!allowCrits)
                return 0;
            bool isPlayer = attacker == playerEntity;
            int damage = 0;
            int critSkill = attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike);
            int weaponDamage;
            float damageMultiplier;
            float toHitMultiplier;
            int weaponToHit = -2;
            int weaponSkill = -99;

            if (isPlayer)
            {
                damageMultiplier = playerCritDamage;
                toHitMultiplier = playerCritToHit;
            }
            else
            {
                damageMultiplier = enemyCritDamage;
                toHitMultiplier = enemyCritToHit;
            }

            if (weapon == null)
            {
                weaponDamage = CalculateHandToHandMaxDamage(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.HandToHand));
            }
            else
            {
                weaponSkill = weapon.GetWeaponSkillUsed();
                weaponDamage = weapon.GetBaseDamageMax();
                weaponToHit = (int)Mathf.Round(-weapon.weightInKg);
            }
            
            int luckMod = (attacker.Stats.LiveLuck - target.Stats.LiveLuck) / 5;
            int skillMod = critSkill / 5;
            ToHitAndDamageMods weaponTypeMod = WeaponBaseToHit(weaponSkill);
            
            int critRoll = (int)(toHitMultiplier * ( luckMod + skillMod + weaponTypeMod.toHitMod + weaponToHit ));
            if(Dice100.SuccessRoll(critRoll))
            {
                
                damage = (int)(damageMultiplier * ( weaponTypeMod.damageMod + ( critSkill * (weaponDamage / 100f + 1/10f ) ) ));

                string critMessageString = isPlayer ? "You deal a critical hit." : attacker.Name + "deals a critical hit.";
                DaggerfallUI.Instance.PopupMessage(critMessageString);
            }
            return damage;
        }

        //base values per weapontype for critical hits
        private ToHitAndDamageMods WeaponBaseToHit( int type)
        {
            ToHitAndDamageMods r = new ToHitAndDamageMods();

            switch (type)
            {
                case (int)DaggerfallConnect.DFCareer.ProficiencyFlags.ShortBlades:
                    r.toHitMod = 5;
                    r.damageMod = 8;                   
                    return r;
                case (int)DaggerfallConnect.DFCareer.ProficiencyFlags.LongBlades:
                    r.toHitMod = -2;
                    r.damageMod = 2;                   
                    return r;
                case (int)DaggerfallConnect.DFCareer.ProficiencyFlags.Axes:
                    r.toHitMod = 6;
                    r.damageMod = 2;                   
                    return r;
                case (int)DaggerfallConnect.DFCareer.ProficiencyFlags.BluntWeapons:
                    r.toHitMod = 2;
                    r.damageMod = 6;                   
                    return r;
                case (int)DaggerfallConnect.DFCareer.ProficiencyFlags.MissileWeapons:
                    r.toHitMod = -4;
                    r.damageMod = 8;                   
                    return r;

                default:
                    r.toHitMod = -2;
                    r.damageMod = 4;                   
                    return r;
            }
        }
        private void ApplyRingOfNamira(DaggerfallEntity attacker, DaggerfallEntity target, int damage) 
        {
            if (target == playerEntity)
            {
                DaggerfallUnityItem[] equippedItems = target.ItemEquipTable.EquipTable;
                DaggerfallUnityItem item = null;
                if (equippedItems.Length != 0)
                {
                    if (IsRingOfNamira(equippedItems[(int)EquipSlots.Ring0]) || IsRingOfNamira(equippedItems[(int)EquipSlots.Ring1]))
                    {
                        IEntityEffect effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(RingOfNamiraEffect.EffectKey);
                        effectTemplate.EnchantmentPayloadCallback(EnchantmentPayloadFlags.None,
                            targetEntity: attacker.EntityBehaviour,
                            sourceItem: item,
                            sourceDamage: damage);
                    }
                }
            }
        }
        //reimplented beacause original is private
        private bool IsRingOfNamira(DaggerfallUnityItem item)
        {
             return item != null && item.ContainsEnchantment(DaggerfallConnect.FallExe.EnchantmentTypes.SpecialArtifactEffect, (int)ArtifactsSubTypes.Ring_of_Namira);
        }
        #endregion
    }
}
