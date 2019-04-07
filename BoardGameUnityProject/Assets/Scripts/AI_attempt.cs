﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_attempt : MonoBehaviour
{
    //create static instance of ai_attempt
    private static AI_attempt _ai;
    private static bool aggressive; //used to hold if the AI is aggressive or not.

    public static AI_attempt ai
    {
        get
        {
            if (_ai != null)
                return _ai;
            else
            {
                Debug.Log("ai object is null");
                return null;
            }
        }
    }

    private void Awake()
    {
        _ai = this;
        //50% chance of AI being aggressive.
        if (Random.Range(0, 2) == 0) aggressive = false;
        else aggressive = true;

        //-------- TESTING ---------
        //aggressive always is true for testing... for now.
        aggressive = true;
        Debug.Log("AI Aggression is " + aggressive);
    }

    //class variable decleration
    int[] tileWeight = new int[3]; //used to save weights of updated characterLocation tiles.

    //computer's turn
    public void Comp_turn()
    {
        //variable dec.
        int[] characterLocations = new int[3]; //used only to check where characters will go. does NOT store current location.
        int diceRoll1, diceRoll2; //store dice rolls for rolling animation.
        int move = 0; 

        //grab character current location.
        for (int i = 0; i < 3; i++) {
            characterLocations[i] = ObjectHandler.Instance.player2Characters[i].GetComponent<Character>().GetCurrentTile();
            if (characterLocations[i] >= 78) characterLocations[i] = -3; //-3 means never pick it.
            //-1 is off the board, but isn't really a space. So the AI will look at the space it will actually land on.
            else if (characterLocations[i] == -1) characterLocations[i] = 0;
        }

        //start dice roll
        //generate diceroll for character movement.
        diceRoll1 = ObjectHandler.Instance.Dice.GetComponent<Dice>().RollDice();
        diceRoll2 = ObjectHandler.Instance.Dice.GetComponent<Dice>().RollDice();
        move = diceRoll1 + diceRoll2;

        //see what the weight of where each character moves to would be.
        for (int i = 0; i < 3; i++)
        {
            tileWeight[i] = FindMoveWeight(characterLocations[i], move, i);
        }

        //output move distance and weights of tiles characters would land on.
        Debug.Log("AI rolled a total of " + move);
        Debug.Log("tileWeight[0] is " + tileWeight[0] + " TileWeight[1] is " + tileWeight[1] + " Tileweight[2] is " + tileWeight[2]);

        StartCoroutine(DisplayDiceRoll(diceRoll1, diceRoll2)); //displays dice roll, then moves the appropriate character.

        return;
    }

    //find weight of next move. All movement related decision making goes here.
    private int FindMoveWeight(int location, int move, int arrayLocation)
    {
        int tileWeight = -6;

        //if character is at start and all moves are neutral, get character out of start safely.
        if (location == 0 && ObjectHandler.Instance.tiles[move].GetComponent<Tile>().GetTileWeight() == 0)
            tileWeight = 1;
        else if (location != -3)
        {
            location += move;
            tileWeight = ObjectHandler.Instance.tiles[location].GetComponent<Tile>().GetTileWeight();

            //if a character would die from landing on the space, heavily discourage the move.
            if (tileWeight == -2 && ObjectHandler.Instance.player2Characters[arrayLocation].GetComponent<Character>().GetHealth() <= 3)
                tileWeight -= 4;

            //if tile is occupied by an enemy, view it as more important to land on if it isn't a trap location.
            if ((ObjectHandler.Instance.tiles[location].GetComponent<Tile>().CheckFaction() == 1 ||
                ObjectHandler.Instance.tiles[location].GetComponent<Tile>().CheckFaction() == 3) &&
                    tileWeight > -5)
            {
                if (!aggressive) tileWeight++;
                else tileWeight += 3;
            }
            //if a character is at full hp, a health tile is neutral.
            if (tileWeight == 2 && ObjectHandler.Instance.player2Characters[arrayLocation].GetComponent<Character>().GetHealth() == 
                ObjectHandler.Instance.player2Characters[arrayLocation].GetComponent<Character>().GetMaxHealth())
            {
                tileWeight = 0;
            }
        }
        return tileWeight;
    }

    //wait for dice roll and then move best character choice.
    private IEnumerator DisplayDiceRoll(int roll1, int roll2)
    {
        //this function plays the animation to roll the dice.

        //to play the dice roll animation
        ObjectHandler.Instance.Dice.GetComponent<Dice>().DiceRollAnimation();
        ObjectHandler.Instance.Dice2.GetComponent<Dice>().DiceRollAnimation();

        float waitTime = GameManager.Instance.getDiceRollAnimTime();

        //set dice face.
        ObjectHandler.Instance.Dice.GetComponent<Dice>().SetDiceFace(roll1);
        ObjectHandler.Instance.Dice2.GetComponent<Dice>().SetDiceFace(roll2);
        yield return new WaitForSeconds(waitTime);
        MakeBestMove(roll1 + roll2);

    }

    //find the best move in the array. Returns what character moved, though it's unused.
    private int MakeBestMove(int move)
    {
        int charToMove = 0; //default to moving character 0.
        int best = -6; //default value that will be overwritten by all other options.

        //find best move possible.
        for (int i = 0; i < 3; i++)
        {
            if (best < tileWeight[i])
            {
                best = tileWeight[i];
                charToMove = i;
            }
        }

        //if characters have identical weights with a move, randomly choose one to move.
        //this is to encourage the AI not to just move one person across the board at a time.
        //While effective, that strategy is boring to play against
        if (tileWeight[0] == tileWeight[1] && tileWeight[1] == tileWeight[2]) charToMove = Random.Range(0, 3);
        else if (tileWeight[0] == tileWeight[1] && best == tileWeight[0]) charToMove = Random.Range(0, 2);
        else if (tileWeight[1] == tileWeight[2] && best == tileWeight[1]) charToMove = Random.Range(1, 3);
        else if (tileWeight[0] == tileWeight[2] && best == tileWeight[0])
        {
            int temp = Random.Range(0, 2);
            if (temp == 0) charToMove = 0;
            else charToMove = 2;
        }

        //make the move
        ObjectHandler.Instance.player2Characters[charToMove].GetComponent<Character>().UpdateTile(move);

        return charToMove;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Do not need to be implemented if not used
    }

    // Update is called once per frame
    void Update()
    {
        //Do not need to be implemented if not used
    }
}

