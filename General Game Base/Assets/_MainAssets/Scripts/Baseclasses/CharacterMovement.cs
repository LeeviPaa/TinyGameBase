using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour {

    public float moveSpeed = 20;
    public float rotSpeed = 1;
    [Range(0, 1)]
    public float wallSpeed = 0.5f;
    [HideInInspector]
    public float slideRayMagnitude = 2;
    public LayerMask collisionLayer;
    public LayerMask networkLayer;

    //private
    private bool possessed = false;
    private Rigidbody rb;

    private Ray r;
    private RaycastHit hit;
    private CapsuleCollider thisCollider;

    protected virtual void Awake()
    {
        thisCollider = GetComponentInChildren<CapsuleCollider>();
    }

    private int layermask_to_layer(LayerMask layerMask)
    {
        int layerNumber = 0;
        int layer = layerMask.value;
        while (layer > 0)
        {
            layer = layer >> 1;
            layerNumber++;
        }
        return layerNumber - 1;
    }

    protected void MoveDirection(Vector3 direction)
    {
        if (thisCollider != null && rb != null)
        {
            Vector3 moveVector = direction * moveSpeed * Time.deltaTime;

            //disable the collider because it would otherwise get hit with the capsule casts and checks
            thisCollider.enabled = false;

            //capsule cast positions
            Vector3 P1 = thisCollider.center + thisCollider.transform.position - (Vector3.up * (thisCollider.height * 0));
            Vector3 P2 = thisCollider.center + thisCollider.transform.position + (Vector3.up * (thisCollider.height / 4));
            //Debug.DrawLine(P1, P2);

            Vector3 P3 = thisCollider.center + thisCollider.transform.position - (Vector3.up * (thisCollider.height * 0));
            P3 += moveVector;
            Vector3 P4 = thisCollider.center + thisCollider.transform.position + (Vector3.up * (thisCollider.height / 4));
            P4 += moveVector;
            //Debug.DrawLine(P3, P4, Color.red);

            //check with the specs of the current collider if there is something in the way
            if (!Physics.CapsuleCast(P1, P2, thisCollider.radius * 1.0f, moveVector, out hit, moveVector.magnitude, collisionLayer, QueryTriggerInteraction.Ignore) &&
                !Physics.CheckCapsule(P3, P4, thisCollider.radius * 1.0f, collisionLayer, QueryTriggerInteraction.Ignore))
            {
                //Enable the collider for further non input related collision
                thisCollider.enabled = true;
                rb.MovePosition(transform.position + moveVector);
            }
            else
            {
                //something is in the way, correcting movement
                if(Physics.Raycast(transform.position, moveVector, out hit, moveVector.magnitude+thisCollider.radius *slideRayMagnitude,  collisionLayer, QueryTriggerInteraction.Ignore))
                {
                    //check with a ray if there is a collider close that we can get a position to move towards slightly
                    Vector3 moveVectorClose = (hit.point + (hit.normal*thisCollider.radius));

                    ////check if point is available because we can get jerky in corners
                    //P3 = moveVectorClose;
                    //P4 = moveVectorClose + (Vector3.up * (thisCollider.height / 4));

                    //Debug.DrawLine(P3, P4, Color.red);
                    //if (!Physics.CheckCapsule(P3, P4, thisCollider.radius * 0.9f, collisionLayer, QueryTriggerInteraction.Ignore))
                    //{

                    //}

                    rb.MovePosition(Vector3.MoveTowards(transform.position, moveVectorClose, moveSpeed * wallSpeed * Time.deltaTime));
                    //Debug.Log("ray move");

                }
                else
                {
                    //ray did not hit so we move check left and right from the next position
                    if(Physics.Raycast(transform.position + moveVector, transform.right, out hit, thisCollider.radius, collisionLayer, QueryTriggerInteraction.Ignore))
                    {
                        Vector3 moveVectorClose = (hit.point + (hit.normal * thisCollider.radius));
                        rb.MovePosition(Vector3.MoveTowards(transform.position, moveVectorClose, moveSpeed * wallSpeed * Time.deltaTime));
                    }
                    else if(Physics.Raycast(transform.position + moveVector, -transform.right, out hit, thisCollider.radius, collisionLayer, QueryTriggerInteraction.Ignore))
                    {
                        Vector3 moveVectorClose = (hit.point + (hit.normal * thisCollider.radius));
                        rb.MovePosition(Vector3.MoveTowards(transform.position, moveVectorClose, moveSpeed * wallSpeed * Time.deltaTime));
                    }
                }
            }
            //enable the collider whatever happens
            thisCollider.enabled = true;
        }

    }

    protected void RotateYDirection(float rotation)
    {
        rb.MoveRotation(transform.rotation*Quaternion.Euler(0, rotation*Time.deltaTime*rotSpeed, 0));
    }
}
