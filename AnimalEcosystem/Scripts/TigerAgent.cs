using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;
using System.Linq;

public class TigerAgent : Agent
{
    public float moveSpeed = 0.6f;
    public float turnSpeed = 160f;
    public float detectionRadius = 12f;

    private Rigidbody rb;
    private Transform targetPrey;
    private Vector3 lastPosition;
    private SphereCollider triggerCollider;
    private Terrain terrain;

    private bool isEating = false;
    private float timeSinceLastFood = 0f;
    private float targetRefreshTimer = 0f;
    private float targetRefreshInterval = 0.5f;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        terrain = Terrain.activeTerrain;
        lastPosition = transform.position;

        triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = detectionRadius;

        FindNewTarget();
    }

    public override void OnEpisodeBegin()
    {
        transform.position = GetValidSpawnPosition();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        lastPosition = transform.position;
        timeSinceLastFood = 0f;
        isEating = false;

        FindNewTarget();
    }

    private void LateUpdate()
    {
        float terrainHeight = terrain.SampleHeight(transform.position);
        if (transform.position.y > terrainHeight + 0.5f)
        {
            transform.position = new Vector3(transform.position.x, terrainHeight + 0.3f, transform.position.z);
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        }
    }

    private Vector3 GetValidSpawnPosition()
    {
        for (int i = 0; i < 15; i++)
        {
            float x = Random.Range(terrain.transform.position.x + 5f, terrain.terrainData.size.x - 5f);
            float z = Random.Range(terrain.transform.position.z + 5f, terrain.terrainData.size.z - 5f);
            float y = terrain.SampleHeight(new Vector3(x, 0, z));
            Vector3 pos = new Vector3(x, y + 1.5f, z);
            if (!IsSteepSlope(pos)) return pos;
        }

        return new Vector3(30f, terrain.SampleHeight(new Vector3(30f, 0, 30f)) + 1.5f, 30f);
    }

    private bool IsSteepSlope(Vector3 position)
    {
        if (Physics.Raycast(position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2f))
        {
            return Vector3.Angle(hit.normal, Vector3.up) > 15f;
        }
        return false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.forward.x);
        sensor.AddObservation(transform.forward.z);
        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.z);
        sensor.AddObservation(timeSinceLastFood / 60f);

        if (targetPrey != null)
        {
            Vector3 delta = targetPrey.position - transform.position;
            Vector3 dir = delta.normalized;
            float dist = delta.magnitude / detectionRadius;

            sensor.AddObservation(dir.x);
            sensor.AddObservation(dir.z);
            sensor.AddObservation(dist);
            sensor.AddObservation(delta.x / detectionRadius);
            sensor.AddObservation(delta.z / detectionRadius);
        }
        else
        {
            sensor.AddObservation(0f); sensor.AddObservation(0f);
            sensor.AddObservation(1f); sensor.AddObservation(0f); sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isEating) return;

        float move = Mathf.Clamp(actions.ContinuousActions[0], -0.3f, 0.3f);
        float turn = Mathf.Clamp(actions.ContinuousActions[1], -0.5f, 0.5f);

        transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime);
        rb.MovePosition(rb.position + transform.forward * move * moveSpeed * Time.fixedDeltaTime);

        float moveDist = Vector3.Distance(transform.position, lastPosition);
        AddReward(moveDist * 0.01f);
        lastPosition = transform.position;

        if (targetPrey != null)
        {
            float dist = Vector3.Distance(transform.position, targetPrey.position);
            float diff = Vector3.Distance(lastPosition, targetPrey.position) - dist;
            AddReward(diff * 0.3f);

            if (dist < 1.5f) StartCoroutine(EatPreyRoutine());
        }

        if (Mathf.Approximately(move, 0f) && Mathf.Approximately(turn, 0f))
            AddReward(-0.0015f * Time.deltaTime);

        AddReward(0.007f * Time.deltaTime);

        timeSinceLastFood += Time.deltaTime;
        if (timeSinceLastFood > 180f) // 60f
        {
            AddReward(-2.5f);
            EndEpisode();
            Destroy(gameObject);
        }

        if (transform.position.y < 0f || transform.position.y > 25f)
        {
            AddReward(-3.0f);
            EndEpisode();
        }
    }

    private void FixedUpdate()
    {
        targetRefreshTimer += Time.fixedDeltaTime;
        if (targetRefreshTimer >= targetRefreshInterval)
        {
            FindNewTarget();
            targetRefreshTimer = 0f;
        }
    }

    private IEnumerator EatPreyRoutine()
    {
        if (isEating || targetPrey == null) yield break;

        isEating = true;
        moveSpeed = 0f;
        yield return new WaitForSeconds(1f);

        if (targetPrey != null)
        {
            if (targetPrey.CompareTag("Dog")) 
            { Debug.Log($"호랑이가 개를 먹었다! +보상");
                AddReward(30f); }
            else if (targetPrey.CompareTag("Chicken")) 
            { Debug.Log($"호랑이가 닭을 먹었다! +보상");
                AddReward(20f); }

            Destroy(targetPrey.gameObject);
            timeSinceLastFood = 0f;
            targetPrey = null;
            FindNewTarget();
        }

        moveSpeed = 0.4f;
        isEating = false;
    }

    private void FindNewTarget()
    {
        var preys = GameObject.FindGameObjectsWithTag("Dog")
            .Concat(GameObject.FindGameObjectsWithTag("Chicken"));

        float minDist = float.MaxValue;
        Transform closest = null;

        foreach (var prey in preys)
        {
            float dist = Vector3.Distance(transform.position, prey.transform.position);
            Vector3 dir = (prey.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dir);

            if (angle < 180f && dist < detectionRadius && dist < minDist)
            {
                minDist = dist;
                closest = prey.transform;
            }
        }

        targetPrey = closest;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Dog") || other.CompareTag("Chicken")) &&
            (targetPrey == null || Vector3.Distance(transform.position, other.transform.position) < Vector3.Distance(transform.position, targetPrey.position)))
        {
            targetPrey = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Dog") || other.CompareTag("Chicken")) && other.transform == targetPrey)
        {
            targetPrey = null;
            FindNewTarget();
        }
    }
}
