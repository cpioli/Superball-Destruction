using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class MirrorBehavior : MonoBehaviour {

    void OnCollisionExit(Collision collision)
    {
        
        this.GetComponent<MeshRenderer>().enabled = false;
        if(GetComponent<BoxCollider>() != null)
            this.GetComponent<BoxCollider>().enabled = false;
        else
        {
            this.transform.GetChild(0).gameObject.GetComponent<MeshCollider>().enabled = false;
        }
        ExecuteEvents.Execute<IGameEventHandler>(GameObject.Find("GameManager"), null, (x, y) => x.RegisterGameObjectDestroyed(this.gameObject));
    }
}