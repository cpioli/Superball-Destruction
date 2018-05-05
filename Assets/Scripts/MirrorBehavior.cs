using UnityEngine;
using System.Collections;

public class MirrorBehavior : MonoBehaviour {

    public float maxHitPoints = 20f;
    public float hardness = 5f;
    // number representing the accumulated force from impact necessary
    // to shatter an object

    private float currentHitPoints;
    private Color alpha = new Color(0f, 0f, 0f, 0f);


	// Use this for initialization
	void Start ()
	{
        currentHitPoints = maxHitPoints;

    }

	// Update is called once per frame
	void Update ()
	{
	}

    void OnCollisionExit(Collision collision)
    {
        float forceOfCollision = collision.collider.gameObject.GetComponent<Rigidbody>().velocity.magnitude;
        // magnitude of a vector is its length, and it tells us how fast the ball is going

        float damage = forceOfCollision - hardness;
        if (damage > currentHitPoints)
        // if the momentum of the ball is strong enough to overcome the object's hardness
        // instantly shatter
        {
            //mat.SetColor("_Color", alpha);
            this.GetComponent<MeshRenderer>().enabled = false;
            this.GetComponent<BoxCollider>().enabled = false;
            //TODO: calculate if amount of force is great enough to push through material or bounce off.
        }
        else
        // if the moment is strong enough to overcome the object's hardness but HP still remains
        // deduct HP from object
        {
            currentHitPoints -= damage;
        }
    }
}