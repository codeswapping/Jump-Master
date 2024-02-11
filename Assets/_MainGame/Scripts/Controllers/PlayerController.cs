using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    //Static variables
    public static bool isRestarted;             //To check if user has seleted restart option on game over panel.
#region Public Declarations
    [Header("Game Play")]
    public GameData gameData;                   // Game setting data reference.
    public LineRenderer springLine;             //Input feedback line.
    public Transform wallTrans;                 //Background wall transform reference.
    public Collider[] platforms;                //Platforms list where player will need to jump.

    [Header("UI Reference")]
    public GameObject mainMenuPanel;            // Main menu panel UI reference.
    public GameObject gameOverPanel;            // Game over panel UI reference.
    public GameObject gamePlayPanel;            // Game play panel UI reference.
    public TMPro.TextMeshProUGUI scoreText, gameOverCurrentScoreText, gameOverMaxScoreText; // Text references.
#endregion

#region Private Declarations
    // Player and environment settings
    private Rigidbody playerRb;                 //Reference to player Rigidbody
    private Camera mainCam;                     //Reference to main camera.
    private bool isPlayerSelected;
    private Vector3 camOffset, wallOffset;      //Camera and Background wall position offset

    //Platforms settings
    private bool isOnPlatform = true;           //flag to check if player is on the platform or not.
    private int currentPlatformIndex;           //Current platform player is standing on
    private float maxPlaformX;                  //Maximum X position for platform, influenced by Direction
    private float minPlatformY = 2;             //Minimum Y position for platform to appear.
    private float maxPlatformY;                 //Maximum Y position for platform to appear.
    private float gameOverYPos = -3;            //Game over if player falls below this Y position.


    //Game cycle variables
    private bool isStarted = false;             //Flag to set when game is started.
    private bool isFirst = true;                //Used to ingnore collision first time game is started.
    private bool isGameOver = false;            //Indicate if game is over.
    private int currentScore = 0;               //Store current Score.
    private int maxScore;                       //Last max score.
#endregion

#region Unity Methods
    private void Awake()
    {
        //Setup game scene

        maxScore = PlayerPrefs.GetInt("MaxScore", 0);                                   //Get Max Score.
        mainCam = Camera.main;                                                          //Get camera reference. Requires "MainCamera" tag on gameobject with Camera component is attached.
        playerRb = GetComponent<Rigidbody>();                                           //Get player rigidbody
        springLine.positionCount = 2;                                                   //Set position count for line to draw when your start input
        camOffset = transform.position - mainCam.transform.position;                    //Calculate camera offset.
        wallOffset = transform.position - wallTrans.position;                           //Calculate wall offset.

        //functionality to calculate minimum and maximum X and Y position to place platform
        var ray = mainCam.ScreenPointToRay(new Vector3(Screen.width,Screen.height,0));  
        if(Physics.Raycast(ray, out var hit, 100, gameData.wallMask))
        {
            maxPlaformX = hit.point.x / 2f;
            maxPlatformY = hit.point.y - 2f;
        }
        //Check if user has restarted the game.
        if(isRestarted)
        {
            OnStartButtonClick();
        }
    }    
    private void Update()
    {
        if(isGameOver) //If game is over return;
            return;
        if(!isStarted) //If game is not started return;
            return;
        mainCam.transform.position = transform.position - camOffset;    //Update Camera position.
        wallTrans.position = transform.position - wallOffset;           //Update BG wall position.
        if(isOnPlatform)                                                //If player is on platform, then check for inputs.
        {
            // If player is on the platform then reset rigidbody
            //Reset player velocity.
            playerRb.velocity = Vector3.zero;
            //Reset player angular velocity.
            playerRb.angularVelocity = Vector3.zero;

            //Check for input
            if(Input.GetMouseButtonDown(0))
            {
                //Cast a ray from screen point and check if it hits the player.
                var ray = mainCam.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, gameData.rayDistance, gameData.playerMask))
                {
                    //If ray hits player then set player selected flag to true.
                    isPlayerSelected = true;
                    springLine.enabled = true; //Enable draw line.
                }
            }
            else if(Input.GetMouseButton(0) && isPlayerSelected) //Check player mouse or finger movement.
            {
                var ray = mainCam.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, out var hitInfo, gameData.rayDistance, gameData.wallMask)) //This ray cast is checked against the wall to get position and direction user is moving his mouse or finger.
                {
                    var hitPoint = hitInfo.point;
                    //We need to set hitpoint z axis to same as player z axis.
                    hitPoint.z = transform.position.z;

                    var distance = Vector3.Distance(transform.position, hitPoint);
                    if(distance >= gameData.minSpring && distance <= gameData.maxSpring) //Check if distance is greater than minimum and less than maximum spring.
                    {
                        //Set Line renderer positions.
                        springLine.SetPosition(0, transform.position);
                        springLine.SetPosition(1, hitPoint);
                    }
                    else
                    {
                        if(distance > gameData.maxSpring) //If distance is greater than max spring, then clamp it to max.
                        {
                            var dir = transform.position - hitPoint;
                            dir.Normalize();
                            var nextPos = transform.position - dir * gameData.maxSpring;
                            springLine.SetPosition(1, nextPos);
                        }
                    }
                }
            }
            else if(Input.GetMouseButtonUp(0) && isPlayerSelected)
            {
                var ray = mainCam.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, out var hitInfo, gameData.rayDistance, gameData.wallMask))
                {
                    var hitPoint = hitInfo.point;
                    hitPoint.z = transform.position.z;

                    var distance = Vector3.Distance(transform.position, hitPoint);
                    if(distance >= gameData.minSpring)
                    {
                        distance = Mathf.Clamp(distance, gameData.minSpring, gameData.maxSpring);   //Clamp distance to max spring.
                        var dir = transform.position - hitPoint;                                    //Get direction to app spring force.
                        dir.Normalize();                                                            //Normalize direction.
                        var force = gameData.springForce * dir * distance;                          //Calculate force.
                        playerRb.AddForce(force, ForceMode.Impulse);                                //Apply force to rigidbody
                        platforms[currentPlatformIndex].enabled = false;                            //Set current platform collider disable to prevent player from landing on it again.
                        springLine.enabled = false;                                                 //Hide draw line.
                    }
                    //Reset player selected and on platform flag.
                    isPlayerSelected = false;               
                    isOnPlatform = false;
                }
            }
        }
        //If player is not on the platform, then check for the game over
        else if(transform.position.y < gameOverYPos)     
        {
            isGameOver = true;                                                      //Set game over flag.
            gameOverPanel.SetActive(true);                                          //Show gameover panel.
            gamePlayPanel.SetActive(false);                                         //Hide game play panel.
            gameOverCurrentScoreText.text = $"Current Score : {currentScore} ";     //Show current score.
            gameOverMaxScoreText.text = $"MaxScore : {maxScore}";                   //Show Max Score.
        }
    }

    private void OnCollisionEnter(Collision c)
    {
        if (c.contacts.Length > 0)
        {
            if(isFirst)
            {
                isFirst = false;
                return;
            }
            // Get the normal of the collision contact point
            Vector3 normal = c.contacts[0].normal;
            var xDiff = Mathf.Abs( Mathf.Abs(c.transform.position.x) - Mathf.Abs(transform.position.x));
            // Check if the collision is happening from above (e.g., normal pointing upwards) and xDiff is less than 1.98, if this condition is true, then we consider player is on the platform.
            if (normal.y > 0.5f && xDiff < 1.98f)
            {
                SetPlayerOnPlatform();
            }   
        }
    }

    private void OnCollisionStay(Collision c)
    {
        if(c.contactCount > 0)
        {
            Vector3 normal = c.contacts[0].normal;
            var xDiff = Mathf.Abs( Mathf.Abs(c.transform.position.x) - Mathf.Abs(transform.position.x));
            var pos = c.transform.position;
            //If player is on the platform, but condition is not satisfied because of any error, we will check it here again to stop game from stuck at this stage. 
            var onPlatform = normal.y >= 0.5f && xDiff < 1.98f;
            if(isOnPlatform)
            {
                if(!onPlatform)
                {
                    isOnPlatform = false;
                }
            }
            else 
            {
                if(isOnPlatform)
                {
                    SetPlayerOnPlatform();
                }
            }
        }
    }
#endregion

#region Private Methods

    /// <summary>
    /// Reset player parameters and set player on platform
    /// </summary>
    private void SetPlayerOnPlatform()
    {
        currentScore++;                             //Update Score.
        UpdateScoreUI();                            // Update score on UI.
        isOnPlatform = true;                        // Set player inOnPlatform flag to true.
        playerRb.velocity = Vector3.zero;           // Reset player velocity.
        playerRb.angularVelocity = Vector3.zero;    //Reset player angular velocity.
        GenerateNextPlatform();                     // Place next platform above.
    }
    /// <summary>
    /// Update UI
    /// </summary>
    private void UpdateScoreUI()
    {
        scoreText.text = $"Score : {currentScore}";
        if(currentScore > maxScore)
        {
            maxScore = currentScore;
            PlayerPrefs.SetInt("MaxScore", currentScore);
        }
    }
    /// <summary>
    /// Generate next platform position and enable it's collider.
    /// </summary>
    private void GenerateNextPlatform()
    {
        //Functionality to generate next platform
        var previous = currentPlatformIndex;                                                    //Store current index of platform.
        currentPlatformIndex ^= 1;                                                              //Swap platform index.
        
        int dir = (Random.Range(0, 2) == 0) ? -1 : 1;                                           //Whether to place next platform on right side or left side, -1 left, 1 right
        var currentX = platforms[currentPlatformIndex].transform.position.x;                    //Set current platform X in local variable.
        float xPos = Random.Range(1, maxPlaformX) * dir;                                        //Random X position for platform
        xPos = Mathf.Clamp(xPos, currentX - maxPlaformX, maxPlaformX + currentX);               //Clamp X position to be within max and min;
        var diff = Mathf.Abs(Mathf.Abs(xPos) - Mathf.Abs(currentX));                            //Check difference between new X position and current platform x position.
        if(diff < 5)                                                                            //If difference is less than 5, then calculate x position again to avoid next platform to place exactly about current platform.
            xPos += currentX + 4 * dir;
        
        float randomHeight = Random.Range(minPlatformY, maxPlatformY);                          //Random height for next platform.
        float yPos = randomHeight + platforms[currentPlatformIndex].transform.position.y;       //Calculate Y position for next platform.
        platforms[previous].transform.position = new Vector3(xPos,yPos);                        //Set position of next platform.
        gameOverYPos = transform.position.y - 3f;
        platforms[previous].enabled = true;          // Set gameover point position.
    }
#endregion

#region Public Methods
    public void OnStartButtonClick()
    {
        isStarted = true;
        playerRb.isKinematic = false;
        mainMenuPanel.SetActive(false);
        gamePlayPanel.SetActive(true);
    }
    public void OnRestartButtonClick()
    {
        isRestarted = true;
        SceneManager.LoadScene(0);
    }
    public void OnQuitButtonClick()
    {
        Application.Quit();
    }
#endregion
}
