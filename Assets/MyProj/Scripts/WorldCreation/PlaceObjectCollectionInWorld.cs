using System.Collections.Generic;
using UnityEngine;

public class PlaceObjectCollectionInWorld : MonoBehaviour
{
    //TODO: IMPROVE THE PLACING OF OBJECTS
    [SerializeField] private float yOffset;
    [SerializeField] private List<Transform> groundCheckPoints = new List<Transform>();
    [SerializeField] private LayerMask groundLayer = 6;
    [SerializeField] private bool useCheckPoints; 
    private void Start()
    {
        PlaceBuildings();
    }

    private void PlaceBuildings()
    {
        if (useCheckPoints)
        {
            for (int i = 0; i < groundCheckPoints.Count; i++)
            {
                int y = 300;

                RaycastHit hit;

                GameObject house = groundCheckPoints[i].parent.gameObject;

                //Debug.DrawRay(groundCheckPoints[i].position, groundCheckPoints[i].TransformDirection(Vector3.down) * 10000, Color.red, 30F);

                if (Physics.Raycast(groundCheckPoints[i].position,
                    groundCheckPoints[i].TransformDirection(Vector3.down), out hit, 10000, groundLayer))
                {
                    house.transform.position = new Vector3(hit.point.x, hit.point.y + yOffset, hit.point.z);
                    house.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                }
            }
        }
        else
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                int y = 300;

                RaycastHit hit;

                GameObject house = this.transform.GetChild(i).gameObject;
                
                Debug.DrawRay(house.transform.position,
                    house.transform.TransformDirection(Vector3.down) * 10000, Color.red, 30F);

                if (Physics.Raycast(house.transform.position,
                    house.transform.TransformDirection(Vector3.down), out hit, 10000, groundLayer))
                {
                    house.transform.position = new Vector3(hit.point.x, hit.point.y + yOffset, hit.point.z);
                    house.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                }
            }
        }

    }
}
