# Jump-Master
Jumping platform game with spring like physics for jump

# Intro

Game is based on the jumping machanisum where you need to pull the spring from player to apply required force on the player to jump on next platform. 

# Navigation
1) Clone or download this repo and Open project with Unity. Any unity version can be used but this project is build in 2022.3.9f1.
2) Open MainScene by searching on project tab search bar or from _MainGame/Scenes/MainScene.Unity.
3) Click on the "Play" button in unity editor to play the game.
4) Main menu will be shown, click on the Start button to begin the game.
5) Drag mouse or finger from the player (Blue Sphere) object to pull the spring attached to player and release it to make player jump.

There is main script is attached to the player, "PlayerController". 

# PlayerController.cs

Public Variables

1) gameData : A ScriptableObject file reference. GameData contains all the parameters required for spring force and layermask.
2) sprintLine : A LineRenderer reference to show as a spring when user drag.
3) wallTrans : Background wall transform reference to move with player. It is required to check user input.
4) platforms : A Collider array to store platforms gameobject. It is required to place platform to new location.
5) mainMenuPanel : Main Menu UI gameobject reference.
6) gameOverPanel : Game Over UI gameobject reference.
7) gamePlayPanel : Game Play UI gameobject reference.
8) ScoreText : Text UI gameobject reference to show Score on the UI.
9) gameOverCurrentScoreText : Text UI gameobject reference in the game over UI panel.
10) gameOverMaxScoreText : Text UI gameobject reference in the game over UI panel to show High Score.

Main Functions

1) Awake()
    Set up game play initialization.
   
2) Update()
     Used to check user input.

3) OnCollisionEnter(Collision c)
   Called when player collides with any platform collider. It checks for and validate player position on the platform if it meets the condition to stand on the platform.

4) OnCollisionStay(Collision c)
   Called every frame when player is on the platform collider. But only requires if OnCollisionEnter method is failed due to precision error.

5) SetPlayerOnPlatform()
   When player hits a platform and it is on valid position, This method is called from either OnCollisionStay or OnCollisionEnter to reset player gameobject so that user can start input again.

6) UpdateScoreUI()
   Update current user score on the UI.

7) GenerateNextPlatform()
   Generates new platform above player's current platform.

# GameData.cs

A ScriptableObject class to create game data scriptable object file. It contains necessary parameters to make game play.

Public Properties

1) minSpring : Minimum pull required for the spring to register pull.
2) maxSpring : Maximum pull value spring can be pulled.
3) springForce : Spring initial force.
4) playerMask : Layer mask added to player gameobject.
5) wallMask : Layer mask added to wall gameobject.
6) rayDistance : Raycast length to check when casting a ray from user input.

