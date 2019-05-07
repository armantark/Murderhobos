using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MultipleTargetCamera : MonoBehaviour
{

    #region movement_targets
    // List of targets for the camera to follow
    private List<Transform> targets;
    public List<BasePlayerController> player_list;
    #endregion


    #region camera_movement_variables
    // Camera offset against the targets
    public Vector3 offset;

    // For smoothing the camera motion between updates
    public float smoothTime = 0.25f;

    // For smoothing the camera motion between updates
    private Vector3 velocity;
    #endregion

    #region camera_zoom_variables
    public float minZoom = 100f;
    public float maxZoom = 40f;
    public float zoomLimiter = 40f;
    private float curZoom;
    #endregion

    #region camera_variables
    private Camera cam;
    #endregion


    #region Unity_functions
    private void Start()
    {
        cam = GetComponent<Camera>();
        targets = new List<Transform>();

        for (int i = 0; i < player_list.Count; i++)
        {
            targets.Add(player_list[i].transform);
        }
    }


    // Updates the camera position after player positions have been updated
    private void LateUpdate()
    {
        if (targets.Count == 0)
        {
            return;
        }

        Bounds bounds = GetBounds();

        Move(bounds);
        Zoom(bounds);
        //if (Mathf.Approximately(cam.fieldOfView, minZoom))
        //ClampPlayers();
    }
    #endregion

    #region camera_shift_functions
    // Moves the camera based on the bounds of the players on screen
    void Move(Bounds bounds)
    {
        //Debug.Log("Moving Camera Around");
        Vector3 centerPoint = GetCenterPoint(bounds);

        Vector3 newPosition = centerPoint + offset;
        newPosition.z = transform.position.z;

        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    // Zooms the camera based on the bounds of the players on screen
    void Zoom(Bounds bounds)
    {
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance(bounds) / zoomLimiter);
        //curZoom = newZoom;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, newZoom, Time.deltaTime);
    }
    #endregion

    #region helper_functions
    // Gets the bounding box for all players on screen
    Bounds GetBounds()
    {
        var bounds = new Bounds();

        bool p1_downed = player_list[0].IsDowned(), p2_downed = player_list[1].IsDowned();
        if (!p1_downed && p2_downed)
        {
            bounds.Encapsulate(targets[0].position);
        }
        else if (!p2_downed && p1_downed)
        {
            bounds.Encapsulate(targets[1].position);
        }
        else
        {
            bounds.Encapsulate(targets[0].position);
            bounds.Encapsulate(targets[1].position);
        }

        return bounds;
    }


    // Calculates the center of the bounding box for all players
    Vector3 GetCenterPoint(Bounds bounds)
    {
        if (targets.Count == 1)
        {
            return targets[0].position;
        }

        return bounds.center;

    }

    // Calculates the largest axis of the bounding box for all players on screen
    float GetGreatestDistance(Bounds bounds)
    {
        return Mathf.Max(bounds.size.x, bounds.size.y);
    }

    #endregion

    #region player_clamping_functions
    void ClampPlayers()
    {
        foreach (BasePlayerController player in player_list)
        {
            ClampObjectIntoView(player.GetRigidBody());
        }
    }

    void ClampObjectIntoView(Rigidbody2D r)
    {

        // Set limits within the frustrum of the camera
        Vector3 objectPosition = r.position;
        float camZ = cam.transform.position.z;
        Vector2 topRight = cam.ViewportToWorldPoint(new Vector3(0, 0, camZ));
        Vector2 bottomLeft = cam.ViewportToWorldPoint(new Vector3(1, 1, camZ));
        Debug.Log("player pos: " + r.position);
        Debug.Log("x range: " + bottomLeft.x + "-" + topRight.x);
        Debug.Log("y range: " + bottomLeft.y + "-" + topRight.y);

        r.transform.position = new Vector2(
            Mathf.Clamp(transform.position.x, bottomLeft.x, topRight.x), 
            Mathf.Clamp(transform.position.y, bottomLeft.y, topRight.y));
    }
    #endregion
}
