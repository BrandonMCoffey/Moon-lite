*This is the Planning Page for the Game for the 05/16 Alpha Release*
# Planning

## 1. Main Menu
- [TASK] UI Sounds

## 2. The Ship
- [TASK] NPC Art
- [TASK] How To Art (WASD to Move, LClick to Shoot, etc)
- [TASK] Ship Background
- [TASK] Teleporter Art
- [TASK] Make Copper Fast Bullet
- [TASK] Make Research and Equip System
- [TASK] Make Stars Whiz By

## 3. Moon 1
- [TASK] Correctly use the right TileMap
- [TASK] Enemy Art
- [TASK] Set Up Inventory Navigation (Multiple Ways to Navigate)
- [TASK] Playtest Playtest Playtest
- [TASK] In Pause Quest Menu
- [TASK] Quick Dialogue Menu for Telling Player What To Do
- [TASK] Replace TextMeshProFont with **monogram-extended SDF** font in Assets/Font folder
- [TASK] Fix Enemies
- [TASK] Flashlight is dope as well, please enable it
- [TASK] Mess with Lighting
- [TASK] Teleport out When Low on Health
- [TASK] Make Bullets Do what they say
- [TASK] Mess with Laser and Lighting Line Renderer effects in the Bullet Manager
- [TASK] Really stupid and basic Dialogue System
- [TASK] Dialogue System Art (Really Basic)

## Bullets
- Copper     ->  Fast Less Damage
- Gravislag  ->  Basic Bullet
- Cryselium  ->  Freeze Enemies (call SetFreezeForDuration() on EnemyBrain Component)
- Voltthorn  ->  Chain Lighting (Use spherecast and set to the enemy layer with a little bit of delay)
- Phaseum    ->  Phases through walls and pierces enemies
- Pyrokite   ->  Flame Thrower with Damage over Time
- Aethergem  ->  Breaks blocks and pierces enemies
- Corestone  ->  Heavy bullet, bouncy with damage
- Explosive  ->  Fast AOE with no Damage Over Time
- Moon       ->  Light, floaty, slow, with really really heavy knockback