using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remover membros privados n�o utilizados", Justification = "To avoid warnings in private methods provided by Unity.")]
public class Crusher : Enemy
{
    [SerializeField]
    Transform crusherLine, crusherSpikes;
    SpriteRenderer crusherLineRenderer;
    [SerializeField]
    float lineLimit = 1.5f, spikesDelay=0.01f;
    float defaultCrushSpikesY = -0.13f, activeZoneCrushSpikesY=-0.2387f;

    float delayToAttack = 0;
    [SerializeField]
    float waitDelayToAttack = 5f;

    bool isAttacking = false, hitFloor = false;

    public bool HitFloor { get => hitFloor; set => hitFloor = value; }

    // Start is called before the first frame update
    void Start()
    {
        InitializeComponent();
        InitializeHurthVar();
        delayToAttack = 0;
        isAttacking = false;
        hitFloor = false;
        canMove = true;
        crusherLineRenderer = crusherLine.GetComponent<SpriteRenderer>();
    }

    protected override void Movement(Vector2 dir)
    {
        transform.Translate(speed * Time.deltaTime * Vector2.right);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        AmmoDetected(other);
        if (other.CompareTag("PatrolArea"))
        {
            speed *= -1;
        }
    }

    private void Update()
    {
        if (canMove)
        {
            Movement(initialDi);            
        }
        if (!isAttacking)
        {
            delayToAttack += Time.deltaTime;
        }
        
        if (delayToAttack >= waitDelayToAttack)
        {
            ani.SetBool("attack", true);
            delayToAttack = 0;
        }
    }


    private void DownSpikes()
    {
        ani.SetBool("attack", false);
        if(isAlive && !isAttacking)
        {
            isAttacking = true;
            canMove = false;
            StartCoroutine(MoveSpikeToTargetYPos(activeZoneCrushSpikesY));
        }
    }


    private void UpSpikes()
    {
        if (isAlive && isAttacking)
        {
            StartCoroutine(MoveSpikeToTargetYPos(defaultCrushSpikesY, false,false));
        }
    }

    IEnumerator MoveSpikeToTargetYPos(float targetY, bool isDown=true, bool firstRun=true)
    {
        float signal = isDown ? -0.02f : 0.02f;
        while( ( (isDown && crusherSpikes.localPosition.y > targetY && crusherSpikes.localPosition.y > -lineLimit && !hitFloor)
           || (!isDown && crusherSpikes.localPosition.y < targetY && crusherSpikes.localPosition.y < defaultCrushSpikesY) ) 
           && isAlive && crusherSpikes)
        {
            crusherSpikes.localPosition = new Vector2(0,crusherSpikes.localPosition.y+signal);
            crusherLineRenderer.size += !firstRun ? new Vector2(0, -signal) : Vector2.zero;
            yield return new WaitForSeconds(spikesDelay);
        }
        if (firstRun)
        {
            yield return new WaitForSeconds(spikesDelay*10);
            StartCoroutine(MoveSpikeToTargetYPos(-lineLimit,true,false));
        }
        else if (!isDown)
        {
            canMove = true;
            isAttacking = false;
            hitFloor = false;
            crusherLineRenderer.size = new Vector2(crusherLineRenderer.size.x, 0);
            yield return null;
        }
        else
        {
            UpSpikes();
        }
        
    }
    protected override void SelfDestruction()
    {
        crusherSpikes.parent = null;
        crusherLine.parent = null;
        crusherLine.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        Destroy(crusherLine.gameObject, 3);
        base.SelfDestruction();
    }


    public void CallDeath(int damage)
    {
        CheckIfIsDeath(damage);
    }

    public void CallLoseHealth(int damage)
    {
        LoseHealth(damage);
    }
}
