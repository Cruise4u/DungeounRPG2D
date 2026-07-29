# DungeounRPG2D

2D merge-combat auto-battler (Unity, URP 2D). The player spends **tokens** on a prep grid to spawn/merge units, then presses **Fight** — battles are fully automatic, real-time, watched not played. Each round is a battle to the death; losing ends the match.

## Screen layout

- **CombatZone** (top): where battles actually happen. Both teams have spawn/battle point transforms inside it.
- **4×4 prep grid** (bottom): player-only staging area for spawning and (future) merging. Units are teleported from here to the CombatZone when Fight is pressed. Combat never happens on the grid.

## Architecture — current (use this)

I'm going to work with you on a step-by-step basis where at each milestone there will be multiple phases : 


Ideation phase (This is where you think about what you'll do and how) -> If needed make questions if uncertain about the task. Never assume anything unless explicitly mentioned.

Build phase (This is where you'll code and make sure there's no errors)

Testing phase (This is the moment you'll wait i verify the results, help you organize and manage your work and decide either to move to the next milestone or not).



Rules : 

Don't destroy any existing code. If needed write a new script on it or class/function etc. 
Before moving to the next phase, prompt me to check it works and i can test it, look at the code,etc. See if the direction is that.


