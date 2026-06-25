using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private string idle;
    [SerializeField] private string walk;
    [SerializeField] private string jump;
    [SerializeField] private string fall;
    [SerializeField] private string die;
    [SerializeField] private string dash;
    
    private string currentlyPlaying;
    private bool forceAnim;
    private SpriteRenderer sprite; // SpriteRenderer variabel som blir satt i Start()
    private Animator anim;
    private PlayerController player;
    
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>(); // Få en referens till spelarobjektets SpriteRenderer komponent och sätt variabeln 'sprite' till detta.
        anim = GetComponent<Animator>();
        player = GetComponent<PlayerController>();
    }

    // Matchar det spelaren håller på med till en animation. Att skriva på detta sätt är inte optimalt, långa rader med if/else är svår att debugga, svår att läsa och svår att expandera. I detta fall är det okej
    // då alternativen är lite överkurs för ens första projekt, det finns inte heller särskillt många animationer i denna demo, men ni kan förstå att det kan bli jobbigt om man lägger till 20 animationer till.
    void Update()
    {
        if (player.grounded && MathF.Abs(player.velocity.x) > 0.1f) Animate("walk"); // Spelaren är på marken och har en absolut hastighet av mer än 0.1 
        else if (player.velocity.x > player.moveSpeed) Animate("dash"); // Om spelaren rör sig snabbare än deras topphastighet.
        else if (player.velocity.y > 0) Animate("jump"); // Om spelaren rör sig uppåt.
        else if (player.velocity.y < 0) Animate("fall"); // Om spelaren faller nedåt.
        else Animate("idle");
    }
    
    public void Animate(string animation)
    {
        if (animation == currentlyPlaying || forceAnim) return;

        currentlyPlaying = animation;

        switch (animation)
        {
            case "idle":
            PlayAnimation(idle);
                break;
            case "walk":
            PlayAnimation(walk);
                break;
            case "jump":
            PlayAnimation(jump);
                break;
            case "fall":
            PlayAnimation(fall);
                break;
            case "die":
            PlayAnimation(die, false);
                break;
            case "dash":
            PlayAnimation(dash);
                break;
        }
    }

    private void PlayAnimation(string animation, bool looping = true)
    {
        anim.Play(animation); // Spelar den givna animationen.

        if (!looping)
        {
            IEnumerator WaitForAnimation() // Enumerator metod, se PlayerScript för mer information.
            {
                forceAnim = true; // Tvingar denna animation att spela klart innan en ny kan börja.

                while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1) // Väntar på att animationen är klar.
                {
                    yield return null;
                }

                forceAnim = false;
            }

            StartCoroutine(WaitForAnimation()); // Startar coroutinen.
        }
    }
}
