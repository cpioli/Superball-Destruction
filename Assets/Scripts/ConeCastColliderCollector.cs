using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
This object is a container that stores every GameObject the collision cone connects with
 */
public class ConeCastColliderCollector : MonoBehaviour {


    private List<Collider> colliderContainer;

	// Use this for initialization
	void Start () {
        colliderContainer = new List<Collider>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ClearColliderContainer()
    {
        colliderContainer.Clear();
    }

    public List<Collider> GetColliderContainer()
    {
        return colliderContainer;
    }

    public void OnCollisionEnter(Collision collision)
    {
        colliderContainer.Add(collision.collider);
    }
}
