using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SCP_106 : Roam_NPC
{
    NavMeshAgent _navMeshagent;
    [SerializeField]
    private LayerMask Ground;
    float PlayerDistance= 20, timer, ambianceTimer;
    [SerializeField]
    private GameObject Player, Eyes;
    bool playedHorror, usingAStar = true, isSpawn = false, isBlocked = false, isOut = false, isPath, pathInvalid, eyesActive = true;
    Quaternion toAngle, realAngle;
    [SerializeField]
    private float normalSpeed, crawlSpeed, spawntimer, Distance;
    private float speed;
    float escapeTimer;
    bool Escaped=false, lastDest, isChase=false;
    AudioSource sfx;
    Vector3 Destination;
    int frameInterval=20;
    [SerializeField]
    private AudioClip[] Horror, Sfx;
    [SerializeField]
    private AudioClip music;
    [SerializeField]
    private Animator anim;
    NavMeshHit shit;
    Vector3 velocity;
    Transform[] ActualPath;
    int currentNode;
    [SerializeField]
    private CapsuleCollider col;



    void Awake()
    {
        Player = GameController.instance.player;
        _navMeshagent = this.GetComponent<NavMeshAgent>();
        sfx = GetComponent<AudioSource>();
        _navMeshagent.enabled = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, 0.3f);
        Gizmos.DrawSphere(transform.position + (Vector3.up * 0.5f), 0.3f);
    }

    void Update()
    {
        if (PlayerDistance > 8 && eyesActive != true)
        {
            Eyes.SetActive(true);
            eyesActive = true;
        }
        if (PlayerDistance < 8 && eyesActive != false)
        {
            Eyes.SetActive(false);
            eyesActive = false;
        }

        if (data.isActive)
        {
            if (!isEvent)
            {
                if (Time.frameCount % 10 == 0)
                {
                    PlayerDistance = (Vector3.Distance(Player.transform.position, transform.position));

                    isOut = !NavMesh.SamplePosition(transform.position, out shit, 0.2f, NavMesh.AllAreas);

                    isBlocked = (Physics.CheckSphere(transform.position + (Vector3.up * 0.5f), 0.3f, Ground, QueryTriggerInteraction.Ignore));

                    pathInvalid = ((_navMeshagent.enabled && !_navMeshagent.pathPending && (_navMeshagent.pathStatus == NavMeshPathStatus.PathPartial || _navMeshagent.pathStatus == NavMeshPathStatus.PathInvalid)));
                }

                if (isBlocked)
                    speed = crawlSpeed;
                else
                    speed = normalSpeed;



                timer -= Time.deltaTime;
                if (timer <= 0 && isSpawn == false)
                {
                    isSpawn = true;
                    col.enabled = true;
                }

                escapeTimer += Time.deltaTime;

                if (data.npcvalue[data.npcvalue[0]] == 0 && escapeTimer >= 45)
                {
                    Escaped = true;
                }
                if (data.npcvalue[data.npcvalue[0]] == 1 && escapeTimer >= 75)
                {
                    Escaped = true;
                }


                if (PlayerDistance < 3f || isBlocked || isOut || pathInvalid)
                {
                    usingAStar = false;
                }
                else
                    usingAStar = true;

                DoSFX();


                if (isSpawn)
                {
                    HorrorPlay();
                    if (usingAStar)
                    {
                        _navMeshagent.enabled = true;
                        _navMeshagent.speed = speed;
                        if (Time.frameCount % frameInterval == 0)
                            SetDestination();

                    }
                    else
                    {
                        Vector3 Point = new Vector3(Player.transform.position.x, transform.position.y, Player.transform.position.z) - transform.position;
                        toAngle = Quaternion.LookRotation(Point);
                        realAngle = Quaternion.LookRotation(new Vector3(Player.transform.position.x, Player.transform.position.y - 1f, Player.transform.position.z) - transform.position);
                        _navMeshagent.enabled = false;

                        transform.position += (realAngle * (Vector3.forward * speed)) * Time.deltaTime;
                        transform.rotation = Quaternion.Lerp(transform.rotation, toAngle, 1f * Time.deltaTime);

                        if (pathInvalid && Time.frameCount % frameInterval == 0)
                        {
                            NavMeshPath path = new NavMeshPath();
                            NavMesh.CalculatePath(transform.position, Player.transform.position, NavMesh.AllAreas, path);
                            pathInvalid = (path.status == NavMeshPathStatus.PathPartial || path.status == NavMeshPathStatus.PathInvalid);
                        }

                    }

                    if (data.npcvalue[0] != 0 && PlayerDistance > 20 && !Escaped)
                    {
                        Spawn(true, new Vector3(Player.transform.position.x, 0.01f, Player.transform.position.z));
                    }

                    if (Escaped && Time.frameCount % frameInterval == 0)
                    {
                        float dot = Vector3.Dot(Player.transform.forward, (transform.position - Player.transform.position).normalized);
                        Debug.Log("Dot product of lookAt = " + dot + " using a star " + usingAStar + " distance to latest point " + (usingAStar ? _navMeshagent.remainingDistance : Mathf.Infinity) + " radius " + _navMeshagent.radius); 
                        if ((dot < 0 && ((!usingAStar && PlayerDistance > 8)) || (usingAStar && (_navMeshagent.remainingDistance < _navMeshagent.radius) && PlayerDistance > 8)))
                            UnSpawn();
                    }

                    

                }

                //bool shouldMove = velocity.magnitude > 0.5f;

                // Update animation parameters
                anim.SetBool("move", isSpawn);

            }
            else
            {
                if (isPath)
                    Path();

                anim.SetBool("move", isPath);
            }
        }

        
    }


    void DoSFX()
    {

        ambianceTimer -= Time.deltaTime;
        if (ambianceTimer <= 0)
        {
            sfx.PlayOneShot(Sfx[Random.Range(1, Sfx.Length)]);
            ambianceTimer = 2 * Random.Range(1, 5);
        }
    }

    void HorrorPlay()
    {
        if (PlayerDistance < 16 && PlayerDistance > 4)
        {
                if (playedHorror == false)
                {
                    GameController.instance.PlayHorror(Horror[Random.Range(0, Horror.Length)], this.transform, npc.scp106);
                    playedHorror = true;
                }
        }
    }

    public override void UnSpawn()
    {
        Debug.Log("SCP-106 Unspawning");
        _navMeshagent.enabled = false;
        transform.position = (new Vector3(0, -10, 0));
        data.isActive = false;
        isSpawn = false;
        isChase = false;
        Escaped = false;
        anim.SetBool("move", false);

        GameController.instance.DefMusic();
    }

    public override void Spawn(bool beActive, Vector3 here)
    {
        if (data.state == npcstate.death)
            return;
        transform.position = here;
        _navMeshagent.enabled = true;
        _navMeshagent.Warp(here);
        if (beActive)
        {
            col.enabled = false;
            anim.SetBool("move", false);
            //anim.SetTrigger("spawn");
            anim.PlayInFixedTime("Base Layer.106_2_skeleton|106_2_rise", 0, 0.1f);
            transform.position = here;
            _navMeshagent.enabled = true;
            _navMeshagent.Warp(here);
            data.isActive = true;
            isPath = false;
            isEvent = false;
            isSpawn = false;
            sfx.PlayOneShot(Sfx[0]);

            playedHorror = false;
            here.y += 0.05f;
            RaycastHit ray;
            if (Physics.Raycast(transform.position + (Vector3.up*0.2f), Vector3.down, out ray, 1.5f, Ground, QueryTriggerInteraction.Ignore))
            {
                DecalSystem.instance.Decal(ray.point+(Vector3.up*0.05f), new Vector3(90f, 0, 0), 6f, false, 5f, 2, 0);
            }
            
            if (isChase == false)
            {
                timer = spawntimer;
                escapeTimer = 0;
                GameController.instance.ChangeMusic(music);
            }
            else
                timer = spawntimer - 2;

            isChase = true;
        }
        else
        {
            data.isActive = false;
            anim.SetBool("move", false);
        }

    }


    private void SetDestination()
    {
      if (!Escaped || (Escaped && PlayerDistance < 7))
      {
            Debug.Log("SCP 106 getting path");
            _navMeshagent.SetDestination(Player.transform.position);
      }

    }

    void Path()
    {
        if (Vector3.Distance(new Vector3(ActualPath[currentNode].position.x, transform.position.y, ActualPath[currentNode].position.z), transform.position) < Distance)
        {
            if (currentNode != ActualPath.Length-1)
                currentNode += 1;
            Debug.Log("Nodo " + currentNode + " de " + ActualPath.Length);
        }

        Vector3 FakePoint = new Vector3(ActualPath[currentNode].position.x, transform.position.y, ActualPath[currentNode].position.z) - transform.position;

        Vector3 Point = ActualPath[currentNode].transform.position - transform.position;

  

        Quaternion lookangle = Quaternion.LookRotation(FakePoint);

        transform.position += (Point.normalized * speed) * Time.deltaTime;
        transform.rotation = Quaternion.Lerp(transform.rotation, lookangle, 4f * Time.deltaTime);

        if ((Vector3.Distance(ActualPath[currentNode].transform.position, transform.position) < Distance) && currentNode == ActualPath.Length - 1)
        {
            isPath = false;
            Debug.Log("Path Terminado");
        }
    }

    public void EndPath()
    {
        isPath = false;
    }

    public void SetPath(Transform[] path)
    {
        if (data.state == npcstate.death)
            return;

        Debug.Log("Iniciando Path");
        currentNode = 0;
        ActualPath = path;
        isPath = true;
    }


    public override void Event_Spawn(bool instant, Vector3 here)
    {
        if (data.state == npcstate.death)
            return;

        col.enabled = false;
        data.isActive = true;
        isSpawn = true;
        anim.SetBool("move", false);
        _navMeshagent.enabled = false;
        if (!instant)
        {
            timer = spawntimer;
            anim.SetTrigger("spawn");
            isSpawn = false;

            RaycastHit ray;
            if (Physics.Raycast(here + (Vector3.up * 0.2f), Vector3.down, out ray, 1.5f, Ground, QueryTriggerInteraction.Ignore))
            {
                DecalSystem.instance.Decal(ray.point + (Vector3.up * 0.1f), new Vector3(90f, 0, 0), 6f, false, 5f, 2, 0);
            }
        }
        
        transform.position = here;
        isEvent = true;
        
        sfx.PlayOneShot(Sfx[0]);
        playedHorror = false;
        escapeTimer = 20;

        isChase = true;

    }

    public override void StopEvent()
    {
        if (data.state == npcstate.death)
            return;
        isEvent = false;
        col.enabled = true;
        GameController.instance.ChangeMusic(music);
    }








    private void OnTriggerStay(Collider other)
    {
        if ((isSpawn) && (other.gameObject.CompareTag("Player")) && !isEvent && GameController.instance.isAlive)
        {
            GameController.instance.playercache.Health -= 25;
            if (GameController.instance.playercache.Health > 0)
            {
                _navMeshagent.enabled = false;
                GameController.instance.Death = DeathEvent.pocketDimension;
                other.gameObject.GetComponent<Player_Control>().FakeDeath(2);
                isEvent = true;
                Debug.Log("You are ded ded ded");
            }
        }
    }


}
