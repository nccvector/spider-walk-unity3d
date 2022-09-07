using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : MonoBehaviour
{
    public float controlSpeedForward = 0.1f;
    public float controlSpeedTurning = 0.1f;
    private SpiderLeg[] legs;
    private List<Transform> targets = new List<Transform>();
    public float stepDistance = 4f;
    public float spiderHeight = 2f;
    public float targetDistance = 2f;
    public LayerMask walkableMask = 1 << 8;
    private int numLegs;
    
    // Start is called before the first frame update
    void Start()
    {
        legs = GetComponentsInChildren<SpiderLeg>();
        numLegs = legs.Length;
        foreach (SpiderLeg leg in legs)
        {
            GameObject newTarget = new GameObject("Target (" + leg.name + ")");
            newTarget.transform.SetParent(transform);
            Vector3 pushOutTargetFromCenter = (leg.transform.position - transform.position) * targetDistance;
            pushOutTargetFromCenter.x *= 0.5f;
            newTarget.transform.position = leg.transform.position + pushOutTargetFromCenter;
            targets.Add(newTarget.transform);
        }

        // Casting rays towards targets and moving legs to the respective coordinate
        for(int i=0; i< numLegs; i++)
        {
            RaycastHit hit;
            if(Physics.Raycast(targets[i].position, -transform.up, out hit, Mathf.Infinity, walkableMask))
            {
                legs[i].SetTargetPos(hit.point + transform.forward * (i % 2));
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 upVector = Vector3.zero;
        // Casting rays towards targets and moving legs to the respective coordinate
        for(int i=0; i<numLegs; i++)
        {
            // Casting a ray towards target i
            RaycastHit hit;
            if(Physics.Raycast(targets[i].position, -transform.up, out hit, Mathf.Infinity, walkableMask))
            {
                upVector += hit.normal;
                float distance = Vector3.Distance(hit.point, legs[i].targetPos);
                if (distance > stepDistance)
                {
                    legs[i].SetTargetPos(hit.point + transform.forward * (i%2));
                }
            }
        }

        // Normalization
        upVector = Vector3.Normalize(upVector);

        // Placing the spider body with respect to feet
        Vector3 averagePos = Vector3.zero;
        for(int i=0; i<numLegs; i++)
        {
            averagePos += legs[i].targetPos;
        }

        averagePos /= numLegs;
        
        // Rotating the body wrt to the foot placement normals
        Vector3 forwardVector = Vector3.Cross(upVector, -this.transform.right);
        transform.position = Vector3.Lerp(transform.position, averagePos + upVector * spiderHeight, Time.deltaTime);
        Quaternion targetRot = Quaternion.LookRotation(forwardVector);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.deltaTime);
    }

    // TO CONTROL WITH WASD / GAMEPAD / ARROWS 
    void Update()
    {
        transform.position += transform.forward * controlSpeedForward * Time.deltaTime * Input.GetAxis("Vertical");
        transform.Rotate(0, controlSpeedTurning * Time.deltaTime * Input.GetAxis("Horizontal"), 0);
    }
}
