using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class SCP895Controller : MonoBehaviour
{
    // Start is called before the first frame update
    public Material monitorRend;
    public Texture2D fakeTexture;
    [System.NonSerialized]
    public GameObject currMonitor;
    public Texture2D[] jumpscareTextures;
    public static MaterialPropertyBlock matBlock;

    [System.NonSerialized]
    public static SCP895Controller instance;

    [SerializeField]
    AudioClip[] horrorSoundEffect;
    [SerializeField]
    AudioSource heartBeat;
    [SerializeField]
    Volume effectVol;

    [Header("General")]
    public float maxIntensity;
    public float deathIntensity;
    public float cognitoIntensity;
    public float maxDistance;
    public float currInten;
    public float minVal;

    public float minRanHigh, minRanLow, maxRanHigh, maxRanLow;

    public float currentStatTime, firstScareTime;

    [Header("Chances")]
    public float chanceBarsLow;
    public float chanceBarsHigh, chanceWaveLow, chanceWaveHigh, chanceTileLow, chanceTileHigh, chanceJumpLow, chanceJumpHigh;
    public float postTargetSpeed;

    [Header("Bars")]
    public float minBarsLow;
    public float maxBarsLow, minBarsHigh, maxBarsHigh;

    [Header("Wave")]
    public float minWaveLow;
    public float maxWaveLow, minWaveHigh, maxWaveHigh;

    [Header("Tile")]
    public float minTileLow;
    public float maxTileLow, minTileHigh, maxTileHigh;

    [Header("Jumpscare")]
    public float minJumpLow;
    public float maxJumpLow, minJumpHigh, maxJumpHigh;

    float currBarTime, currTileTime, currJumpTime, currJumpSpeed;

    [Header("Short times")]
    public float minBarTime;
    public float maxBarTime, minTileTime, maxTileTime, minJumpTime, maxJumpTime, jumpDuration;

    bool doingBars, doingWave, doingTile, doingJump, settedWave, doingCognito;

    public AnimationCurve jumpScareCurve;
    bool firstScare = false, secondScare=false, doSecondScare=false;
    [System.NonSerialized]
    public bool OnView, is895On=true;

    float effectWeightVel, effectWeightTarget=0, effectWeight;

    private void Awake()
    {
        instance = this;
        OnView = false;
        currInten = minVal;
        monitorRend.SetTexture("_ScreenTex", SCP895Controller.instance.fakeTexture);
        doingCognito = false;
    }


    // Update is called once per frame
    void Update()
    {
        if (GameController.ins.isAlive && GameController.ins.isStart)
        {

            if(currMonitor != null)
            {
                //Debug.Log("Curr Inten: " + currInten);
                if((Time.frameCount % 15 == 0))
                {
                    Vector2Int roomPos = GameController.ins.GetRoom("Heavy_COFFIN");
                    if (roomPos.x == -1)
                    {
                        GameController.ins.globalBools[7] = false;
                    }


                    //Debug.Log("Frame time!");
                    if (currMonitor.transform.position.SqrDistance(GameController.ins.currPly.transform.position) < (maxDistance*maxDistance))
                    {
                        if (currInten > firstScareTime)
                        {
                            if (Vector3.Dot(currMonitor.transform.forward, currMonitor.transform.position.DirectionTo(GameController.ins.currPly.transform.position)) < 0)
                                GameController.ins.currPly.ForceLook(currMonitor.transform.position, 3f);
                        }
                        else
                            GameController.ins.currPly.StopLook();

                        if (Lib.IsInView(GameController.ins.currPly.PlayerCam, currMonitor.transform) && (Vector3.Dot(GameController.ins.currPly.PlayerCam.transform.forward, currMonitor.transform.forward) > 0))
                        {
                            OnView = true;
                        }
                    }
                    else
                    {
                        if(OnView||currInten > 0)
                            GameController.ins.currPly.StopLook();
                        OnView = false;
                    }
                }
            }
            else
            {
                OnView = false;
            }

            is895On |= GameController.ins.globalBools[7];

            //is895On |= (NightVisionController.ins != null && NightVisionController.ins.distance < 1f);
            //OnView &= GameController.ins.globalBools[7];
            //OnView |= (NightVisionController.ins != null && NightVisionController.ins.distance < 1f);

            if (!OnView)
            {
                currInten -= Time.deltaTime;
                if (doingCognito)
                {
                    GameController.ins.currPly.CognitoHazard(false);
                    doingCognito = false;
                }
                if (currInten < minVal)
                {
                    currInten = minVal;
                    firstScare = false;
                }
            }
            else
            {
                currInten += Time.deltaTime;
                if (currInten > cognitoIntensity)
                {
                    GameController.ins.currPly.CognitoHazard(true);
                    doingCognito = true;
                }

                if(currInten > deathIntensity && !GameController.ins.currPly.godmode)
                {
                    GameController.ins.Horror.PlayOneShot(horrorSoundEffect[1]);
                    doingCognito = false;
                    GameController.ins.currPly.CognitoHazard(false);
                    GameController.ins.currPly.Death(0);
                }
            }

            
            Update895();

            if (!GameController.ins.globalBools[7] && is895On)
            {
                is895On = false;
                Debug.Log("Shutting down 895");
                monitorRend.SetTexture("_ScreenTex", Texture2D.blackTexture);
            }
        }
        else
            doingCognito = false;
    }

    public void SetClosestScreen(GameObject screen)
    {
        if (currMonitor == screen)
            return;
        if(currMonitor == null)
        {
            currMonitor = screen;
            return;
        }

        if (currMonitor.transform.position.SqrDistance(GameController.ins.currPly.transform.position) > screen.transform.position.SqrDistance(GameController.ins.currPly.transform.position))
            currMonitor = screen;
    }

    public void DeSetScreen(GameObject screen)
    {
        if (currMonitor == screen)
            currMonitor = null;
    }

    public void Update895()
    {
        float currIntPorc = currInten / maxIntensity;

        for(int i = 0; i < 4; i++)
        {
            GameItem currItem = GameController.ins.currPly.equipment[i];
            if (currItem == null)
                continue;
            Item itemData = ItemController.instance.items[currItem.itemFileName];
            if(itemData is Equipable_Wear && ((Equipable_Wear)itemData).protectCogn)
            {
                doingJump = false;
                currJumpTime = -1;
                currInten = firstScareTime - Mathf.Epsilon;
            }
        }

        currentStatTime -= Time.deltaTime;
        float currRand;
        float currChance;

        if (Mathf.Approximately(currIntPorc, 0)|| currIntPorc< 0)
        {
            doingWave = false;
            doingBars = false;
            doingTile = false;
            doingJump = false;
        }
        else
        {
            


            if (currentStatTime < 0)
            {
                currJumpTime = 1;

                currRand = Random.value;
                currChance = Mathf.Lerp(chanceBarsLow, chanceBarsHigh, currIntPorc);
                //Debug.Log("Bars chance: " + currRand + "/" + currChance);
                doingBars = (currRand <= currChance);

                currRand = Random.value;
                currChance = Mathf.Lerp(chanceWaveLow, chanceWaveHigh, currIntPorc);
                doingWave = (currRand <= currChance);

                currRand = Random.value;
                currChance = Mathf.Lerp(chanceTileLow, chanceTileHigh, currIntPorc);
                doingTile = (currRand <= currChance);

                currRand = Random.value;
                currChance = Mathf.Lerp(chanceJumpLow, chanceJumpHigh, currIntPorc);
                doingJump = (currRand <= currChance);

                currRand = Random.value;

                float currMinTime, currMaxTime;

                currMinTime = Mathf.Lerp(minRanLow, minRanHigh, currIntPorc);
                currMaxTime = Mathf.Lerp(maxRanLow, maxRanHigh, currIntPorc);

                currentStatTime = Mathf.Lerp(currMinTime, currMaxTime, currRand);

                if (currInten > firstScareTime)
                {
                    if (!firstScare)
                    {
                        doingJump = true;
                    }
                }
                else
                {
                    currJumpTime = jumpDuration * 2;
                }
            }
        }

        if (currInten > firstScareTime)
            effectWeightTarget = 1;
        else
            effectWeightTarget = 0;

        effectWeight = Mathf.SmoothDamp(effectWeight, effectWeightTarget, ref effectWeightVel, postTargetSpeed);

        heartBeat.volume = effectWeight;
        effectVol.weight = effectWeight;
        if (Mathf.Approximately(effectWeight, 0) || effectWeight < 0)
        {
            if (heartBeat.isPlaying)
                heartBeat.Stop();
        }
        else
        {
            if (!heartBeat.isPlaying)
                heartBeat.Play();
        }
        


        if (doingBars)
        {
            currBarTime -= Time.deltaTime;
            if (currBarTime <= 0)
            {
                currRand = Random.value;

                float currMinVal = Mathf.Lerp(minBarsLow, minBarsHigh, currIntPorc);
                float currMaxVal = Mathf.Lerp(maxBarsLow, maxBarsHigh, currIntPorc);
                float currValue = Mathf.Lerp(currMinVal, currMaxVal, currRand);

                currRand = Random.value;
                currBarTime = Mathf.Lerp(minBarTime, maxBarTime, currRand);
                currRand = Random.value;
                monitorRend.SetFloat("_RowOffset", currValue * ((currRand < 0.5f) ? 1 : -1));
            }
        }
        else
        {
            monitorRend.SetFloat("_RowOffset", 0f);
        }

        if (doingTile)
        {
            currTileTime -= Time.deltaTime;
            if (currTileTime <= 0)
            {
                currRand = Random.value;
                currTileTime = Mathf.Lerp(minTileTime, maxTileTime, currRand);

                float currMinVal = Mathf.Lerp(minTileLow, minTileHigh, currIntPorc);
                float currMaxVal = Mathf.Lerp(maxTileLow, maxTileHigh, currIntPorc);

                currRand = Random.value;
                float currValue = Mathf.Lerp(currMinVal, currMaxVal, currRand);
                currRand = Random.value;

                float x = currValue * ((currRand < 0.5f) ? 1 : -1);

                currRand = Random.value;
                currValue = Mathf.Lerp(currMinVal, currMaxVal, currRand);
                currRand = Random.value;

                float y = currValue * ((currRand < 0.5f) ? 1 : -1);
                monitorRend.SetTextureOffset("_ScreenTex", new Vector2(x, y));

                currRand = Random.value;
                currValue = Mathf.Lerp(currMinVal, currMaxVal, currRand);
                currRand = Random.value;

                x = currValue * ((currRand < 0.5f) ? 1 : -1);

                currRand = Random.value;
                currValue = Mathf.Lerp(currMinVal, currMaxVal, currRand);
                currRand = Random.value;

                y = currValue * ((currRand < 0.5f) ? 1 : -1);
                monitorRend.SetTextureScale("_ScreenTex", new Vector2(1+x, 1+y));
            }
        }
        else
        {
            monitorRend.SetTextureOffset("_ScreenTex", new Vector2(0, 0));
            monitorRend.SetTextureScale("_ScreenTex", new Vector2(1, 1));
        }

        if (doingWave)
        {
            if (Time.frameCount % 15 == 0)
                monitorRend.SetFloat("_FakeTime", Time.time);
            if (!settedWave)
            {
                settedWave = true;
                currRand = Random.value;
                float currMinVal = Mathf.Lerp(minWaveLow, minWaveHigh, currIntPorc);
                float currMaxVal = Mathf.Lerp(maxWaveLow, maxWaveHigh, currIntPorc);
                float currValue = Mathf.Lerp(currMinVal, currMaxVal, currRand);

                monitorRend.SetFloat("_GlitchAmount", currValue);
                currValue = Mathf.Lerp(currMinVal, currMaxVal, currRand);
                monitorRend.SetFloat("_GlitchAmountR", currValue*2);

            }
        }
        else
        {
            settedWave = false;
            monitorRend.SetFloat("_GlitchAmount", 0);
            monitorRend.SetFloat("_GlitchAmountR", 0);
        }

        if (doingJump)
        {
            currJumpTime += Time.deltaTime * (currJumpTime>0 ? currJumpSpeed:1);
            if(currJumpTime > jumpDuration)
            {
                //if(NightVisionController.ins != null && NightVisionController.ins.distance < 1f)
                //{
                //    NightVisionController.ins.jumpText.texture = jumpscareTextures[Random.Range(0, jumpscareTextures.Length)];
                //}
                monitorRend.SetTexture("_JumpTex", jumpscareTextures[Random.Range(0, jumpscareTextures.Length)]);
                currRand = Random.value;
                float currMinVal = Mathf.Lerp(minJumpLow, minJumpHigh, currIntPorc);
                float currMaxVal = Mathf.Lerp(maxJumpLow, maxJumpHigh, currIntPorc);
                float currValue = Mathf.Lerp(currMinVal, currMaxVal, currRand);

                currJumpTime = 0 - currValue;

                currRand = Random.value;
                currJumpSpeed = Mathf.Lerp(minJumpTime, maxJumpTime, currRand);
                if (currInten > firstScareTime && !firstScare)
                {
                    currJumpTime = 0;
                    currentStatTime = jumpDuration;
                    firstScare = true;
                    GameController.ins.PlayHorror(horrorSoundEffect[0], currMonitor.transform, npc.none);
                }

            }
            //if (NightVisionController.ins != null && NightVisionController.ins.distance < 1f)
            //{
            //    NightVisionController.ins.jumpText.color = new Color(1f, 1f, 1f, jumpScareCurve.Evaluate(Mathf.Clamp(currJumpTime, 0, jumpDuration)));
            //}
            monitorRend.SetFloat("_ScareA", jumpScareCurve.Evaluate(Mathf.Clamp(currJumpTime, 0, jumpDuration)));

        }
        else
        {
            //if (NightVisionController.ins != null && NightVisionController.ins.distance < 1f)
            //{
            //    NightVisionController.ins.jumpText.color = new Color(1f, 1f, 1f, 0f);
            //}
            monitorRend.SetFloat("_ScareA", 0);
        }
    }

}
