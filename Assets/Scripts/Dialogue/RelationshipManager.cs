
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelationshipManager : MonoBehaviour
{
    //Public enum of character names
    public enum CharacterName {Warwick, ChumpWhump};

    //Private singleton dictionary of RelationshipManagers
    private static Dictionary<CharacterName, RelationshipManager> relationships = null;

    //Enum that represents character name
    [SerializeField]
    private CharacterName character = CharacterName.Warwick;

    //Constants representing format
    private const int LINES_PER_OVERVIEW_SEG = 5;

    //Requirement dictionaries and other variables
    private Dictionary<int, DialogueEvent> freqEvents;
    private Dictionary<Ingredient.IngredientType, DialogueEvent> ingredientEvents;
    private int numTimesMet = 0;
    [SerializeField]
    private TextAsset defaultScript = null;
    [SerializeField]
    private Inventory playerInventory = null;


    //Accessor method to get RelationshipMangager
    //  Returns null if it doesn't exist
    public static RelationshipManager GetRelationship(CharacterName c)
    {

        if (relationships != null && relationships.ContainsKey(c))
        {
            return relationships[c];
        }

        return null;
    }


    //On awake
    private void Awake()
    {
        if (GetRelationship(character) != null && GetRelationship(character) != this)
        {
            Destroy(gameObject);
        }
        else
        {
            if (relationships == null)
                relationships = new Dictionary<CharacterName, RelationshipManager>();

            if (!relationships.ContainsKey(character))
                relationships.Add(character, this);

            ParseDialogueOverview();
        }

    }


    //Method to process script concerning dialogue events
    private void ParseDialogueOverview()
    {
        //Initialize variables
        freqEvents = new Dictionary<int, DialogueEvent>();
        ingredientEvents = new Dictionary<Ingredient.IngredientType, DialogueEvent>();

        TextAsset rawOverview = Resources.Load<TextAsset>("DialogueScripts/" + character.ToString() + "/Overview");
        string[] overview = rawOverview.text.Split('\n');

        if (overview.Length % LINES_PER_OVERVIEW_SEG != 0)
        {
            Debug.Log("Relationship overview invalid format: " + overview.Length + " Lines.");
        }
        else
        {
            for (int i = 0; i < overview.Length; i += 5)
            {
                //Get DialogueEvent from first and fourth line
                string stringPriority = overview[i];
                string scriptName = overview[i + 3];
                int priority = int.Parse(stringPriority);
                TextAsset script = Resources.Load<TextAsset>("DialogueScripts/" + character.ToString() + "/" + scriptName);

                DialogueEvent curEvent = new DialogueEvent(priority, script);


                //Map a requirement to this dialogue event using second line (req type) and third line (requirement indicator)
                string reqType = overview[i + 1];
                string reqInfo = overview[i + 2];

                if (reqType == "f")
                {
                    int freq = int.Parse(reqInfo);
                    freqEvents.Add(freq, curEvent);
                }
                else if (reqType == "i")
                {
                    Ingredient.IngredientType ingType = Ingredient.ParseIngredientType(reqInfo);
                    ingredientEvents.Add(ingType, curEvent);
                }

            }
        }

    }


    //Variable to obtain best valid dialogue
    //  Post: numTimesMet will be incremented and will return a dialogue script associated with the highest priority (given requirements)
    public TextAsset TalkToCharacter()
    {
        //Increment times met to indicate you met once
        numTimesMet++;

        //Find dialogue event depending on frequency of times met
        DialogueEvent bestEvent = null;
        if (freqEvents.ContainsKey(numTimesMet))
            bestEvent = freqEvents[numTimesMet];
        
        //Find dialogue event concerning ingredients
        foreach(KeyValuePair<Ingredient.IngredientType, DialogueEvent> entry in ingredientEvents)
        {
            if (playerInventory.HasIngredient(entry.Key))
            {
                if (bestEvent == null || entry.Value.IsHigherPriority(bestEvent))
                {
                    bestEvent = entry.Value;
                }
            }
        }

        //Return the best script. If it doesn't exist, just go to defaultScript
        return (bestEvent != null) ? bestEvent.GetScript() : defaultScript;
    }




    //Private nested class to handle Text Assets as seperate Dialogue Events
    class DialogueEvent
    {
        private int priority;
        private TextAsset script;

        //Constructor
        public DialogueEvent(int p, TextAsset txt)
        {
            priority = p;
            script = txt;
        }

        //Returns if this instance has a higher priority than other.
        //  If it is equal, flip a coin to see if it's higher
        public bool IsHigherPriority(DialogueEvent other)
        {
            if (other.priority == priority)
                return Random.Range(0, 2) == 0;
            
            return priority > other.priority;
        }

        //Method to return TextAsset
        public TextAsset GetScript()
        {
            return script;
        }
    }

}
