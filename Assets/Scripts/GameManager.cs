using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool[] chamberOrder;
    public int chamberSize = 6;
    [HideInInspector] public int indexInChamber = 0;

    public int amountOfTimesPossibleToRespin = 2;
    [HideInInspector] public int timesLeftToRespinPlayer = 0;
    
    public int amountOfTimesPossibleToShootOpponent = 1;
    [HideInInspector] public int timesLeftToShootOpponent = 0;

    [HideInInspector] public bool isPlaying = false;
    private int turnIndex = 0;

    public EnemyAI enemyAI;
    

    void Start()
    {
        InitializeGame();
    }


    void InitializeGame()
    {
        // NOTE :
        // Create new chamber and place bullet
        ReshuffleChamber();
        
        // NOTE :
        // Init abilities/options
        timesLeftToRespinPlayer = amountOfTimesPossibleToRespin;
        timesLeftToShootOpponent = amountOfTimesPossibleToShootOpponent;
        enemyAI.timesLeftToRespin = amountOfTimesPossibleToRespin;
        enemyAI.timesLeftToShootPlayer = amountOfTimesPossibleToShootOpponent;
        
        // NOTE : 
        // Initialize player/enemy 
        turnIndex = 0 - (Random.Range(0, 11) % 2);
        isPlaying = true;
        ManageTurn();
    }


    void Update()
    {
        ManageTurn();
    }
    
    
    void ManageTurn()
    {
        if (!isPlaying) return;
        
        if (turnIndex % 2 == 0) {
            HandlePlayerTurn();
        } else {
            enemyAI.HandleEnemyAI();
        }
    }


    void HandlePlayerTurn()
    {
        bool isInput = false;

        if (Input.GetKeyDown(KeyCode.Q)){
            // TODO
            // Connect With Wagers / Scoring --> Bet 1/2 Pot To Shoot After 0
            if (timesLeftToShootOpponent > 0){
                ShootOpponent();
                isInput = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.W)){
            // TODO
            // Connect With Wagers / Scoring --> Bet 1/2 Pot To Respin After 0
            if (timesLeftToRespinPlayer > 0) {
                RespinChamber(ref timesLeftToRespinPlayer);
                isInput = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            ShootSelf();
            isInput = true;
        }
        
        if (!isInput) return; 
        
        SwitchTurn();
        Debug.Log($"{indexInChamber+1}/{chamberSize}");
    }


    public void RespinChamber(ref int timesLeftToRespin)
    {
        timesLeftToRespin--;
        ReshuffleChamber();
    }
    
    
    void ShootSelf()
    {
        if (GetBulletInChamber()) {
            isPlaying = false;
            Debug.Log("You shot yourself\nPLAYER LOSES!");
        } else {
            Debug.Log("You Shot a Blank at Yourself");
            indexInChamber++;
        }
        
    }
    

    void ShootOpponent()
    {
        timesLeftToShootOpponent--;
        if (GetBulletInChamber()) {
            isPlaying = false;
            Debug.Log("Opponent Was Shot!\nPLAYER WINS!");
        } else {
            Debug.Log("You Shot a Blank at the Enemy");
            indexInChamber++;
        }
    }
    

    public void SwitchTurn()
    {
        turnIndex++;
    }


    public bool GetBulletInChamber()
    {
        return chamberOrder[indexInChamber];
    }
    
    
    public void ReshuffleChamber()
    {
        chamberOrder = new bool[chamberSize];
        int chamberIndexToPlaceLive = Random.Range(0, chamberSize);
        for (int i = 0; i < chamberSize; i++) {
            chamberOrder[i] = false; }
        chamberOrder[chamberIndexToPlaceLive] = true;
        indexInChamber = 0;
    }
}
