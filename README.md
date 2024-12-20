This is a repository for my Daggerfall Unity Mods

## System overhaul
Changes most of formulas defining the RPG rules with defaults that I feel improve the game and allows the user to tweak them to their liking and to account for other mods.
Will surface more options to the player and add most likly add more optional changes as time goes on.

### Attribute Points On Level
Changes the formula to geometric one and allows the user to set an attribute goal of the total amount of attribute points the player will get when their level gets high. Allows to choose the number of points below the formula the player may roll as minimum.
The new formula is: AttributeGoal * 0.95 * 0.05^playerEntity.Level

Potential Conflicts: Override the class BonusPool()
### More Starting Health
Change the health the player will start with.
**BaseHealth** is the players Base health its 25 in Daggerfall Unity. Default is to change it to 30 to reduce resting in early game and account for crits.
**EnduranceBonus** This will add the endurance bonus health on level up from character creation to the starting health.
**BonusMultiplier** multiplies the games default health bonus derived from class max hit points on level up and the endurance bonus by this value. 2 by deafult to reduce early game resting and to account for crits.

Potential Conflicts: Override the class RollMaxHealth()

### Adjust Spell Points 
Let the user set a base amount of spellpoints similar to base health, not affected by intelligence or class. 30 by default to mirror health.
Allows the user to change the multiplier for spellpoints for all classes. Stacks multiplicativly with class modifier. 1.5 by default to make relying primarly on magic more viable and less expensive for the player (and reduce resting while allowing a slow reggen if using Basic Magic Regen)

Potential Conflicts: Override the class SpellPoints()
### Governing Attributes
This enables governing attributes as listed for the skills in the game to acctually do something. Governing attributes change the level speed (ammount of uses needed to level up) of the skill based on the attribute points in the governing skill (50 / attribute level).

Potential Conflicts: Override the class CalculateSkillUsesForAdvancement()
### To hit adjustments
The game applies a hidden modifier at the end before the hit roll to increase the players chance to hit monsters. By default this is 40. The new default is 25 to compensate for agility and luck mattering more.
The setting also adds a -25 modifier for enemies to hit the player to make the game more intresting as less enemies will have 80+ hit chance and make their agility and luck matter more.

### Weapon to hit
Simply changes the multipler for a weapons material to 1 to make it a minor part of the hit roll instead of daggerfalls default 10 which made it by far the most important factor. Allows user adjustment of the modifier.

### Healing Rate
Change how endurance affects healing rate from -5/5 to -10/10 to make it matter more and allow the health to regain fully from a nights rest at good but not maxed endurance. Also follows a convention closer to Arena 

### Magic Resist
Change how willpower affect spell resist from -5/5 to -10/10 to follow the convention and make it matter more.

### New Attack
By far the largest change. It changes most of the attack roll featuring critical hits, damage reducing resistance system, removed hard material requirments, and redone enemy special resistance.
