using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : MonoBehaviour
{
    public float controlSpeedForward = 0.1f;
    public float controlSpeedTurning = 0.1f;

    public List<GameObject> legs = new List<GameObject>();
    public List<Transform> targets = new List<Transform>();
    public float stepDistance = 4f;
    public float spiderHeight = 2f;

    float[] offsets = {1, -1, 1, -1, 1, -1};
    int layerMask = 1 << 8; // Walkable

    // Start is called before the first frame update
    void Start()
    {
        // Legs and targets must be in the same sequence
        for(int i=0; i<12; i++)
        {
            if(i < 6)
            {
                legs.Add(this.transform.GetChild(i).gameObject);
            }
            else
            {
                targets.Add(this.transform.GetChild(i).transform);
            }
        }

        // Casting rays towards targets and moving legs to the respective coordinate
        for(int i=0; i<6; i++)
        {
            float movePos;
            if(i % 2 == 0)
                movePos = 1f;
            else
                movePos = -1f;

            // Casting a ray towards target i
            RaycastHit hit;
            if(Physics.Raycast(targets[i].position, -transform.up, out hit, Mathf.Infinity, layerMask))
            {
                legs[i].GetComponent<SpiderLeg>().SetTargetPos(hit.point + transform.forward * offsets[i]);
                // Changing the offset for next iteration for alternating step walk
                offsets[i] *= -1;
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 upVector = Vector3.zero;

        // Casting rays towards targets and moving legs to the respective coordinate
        for(int i=0; i<6; i++)
        {
            // Casting a ray towards target i
            RaycastHit hit;
            if(Physics.Raycast(targets[i].position, -transform.up, out hit, Mathf.Infinity, layerMask))
            {
                upVector += hit.normal;
                float distance = Vector3.Distance(hit.point, legs[i].GetComponent<SpiderLeg>().targetPos);

                if (distance > stepDistance)
                {
                    legs[i].GetComponent<SpiderLeg>().SetTargetPos(hit.point + transform.forward * offsets[i]);
                    // Changing the offset for next iteration for alternating step walk
                    offsets[i] *= -1;
                }
            }
        }

        // Normalization
        upVector = Vector3.Normalize(upVector);

        // Placing the spider body with respect to feet
        Vector3 averagePos = Vector3.zero;
        for(int i=0; i<6; i++)
        {
            averagePos += legs[i].GetComponent<SpiderLeg>().targetPos;
        }
        averagePos /= 6f;
        
        // Rotating the body wrt to the foot placement normals
        Vector3 forwardVector = Vector3.Cross(upVector, -this.transform.right);
        this.transform.position = Vector3.Lerp(this.transform.position, averagePos + upVector * spiderHeight, Time.deltaTime);
        Quaternion targetRot = Quaternion.LookRotation(forwardVector);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRot, 5f * Time.deltaTime);
    }

    // TO CONTROL WITH WASD
    void Update()
    {
        // Up
        if(Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * controlSpeedForward * Time.deltaTime;
        }

        // Down
        if(Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * controlSpeedForward * Time.deltaTime;
        }

        // Left
        if(Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0, -controlSpeedTurning * Time.deltaTime, 0);
        }

        // Right
        if(Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0, controlSpeedTurning * Time.deltaTime, 0);
        }
    }
}
