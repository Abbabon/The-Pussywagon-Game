using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraLogic : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 20.0f;
    public Vector3 offset = new Vector3(0, 0, -10);
    private BoxCollider2D cameraBox;

    private GameObject levelEnd;

    public ParticleSystem[] particleSystems;

    private void Start()
    {
        levelEnd = GameObject.Find("LevelEnd");
        cameraBox = GetComponent<BoxCollider2D>();
    }

    void FixedUpdate()
    {
        float targetPositionX = target.position.x + offset.x;
        float targetPositionY = transform.position.y;
        float targetPositionZ = target.position.z + offset.z;

        if (levelEnd != null){
            targetPositionX = Mathf.Clamp(targetPositionX, -100f, (levelEnd.transform.position.x - cameraBox.size.x / 2));
        }

        transform.position = new Vector3(targetPositionX, targetPositionY, targetPositionZ);
    }

    public void StartParticles()
    {
        foreach (ParticleSystem ps in particleSystems){
            ps.Play();
        }
    }
}
