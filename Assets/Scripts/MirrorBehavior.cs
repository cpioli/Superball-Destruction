using UnityEngine;
using System.Collections;

public class MirrorBehavior : MonoBehaviour {

    void OnCollisionExit(Collision collision)
    {
        float forceOfCollision = collision.collider.gameObject.GetComponent<Rigidbody>().velocity.magnitude;
        // magnitude of a vector is its length, and it tells us how fast the ball is going

        this.GetComponent<MeshRenderer>().enabled = false;
        this.GetComponent<BoxCollider>().enabled = false;
    }
}