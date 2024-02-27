using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum OffMeshLinkMoveMethod
{
    Teleport,
    NormalSpeed,
    Parabola,
    Curve
}

[RequireComponent(typeof(NavMeshAgent))]
public class AgentLinkMover : MonoBehaviour
{
    public OffMeshLinkMoveMethod m_Method = OffMeshLinkMoveMethod.Parabola;
    [SerializeField]
    private bool stopAtDoors = true;

    IEnumerator Start()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false;
        while (true)
        {
            if (agent.isOnOffMeshLink)
            {
               // Debug.Log(gameObject.name + " starting off-mesh travel");
                if (m_Method == OffMeshLinkMoveMethod.NormalSpeed)
                    yield return StartCoroutine(NormalSpeed(agent));
                else if (m_Method == OffMeshLinkMoveMethod.Parabola)
                    yield return StartCoroutine(Parabola(agent, 2.0f, 0.5f));

                if (agent.enabled)
                    agent.CompleteOffMeshLink();
                /*else
                    agent.Warp(agent.currentOffMeshLinkData.startPos);*/
            }
            yield return null;
        }
    }

    IEnumerator NormalSpeed(NavMeshAgent agent)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        Quaternion endRota = Quaternion.LookRotation((endPos - startPos).normalized);
        Quaternion startRota = agent.transform.rotation;


        float duration = ((startPos - endPos).magnitude / agent.speed);
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            if(!agent.enabled)
            {
                normalizedTime = 2;
                yield break;
            }
            if (stopAtDoors && data.offMeshLink!=null && data.offMeshLink.area==Object_Door.closedDoorFlag)
            {
                agent.Warp(startPos);
                normalizedTime = 2;
                yield break;
            }
            while (agent.isStopped)
            {
                yield return null;
            }

            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime);
            agent.transform.rotation = Quaternion.Lerp(startRota, endRota, normalizedTime);
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
    }

    IEnumerator Parabola(NavMeshAgent agent, float height, float duration)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            float yOffset = height * 4.0f * (normalizedTime - normalizedTime * normalizedTime);
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
    }
}
