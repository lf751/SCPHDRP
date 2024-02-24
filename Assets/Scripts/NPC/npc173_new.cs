using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public struct StatueDifficultyLevels
{
    public float maxStep, stepWait, doorTime, doorCoolDown, teleCool;
    public int minTele, maxTele;
    public bool canPatrol;
}



public class npc173_new : Roam_NPC
{
    public CapsuleCollider controller;
    public int frameInterval;
    //public float maxStep, stepWait;
    public LayerMask groundMask, doorMask;
    public bool debugNoTargeting;
    public float horrorNearDistance, horrorFarDistance, horrorResetDistance, targetSnap, teleportDistance;
    public AudioClip[] horrorNearClips, horrorFarClips;
    public AudioClip teleportClip;
    public StatueDifficultyLevels[] statueLevels;
    public Transform head;

    Collider scrambleBox;

    AudioSource stoneDrag;
    RaycastHit colHit;
    Camera mainCamera;
    Plane[] frustum;
    NavMeshPath path;
    bool hasPath, needsPath, targetingPlayer, horrorNear = false, horrorFar = true, directView = false, onPursuit, currCanPatrol, hasDoor, newTarget = false, isVisible = false, doScramble = false, onPath, readyForTele;
    Vector3 currentPoint, currentTarget, nextMove, randomPoint;
    Quaternion nextRotation;
    int currentPathNode, currMinTele, currMaxTele;
    float currTimer, currMaxStep, currStepWait, distanceToPlayer = Mathf.Infinity, doorTimer, currDoorTime, currDoorCooldown, doorCoolTimer, currTeleCool, teleTimer;


    // Start is called before the first frame update
    void Awake()
    {
        path = new NavMeshPath();
        stoneDrag = GetComponent<AudioSource>();
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        ACT_Difficulty();

        currTimer -= Time.deltaTime;
        doorCoolTimer -= Time.deltaTime;
        doorTimer -= Time.deltaTime;
        teleTimer -= Time.deltaTime;

        if (data.isActive)
        {

            if (!debugNoTargeting)
            {
                distanceToPlayer = Vector3.Distance(transform.position, GameController.ins.player.transform.position);
                directView = CheckDirectView();
                isVisible = OnView();
            }

            if (Time.frameCount % frameInterval == 0)
            {
                ACT_GetPath();
            }
            //Debug.DrawLine(new Vector3(transform.position.x, GameController.instance.player.transform.position.y, transform.position.z) - (transform.forward * (controller.radius + 0.5f)), new Vector3(transform.position.x, GameController.instance.player.transform.position.y, transform.position.z) - (transform.forward * (controller.radius + 0.5f)) + (Vector3.up * 0.6f), Color.red);


            /*if (hasPath)
            {
                ACT_PathDebugging();
            }*/

            if (horrorNear == true && distanceToPlayer > horrorFarDistance)
                horrorNear = false;

            if (horrorFar == true && distanceToPlayer > horrorResetDistance)
                horrorFar = false;

            if (distanceToPlayer < horrorFarDistance)
                onPursuit = true;


            if (CanMove())
            {
                if (currTimer < 0)
                {
                    ACT_Move();
                }
                if (!stoneDrag.isPlaying)
                    stoneDrag.Play();

                if (horrorNear == false && distanceToPlayer < horrorNearDistance && directView)
                {
                    GameController.ins.PlayHorror(horrorNearClips[Random.Range(0, horrorNearClips.Length)], head, npc.scp173);
                    horrorNear = true;
                }
            }
            else
            {
                currTimer = 0;
                if (stoneDrag.isPlaying)
                    stoneDrag.Pause();

                if (horrorFar == false && distanceToPlayer < horrorFarDistance && directView)
                {
                    GameController.ins.PlayHorror(horrorFarClips[Random.Range(0, horrorFarClips.Length)], head, npc.scp173);
                    horrorFar = true;
                    horrorNear = false;
                }
            }
        }
    }

    void ACT_PathDebugging()
    {
        for (int i = 0; i < path.corners.Length - 1; i++)
            Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
    }

    void ACT_GetPath()
    {
        if (!debugNoTargeting)
        {
            if (distanceToPlayer > teleportDistance)
            {
                if (!readyForTele)
                {
                    teleTimer = currTeleCool;
                    readyForTele = true;
                }
                if (teleTimer < 0)
                {
                    //Debug.Log("Preparing for 173 teleport");
                    do
                    {
                        currentTarget = GameController.ins.GetPatrol(GameController.ins.player.transform.position, currMaxTele, currMinTele);
                    }
                    while (!GameController.ins.PlayerNotHere(currentTarget));
                    Debug.Log("Teleporting 173 teleported!");
                    Spawn(true, currentTarget);
                    teleTimer = currTeleCool;
                }
            }

            if (onPursuit)
            {
                currentTarget = GameController.ins.currPly.gameObject.transform.position;
                needsPath = true;
            }
            else if ((currCanPatrol) && newTarget)
            {
                currentTarget = GameController.ins.GetPatrol(transform.position, 4, 0);
                needsPath = true;
                newTarget = false;
            }
            if (!currCanPatrol && !onPursuit)
            {
                needsPath = false;
            }

            if (needsPath)
            {
                currentPathNode = 0;
                //Debug.Log("SCP 173 getting path");
                hasPath = NavMesh.CalculatePath(transform.position, currentTarget, NavMesh.AllAreas, path);
                needsPath = !hasPath;
                newTarget = !hasPath;
            }
        }
        else
            hasPath = false;
    }

    void ACT_Move()
    {
        float distance = 0, distanceTarget;
        if (hasPath && !needsPath && currentPathNode < path.corners.Length && !doScramble)
        {

            onPath = true;
            do
            {
                //Debug.Log("173 Moving on path");
                currentPoint = path.corners[currentPathNode];
                if (currentPathNode == (path.corners.Length - 1))
                {
                    needsPath = true;
                }
                distance = Vector3.Distance(currentPoint, transform.position);
                if (distance < 1)
                    currentPathNode++;
            }
            while (distance < 1 && !needsPath);
        }
        else
        {
            onPath = false;
            needsPath = true;
            currentPoint = transform.position + Random.insideUnitSphere * 5;
            currentPoint.y = transform.position.y;
            if (doScramble)
            {
                currentPoint = scrambleBox.ClosestPoint(currentPoint);
                /* if ((closePoint-currentPoint).sqrMagnitude < 0.5)
                     currentPoint = */
            }
            //Debug.Log("173 Random Point!");
        }

        distanceTarget = Vector3.Distance(currentTarget, transform.position);
        distance = Vector3.Distance(currentPoint, transform.position);

        if (!hasDoor)
        {

            //Debug.Log("Entre el punto " + currentPathNode + " " + currentPoint + " y mi lugar " + transform.position + " hay una distancia de " + distance);
            if (distance > currMaxStep)
            {
                nextMove = transform.position + (currentPoint - transform.position).normalized * currMaxStep;
                currTimer = currMaxStep * currStepWait;
            }
            else
            {
                nextMove = currentPoint;
                currTimer = distance * currStepWait;
                if (onPath)
                    currentPathNode++;
            }

            if (Physics.Raycast(transform.position + (Vector3.up * 0.75f), (currentPoint - transform.position).normalized, out colHit, distance, groundMask))
            {
                nextMove = colHit.point + (((transform.position - currentPoint).normalized) * (controller.radius + 0.2f));
                nextMove -= (Vector3.up * 0.6f);
                needsPath = true;
            }


            if (distanceTarget < targetSnap)
            {
                if (Physics.Raycast(transform.position + (Vector3.up * 0.75f), (currentTarget - transform.position).normalized, out colHit, distanceTarget, groundMask))
                {
                    needsPath = true;
                    newTarget = true;
                    nextMove = colHit.point + (((transform.position - currentTarget).normalized) * (controller.radius + 0.5f));
                    currTimer = targetSnap * currStepWait;
                    //Debug.Log("Snap! " + distanceTarget);
                }
            }

            Vector3 movement = new Vector3(nextMove.x, transform.position.y, nextMove.z) - transform.position;

            if (movement.sqrMagnitude > Mathf.Epsilon)
            {
                nextRotation = Quaternion.LookRotation(movement.normalized);
            }
        }
        if (!doScramble)
        {
            if (Physics.Raycast(transform.position + (Vector3.up * 0.6f), (currentPoint - transform.position).normalized, out colHit, distance, doorMask))
            {
                if (!colHit.transform.gameObject.GetComponent<Object_Door>().GetState())
                {
                    needsPath = false;
                    if (hasDoor == false)
                    {
                        hasDoor = true;
                        doorTimer = currDoorTime;
                    }
                    else if (doorTimer < 0)
                    {

                        hasDoor = false;
                        if (colHit.transform.gameObject.GetComponent<Object_Door>().Door173())
                        {
                            doorCoolTimer = currDoorCooldown;
                            needsPath = true;
                        }
                    }
                    nextMove = transform.position;
                    nextRotation = Quaternion.LookRotation(new Vector3(colHit.point.x, transform.position.y, colHit.point.z) - transform.position);
                }
                else
                {
                    hasDoor = false;
                    doorTimer = 0;
                }
            }
            else
            {
                hasDoor = false;
                doorTimer = 0;
            }
        }
        else
        {
            hasDoor = false;
            doorTimer = 0;
        }


        if (Physics.Raycast(nextMove + (Vector3.up * 0.6f), Vector3.down, out colHit, 2, groundMask))
        {
            transform.position = colHit.point;
            transform.rotation = nextRotation;
        }
        else
        {
            needsPath = true;
        }
    }

    /// <summary>
    /// Adjust Difficulty dependent values
    /// </summary>
    void ACT_Difficulty()
    {
        currMaxStep = statueLevels[data.npcvalue[0]].maxStep;
        currStepWait = statueLevels[data.npcvalue[0]].stepWait;
        currCanPatrol = statueLevels[data.npcvalue[0]].canPatrol;
        currDoorTime = statueLevels[data.npcvalue[0]].doorTime;
        currDoorCooldown = statueLevels[data.npcvalue[0]].doorCoolDown;
        currMinTele = statueLevels[data.npcvalue[0]].minTele;
        currMaxTele = statueLevels[data.npcvalue[0]].maxTele;
        currTeleCool = statueLevels[data.npcvalue[0]].teleCool;
    }

    /// <summary>
    /// Returns if SCP 173 can move
    /// </summary>
    /// <returns></returns>
    bool CanMove()
    {
        return (GameController.ins.currPly.IsBlinking() || !isVisible) && doorCoolTimer < 0;
    }

    /// <summary>
    /// Check if SCP 173 is on the camera view
    /// </summary>
    /// <returns></returns>
    bool OnView()
    {
        frustum = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        return GeometryUtility.TestPlanesAABB(frustum, controller.bounds);
    }

    /// <summary>
    /// Checks if SCP 173 is in view not blocked by any object.
    /// </summary>
    /// <returns></returns>
    bool CheckDirectView()
    {
        Vector3 playerPos = GameController.ins.player.transform.position + new Vector3(0, 0.5f, 0);
        if (!Physics.Raycast(playerPos, head.position - playerPos, (head.position - playerPos).magnitude, groundMask))
            return true;
        else
            return false;
    }

    private void FixedUpdate()
    {
        doScramble = false;
    }
    private void OnTriggerStay(Collider other)
    {
        if ((CanMove() && (other.gameObject.CompareTag("Player")) && data.isActive && GameController.ins.isAlive && !GameController.ins.currPly.godmode))
        {
            GameController.ins.currPly.playerWarp(new Vector3(transform.position.x, GameController.ins.player.transform.position.y, transform.position.z) + (transform.forward * (controller.radius + 0.7f)), 180);

            GameController.ins.deathmsg = Localization.GetString("deathStrings", "death_173");
            if (GameController.ins.currentRoom.Equals("Light_2-Way_Doors"))
                GameController.ins.deathmsg = Localization.GetString("deathStrings", "death_173_doors");
            if (GameController.ins.currPly.onCam)
                GameController.ins.deathmsg = Localization.GetString("deathStrings", "death_173_surv");
            if (!GameController.ins.doGameplay)
                GameController.ins.deathmsg = Localization.GetString("deathStrings", "death_173_intro");

            data.isActive = false;

            if (stoneDrag.isPlaying)
                stoneDrag.Pause();

            other.gameObject.GetComponent<PlayerControl>().Death(1);
            //data.isActive = false;
        }

        if (other.gameObject.CompareTag("Scramble"))
        {
            scrambleBox = other;
            doScramble = true;
        }
    }


    public override void Spawn(bool beActive, Vector3 warppoint)
    {
        horrorNear = false;
        horrorFar = false;
        hasDoor = false;
        needsPath = true;
        newTarget = true;
        onPursuit = false;
        readyForTele = false;

        if (!beActive)
        {
            transform.position = warppoint;
            data.isActive = beActive;
        }
        else
        {
            if (distanceToPlayer > horrorResetDistance)
            {
                data.isActive = beActive;

                NavMeshHit here;
                //Debug.Log("173 Trying to spawn");
                if (NavMesh.SamplePosition(warppoint, out here, 0.5f, NavMesh.AllAreas))
                {
                    transform.position = warppoint;

                    //Debug.Log("173 I tried to spawn and it worked");
                }
                else if (NavMesh.SamplePosition(warppoint, out here, 15f, NavMesh.AllAreas))
                {
                    transform.position = here.position;
                    data.isActive = beActive;
                    //Debug.Log("173 I tried to spawn and it worked kinda");
                }
                else
                    Debug.Log("173 I failed to spawn :C ");

                if (beActive)
                {
                    stoneDrag.PlayOneShot(teleportClip);
                }
            }
            /*else
                Debug.Log("173 Im too close to respawn!");*/
        }
    }

    public override void Event_Spawn(bool instant, Vector3 here)
    {
        if (Physics.Raycast(here + (Vector3.up * 0.6f), Vector3.down, out colHit, 2, groundMask))
        {
            transform.position = colHit.point;
        }
        else
            transform.position = here;

        readyForTele = false;
        horrorNear = false;
        horrorFar = false;
        hasDoor = false;
        needsPath = true;
        newTarget = true;
        onPursuit = false;
        data.isActive = true;
    }

}
