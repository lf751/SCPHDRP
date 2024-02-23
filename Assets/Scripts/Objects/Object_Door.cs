using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class Object_Door : MonoBehaviour
{
    public static int openDoorFlag=0, closedDoorFlag = 8;
    [SerializeField]
    private GameObject Door01, Door02;
    [SerializeField]
    private float OpenSpeed, DoorEndPos, DoorTime;
    [SerializeField]
    private int id;

    [SerializeField]
    private AudioClip[] Open_AUD;
    [SerializeField]
    private AudioClip[] SCP_AUD;
    [SerializeField]
    private AudioClip[] Close_AUD;
    [SerializeField]
    private AudioSource AUD;

    [SerializeField]
    private NavMeshObstacle carveMesh;
    [SerializeField]
    private NavMeshLink doorLink;
    [SerializeField]
    private bool UseParticle = false;

    float LastPos1;

    Vector3 Pos1, Pos2;
    public bool switchOpen = false, scp173 = true, ignoreSave = false, isDisabled = false;
    bool IsOpen = false, isForcing = false;

    // Start is called before the first frame update

    private void Awake()
    {
        openDoorFlag = NavMesh.GetAreaFromName("Walkable");
        closedDoorFlag = NavMesh.GetAreaFromName("ClosedDoor");
        AUD = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (!ignoreSave)
        {
            id = GameController.instance.GetDoorID();
            transform.parent = GameController.instance.doorParent.transform;
            resetState();
        }

        Pos1 = Door01.transform.position;
        Pos2 = Door02.transform.position;
        LastPos1 = 10f;
    }

    public void resetState()
    {
        int doorState = GameController.instance.GetDoorState(id);
        if (doorState != -1)
        {
            LastPos1 = 10f;

            if (doorState == 0)
            {
                IsOpen = true;
                switchOpen = false;
            }
            if (doorState == 1)
            {
                IsOpen = false;
                switchOpen = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOpen == false && switchOpen == true)
            DoorOpen();

        if (switchOpen == false && IsOpen == true)
            DoorClose();

        if (isForcing == true)
            Hodor();
    }

    void DoorOpen()
    {
        float tempdis = Vector3.Distance(Door01.transform.position, Pos1 - (Door01.transform.right * DoorEndPos));
        if (tempdis >= 0.02)
        {
            Door01.transform.position -= OpenSpeed * Time.deltaTime * Door01.transform.right;
            if (tempdis > LastPos1)
            {
                Door01.transform.position = Pos1 - (Door01.transform.right * DoorEndPos);
                Door02.transform.position = Pos2 - (Door02.transform.right * DoorEndPos);
                IsOpen = true;
                carveMesh.enabled = false;
                doorLink.area = openDoorFlag;
                if (!ignoreSave)
                    GameController.instance.SetDoorState(true, id);
            }
            else
                LastPos1 = tempdis;

        }
        else
        {
            carveMesh.enabled = false;
            IsOpen = true;
            doorLink.area = openDoorFlag;
            if (!ignoreSave)
                GameController.instance.SetDoorState(true, id);
        }

        tempdis = Vector3.Distance(Door02.transform.position, Pos2 - (Door02.transform.right * DoorEndPos));
        if (tempdis >= 0.02)
        {
            Door02.transform.position -= OpenSpeed * Time.deltaTime * Door02.transform.right;
        }




    }

    void DoorClose()
    {
        float tempdis = Vector3.Distance(Door01.transform.position, Pos1);
        if (tempdis >= 0.00005)
        {
            Door01.transform.position -= -OpenSpeed * Time.deltaTime * Door01.transform.right;
            if (tempdis > LastPos1)
            {
                Door01.transform.position = Pos1;
                Door02.transform.position = Pos2;
                carveMesh.enabled = true;
                doorLink.area = closedDoorFlag;

                if (UseParticle)
                {
                    GameController.instance.particleController.StartParticle(1, transform.position, transform.rotation);
                }

                IsOpen = false;
                if (!ignoreSave)
                    GameController.instance.SetDoorState(false, id);
            }
            else
                LastPos1 = tempdis;
        }
        else
        {
            carveMesh.enabled = true;
            doorLink.area = closedDoorFlag;
            IsOpen = false;
            if (!ignoreSave)
                GameController.instance.SetDoorState(false, id);
        }

        tempdis = Vector3.Distance(Door02.transform.position, Pos2);
        if (tempdis >= 0.05)
            Door02.transform.position -= -OpenSpeed * Time.deltaTime * Door02.transform.right;
    }

    public void DoorSwitch()
    {

        if (!isDisabled)
        {
            if (switchOpen == true && IsOpen == true)
            {
                switchOpen = false;
                PlayClose();
                LastPos1 = 10f;
            }
            if (switchOpen == false && IsOpen == false)
            {
                switchOpen = true;
                PlayOpen();
                LastPos1 = 10f;
            }
        }
    }

    public void InstantSet(bool open)
    {
        if (!isDisabled)
        {
            if (open == false)
            {
                switchOpen = false;
                IsOpen = false;
                Door01.transform.position = Pos1;
                Door02.transform.position = Pos2;
                doorLink.area = closedDoorFlag;
                carveMesh.enabled = false;
                if (!ignoreSave)
                    GameController.instance.SetDoorState(false, id);
            }
            else
            {
                IsOpen = true;
                switchOpen = true;
                Door01.transform.position = Pos1 - (Door01.transform.right * DoorEndPos);
                Door02.transform.position = Pos2 - (Door02.transform.right * DoorEndPos);
                doorLink.area = openDoorFlag;
                carveMesh.enabled = true;
                if (!ignoreSave)
                    GameController.instance.SetDoorState(true, id);
            }
        }
    }

    public void ForceOpen(float time)
    {
        if (!isDisabled)
        {
            isForcing = true;
            DoorTime = time;
        }
    }

    void Hodor()
    {
        if (switchOpen != true)
            DoorSwitch();
        DoorTime -= (Time.deltaTime);
        if (DoorTime <= 0.0f)
        {
            isForcing = false;
            DoorSwitch();
        }
    }



    public bool Door173()
    {
        if (switchOpen == false && IsOpen == false && scp173 == true && !isDisabled)
        {
            switchOpen = true;
            PlaySCP(0);
            LastPos1 = 10f;
            return (true);
        }
        else
        {
            return (false);
        }
    }

    void PlayOpen()
    {
        AUD.clip = Open_AUD[Random.Range(0, Open_AUD.Length)];
        AUD.Play();
    }
    void PlaySCP(int i)
    {
        AUD.clip = SCP_AUD[i];
        AUD.Play();
    }
    void PlayClose()
    {
        AUD.clip = Close_AUD[Random.Range(0, Close_AUD.Length)];
        AUD.Play();
    }
    /// <summary>
    /// GetState() Obtiene el estado de la puerta, True si Abierta
    /// </summary>
    /// <returns>true Si la puerta esta abierta</returns>
    public bool GetState()
    {
        return (IsOpen&&switchOpen);
    }

}
