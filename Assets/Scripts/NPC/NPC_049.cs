using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


//TODO
//NPCS THAT USE ELEVATOR

[System.Serializable]
public struct DoctorDifficultyLevels
{
    public float distanceTele, timeBeforeTele;
    public int minTele, maxTele, ringStruggleTime;
}

public class NPC_049 : Roam_NPC
{
    enum scp049State { idle, patrol, chase, trail, kill, hearing, soundChase, trailIdle}

    //public AudioClip idle, panic, horror, chaseClip, scream;
    public DoctorDifficultyLevels[] levels;
    Camera mainCamera;
    Plane[] frustum;
    Collider[] closeSounds;
    NavMeshAgent agent;
    AudioSource voiceSource;
    public AudioClip [] Chase, Search, Kill;
    public AudioClip chaseSong, notice714;
    public Animator animator;
    public float viewLimit, normalSpeed, chaseSpeed, listeningRange, closeRange, removeRingTime;
    public LayerMask ground, doors, playerMask, soundLayer;
    public bool debugIsTargeting, allowMapPath=true;
    scp049State state, currAnim;
    float audTimer, Timer, trailTimer, fallSpeed, framerate = 10, teleportTimer=0, framerate2 = 60, distanceFromPlayer=Mathf.Infinity, currRemRing;
    int currNode, currSoundLevel;
    [System.NonSerialized]
    public bool seePlayer;
    bool onPath, isRota, foundSound, hasPath,pathIsMap=false,isPlayingChase, hasNotice714;
    Vector3 currTarget;
    NavMeshPath currPath;
    Quaternion toRota, fromAngle;
    static int valIsPanic = 0, valIsChase = 1, valTimer = 2;

    // Start is called before the first frame update
    void Start()
    {
        state = scp049State.idle;
        currAnim = scp049State.idle;
        mainCamera = Camera.main;
        agent = GetComponent<NavMeshAgent>();
        voiceSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.DrawRay(transform.position + Vector3.up, (GameController.instance.playercache.transform.position - transform.position).normalized);
        if (data.isActive)
        {
            if (isEvent)
                NPCEvent();
            else
                NPCUpdate();
        }
    }

    private void LateUpdate()
    {
        animator.SetFloat("moveSpeed", agent.isOnOffMeshLink && !agent.isStopped? 1 : agent.velocity.magnitude);
        if (currAnim != state)
        {
            switch (state)
            {
                case scp049State.trailIdle:
                case scp049State.hearing:
                    {
                        animator.SetTrigger("toHearing");
                        break;
                    }
                case scp049State.kill:
                    {
                        animator.SetTrigger("toHeal");
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
        
        if (debugIsTargeting && Time.frameCount % framerate == 0)
        {
            distanceFromPlayer = Vector3.Distance(transform.position, GameController.ins.currPly.transform.position);
            seePlayer = CanSee();
            if(seePlayer)
            {
                if (!isPlayingChase)
                {
                    GameController.ins.npcController.npcLevel(npc.scp049);
                    GameController.ins.ChangeMusic(chaseSong);
                    isPlayingChase = true;
                    hasNotice714 = false;
                    currRemRing = removeRingTime;
                    Debug.Log("049 started chase!");
                }
                currTarget = GameController.ins.currPly.transform.position;
                teleportTimer = 0;
            }
            
            if (state != scp049State.chase && state != scp049State.kill && seePlayer)
            {
                data.npcvalue[valIsPanic] = 1;
                state = scp049State.chase;
                audTimer = 0;
            }

            if (distanceFromPlayer > 18f && isPlayingChase)
            {
                GameController.ins.DefMusic();
                isPlayingChase = false;
            }
        }

        //Debug.Log("049 state: " + state);



        teleportTimer += Time.deltaTime;

        if (Time.frameCount % framerate == 0 && !foundSound)
        {
            CheckSounds();
        }

        if (Time.frameCount % framerate2 == 0 && teleportTimer > levels[data.npcvalue[0]].timeBeforeTele && distanceFromPlayer > levels[data.npcvalue[0]].distanceTele)
        {
            Vector3 teleportTo;
            teleportTo = GameController.ins.GetPatrol(GameController.ins.player.transform.position, levels[data.npcvalue[0]].maxTele, levels[data.npcvalue[0]].minTele);
            Spawn(true, teleportTo);
            teleportTimer = 0;
            agent.isStopped = false;
            allowMapPath = true;
        }
         ///DEBUG DATA
        //Debug.DrawRay(transform.position + Vector3.up, (GameController.instance.playercache.transform.position - transform.position));
        //Debug.Log("Dot de vision " + Vector3.Dot((GameController.instance.playercache.transform.position - transform.position).normalized, transform.forward));


        //Current State
        switch (state)
        {
            case scp049State.idle:
                {
                    onPath = false;
                    agent.ResetPath();
                    if (foundSound)
                    {
                        if (currSoundLevel < 2 )
                        {
                            audTimer = 0;
                            foundSound = false;
                            state = scp049State.hearing;
                            Timer = Random.Range(2, 5);
                        }
                        else
                        {
                            audTimer = 0;
                            state = scp049State.soundChase;
                            onPath = false;
                        }
                        foundSound = false;
                    }
                    break;
                }
            case scp049State.trailIdle:
            case scp049State.hearing:
                {
                    agent.ResetPath();
                    if (foundSound)
                    {
                        if (currSoundLevel > 0)
                        {
                            audTimer = 0;
                            state = scp049State.soundChase;
                            onPath = false;
                        }
                    }
                    break;
                }
            case scp049State.trail:
                {
                    agent.speed = normalSpeed;

                    if (foundSound)
                    {
                        if (currSoundLevel < 1)
                        {
                            audTimer = 0;
                            state = scp049State.hearing;
                            Timer = Random.Range(1, 4);
                            foundSound = false;
                        }
                        else
                        {
                            audTimer = 0;
                            state = scp049State.soundChase;
                            onPath = false;
                        }
                    }

                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, 0.5f, doors))
                    {
                        if (!hit.transform.gameObject.GetComponent<Object_Door>().GetState())
                        {
                            agent.isStopped = true;
                            agent.velocity = Vector3.zero;
                            Timer += Time.deltaTime;
                        }
                        else
                        {
                            agent.isStopped = false;
                            if (!agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathInvalid)
                                agent.SetDestination(getRandomPoint());
                        }
                        hit.transform.gameObject.GetComponent<Object_Door>().ForceOpen(5);
                    }
                    else
                    {
                        agent.isStopped = false;
                        if (!agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathInvalid)
                            agent.SetDestination(getRandomPoint());
                    }

                    break;
                }
            case scp049State.patrol:
                {
                    agent.speed = normalSpeed;
                    
                    

                    if (foundSound)
                    {
                        if (currSoundLevel < 2)
                        {
                            audTimer = 0;
                            state = scp049State.hearing;
                            Timer = Random.Range(2, 4);
                            foundSound = false;
                        }
                        else
                        {
                            audTimer = 0;
                            state = scp049State.soundChase;
                        }
                    }

                    if (agent.hasPath && agent.remainingDistance < (pathIsMap ? 5 : 0.5f))
                    {
                        state = scp049State.idle;
                        onPath = false;
                        hasPath = false;
                        Timer = Random.Range(3, 7);
                    }

                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, 0.5f, doors))
                    {
                        if (!hit.transform.gameObject.GetComponent<Object_Door>().GetState())
                        {
                            agent.isStopped = true;
                            agent.velocity = Vector3.zero;
                            Timer += Time.deltaTime;
                        }
                        else
                        {
                            agent.isStopped = false;
                            if (!onPath)
                            {
                                agent.SetDestination(getPatrol());
                                onPath = true;
                            }
                        }
                        hit.transform.gameObject.GetComponent<Object_Door>().ForceOpen(5);
                    }
                    else
                    {
                        agent.isStopped = false;
                        if (!onPath)
                        {
                            agent.SetDestination(getPatrol());
                            onPath = true;
                        }
                    }

                    break;
                }
            case scp049State.soundChase:
                {
                    agent.speed = normalSpeed;
                    


                    if (agent.hasPath && agent.remainingDistance < 0.5f)
                    {
                        if (trailTimer > 0)
                        {
                            state = scp049State.trail;
                            Timer = Random.Range(5, 10);
                        }
                        else
                        {
                            state = scp049State.idle;
                            Timer = Random.Range(2, 5);
                        }
                        onPath = false;
                        currSoundLevel = 0;
                        
                    }

                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, 0.5f, doors))
                    {
                        if (!hit.transform.gameObject.GetComponent<Object_Door>().GetState())
                        {
                            agent.isStopped = true;
                            agent.velocity = Vector3.zero;
                            Timer += Time.deltaTime;
                        }
                        else
                        {
                            agent.isStopped = false;
                            if (!onPath)
                            {
                                agent.SetDestination(currTarget);
                                onPath = true;
                            }
                        }
                        hit.transform.gameObject.GetComponent<Object_Door>().ForceOpen(5);
                    }
                    else
                    {
                        agent.isStopped = false;
                        if (!onPath)
                        {
                            agent.SetDestination(currTarget);
                            onPath = true;
                        }
                    }
                    break;
                }
            case scp049State.chase:
                {
                    foundSound = false;
                    currSoundLevel = 0;
                    agent.speed = chaseSpeed;

                    if (Physics.OverlapSphere(transform.position + transform.forward, 0.5f, playerMask).Length > 0 && !GameController.ins.currPly.godmode)
                    {
                        bool isProtected = false;
                        int part = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            GameItem currItem = GameController.ins.currPly.equipment[i];
                            if (currItem == null)
                                continue;
                            Item itemData = ItemController.instance.items[currItem.itemFileName];
                            if (itemData is Equipable_Wear && ((Equipable_Wear)itemData).protectCogn)
                            {
                                isProtected = true;
                                part = i;
                                break;
                            }
                        }
                        agent.isStopped = true;
                        if (isProtected){
                            currRemRing -= Time.deltaTime;
                            if (!hasNotice714)
                            {
                                hasNotice714 = true;
                                PlayVoice(4);
                                Debug.Log("Stop strugling");
                            }
                            

                            if (currRemRing < 0f)
                            {
                                GameController.ins.currPly.ACT_UnEquip((bodyPart)part);
                            }
                            else
                            {
                                break;
                            }
                        }
                        

                        //agent.isStopped = true;
                        agent.velocity = Vector3.zero;
                        GameController.ins.deathmsg = Localization.GetString("deathStrings", "death_049");
                        GameController.ins.currPly.Death(0);
                        Debug.Log("Kill");
                        state = scp049State.kill;
                        PlayVoice(3);
                    }
                    else{
                        if (agent.hasPath && agent.remainingDistance < 0.5f && distanceFromPlayer > 6f)
                        {
                            state = scp049State.trail;
                            Timer = Random.Range(5, 10);
                            trailTimer = Random.Range(20, 30);
                        }


                        RaycastHit hit;
                        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, 0.5f, doors))
                        {
                            if (!hit.transform.gameObject.GetComponent<Object_Door>().GetState())
                            {
                                agent.isStopped = true;
                                agent.velocity = Vector3.zero;
                                Timer += Time.deltaTime;
                                animator.SetBool("reach", false);
                            }
                            else
                            {
                                agent.isStopped = false;
                                if (Time.frameCount % framerate == 0)
                                {
                                    if (seePlayer || distanceFromPlayer < 10f)
                                        agent.SetDestination(GameController.ins.currPly.transform.position);
                                }
                                animator.SetBool("reach", distanceFromPlayer < 5 && state != scp049State.kill);
                            }
                            hit.transform.gameObject.GetComponent<Object_Door>().ForceOpen(5);
                        }
                        else
                        {
                            agent.isStopped = false;
                            if (Time.frameCount % framerate == 0)
                            {
                                if (seePlayer || distanceFromPlayer < 10f)
                                    agent.SetDestination(GameController.ins.currPly.transform.position);
                            }
                        }
                    }

                    break;
                }
        }

        Timer -= Time.deltaTime;
        trailTimer -= Time.deltaTime;
        audTimer -= Time.deltaTime;

        if (trailTimer < 0 && (state == scp049State.trailIdle || state == scp049State.trail))
        {
            state = scp049State.idle;
            Timer = Random.Range(3, 6);
        }

        //Next State
        if (Timer < 0)
        {
            switch (state)
            {
                case scp049State.trailIdle:
                    {


                        state = scp049State.trail;
                        Timer = Random.Range(3, 10);
                        getRandomPoint();

                        break;
                    }
                case scp049State.trail:
                    {
                        state = scp049State.trailIdle;
                        Timer = Random.Range(3, 10);
                        break;
                    }
                case scp049State.idle:
                    {
                        state = scp049State.patrol;
                        Timer = Random.Range(10, 15);
                        //getRandomPoint();

                        break;
                    }
                case scp049State.hearing:
                case scp049State.patrol:
                    {
                        if (trailTimer > 0)
                            state = scp049State.trail;
                        else
                            state = scp049State.idle;
                        Timer = Random.Range(3, 7);
                        break;
                    }
            }
        }

        //Audio state
        if (audTimer < 0)
        {
            switch (state)
            {
                case scp049State.hearing:
                case scp049State.trail:
                case scp049State.trailIdle:
                    {

                        PlayVoice(1);
                        audTimer = Random.Range(8, 15);

                        break;
                    }
                case scp049State.soundChase:
                case scp049State.chase:
                    {
                        PlayVoice(2);
                        audTimer = Random.Range(8, 13);
                        break;
                    }
            }
        }
    }

    void CheckSounds()
    {
        float lastdistance = 100f;
        float currdistance;
        int tempSound;
        foundSound = false;
        WorldSound currentSound;
        closeSounds = Physics.OverlapSphere(transform.position, listeningRange, soundLayer);
        if (closeSounds.Length != 0)
        {
            for (int i = 0; i < closeSounds.Length; i++)
            {
                currentSound = closeSounds[i].gameObject.GetComponent<WorldSound>();
                tempSound = currentSound.SoundLevel;
                currdistance = Vector3.Distance(transform.position, closeSounds[i].transform.position);
                if (currdistance > closeRange)
                    tempSound -= 1;

                if (currSoundLevel < tempSound)
                {
                    if (currdistance < lastdistance)
                    {
                        lastdistance = currdistance;
                        currTarget = closeSounds[i].gameObject.transform.position;
                        currSoundLevel = tempSound;
                        //soundlevel = "sonido " + data.npcvalue[valSoundLevel] + " distancia " + currdistance + " ";
                        foundSound = true;
                    }

                }
            }
        }
    }

    Vector3 getRandomPoint()
    {
        currTarget = transform.position + Random.insideUnitSphere * 5;
        currTarget.y = transform.position.y;
        return currTarget;
    }

    Vector3 getPatrol()
    {
        if (!hasPath)
        {
            if (!GameController.ins.mapless && allowMapPath)
            {
                currTarget = GameController.ins.GetPatrol(transform.position, 4, 0);
                pathIsMap = true;
                hasPath = true;
            }
            else
            {
                currTarget = getRandomPoint();
                pathIsMap = false;
                hasPath = true;
            }
        }
        return currTarget;
    }

    /// <summary>
    /// PLays a sound from any library
    /// </summary>
    /// <param name="library"></param>
    void PlayVoice(int library)
    {
        voiceSource.Stop();
        float delay = 0;
        if (library == 1)
        {
            voiceSource.clip = Search[Random.Range(0, Search.Length)];
            delay = 0.5f;
        }
        if (library == 2)
        {
            voiceSource.clip = Chase[Random.Range(0, Chase.Length)];
            delay = 2;
        }
        if (library == 3)
        {
            voiceSource.clip = Kill[Random.Range(0, Kill.Length)];
            delay = 0;
        }
        if (library == 4)
        {
            voiceSource.clip = notice714;
            delay = 0;
        }
        voiceSource.PlayDelayed(delay);
        if(distanceFromPlayer < 15f)
            SubtitleEngine.instance.playVoice(voiceSource.clip.name, true);
    }

    void NPCEvent()
    {
        if (Time.frameCount % framerate == 0)
        {
            
            distanceFromPlayer = Vector3.Distance(transform.position, GameController.ins.currPly.transform.position);
            //Debug.Log("Distance " + distanceFromPlayer);
            seePlayer = CanSee();
        }

        switch (state)
        {
            case scp049State.idle:
                {
                    agent.ResetPath();
                    break;
                }
            case scp049State.patrol:
                {
                    agent.speed = chaseSpeed;
                    
                    if (Time.frameCount % framerate == 0)
                    {
                        agent.SetDestination(currTarget);
                        if (agent.hasPath && agent.remainingDistance < 0.5f)
                        {
                            state = scp049State.idle;
                        }
                    }
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, 4f, doors))
                    {
                        hit.transform.gameObject.GetComponent<Object_Door>().ForceOpen(5);
                    }
                    break;
                }
        }

        if (isRota)
            ACT_Rotation();
    }

    public override void Event_Spawn(bool instant, Vector3 warppoint)
    {
        agent.speed = chaseSpeed;
        data.isActive = true;
        isEvent = true;
        agent.isStopped = false;
        base.Event_Spawn(instant, warppoint);
        agent.Warp(warppoint);
        animator.Rebind();
        state = scp049State.idle;
        currAnim = state;
        agent.Warp(warppoint);
        

    }

    public override void Spawn(bool beActive, Vector3 warppoint)
    {
        base.Spawn(beActive, warppoint);
        data.isActive = beActive;
        animator.Rebind();
        state = scp049State.idle;
        currAnim = state;
        if(agent.Warp(warppoint))
            agent.isStopped=false;
    }

    public override void StopEvent()
    {
        Debug.Log("Finishing event");
        base.StopEvent();
        Timer = 0;
        isEvent = false;

        data.isActive = true;
        animator.Rebind();
        state = scp049State.idle;
        currAnim = state;
    }

    public void evWalkTo(Vector3 to)
    {
        isEvent = true;
        currTarget = to;
        state = scp049State.patrol;
        agent.SetDestination(currTarget);
        Debug.Log("Walking Event");
    }
    /// <summary>
    /// Change 096 fake state
    /// </summary>
    /// <param name="newState">state (0 = idle, sitting, patrol, panic, run, attack)</param>
    public void evChangeState(int newState)
    {
        isEvent = true;
        state = (scp049State)newState;
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
    }

    void ACT_Rotation()
    {
        //Debug.Log("I'm rotating");
        Timer += Time.deltaTime;
        if (Timer > 1f)
        {
            Timer = 1f;
            isRota = false;
        }

        //lerp!
        float perc = Timer / 1f;
        transform.rotation = Quaternion.Lerp(fromAngle, toRota, perc);
    }

    public void ForceTarget(Vector3 target)
    {
        currTarget = target;
        teleportTimer = 0;
    
        if (state != scp049State.chase && state != scp049State.kill)
        {
            data.npcvalue[valIsPanic] = 1;
            state = scp049State.chase;
            Timer = 5f;
            audTimer = 0;
            seePlayer = true;
            distanceFromPlayer = 4f;
        }
        agent.SetDestination(target);
        Debug.Log("049 forcing state! target: " + currTarget);

    }




    /// <summary>
    /// Check if SCP 049 can see the player;
    /// </summary>
    /// <returns></returns>
    public bool CanSee()
    {
        Vector3 playerDir = (GameController.ins.currPly.transform.position - this.transform.position).normalized;
        if(distanceFromPlayer < 3f || ((Vector3.Dot(playerDir, this.transform.forward) > viewLimit) && !Physics.Raycast(this.transform.position + (Vector3.up*1.5f), playerDir, distanceFromPlayer, ground) && distanceFromPlayer < 20f))
        {
            //Debug.Log("Intern I see you");
            return true;
        }
        else
        {
            //Debug.Log("I dont see you");
            return (false);
        }
        
    }
}

