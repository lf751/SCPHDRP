using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



[System.Serializable]
public struct ShyGuyDifficultyLevels
{
    public float distanceTele, timeBeforeTele;
    public int minTele, maxTele;
}

public class SCP_096 : Roam_NPC
{
    enum scp096State { idle, sitting, patrol, panic, run, attack }

    public AudioClip idle, panic, horror, chaseClip, scream;
    Camera mainCamera;
    Plane[] frustum;
    NavMeshAgent agent;
    AudioSource screamSource;
    public ShyGuyDifficultyLevels[] levels;
    public Animator animator;
    public BoxCollider faceCol;
    public float viewLimit, cryTimer, normalSpeed, panicSpeed, accel, panicAccel, turn, panicTurn;
    public LayerMask ground, doors, playerMask;
    public bool debugIsTargeting;
    scp096State state, currAnim;
    float Timer, fallSpeed, framerate = 15, framerate2 = 60, speed, turnSpeed, playerDis, teleportTimer;
    int currNode;
    bool hasPath, isRota, hasSeen = false, isPanicking = false, isRunning = false;
    Vector3 currTarget, movement, tempMovement;
    NavMeshPath currPath;
    Quaternion toRota, fromAngle;
    private const int valState = 1, valIsChase = 2, valTimer = 3, valTeleTimer = 4;

    // Start is called before the first frame update
    void Start()
    {
        state = scp096State.idle;
        currAnim = scp096State.idle;
        mainCamera = Camera.main;
        agent = GetComponent<NavMeshAgent>();
        screamSource = GetComponent<AudioSource>();
    }

    public override NPC_Data getData()
    {
        data.npcvalue[valState] = (int)state;
        data.npcvalue[valTeleTimer] = (int)teleportTimer;
        data.npcvalue[valTimer] = (int)Timer;
        Debug.Log("State saved = " + (scp096State)data.npcvalue[valState]);
        return base.getData();
    }

    // Update is called once per frame
    void Update()
    {
        if (data.isActive)
        {
            if (isEvent)
                NPCEvent();
            else
                NPCUpdate();
        }

        screamSource.mute = (!data.isActive);
    }

    public override void setData(NPC_Data state)
    {
        base.setData(state);
        teleportTimer = state.npcvalue[valTeleTimer];
        Timer = state.npcvalue[valTeleTimer];
        Debug.Log("State loaded = " + (scp096State)state.npcvalue[valState]);
        this.state = (scp096State)state.npcvalue[valState];
        if (this.state == scp096State.run)
            animator.Play("scp096_skeleton|scp096_run", -1, 1);
        agent.Warp(state.Pos.toVector3());
    }

    private void LateUpdate()
    {
        if (currAnim != state)
        {
            if (state != scp096State.panic && state != scp096State.attack && state != scp096State.run && (currAnim == scp096State.attack || currAnim == scp096State.panic || currAnim == scp096State.run))
            {
                //Debug.Log("currANim = " + currAnim + " new state = " + state);
                animator.SetBool("panic", false);
            }


            switch (state)
            {
                case scp096State.idle:
                    {
                        animator.SetTrigger("toIdle");
                        break;
                    }
                case scp096State.sitting:
                    {
                        if (currAnim != scp096State.attack)
                            animator.SetTrigger("toSit");
                        break;
                    }
                case scp096State.patrol:
                    {
                        animator.SetTrigger("toWalk");
                        break;
                    }
                case scp096State.panic:
                    {
                        animator.SetBool("panic", true);
                        break;
                    }
                case scp096State.attack:
                    {
                        Debug.Log("Doing to attack");
                        animator.SetTrigger("toAttack");
                        break;
                    }
                case scp096State.run:
                    {
                        animator.SetTrigger("toWalk");
                        break;
                    }
            }
            currAnim = state;
        }
    }

    private void OnDrawGizmos()
    {
        if (currTarget != null)
            Gizmos.DrawSphere(currTarget, 0.3f);
    }

    void NPCUpdate()
    {



        //Current State
        switch (state)
        {
            case scp096State.panic:
                {
                    agent.ResetPath();
                    break;
                }
            case scp096State.idle:
                {
                    agent.ResetPath();
                    break;
                }
            case scp096State.patrol:
                {
                    agent.speed = normalSpeed;
                    agent.acceleration = accel;
                    agent.angularSpeed = turn;
                    if (!agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathInvalid)
                        agent.SetDestination(getRandomPoint());

                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, 1f, doors))
                    {
                        hit.transform.gameObject.GetComponent<Object_Door>().ForceOpen(5);
                    }

                    break;
                }
            case scp096State.run:
                {
                    agent.speed = panicSpeed;
                    agent.acceleration = panicAccel;
                    agent.angularSpeed = panicTurn;

                    if (Time.frameCount % framerate == 0)
                    {

                        agent.SetDestination(GameController.ins.currPly.transform.position);
                    }

                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, 1f, doors))
                    {
                        hit.transform.gameObject.GetComponent<Object_Door>().ForceOpen(5);
                    }

                    if (Physics.OverlapSphere(transform.position + transform.forward, 0.5f, playerMask).Length > 0)
                    {
                        GameController.ins.deathmsg = Localization.GetString("deathStrings", "death_096");
                        GameController.ins.currPly.Death(4);
                        debugIsTargeting = false;
                        state = scp096State.attack;
                        Timer = 4f;
                    }

                    break;
                }
        }

        teleportTimer += Time.deltaTime;



        if (debugIsTargeting)
        {
            if (Time.frameCount % framerate == 0)
                playerDis = Vector3.Distance(faceCol.transform.position, GameController.ins.currPly.transform.position);

            if ((state != scp096State.attack && state != scp096State.panic && state != scp096State.run) && teleportTimer > levels[data.npcvalue[0]].timeBeforeTele && playerDis > levels[data.npcvalue[0]].distanceTele)
            {
                Vector3 teleportTo;
                teleportTo = GameController.ins.GetPatrol(GameController.ins.player.transform.position, levels[data.npcvalue[0]].maxTele, levels[data.npcvalue[0]].minTele);
                Spawn(true, teleportTo);
                teleportTimer = 0;
            }

            if (!hasSeen && (Time.frameCount % framerate2 == 0) && Encountered())
            {
                hasSeen = true;
                GameController.ins.npcController.npcLevel(npc.scp096);
            }

            if (playerDis > 30f)
                hasSeen = false;

            //Check if seeing head.
            if ((Time.frameCount % framerate == 0 && state != scp096State.attack && state != scp096State.panic && state != scp096State.run) && OnView())
            {
                state = scp096State.panic;
                Timer = cryTimer;
            }

            if (state == scp096State.panic && !isPanicking)
            {
                isPanicking = true;
                screamSource.Stop();
                GameController.ins.ChangeMusic(panic);
                GameController.ins.PlayHorror(horror, this.transform, npc.none);
                GameController.ins.currPly.CognitoHazard(true);
            }
            if (state == scp096State.run && !isRunning)
            {
                MusicPlayer.instance.ChangeMusic(chaseClip);
                isRunning = true;
                screamSource.Stop();
                screamSource.clip = scream;
                screamSource.Play();
            }
        }

        Timer -= Time.deltaTime;

        //Next State
        if (Timer < 0)
        {
            switch (state)
            {
                case scp096State.idle:
                    {
                        int chance = Random.Range(0, 100);
                        if (chance < 25)
                        {
                            state = scp096State.sitting;
                            Timer = Random.Range(10, 30);
                        }
                        else
                        {
                            state = scp096State.patrol;
                            Timer = Random.Range(10, 15);
                            getRandomPoint();
                        }
                        break;
                    }
                case scp096State.patrol:
                case scp096State.sitting:
                    {
                        state = scp096State.idle;
                        Timer = Random.Range(5, 10);
                        break;
                    }
                case scp096State.panic:
                    {
                        state = scp096State.run;
                        break;
                    }
                case scp096State.attack:
                    {
                        state = scp096State.sitting;
                        Timer = 7;
                        break;
                    }
            }
        }
    }

    void ACT_PathDebugging()
    {
        for (int i = 0; i < currPath.corners.Length - 1; i++)
            Debug.DrawLine(currPath.corners[i], currPath.corners[i + 1], Color.red);
    }

    void SetNavPath(Vector3 pos)
    {
        currNode = 0;
        hasPath = NavMesh.CalculatePath(transform.position, pos, NavMesh.AllAreas, currPath);
        if (!hasPath)
            Debug.Log("Path 096 failed!");
    }

    void DoNavPath()
    {
        movement = agent.desiredVelocity;
    }

    Vector3 getRandomPoint()
    {
        currTarget = transform.position + Random.insideUnitSphere * 5;
        currTarget.y = transform.position.y;
        return currTarget;
    }

    void NPCEvent()
    {
        switch (state)
        {
            case scp096State.panic:
                {
                    agent.ResetPath();
                    break;
                }
            case scp096State.idle:
                {
                    agent.ResetPath();
                    break;
                }
            case scp096State.patrol:
                {
                    agent.speed = normalSpeed;
                    if (agent.remainingDistance < 0.5)
                        state = scp096State.idle;

                    break;
                }
            case scp096State.run:
                {
                    agent.speed = panicSpeed;

                    if (agent.remainingDistance < 0.5)
                    {
                        Debug.Log("To Event Attack");
                        state = scp096State.attack;
                    }

                    break;
                }
        }

        if (isRota)
            ACT_Rotation();
    }

    public override void Event_Spawn(bool instant, Vector3 warppoint)
    {
        base.Event_Spawn(instant, warppoint);
        data.isActive = true;
        agent.Warp(warppoint);
        animator.Rebind();
        state = scp096State.idle;
        currAnim = state;
        agent.Warp(warppoint);
        isEvent = true;
    }

    public override void Spawn(bool beActive, Vector3 warppoint)
    {
        base.Spawn(beActive, warppoint);

        NavMeshHit here;
        bool spawn = false;

        if (NavMesh.SamplePosition(warppoint, out here, 0.5f, NavMesh.AllAreas))
        {
            spawn = true;

            Debug.Log("I tried to spawn and it worked");
        }
        else if (NavMesh.SamplePosition(warppoint, out here, 15f, NavMesh.AllAreas))
        {
            spawn = true;
            Debug.Log("I tried to spawn and it worked kinda");
        }
        else
            Debug.Log("I failed to spawn :C ");

        if (spawn)
        {
            data.isActive = beActive;
            animator.Rebind();
            state = scp096State.idle;
            currAnim = state;
            agent.Warp(here.position);
        }
    }

    public override void StopEvent()
    {
        Debug.Log("Finishing event");
        base.StopEvent();
        Timer = 0.5f;
        isEvent = false;
    }

    public void evWalkTo(Vector3 to)
    {
        if (state == scp096State.idle)
            state = scp096State.patrol;
        else if (state == scp096State.panic)
            state = scp096State.run;

        isEvent = true;
        agent.SetDestination(to);
    }
    /// <summary>
    /// Change 096 fake state
    /// </summary>
    /// <param name="newState">state (0 = idle, sitting, patrol, panic, run, attack)</param>
    public void evChangeState(int newState)
    {
        isEvent = true;
        state = (scp096State)newState;
    }


    /// <summary>
    /// Rotates 096 in a event
    /// </summary>
    /// <param name="rotateTo">Point to rotate towards</param>
    public void RotateTo(Vector3 rotateTo)
    {
        toRota = Quaternion.LookRotation((new Vector3(rotateTo.x, transform.position.y, rotateTo.z) - transform.position));
        fromAngle = transform.rotation;
        Timer = 0;
        isRota = true;
        agent.updateRotation = false;
    }

    void ACT_Rotation()
    {
        //Debug.Log("I'm rotating");
        Timer += Time.deltaTime;
        if (Timer > 1f)
        {
            Timer = 1f;
            isRota = false;
            agent.updateRotation = true;
        }

        //lerp!
        float perc = Timer / 1f;
        transform.rotation = Quaternion.Lerp(fromAngle, toRota, perc);
    }




    /// <summary>
    /// Check if SCP 096 face is on the camera view
    /// </summary>
    /// <returns></returns>
    bool OnView()
    {
        frustum = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        Vector3 playerDir = GameController.ins.currPly.transform.position - faceCol.transform.position;
        return GeometryUtility.TestPlanesAABB(frustum, faceCol.bounds) && (Vector3.Dot(playerDir.normalized, faceCol.transform.forward) > viewLimit) && !Physics.Raycast(faceCol.transform.position, playerDir, playerDis, ground) && !GameController.ins.currPly.IsBlinking() && playerDis < 15f;
    }

    bool Encountered()
    {
        frustum = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        return GeometryUtility.TestPlanesAABB(frustum, faceCol.bounds) && !Physics.Raycast(faceCol.transform.position, GameController.ins.currPly.transform.position - faceCol.transform.position, playerDis, ground) && playerDis < 15f;
    }

}
