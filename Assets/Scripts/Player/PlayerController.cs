using System;
using System.Collections;
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
    [Tooltip("Positionen (Transform) som GroundCheck() borde ske vid")]
    [SerializeField] private Transform groundCheck;
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

    [HideInInspector] public Vector2 velocity; // En Vector2 som berättar spelarens hastighet i x (vänster/höger) och y (ner/upp).
    private Rigidbody2D rb; // Rigidbody2D variabel som blir satt i Start()

    [HideInInspector] public bool grounded; // En boolean (true/false) om spelaren står på marken eller inte. Detta kollas i Update() av GroundCheck()
    private float coyoteCountDown; // En timer som tickar ner efter man lämnar marken.
    private int jumpAmount; // Räknar hur många gånger spelaren kan hoppa.
    private int dashAmount; // Räknar hur många gånger spelaren kan använda dash.
    private float dashCooldown; // En timer som tickar ner efter man använt dash så att man inte kan spamma den.

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

        MovementInput();

        Flip();

        grounded = GroundCheck(); // Kollar om spelaren är på marken.

        if (grounded) // Om man är på marken.
        {
            velocity.y = Mathf.Max(velocity.y, 0); // Om spelaren är på marken bör deras upp/ner hastighet inte vara mindre än 0, annars hade gravitationen byggt upp.
            coyoteCountDown = coyoteTime; // Startar om coyote timern.
            jumpAmount = extraJumps; // Om spelaren är på marken får de tillbaka sina hopp.
            dashAmount = maxDash; // De får även tillbaka sina dashes.
        }
        else // Om man inte är på marken.
        {
            coyoteCountDown -= Time.deltaTime; // Tickar ner coyote timern.
        }

        if (jump.WasPressedThisFrame() && (grounded || coyoteCountDown > 0 || jumpAmount > 0)) // Om hoppknappen är tryckt OCH spelaren är på marken ELLER coyote time har inte gått ut ELLER mer än 0 hopp kvar.
        {
            if (coyoteCountDown > 0) jumpAmount--; // Om spelaren har varit i luften så länge att coyote timern har tickat ut så använder dem sina extrahopp.
            
            Jump();
        }

        if (dash.WasPressedThisFrame() && dashAmount > 0 && movement.x != 0f && dashCooldown < 0) // Om dashknappen är tryckt OCH spelaren har dashes kvar OCH de trycker en movement knapp i någon riktning.
        {                                                                                         // OCH dash cooldown har tickat ut.
            dashAmount--;
            dashCooldown = dashCooldownTime;
            Dash();
        }
        else dashCooldown -= Time.deltaTime; // Tickar ner dash cooldown.
    }

    // FixedUpdate blir kallad ett bestämt antal gånger varje sekund. Detta är designat för fysikrelaterad kod, som att sätta en Rigidbodys velocity. För saker som
    // måste ske varje frame, använd Update(). För saker som borde ske efter allt annat har hänt (t.ex. en kamera som följer spelaren) använd LateUpdate()
    void FixedUpdate()
    {
        ApplyVelocity();
    }

    // "Ritar" en cirkel runt groundCheck objektets position, och kollar om något objekt i lagret 'groundLayer' (bestämt i inspektorn) är innanför cirkeln. I så fall
    // ger den ut true. Om ingenting i 'groundLayer' är i cirkeln så ger den ut false.
    private bool GroundCheck()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.5f, groundLayer);
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
        coyoteCountDown = 0f;
    }

    // När dash används ger det spelaren en ny velocity i riktningen av deras movement.
    private void Dash()
    {
        float dirX = movement.x > 0f ? dashStrength.x : -dashStrength.x; // Detta är en "ternery operation" Det är en simplificerad if/else där dirX blir 4f om
                                                                        // movement input i x-axeln är mer än 0, annars -4f.
        velocity = new Vector3(dirX, dashStrength.y, 0);
    }

    // Ger spelaren gravitation och sedan sätter 'rb' (Rigidbody2D)s linjära hastighet till 'velocity' variablen.
    private void ApplyVelocity()
    {
        if (disabled)
        {
            velocity *= 0.8f;
        }
        else if (jump.IsPressed() && velocity.y > 0) // För att få effekten av att hoppa högre när hoppknappen hålls in så halveras gravitationen.
        {                                       // (När velocity.y är mer än 0, alltså när spelaren färdas uppåt)
            velocity.y -= gravity / 2 * Time.fixedDeltaTime;
        }
        else if (!grounded)// Om spelaren är i luften men hoppknappen trycks inte ned eller spelaren faller nedåt, så läggs gravitationen till som vanligt.
        {
            velocity.y -= gravity * Time.fixedDeltaTime;
        }

        rb.linearVelocity = velocity; // Sätter spelarens hastighet till det som blev uträknat denna frame.
    }

    // Kallad av Health objektet när hälsan når 0. Stänger av inputs och väntar på att dödsanimationen har spelat klart.
    public void Die()
    {
        Animator anim = GetComponent<Animator>();

        IEnumerator WaitForAnimation() // En metod som ger en IEnumerator. En enumerator används för att göra "Coroutines", en metod som körs i bakgrunden, som kan startas och t.ex. gå igenom en loop, och vänta
        {                              // tills nästa frame, eller en bestämd tid med WaitForSeconds(s) och sedan gå igenom loopen igen.
            disabled = true; // Spelaren blir disabled och kan inte göra inputs.

            while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1) // Loopar sålänge animatorn är igång.
            {
                yield return null; // Säger till coroutinen att sluta köra denna frame.
            }

            disabled = false; // Om animationen är klar så körs inte while loopen, vilket innebär att coroutinen inte blir tillsagd att sluta tidigt och den når detta steg. Spelaren blir inte längre disabled.

            GetComponentInChildren<Health>().Respawn(); // När animationen är klar så kallas Respawn på Health objektet.
        }

        StartCoroutine(WaitForAnimation()); // Koden som startar WaitForAnimation coroutinen. Lite förvirrande att den är här nere, men allt innuti WaitForAnimation bara är koden innanför metoden. Där förklaras vad
                                            //metoden är, och här nere säger programmet till att det ska köras.
    }
}
