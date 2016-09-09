using UnityEngine;
using System.Collections;

public class PanelBehavior : MonoBehaviour
{

    private Material mat;
    private int timesHit;
    private Color colorDecrement;

    public int maxHits = 20;
    public bool worthPoints = false;

    // Use this for initialization
    void Start()
    {
        mat = this.GetComponent<MeshRenderer>().material;
        mat.SetColor("_Color", Color.white);
        colorDecrement = new Color(1.0f / maxHits, 1.0f / maxHits, 1.0f / maxHits);
       // Debug.Log(this.name + "'s normal: " +
         //   this.transform.TransformVector(this.GetComponent<MeshFilter>().mesh.normals[0]));
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.name != "Sphere" || !worthPoints)
        {

            return;
        }
        timesHit++;
        Color currentColor = mat.GetColor("_Color");
        mat.SetColor("_Color", currentColor - colorDecrement);
        // Debug.Log("Panel is hit!");
        Debug.LogFormat("Hit a {0}", this.name);

    }

    public Vector3 GetNormal()
    {
        return this.transform.TransformVector(this.GetComponent<MeshFilter>().mesh.normals[0]);
    }
}