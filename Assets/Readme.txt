Author:		Micah Trent
Updated: 		12-9-2022

How to Play:
-To move around, you can use the WASD keys or the arrow keys.  
-To cancel a movement and stand still, tap the left shift key.  
-The Q and E keys rotate the camera around the player by 90 degrees.  One tap of the Q key will rotate the camera 90 degrees to the left.  Likewise, one tap of the E key will rotate the camera 90 degrees to the right.  Taps can be consecutive if so desired.  
-The forward direction of the camera is also the forward direction of the player.  Thus, tapping the W key will always move the player "away from" the camera no matter the orientation of the camera.  The same applies to ASD and the arrow keys.
-Bumping into (or walking onto) an enemy will attack them.  The enemy which you are attacking cannot attack you.
-Bumping into chests will destroy them and give you a weapon upgrade as well as some XP which can be used to recover health.
-Bumping into the exit portal will give you a prompt to move onto the next level (which you can decline if you so desire).
-Pressing the escape key or clicking the pause button on the bottom right of the screen will pause the game.  From here you can exit to the main menu by clicking on the button to the bottom left or regain health by clicking on the button at the bottom center of the screen if it appears.
-When on the pause menu, clicking on the "green plus" button will recover all of your lost health at the cost of your xp.  If you do not have enough xp, you cannot recover health.  The XP requirement is calculated by 10 * the level number you are on.  Thus, if you are on level 5, it will cost you 50 XP to recover your health.
-While on the main menu, you have the options to either load a previous file or start a new one.  
-If starting a new one, type a username (max of 7 characters) and click the green plus sign to begin the game with your new profile.
-If loading a game, click on any save file names which you wish to play.  Clicking the red X next to them will delete them permanently.
-The game over screen displays the floor level you got to and the XP you were left with before you died.


The Current State of Game Features and Systems:

o Dungeon Floors: This feature is fully completed.  Rooms and hallways are all generated procedurally.  This includes the dimensions of rooms and the placement of hallways.

o Procedurally Generated Rooms: As stated above, this feature is fully implemented.

o Adaptive AI:  This feature is complete.  The enemy AI uses an agent on a navmesh in order to create an array of checkpoints heading towards their target.  Other small algorithms are used to avoid obstacles and perform precise movements at close range.
 
o Enemies: Only one enemy currently exists in the project; the skeleton.  I have focused the majority of my time on the AI, so implementing only one enemy is all that is necessary.  Adding other enemy types would not take long at all.

o Beat Based Movement: This feature has been fully implemented.  No further changes are required.

o Weapons:  I have revised the weapon system I originally developed.  I decided to keep it simple since that would best reflect the low poly design of the game.  The weapons in the game work on an upgrade system.  The weapon you have (or the enemy has) starts off with a certain base damage.  You can gain "weapon levels", however, as you progress through the game.  The weapon damage is incrmented by 1 for every 10 weapon levels gained.

o Obstacles: This feature is fully implemented.  Boxes block paths and force the player and enemies to find new ways through a room.  This helps with breaking up room monotony.

o Chests: This feature is completed.  Chests are technically a subtype of Obstacle, but act different.  If a player "bumps" into one, they are granted 5 weapon levels and a random amount of XP.  The chest is then destroyed.

o Player HUD: This feature is completed.  The HUD displays current health and current weapon level (represented by a bar) and damage (represented by digits).  The HUD also displays the current XP count at the top center.  A pause button is also included on the bottom right of the screen.

o Chunking: This feature is completed.  Since each floor can be broken up into "chambers", chambers/rooms/hallways as well their respective obstacle children objects are set to inactive when the player moves out of view of them.  They are set to active again when the player moves near again.

o Main Menu: This feature is completed.  The main menu allows users to either create a new profile or load an already created one.  Up to three profiles can be created.  The profiles can also be deleted.  Profiles store the username (7 characters max) of the user as well as current xp score and current floor level reached.)

o Data Permanence: This feature is completed.  User profiles are saved using JSON serialization.

o Game Over: This feature is completed.  The game over scene is simple with just the "Game Over" title, "Play Again" and "Main Menu" buttons, and text fields displaying the floor level reached and the most recent XP amount. 

o Pause Screen: This feature is completed.  If the escape key is tapped or the pause button in the bottom right of the screen is clicked, the pause screen will appear while playing the game.  You can unpause by tapping the escape key again or clicking on the pause button again.  The home button on the bottom left of the screen will send you to the main menu.  Additionally, if the green plus "+" sign appears on the bottom center of the page, you can click it in order to restore your full health by spending XP.  The XP is calculated by multiplying 10 by the current level.  Thus, level 7 will cost 70 xp to restore health.

o Spending XP: This feature is complete.  Since I was unable to include a skill tree in the game due to lack of time, I had to find another use for XP.  As such, XP can be used to restore health from the pause screen when applicable.

o Exit Portal: This feature is completed.  In order to clear a level, you must "bump" into the exit portal.  Doing so will display a prompt in order to verify that you would like to proceeed to the next level.

o Particle Effects: This is present in the game.  Whenever a player or enemy is attacked, particle effects are displayed.  Likewise, the exit portal has a constant stream of particle effects visible.

o Sound: This is present in the game.  Sound makes it presense through the consistent "beat" sound effect.

o Animation: This is present in the game.  Player and enemy movement is animated programatically through the use of lerp.  Likewise, the following camera and camera rotation area also animated using lerp.