using System;
using System.Collections;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Tooltip("Spelarens topphastighet när de går")]
    public float moveSpeed = 5f;
    [Tooltip("Hur fort spelaren ska accelerera när de går")]
    [SerializeField] private float acceleration = 20f;
    [Tooltip("Vilken hastighet spelaren ska ha uppåt när de hoppar")]
    [SerializeField] private float jumpPower = 7f;
    [Tooltip("Hur fort spelaren ska accelerera nedåt")]
    [SerializeField] private float gravity = 14f;
    [Tooltip("Positionen (Transform) som CollisionCheck() borde ske vid")]
    [SerializeField] private Transform groundCheck;
    [Tooltip("Positionen (Transform) som CollisionCheck() borde ske vid")]
    [SerializeField] private Transform wallCheck;
    [Tooltip("Positionen (Transform) som CollisionCheck() borde ske vid")]
    [SerializeField] private Transform ceilingCheck;
    [Tooltip("Lagret som klassificeras som mark, används i GroundCheck() för att veta vad den ska kolla efter")]
    [SerializeField] LayerMask groundLayer;
    [Tooltip("Hur lång coyote time man har")]
    [SerializeField] private float coyoteTime = 0.15f;
    
    [Tooltip("Hur många extra hopp spelaren kan göra i luften (1 för dubbelhopp, 2 för trippelhopp etc.)")]
    public int extraJumps;
    [Tooltip("Hur många gånger spelaren får använda dash medan de är i luften")]
    public int maxDash;

    [Tooltip("Styrkan av spelarens dash, i x och y (Blir automatiskt flippat till -x om spelaren tittar åt vänster)")]
    [SerializeField] private Vector2 dashStrength = new Vector2(15f, 4f);
    [Tooltip("Hur lång cooldown det är mellan man kan använda dash")]
    [SerializeField] private float dashCooldownTime = 0.5f;

    [Tooltip("Hur stor groundCheck cirkeln ska vara")]
    [SerializeField] private float checkSize = 0.1f;

    public Vector2 velocity; // En Vector2 som berättar spelarens hastighet i x (vänster/höger) och y (ner/upp).
    private Rigidbody2D rb; // Rigidbody2D variabel som blir satt i Start()
    private PlayerAnimator pAnim; // PlayerAnimator scriptet som sitter på spelaren.

    private PowerupsUI powerupUI; // UI elementet som visar hur många jumps och dashes spelaren har.

    [HideInInspector] public bool grounded; // En boolean (true/false) om spelaren står på marken eller inte. Detta kollas i Update() av CollisionCheck()
    private bool wallTouch; // En boolean (true/false) om spelaren tar i väggen eller inte. Detta kollas i Update() av CollisionCheck()
    private bool ceilingTouch; // En boolean (true/false) om spelaren tar i taket eller inte. Detta kollas i Update() av CollisionCheck()
    private float coyoteCountDown; // En timer som tickar ner efter man lämnar marken.
    private int jumpAmount; // Räknar hur många gånger spelaren kan hoppa.
    private int dashAmount; // Räknar hur många gånger spelaren kan använda dash.
    private float dashCooldown; // En timer som tickar ner efter man använt dash så att man inte kan spamma den.
    public bool wallJumpAbility;
    private bool walled;
    private bool wallJumping;

    private Vector2 movement; // Spelarens input.
    private bool facingRight = true; // Om spelaren är vänd till höger.
    private bool disabled; // Om spelarens input ska stängas av (pga att de är t.ex. i dödsanimationen)


    InputAction move; // Action för att röra sig.
    InputAction jump; // Action för att hoppa.
    InputAction dash; // Action för att använda dash.


    // Start blir kallad på när spelet startar. (Mer exakt är att start blir kallad när objektet skapas, men för objekt som redan finns när spelet börjar så kan man
    // säga att Start körs när spelet startar.)
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Få en referens till spelarobjektets Rigidbody2D komponent och sätt variabeln 'rb' till detta.
        pAnim = GetComponent<PlayerAnimator>();

        powerupUI = FindAnyObjectByType<PowerupsUI>(); // Hittar en referens till UI elementet någonstans i scenträdet.

        // Sätt InputActions 'move' och 'jump' till deras motsvarande inputs i inputsystemet.
        move = InputSystem.actions.FindAction("Move");
        jump = InputSystem.actions.FindAction("Jump");
        dash = InputSystem.actions.FindAction("Dash");
    }

    // Update blir kallad varje frame, detta är för allt som måste ske ofta, som att kolla inputs, flytta på objekt, etc. För fysikrelaterad kod, som t.ex. ändra RigidBody2Ds linearVelocity eller ApplyForce, använd
    // FixedUpdate. För saker som ska ske varje frame, men efter Update har körts på alla objekt, använd LateUpdate.
    void Update()
    {
        if (disabled) return; // Om spelaren är disabled (t.ex. i dödsanimation) så hoppar vi ut och skippar att köra Update denna frame.

        if (!wallJumping) MovementInput();

        Flip();

        CollisionCheck(); // Kollar om spelaren tar i världen.

        if (grounded) // Om man är på marken.
        {
            velocity.y = Mathf.Max(velocity.y, 0); // Om spelaren är på marken bör deras upp/ner hastighet inte vara mindre än 0, annars hade gravitationen byggt upp.
            coyoteCountDown = coyoteTime; // Startar om coyote timern.
            jumpAmount = extraJumps; // Om spelaren är på marken får de tillbaka sina hopp.
            dashAmount = maxDash; // De får även tillbaka sina dashes.
            powerupUI.ResetPowerups(); // Resettar alla powerup ikoner så att de är färgade.
        }

        if (jump.WasPressedThisFrame()) // Om hoppknappen är tryckt OCH spelarens coyote time har inte gått ut ELLER mer än 0 hopp kvar.
        {
            if (walled)
            {
                WallJump();
            }

            else if (coyoteCountDown > 0 || jumpAmount > 0)
            {
                if (coyoteCountDown < 0) // Om spelaren har varit i luften så länge att coyote timern har tickat ut så använder dem sina extrahopp.
                {
                    jumpAmount--; 
                    powerupUI.UseJump(); // Skickar till UI:n att ett extrahopp har används.
                }
                
                Jump();
            }
        }

        if (dash.WasPressedThisFrame() && dashAmount > 0 && movement.x != 0f && dashCooldown < 0) // Om dashknappen är tryckt OCH spelaren har dashes kvar OCH de trycker en movement knapp i någon riktning.
        {                                                                                         // OCH dash cooldown har tickat ut.
            dashAmount--;
            dashCooldown = dashCooldownTime;
            Dash();
        }
        else dashCooldown -= Time.deltaTime; // Tickar ner dash cooldown.

        if (!grounded) // Om man inte är på marken.
        {
            coyoteCountDown -= Time.deltaTime; // Tickar ner coyote timern.
        }
    }

    // FixedUpdate blir kallad ett bestämt antal gånger varje sekund. Detta är designat för fysikrelaterad kod, som att sätta en Rigidbodys velocity. För saker som
    // måste ske varje frame, använd Update(). För saker som borde ske efter allt annat har hänt (t.ex. en kamera som följer spelaren) använd LateUpdate()
    void FixedUpdate()
    {
        ApplyVelocity();
    }

    // "Ritar" en cirkel runt groundCheck objektets position, och kollar om något objekt i lagret 'groundLayer' (bestämt i inspektorn) är innanför cirkeln. I så fall
    // ger den ut true. Om ingenting i 'groundLayer' är i cirkeln så ger den ut false.
    private void CollisionCheck()
    {
        grounded = Physics2D.OverlapCircle(groundCheck.position, checkSize, groundLayer);
        wallTouch = Physics2D.OverlapCircle(wallCheck.position, checkSize, groundLayer);
        ceilingTouch = Physics2D.OverlapCircle(ceilingCheck.position, checkSize, groundLayer);
        
        walled = wallJumpAbility && !grounded && wallTouch && ((facingRight && movement.x > 0f) || (!facingRight && movement.x < 0f));
    }

    // Läser värdet av InputAction 'move' i formen av en Vector2, och sätter variabeln 'movement' till det värdet. Sedan för att göra acceleration och deceleration
    // mjukare så används en MoveTowards, vilket är en funktion som "kliver" från 'a' till 'b' med ett kliv av storleken 't'
    private void MovementInput()
    {
        movement = move.ReadValue<Vector2>();
        velocity.x = Mathf.MoveTowards(velocity.x, movement.x * moveSpeed, acceleration * Time.deltaTime);
    }

    // Flippar spelaren för att titta åt de håll de går.
    private void Flip()
    {
        if ((velocity.x < 0 && facingRight) || (velocity.x > 0 && !facingRight)) // Om man går vänster och tittar höger, eller tvärtom.
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z); // Flippar spelaren på bredden (x)
            facingRight = !facingRight; // facingRight blir satt till motsatsen av vad den var.
        }
    }
    
    // Sätter spelarens velocity.y (ner/upp) till att vara 'jumpPower' vilket får spelaren att flyga uppåt.
    private void Jump()
    {
        velocity.y = jumpPower;
        grounded = false;
        pAnim.Animate("jump");
    }

    private void WallJump()
    {
        wallJumping = true;
        grounded = false;
        velocity.y = jumpPower;
        velocity.x = facingRight ? -jumpPower : jumpPower;
        pAnim.Animate("jump");
        Invoke("WallJumpInputReactivate", 0.1f);
    }

    private void WallJumpInputReactivate()
    {
        wallJumping = false;
    }

    // När dash används ger det spelaren en ny velocity i riktningen av deras movement.
    private void Dash()
    {
        float dirX = movement.x > 0f ? dashStrength.x : -dashStrength.x; // Detta är en "ternery operation" Det är en simplificerad if/else där dirX blir 4f om
                                                                        // movement input i x-axeln är mer än 0, annars -4f.
        velocity = new Vector3(dirX, dashStrength.y, 0);
        powerupUI.UseDash();
    }

    // Ger spelaren gravitation och sedan sätter 'rb' (Rigidbody2D)s linjära hastighet till 'velocity' variablen.
    private void ApplyVelocity()
    {
        if (disabled)
        {
            velocity *= 0.8f;
        }
        else if (walled || (velocity.y > 0 && ceilingTouch)) // Om spelaren rör sig uppåt och de tar i taket, så slutar de ha momentum uppåt.
        {
            velocity.y = 0f;
        }
        else if (jump.IsPressed() && velocity.y > 0) // För att få effekten av att hoppa högre när hoppknappen hålls in så halveras gravitationen.
        {                                       // (När velocity.y är mer än 0, alltså när spelaren färdas uppåt)
            velocity.y -= gravity / 2 * Time.fixedDeltaTime;
        }
        else if (!grounded)// Om spelaren är i luften men hoppknappen trycks inte ned eller spelaren faller nedåt, så läggs gravitationen till som vanligt.
        {
            velocity.y -= gravity * Time.fixedDeltaTime;
        }

        if (Mathf.Abs(velocity.x) > 0 && wallTouch)
        {
            velocity.x = 0f;
        }

        rb.linearVelocity = velocity; // Sätter spelarens hastighet till det som blev uträknat denna frame.
    }

    // Kallad av Health objektet när hälsan når 0. Stänger av inputs.
    public void Die()
    {
        disabled = true;
    }

    // Kallad när dödsanimationen har spelats klart.
    public void Respawn()
    {
        disabled = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, checkSize);
        Gizmos.DrawWireSphere(ceilingCheck.position, checkSize);
    }
}
