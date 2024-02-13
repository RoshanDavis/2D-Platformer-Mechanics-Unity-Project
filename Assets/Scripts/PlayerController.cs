using UnityEngine;
using System.Collections;
public class PlayerController : MonoBehaviour
{
    #region horizontal movement variables
    public float movement;
    //public float moveSpeed=25f;>>>Used for Basic Horizontal Movement>>>line 40
    public Rigidbody2D rb;
    public bool facingRight;
    public float maxSpeed=25f;
    [SerializeField]
    float fHorizontalAcceleration = 1f, fHorizontalVelocity;
    [SerializeField]
    [Range(0, 1)]
    float fHorizontalDampingBasic = 0.5f;
    [SerializeField]
    [Range(0, 1)]
    float fHorizontalDampingWhenStopping = 0.5f;
    [SerializeField]
    [Range(0, 1)]
    float fHorizontalDampingWhenTurning = 0.5f;
    [SerializeField]
    [Range(0, 1)]
    float fHorizontalDampingInAir = 0.5f;
    #endregion
    #region jump variables
    public bool isGrounded;
    [SerializeField]
    int extraJumps, currentJumps;
    [SerializeField]
    float footrad, jumpForce, multiJumpForce, jumpSuppression;
    public Transform footpos;
    [SerializeField]
    LayerMask ground;

    [SerializeField]
    float maxJumpKeyPressTime, maxGroundedTime;
    float jumpKeyPressTime, groundedTime;
    #endregion
    #region dash variables
    [SerializeField]
    float dashDistance,dashTime=0.4f,maxTimeBtwDash;
    [SerializeField]
    [Range(0,1)]
    float dashdamping;
    bool isDashing=false;
    public bool canDash;
    float timeBtwDash;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        rb = FindObjectOfType<Rigidbody2D>();
        facingRight = true;
        currentJumps = extraJumps;
    }

    // Update is called once per frame
    void Update()
    {
        Jump();
        if(canDash)
            DashInput();

    }
    private void FixedUpdate()
    {
        if(!isDashing)
            Move();
    }
    public void Move()
    {
        movement = Input.GetAxis("Horizontal");
        /* 
         * Basic Horizontal Movement...
         * rb.velocity = new Vector2(movement*moveSpeed, rb.velocity.y); 
         */

        //Advanced Horizontal Movement with Acceleration and Damping
        fHorizontalVelocity = rb.velocity.x;
        if(Mathf.Abs(rb.velocity.x)<maxSpeed)
            fHorizontalVelocity += Input.GetAxisRaw("Horizontal") * fHorizontalAcceleration;
        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) < 0.01f)
        {
            if (isGrounded)
                fHorizontalVelocity *= Mathf.Pow(1f - fHorizontalDampingWhenStopping, Time.deltaTime * 10f);
            else
                fHorizontalVelocity *= Mathf.Pow(1f - fHorizontalDampingInAir, Time.deltaTime * 10f);
        }
        else if (Mathf.Sign(Input.GetAxisRaw("Horizontal")) != Mathf.Sign(fHorizontalVelocity))
            fHorizontalVelocity *= Mathf.Pow(1f - fHorizontalDampingWhenTurning, Time.deltaTime * 10f);
        else if(Mathf.Abs(rb.velocity.x)<=maxSpeed)
            fHorizontalVelocity *= Mathf.Pow(1f - fHorizontalDampingBasic, Time.deltaTime * 10f);

        rb.velocity = new Vector2(fHorizontalVelocity, rb.velocity.y);

        if (facingRight == false && movement > 0)
        {
            Flip();
        }
        else if (facingRight == true && movement < 0)
        {
            Flip();
        }
    }
    public void Jump()
    {
        isGrounded = Physics2D.OverlapCircle(footpos.position, footrad, ground);
        if (isGrounded)
        {
            groundedTime = maxGroundedTime;
        }
        groundedTime -= Time.deltaTime;
        jumpKeyPressTime -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            jumpKeyPressTime = maxJumpKeyPressTime;
        }
        if (jumpKeyPressTime > 0 && groundedTime > 0)
        {
            jumpKeyPressTime = 0;
            groundedTime = 0;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            currentJumps = extraJumps;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded == false && currentJumps > 0)
        {
            //Debug.Log("Multi Jump");
            rb.velocity = new Vector2(rb.velocity.x, multiJumpForce);
            currentJumps--;
        }
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpSuppression);
            }
        }
    }
    public void Flip()
    {
        transform.Rotate(new Vector3(0f, 180f, 0f));
        facingRight = !facingRight;
    }
    public void DashInput()
    {
        if (Input.GetKeyDown(KeyCode.C) && timeBtwDash <= 0)
        {

            if (facingRight)
            {
                StartCoroutine(Dash(1f));
            }
            else
            {
                StartCoroutine(Dash(-1f));
            }
            timeBtwDash = maxTimeBtwDash;
        }
        if (timeBtwDash > 0)
        {
            timeBtwDash -= Time.deltaTime;
        }
    }
    IEnumerator Dash(float dir)
    {
        isDashing = true;
        rb.velocity = new Vector2(dashDistance * dir, 0f);
        //rb.AddForce(new Vector2(dashDistance * dir, 0f), ForceMode2D.Impulse);
        float gravity= rb.gravityScale;
        rb.gravityScale = 0;
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
        rb.gravityScale = gravity;
        rb.velocity = new Vector2(dashDistance * dir * (1f- dashdamping), 0f);
    }
    
        
}
