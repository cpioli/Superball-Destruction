using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;

public class UpdateLoggerBehavior : MonoBehaviour {


    public Text logText;
    private StringBuilder logTranscript;

    // Use this for initialization
	void Start () 
	{
        logTranscript = new StringBuilder(4096);
	}

    public bool AddToLogText(string input)
    {
        try
        {
            logTranscript.AppendLine(input);
        }
        catch(System.ArgumentOutOfRangeException e)
        {
            return false;
        }
        logText.text = logTranscript.ToString();
        return true;

    }

}
