This is a repository for my Daggerfall Unity Mods

## System overhaul
Changes most of formulas defining the RPG rules with defaults that I feel improve the game and allows the user to tweak them to their liking and to account for other mods. Disabling a feature will disable the override of the specified classes allowing other mods to take precedence while using still the other features. The defaults represent my take on balance as other mods changing similar things change balance in different ways, parts of this will still work and complement those if they don't change the classes listed in *potential conflicts* of each feature.
Will surface more options to the player and add most likly add more optional changes as time goes on.

### Attribute Points On Level
Changes the formula to geometric one and allows the user to set an attribute goal of the total amount of attribute points the player will get when their level gets high. Allows to choose the number of points below the formula the player may roll as minimum.
The new formula is: AttributeGoal * 0.95 * 0.05^playerEntity.Level

Potential Conflicts: Override the class BonusPool()

### More Starting Health
Change the health the player will start with.
**Base Health** is the players Base health its 25 in Daggerfall Unity. Default is to change it to 30 to reduce resting in early game and account for crits.
**Endurance Bonus** This will add the bonus health on level up based on permanent endurancefrom character creation to the starting health.
**Bonus Multiplier** multiplies the games default health bonus derived from class max hit points on level up and the endurance bonus by this value. 2 by deafult to reduce early game resting and to account for crits.

Potential Conflicts: Override the class RollMaxHealth()

### Adjust Spell Points 
**Base Spell Points** Let the user set a base amount of spellpoints similar to base health, not affected by intelligence or class. 30 by default to mirror health.
**Spell Points Multiplier** Allows the user to change the multiplier for spellpoints for all classes. Stacks multiplicativly with class modifier. 1.5 by default to make relying primarly on magic more viable and less expensive for the player (and reduce resting while allowing a slow regen if using Basic Magic Regen which i recommend)

Potential Conflicts: Override the class SpellPoints()

### Governing Attributes
This enables governing attributes listed for the skills in the game to actually do something. Governing attributes now change the level speed (amount of uses needed to level up) of the skill based on the current attribute points in the governing skill (50 / attribute level).

Potential Conflicts: Override the class CalculateSkillUsesForAdvancement()

### To hit adjustments
The game applies a hidden modifier at the end before the hit roll to increase the players chance to hit monsters. By default this is 40. The new default is 25 (**Bonus Chance To Hit Monster**)to compensate for agility and luck mattering more.
The setting also adds a -25 (**Bonus Chance To Hit Player**) modifier for enemies to hit the player to make the combat more intresting as less enemies will have 80+ hit chance so that their agility and luck matter more.

Potential Conflicts: Override the class CalculateAdjustmentsToHit()

### Weapon to hit
Simply changes the multipler for weapon materials to 1 (max +6) to make it a minor part of the hit roll instead of daggerfalls default 10 (max +60) which made it by far the most important factor. Allows user adjustment of the modifier **Material To Hit Multiplier**.

Potential Conflicts: Override the class CalculateWeaponToHit()

### Healing Rate
Change how endurance affects healing rate from -5/5 to -10/10 to make it matter more and allow the health to regain fully from a nights rest at good but not maxed endurance. Also follows a convention closer to Arena where every 5 points in an attribute matter. 

Potential Conflicts: Override the class HealingRateModifier()
### Magic Resist
Change how willpower affect spell resist from -5/5 to -10/10 to follow the convention and make it matter more.
Potential Conflicts: Override the class Magic Resist()

### AgilityToHitMod
Changes chance to hit bonus for agility from negligible -5/5 to -20/20 also changes lucks modification from -5/5 to -10/10. Both values are compared to the enemies values like in default Daggerfall Unity and the difference is the actual change to hit chance. Will most likely add the option to reverse the luck change later.
Potential Conflicts: Override the classes ToHitModifier() and CalculateStatsToHit() (also known as CalculateStatDiffsToHit)
### New Attack
By far the largest change. It changes most of the attack roll featuring critical hits, damage reducing resistance system, removed hard material requirments, and redone enemy special resistance.
**More Enemy Damage Modifications**  in the default Daggerfall Unity Skeletal warriors take double damage from silver and half damage from bladed weapons.
This setting changes that so all undead take double damage from silver (like Arena and Skyrim). Additonally Ghosts and Wraiths will take a third of the damage from all weapons except silver and magical items. Skeletal Warriors still take half damage from blade weapons
**Material Damage Reduction** Instead of being immune to weapons below a specific level the damage will be reduced by 3 / (4 * material levels Bellow Requirement)
**Allow Crits** This enables the chance to deal critical damage and allows you to scale the hit chance and damage for both enemies and the player separately. The base hit chance is  luckMod + skillMod + weaponTypeMod - weaponWeight where luckMod: (attackers Luk - target Luck) / 5, skillMod: critical hit skill / 5, weaponTypeMod: depends on weapon type varies from -4 to 6. weaponWeight: weight of the weapon rounded to the closest integer, -2 if not a weapon with weight.
Base damage is weaponTypeModDamage + critSkill * weaponMaxDamage / 100 + critSkill / 10, where weaponTypeModDamage varies between weapons from 2 to 8.
**MaterialDamageMultiplier** change material damage from 1 * modifer to 2 * modifer which is the damage displayed in classic Daggerfall. The multipler is adjustable in the settings.

Additionally a resistance system is introduced which reduce all incoming damage by (endurance - 50) / 10 - AC / 2, A negative AC is good in daggerfall like in older DnD versions so both endurance and AC reduce damage. Will make settings for this system in a later version.

Potential Conflicts: Override the class CalculateAttackDamage(), CalculateWeaponAttackDamage()** this will conflict with simlar mods and also Vanilla Combat Event Handler, would like to work on compability with Vanilla Combat Event Handler once I research it more.
