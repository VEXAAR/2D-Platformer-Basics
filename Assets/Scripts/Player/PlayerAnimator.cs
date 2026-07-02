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
    
    private string currentlyPlaying; // Namnet på animationen som spelas just nu.
    private bool forceAnim; // Stoppar andra animationer från att spela tills nuvarande animation är klar.
    private Animator anim; // Animator referensen som blir satt i Start()
    private PlayerController player; // PlayerController referensen som blir satt i Start()
    
    void Start()
    {
        anim = GetComponent<Animator>();
        player = GetComponent<PlayerController>();
    }

    // Matchar det spelaren håller på med till en animation. Att skriva på detta sätt är inte optimalt, långa rader med if/else är svår att debugga, svår att läsa och svår att expandera. I detta fall är det okej
    // då alternativen är lite överkurs för ens första projekt, det finns inte heller särskillt många animationer i denna demo, men ni kan förstå att det kan bli jobbigt om man lägger till 20 animationer till.
    void Update()
    {
        if (player.grounded && MathF.Abs(player.velocity.x) > 0.1f) Animate("walk"); // Spelaren är på marken och har en absolut hastighet av mer än 0.1 
        else if (Mathf.Abs(player.velocity.x) > player.moveSpeed) Animate("dash"); // Om spelaren rör sig snabbare än deras topphastighet.
        else if (!player.grounded) Animate("fall"); // Om spelaren faller nedåt.
        else Animate("idle"); // Om inget annat, så är spelaren idle.
    }
    
    // Kan bli kallad för att animera spelaren.
    public void Animate(string animation)
    {
        if (animation == currentlyPlaying || (forceAnim && animation != "jump" && animation != "die")) return; // Spelar inte animation om samma spelas redan eller om det spelas en animation som måste bli klar
                                                                                                               // Detta gäller inte "jump" och "die", som måste spelas oavsett.
        currentlyPlaying = animation;

        switch (animation) // En switch case operation som matchar "animation" variabeln till en string.
        {
            case "idle":
            PlayAnimation(idle);
                break;
            case "walk":
            PlayAnimation(walk);
                break;
            case "jump":
            PlayAnimation(jump, false);
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

    // Spelar en animation och ifall den inte är loopande så stoppar den andra animationer från att spela tills den är klar.
    private void PlayAnimation(string animation, bool looping = true)
    {
        anim.Play(animation); // Spelar den givna animationen.

        if (!looping) // Om det inte är en loopande animation så tvingas den köra tills den är slut.
        {
            forceAnim = true;
        }
    }

    // När en ickeloopande animation är klar kallar den på denna funktion som säger till scriptet att andra animationer nu får spela. Om animationen var "die" så respawnar spelaren även.
    public void AnimationFinished()
    {
        if (currentlyPlaying == "die") // Om animationen var "die" så respawnar spelaren.
        {
            player.Respawn();
            GetComponent<Health>().Respawn(); // Hittar Health komponenten och kallar Respawn() på den.
        }

        forceAnim = false;
    }
}
