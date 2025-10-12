using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public GameManager gameManager;

    [HideInInspector] public int timesLeftToRespin = 0;
    [HideInInspector] public int timesLeftToShootPlayer = 0;
    
    void ShootPlayer()
    {
        timesLeftToShootPlayer--;
        if (gameManager.GetBulletInChamber()) {
            Debug.Log("Enemy Shot You!\nPLAYER LOSES!");
            gameManager.isPlaying = false;
        } else {
            Debug.Log("Enemy Shot a Blank at You");
            gameManager.indexInChamber++;
        }
    }


    void ShootSelf()
    {
        if (gameManager.GetBulletInChamber()) {
            Debug.Log("Enemy Shot Themselves!\nPLAYER WINS!");
            gameManager.isPlaying = false;
        } else {
            Debug.Log("Enemy Shot a Blank at Themselves");
            gameManager.indexInChamber++;
        }
    }
    
    
    public void HandleEnemyAI()
    {
        bool isCalculatingMove = true;
        Debug.Log("Enemy AI Turn");
        // TODO 
        // Create actual calculative AI instead of randomness
        while (isCalculatingMove)
        {
            switch (Random.Range(1, 4) % 3)
            {
                case 0:
                    if (timesLeftToRespin > 0) {
                        gameManager.RespinChamber(ref timesLeftToRespin);
                        Debug.Log("Enemy Respun The Chamber");
                        isCalculatingMove = false;
                    }
                    break;
                case 1:
                    if (timesLeftToShootPlayer > 0) {
                        ShootPlayer();
                        isCalculatingMove = false;
                    }
                    break;
                case 2:
                    ShootSelf();
                    isCalculatingMove = false;
                    break;
            }
        }

        gameManager.SwitchTurn();
        Debug.Log($"{gameManager.indexInChamber+1}/{gameManager.chamberSize}");
    }
}
