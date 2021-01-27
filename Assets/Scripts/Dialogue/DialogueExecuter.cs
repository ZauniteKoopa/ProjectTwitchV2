using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueExecuter : MonoBehaviour
{
    //Private singleton of this class
    private static DialogueExecuter _instance;

    //Accessor method to get dialogue executer
    public static DialogueExecuter Instance { get { return _instance; } }

    //List of characters that will be considered: each character is in their own line
    [SerializeField]
    private TextAsset characterList = null;
    Dictionary<string, Sprite> characterIcons;

    //Script format constants
    private const int SCRIPT_SEGMENT_LENGTH = 4;

    //Parts of the dialogue executer
    [SerializeField]
    private TMP_Text dialogueDisplay = null;
    [SerializeField]
    private Image iconDisplay = null;
    [SerializeField]
    private AudioSource voicePlayer = null;
    [SerializeField]
    private AudioClip defaultClip = null;

    
    //On awake, initialize this as the main executor if none exist
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;

            //Fill up character icon dictionary by loading files
            if (characterList != null)
            {
                characterIcons = new Dictionary<string, Sprite>();
                string[] names = characterList.text.Split('\n');

                for (int i = 0; i < names.Length; i++)
                {
                    Sprite icon = Resources.Load<Sprite>("DialogueIcons/" + names[i]);
                    characterIcons.Add(names[i], icon);
                }
            }


            gameObject.SetActive(false);
        }
    }


    //public Method to load dialogue: Each dialogue segment of a script has the following
    //         character name
    //         sound effect (or N if no sound effect)
    //         Line of dialogue
    //         (empty line to border between 2 segments)
    public IEnumerator LoadDialogue(TextAsset rawScript, EntityStatus player)
    {
        string[] script = rawScript.text.Split('\n');

        if (script.Length % SCRIPT_SEGMENT_LENGTH != 0)
        {
            Debug.Log("Invalid script format: Script Length - " + script.Length);
        }
        else
        {
            gameObject.SetActive(true);
            yield return StartCoroutine(ExecuteScript(script, player));
        }
    }


    //Coroutine to execute script
    private IEnumerator ExecuteScript(string[] script, EntityStatus player)
    {
        //Disable player movement
        player.GetComponent<TwitchController>().ForceToIdle();

        for (int i = 0; i < script.Length; i+=SCRIPT_SEGMENT_LENGTH)
        {
            //Extract strings from script for the current segment
            string charName = script[i];
            string audioFileName = script[i + 1];
            string dialogue = script[i + 2];

            //Extract files
            AudioClip voiceClip = Resources.Load<AudioClip>("DialogueAudio/" + charName + "/" + audioFileName);
            if (voiceClip == null)
                voiceClip = defaultClip;
            voicePlayer.clip = voiceClip;

            //Set up dialogue
            if (characterIcons.ContainsKey(charName))
            {
                iconDisplay.gameObject.SetActive(true);
                iconDisplay.sprite = characterIcons[charName];
            }
            else
            {
                iconDisplay.gameObject.SetActive(false);
            }
            
            voicePlayer.Play(0);
            dialogueDisplay.text = dialogue;

            //Overall dialogue loop
            yield return new WaitForSeconds(0.2f);

            do
            {
                yield return new WaitForFixedUpdate();

            } while (!Input.GetButtonDown("Contaminate"));
        }

        //Enable character movement
        player.canMove = true;
        gameObject.SetActive(false);
    }

}
