using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using KModkit;
using System.Linq;

public class BlackJackScript : MonoBehaviour
{
    public KMSelectable Bet1;
    public KMSelectable Bet10;
    public KMSelectable Bet100;
    public KMSelectable Bet250;
    public KMSelectable BlackjackBtn;

    public GameObject Buttons, LockedButtons, BjBtn, EmptyBtns;
    public KMSelectable ModuleSelectable;
    public KMBombModule BombModule;
    List<KMSelectable> ListButtons = new List<KMSelectable>();


    public KMSelectable HitBtn;
    public KMSelectable StandBtn;

    public GameObject HingeHit, HingeStand;

    public KMAudio Shuffle;
    public KMAudio DealCard1, DealCard2, DealCard3;

    public Renderer MainCard;
    public Renderer CardClosedRender;
    public Renderer ExtraCard;
    public Renderer HitCard1;
    public Renderer HitCard2;
    public Renderer DealerCard1;
    public Renderer DealerCard2;
    public Renderer DealerCard3;
    public Renderer BettingCoin;

    public TextMesh HitText, StandText;

    public Texture mainCard;

    public Texture AceOfSpades, TwoOfSpades, ThreeOfSpades, FourOfSpades, FiveOfSpades, SixOfSpades, SevenOfSpades, EightOfSpades, NineOfSpades, TenOfSpades, JackOfSpades, KingOfSpades;
    public Texture AceOfHearts, TwoOfHearts, ThreeOfHearts, FourOfHearts, FiveOfHearts, SixOfHearts, SevenOfHearts, EightOfHearts, NineOfHearts, TenOfHearts, JackOfHearts, QueenOfHearts, KingOfHearts;
    public Texture AceOfClubs, TwoOfClubs, ThreeOfClubs, FourOfClubs, FiveOfClubs, SixOfClubs, SevenOfClubs, EightOfClubs, NineOfClubs, TenOfClubs, QueenOfClubs, KingOfClubs;
    public Texture AceOfDiamonds, TwoOfDiamonds, ThreeOfDiamonds, FourOfDiamonds, FiveOfDiamonds, SixOfDiamonds, SevenOfDiamonds, EightOfDiamonds, NineOfDiamonds, TenOfDiamonds, QueenOfDiamonds, KingOfDiamonds;

    public Texture BetCoin1, BetCoin10, BetCoin100, BetCoin250, BetTBD;

    public TextMesh Response, BetWorth;

    public KMBombInfo BombInfo;

    private readonly string TwitchHelpMessage = "Type '!{0} bet 10' to bet $10. Type '!{0} hit' to get another card and '!{0} stand'. To check for blackjack at the start, type '!{0} check'";
    public KMSelectable[] ProcessTwitchCommand(string Command)
    {
        Command = Command.ToLowerInvariant().Trim();

        if (Command.Equals("bet 1"))
        {
            return new[] { Bet1 };
        }
        else if (Command.Equals("bet 10"))
        {
            return new[] { Bet10 };
        }
        else if (Command.Equals("bet 100"))
        {
            return new[] { Bet100 };
        }
        else if (Command.Equals("bet 250"))
        {
            return new[] { Bet250 };
        }
        else if (Command.Equals("hit"))
        {
            return new[] { HitBtn };
        }
        else if (Command.Equals("stand"))
        {
            return new[] { StandBtn };
        }
        else if (Command.Equals("check"))
        {
            return new[] { BlackjackBtn };
        }
        return null;
    }

    static int moduleIdCounter = 1;
    int moduleId;
    int StartingCardGen;
    int Suit;
    int DealtCard;
    int CorrectBet;
    int timer;
    int totalSum;
    int DealingValue;
    int DealingOrder; // 1 = lower Bet1, 2 = lower Bet10, 5 = higher Bet1, etc.
    int Aces = 0;
    int Hits = 0;
    int totalSumDealer = 0;
    int ClosedCardValue = 0;

    string StartingCard;
    string ClosedCard;

    bool BlackjackAtStart = false;
    bool BettingComplete = false;
    bool HittingAllowed = false;
    bool StandingAllowed = false;
    bool Solved = false;

    bool Exception = false;
    bool Exception1 = false;
    bool Exception2 = false;

    bool DealCard = true;
    bool isCard1Dealt = false;
    bool isCard2Dealt = false;

    bool BlackjackMessage = false;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        Bet1.OnInteract += Betting1;
        Bet10.OnInteract += Betting10;
        Bet100.OnInteract += Betting100;
        Bet250.OnInteract += Betting250;
        HitBtn.OnInteract += HitCard;
        StandBtn.OnInteract += Stand;
        Shuffle.PlaySoundAtTransform("Shuffle", transform);
    }

    void Start()
    {
        Bet1.OnInteract = Betting1;
        Bet10.OnInteract = Betting10;
        Bet100.OnInteract = Betting100;
        Bet250.OnInteract = Betting250;
        HitBtn.OnInteract = HitCard;
        StandBtn.OnInteract = Stand;
        BlackjackBtn.OnInteract += BlackjackCheck;
        foreach (Transform child in Buttons.transform)
        {
            GameObject Lol = child.gameObject;
            ListButtons.Add(Lol.activeSelf ? Lol.GetComponent<KMSelectable>() : null);
        }
        foreach (Transform child in BjBtn.transform)
        {
            GameObject Lol = child.gameObject;
            ListButtons.Add(Lol.activeSelf ? Lol.GetComponent<KMSelectable>() : null);
        }
        ModuleSelectable.Children = ListButtons.ToArray();
        ModuleSelectable.UpdateChildren();
        StartingCardGen = 0;
        Suit = 0;
        DealtCard = 0;
        CorrectBet = 0;
        timer = 0;
        totalSum = 0;
        DealingValue = 0;
        DealingOrder = 0;
        StartingCard = "";
        ClosedCard = "";
        BlackjackAtStart = false;
        BettingComplete = false;
        isCard1Dealt = false;
        isCard2Dealt = false;
        BlackjackMessage = false;
        BetWorth.text = "";

        HitText.color = Color.grey;
        StandText.color = Color.grey;

        BettingCoin.material.mainTexture = BetTBD;

        Response.text = "Betting...";

        StartCoroutine("CheckForOver21");
        StartCoroutine("CheckForBlackjack");
        Init();
    }

    void Init()
    {
        StartingCardGen = UnityEngine.Random.Range(1, 5);
        //Starting Card
        if (StartingCardGen == 1)
        {
            StartingCard = "AceOfSpades";
            totalSum = 11;
            Aces++;
            MainCard.material.mainTexture = AceOfSpades;
            Debug.LogFormat("[Blackjack #{0}] Your starting card is the Ace Of Spades", moduleId);
        }
        else if (StartingCardGen == 2)
        {
            StartingCard = "KingOfDiamonds";
            totalSum = 10;
            MainCard.material.mainTexture = KingOfDiamonds;
            Debug.LogFormat("[Blackjack #{0}] Your starting card is the King Of Diamonds", moduleId);
            Exception1 = true;
        }
        else if (StartingCardGen == 3)
        {
            StartingCard = "TwoOfHearts";
            totalSum = 2;
            MainCard.material.mainTexture = TwoOfHearts;
            Debug.LogFormat("[Blackjack #{0}] Your starting card is the Two Of Hearts", moduleId);
        }
        else if (StartingCardGen == 4)
        {
            StartingCard = "TenOfClubs";
            totalSum = 10;
            MainCard.material.mainTexture = TenOfClubs;
            Debug.LogFormat("[Blackjack #{0}] Your starting card is the Ten Of Clubs", moduleId);
            Exception2 = true;
        }
        CardRules();
    } //Starting Card

    void CardRules() //Closed Card
    {
        string Indic = string.Join("", BombInfo.GetIndicators().ToArray());

        if (StartingCard == "AceOfSpades")
        {
            if (BombInfo.IsIndicatorOn(Indicator.BOB))
            {
                ClosedCard = "KingOfHearts";
                totalSum = totalSum + 10;
                CorrectBet = 100;
                ClosedCardValue = 10;
                Debug.LogFormat("[Blackjack #{0}] Your closed card is the King Of Hearts with a value of {1}", moduleId, ClosedCardValue);
                BlackjackAtStart = true;
            }
            else if (BombInfo.GetSerialNumberLetters().Any("AEIOU".Contains))
            {
                totalSum = totalSum + 5;
                ClosedCard = "FiveOfDiamonds";
                CorrectBet = 1;
                ClosedCardValue = 5;
                Debug.LogFormat("[Blackjack #{0}] Your closed card is the Five Of Diamonds with a value of {1}", moduleId, ClosedCardValue);
            }
            else if (BombInfo.GetSerialNumberNumbers().Sum() > 7)
            {
                totalSum = totalSum + 7;
                CorrectBet = 250;
                ClosedCardValue = 7;
                Debug.LogFormat("[Blackjack #{0}] Your closed card is the Seven Of Spades with a value of {1}", moduleId, ClosedCardValue);
                ClosedCard = "SevenOfSpades";
            }
            else
            {
                totalSum = totalSum + 2;
                CorrectBet = 10;
                ClosedCardValue = 2;
                Debug.LogFormat("[Blackjack #{0}] Your closed card is the Two Of Clubs with a value of {1}", moduleId, ClosedCardValue);
                ClosedCard = "TwoOfClubs";
            }
        } //Ace Of Spades
        else if (StartingCard == "KingOfDiamonds")
        {
            if (Indic.Any("GAMBLER".Contains))
            {
                totalSum = totalSum + 10;
                CorrectBet = 250;
                ClosedCardValue = 10;
                Debug.LogFormat("[Blackjack #{0}] Your closed card is the Queen Of Hearts with a value of {1}", moduleId, ClosedCardValue);
                ClosedCard = "QueenOfHearts";
            }
            else if (BombInfo.GetBatteryCount(Battery.D) > 1)
            {
                totalSum = totalSum + 9;
                CorrectBet = 100;
                ClosedCardValue = 9;
                Debug.LogFormat("[Blackjack #{0}] Your closed card is the Nine Of Spades with a value of {1}", moduleId, ClosedCardValue);
                ClosedCard = "NineOfSpades";
            }
            else if (BombInfo.IsPortPresent(Port.Serial))
            {
                totalSum = totalSum + 3;
                CorrectBet = 10;
                ClosedCardValue = 3;
                Debug.LogFormat("[Blackjack #{0}] Your closed card is the Three Of Diamonds with a value of {1}", moduleId, ClosedCardValue);
                ClosedCard = "ThreeOfDiamonds";
            }
            else
            {
                totalSum = totalSum + 4;
                CorrectBet = 1;
                ClosedCardValue = 4;
                Debug.LogFormat("[Blackjack #{0}] Your closed card is the Four Of Clubs with a value of {1}", moduleId, ClosedCardValue);
                ClosedCard = "FourOfClubs";
            }
        } //King Of Diamonds
        else if (StartingCard == "TwoOfHearts")
        {
            if (BombInfo.GetPortCount() > BombInfo.GetBatteryCount())
            {
                totalSum = totalSum + 1;
                ClosedCardValue = 1;
                ClosedCard = "AceOfDiamonds";
                CorrectBet = 100;
                Debug.LogFormat("[Blackjack #{0}] Your closed card is the Ace Of Diamonds with a value of {1}", moduleId, ClosedCardValue);
            }
            else if (BombInfo.IsIndicatorPresent(Indicator.NSA))
            {
                totalSum = totalSum + 3;
                CorrectBet = 10;
                ClosedCardValue = 3;
                Debug.LogFormat("[Blackjack #{0}] Your closed card is the Three Of Hearts with a value of {1}", moduleId, ClosedCardValue);
                ClosedCard = "ThreeOfHearts";
            }
            else if (BombInfo.GetBatteryCount() == 0)
            {
                totalSum = totalSum + 7;
                CorrectBet = 250;
                ClosedCardValue = 7;
                Debug.LogFormat("[Blackjack #{0}] Your closed card is the Seven Of Club with a value of {1}s", moduleId, ClosedCardValue);
                ClosedCard = "SevenOfClubs";
            }
            else
            {
                totalSum = totalSum + 4;
                CorrectBet = 1;
                ClosedCardValue = 4;
                Debug.LogFormat("[Blackjack #{0}] Your closed card is the Four Of Spades with a value of {1}", moduleId, ClosedCardValue);
                ClosedCard = "FourOfSpades";
            }
        } //Two Of Diamonds
        else if (StartingCard == "TenOfClubs")
        {
            if (BombInfo.GetSerialNumberLetters().Any("CASINO".Contains))
            {
                totalSum = totalSum + 5;
                CorrectBet = 100;
                ClosedCardValue = 5;
                Debug.LogFormat("[Blackjack #{0}] Your closed card is the Five Of Clubs with a value of {1}", moduleId, ClosedCardValue);
                ClosedCard = "FiveOfClubs";
            }
            else if (BombInfo.GetBatteryCount(Battery.AA) > 3)
            {
                totalSum = totalSum + 3;
                CorrectBet = 1;
                ClosedCardValue = 3;
                Debug.LogFormat("[Blackjack #{0}] Your closed card is the Three Of Hearts with a value of {1}", moduleId, ClosedCardValue);
                ClosedCard = "ThreeOfHearts";
            }
            else if (BombInfo.IsPortPresent(Port.Parallel))
            {
                totalSum = totalSum + 6;
                CorrectBet = 250;
                ClosedCardValue = 6;
                Debug.LogFormat("[Blackjack #{0}] Your closed card is the Six Of Diamonds with a value of {1}", moduleId, ClosedCardValue);
                ClosedCard = "SixOfDiamonds";
            }
            else
            {
                totalSum = totalSum + 1;
                CorrectBet = 10;
                ClosedCardValue = 1;
                Debug.LogFormat("[Blackjack #{0}] Your closed card is the Ace Of Spades with a value of {1}", moduleId, ClosedCardValue);
                ClosedCard = "AceOfSpades";
                BlackjackAtStart = true;
            }
        } //Ten Of Clubs

        Debug.LogFormat("[Blackjack #{0}] The correct bet is ${1}", moduleId, CorrectBet);
        return;
    }

    void DealingCardExceptions()
    {
        Suit = UnityEngine.Random.Range(1, 5);
        DealtCard = UnityEngine.Random.Range(1, 7);

        if (Exception1 == true)
        {
            if (Suit == 1)
            {
                ExtraCard.material.mainTexture = AceOfSpades;
                totalSum = totalSum + 1;
                DealingValue = 1;
                Debug.LogFormat("[Blackjack #{0}] Your extra card is the Ace Of Spades", moduleId);
            }
            if (Suit == 2)
            {
                ExtraCard.material.mainTexture = AceOfHearts;
                totalSum = totalSum + 1;
                DealingValue = 1;
                Debug.LogFormat("[Blackjack #{0}] Your extra card is the Ace Of Hearts", moduleId);
            }
            if (Suit == 3)
            {
                ExtraCard.material.mainTexture = AceOfClubs;
                totalSum = totalSum + 1;
                DealingValue = 1;
                Debug.LogFormat("[Blackjack #{0}] Your extra card is the Ace Of Clubs", moduleId);
            }
            if (Suit == 4)
            {
                ExtraCard.material.mainTexture = AceOfDiamonds;
                totalSum = totalSum + 1;
                DealingValue = 1;
                Debug.LogFormat("[Blackjack #{0}] Your extra card is the Ace Of Diamonds", moduleId);
            }
            Exception = true;
        }
        else if (Exception2 == true)
        {
            if (Suit == 1)
            {
                ExtraCard.material.mainTexture = TwoOfSpades;
                totalSum = totalSum + 2;
                DealingValue = 2;
                Debug.LogFormat("[Blackjack #{0}] Your extra card is the Two Of Spades", moduleId);
            }
            if (Suit == 2)
            {
                ExtraCard.material.mainTexture = TwoOfHearts;
                totalSum = totalSum + 2;
                DealingValue = 2;
                Debug.LogFormat("[Blackjack #{0}] Your extra card is the Two Of Hearts", moduleId);
            }
            if (Suit == 3)
            {
                ExtraCard.material.mainTexture = TwoOfClubs;
                totalSum = totalSum + 2;
                DealingValue = 2;
                Debug.LogFormat("[Blackjack #{0}] Your extra card is the Two Of Clubs", moduleId);
            }
            if (Suit == 4)
            {
                ExtraCard.material.mainTexture = TwoOfDiamonds;
                totalSum = totalSum + 2;
                DealingValue = 2;
                Debug.LogFormat("[Blackjack #{0}] Your extra card is the Two Of Diamonds", moduleId);
            }

            Exception = true;
        }
        else if (BlackjackAtStart == true)
        {
            //Blackjack
            Exception = true;
        }
        else
        {
        }

        DealingCard();
    }

    void DealingCard()
    {
        if (Exception == false)
        {
            if (Suit == 1) //Aces
            {
                if (DealtCard == 1)
                {
                    ExtraCard.material.mainTexture = AceOfSpades;
                    totalSum = totalSum + 1;
                    DealingValue = 1;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Ace Of Spades", moduleId);
                }
                else if (DealtCard == 2)
                {
                    ExtraCard.material.mainTexture = TwoOfSpades;
                    totalSum = totalSum + 2;
                    DealingValue = 2;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Two Of Spades", moduleId);
                }
                else if (DealtCard == 3)
                {
                    ExtraCard.material.mainTexture = ThreeOfSpades;
                    totalSum = totalSum + 3;
                    DealingValue = 3;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Three Of Spades", moduleId);
                }
                else if (DealtCard == 4)
                {
                    ExtraCard.material.mainTexture = FourOfSpades;
                    totalSum = totalSum + 4;
                    DealingValue = 4;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Four Of Spades", moduleId);
                }
                else if (DealtCard == 5)
                {
                    ExtraCard.material.mainTexture = FiveOfSpades;
                    totalSum = totalSum + 5;
                    DealingValue = 5;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Five Of Spades", moduleId);
                }
                else
                {
                    ExtraCard.material.mainTexture = SixOfSpades;
                    totalSum = totalSum + 6;
                    DealingValue = 6;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Six Of Spades", moduleId);
                }
            } // Spades
            else if (Suit == 2)
            {
                if (DealtCard == 1)
                {
                    ExtraCard.material.mainTexture = AceOfHearts;
                    totalSum = totalSum + 1;
                    DealingValue = 1;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Ace Of Hearts", moduleId);
                }
                else if (DealtCard == 2)
                {
                    ExtraCard.material.mainTexture = TwoOfHearts;
                    totalSum = totalSum + 2;
                    DealingValue = 2;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Two Of Hearts", moduleId);
                }
                else if (DealtCard == 3)
                {
                    ExtraCard.material.mainTexture = ThreeOfHearts;
                    totalSum = totalSum + 3;
                    DealingValue = 3;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Three Of Hearts", moduleId);
                }
                else if (DealtCard == 4)
                {
                    ExtraCard.material.mainTexture = FourOfHearts;
                    totalSum = totalSum + 4;
                    DealingValue = 4;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Four Of Hearts", moduleId);
                }
                else if (DealtCard == 5)
                {
                    ExtraCard.material.mainTexture = FiveOfHearts;
                    totalSum = totalSum + 5;
                    DealingValue = 5;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Five Of Hearts", moduleId);
                }
                else
                {
                    ExtraCard.material.mainTexture = SixOfHearts;
                    totalSum = totalSum + 6;
                    DealingValue = 6;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Six Of Hearts", moduleId);
                }
            } // Hearts
            else if (Suit == 3)
            {
                if (DealtCard == 1)
                {
                    ExtraCard.material.mainTexture = AceOfClubs;
                    totalSum = totalSum + 1;
                    DealingValue = 1;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Ace Of Clubs", moduleId);
                }
                else if (DealtCard == 2)
                {
                    ExtraCard.material.mainTexture = TwoOfClubs;
                    totalSum = totalSum + 2;
                    DealingValue = 2;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Two Of Clubs", moduleId);
                }
                else if (DealtCard == 3)
                {
                    ExtraCard.material.mainTexture = ThreeOfClubs;
                    totalSum = totalSum + 3;
                    DealingValue = 3;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Three Of Clubs", moduleId);
                }
                else if (DealtCard == 4)
                {
                    ExtraCard.material.mainTexture = FourOfClubs;
                    totalSum = totalSum + 4;
                    DealingValue = 4;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Four Of Clubs", moduleId);
                }
                else if (DealtCard == 5)
                {
                    ExtraCard.material.mainTexture = FiveOfClubs;
                    totalSum = totalSum + 5;
                    DealingValue = 5;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Five Of Clubs", moduleId);
                }
                else
                {
                    ExtraCard.material.mainTexture = SixOfClubs;
                    totalSum = totalSum + 6;
                    DealingValue = 6;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Six Of Clubs", moduleId);
                }
            } // Clubs
            else if (Suit == 4)
            {
                if (DealtCard == 1)
                {
                    ExtraCard.material.mainTexture = AceOfDiamonds;
                    totalSum = totalSum + 1;
                    DealingValue = 1;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Ace Of Diamonds", moduleId);
                }
                else if (DealtCard == 2)
                {
                    ExtraCard.material.mainTexture = TwoOfDiamonds;
                    totalSum = totalSum + 2;
                    DealingValue = 2;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Two Of Diamonds", moduleId);
                }
                else if (DealtCard == 3)
                {
                    ExtraCard.material.mainTexture = ThreeOfDiamonds;
                    totalSum = totalSum + 3;
                    DealingValue = 3;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Three Of Diamonds", moduleId);
                }
                else if (DealtCard == 4)
                {
                    ExtraCard.material.mainTexture = FourOfDiamonds;
                    totalSum = totalSum + 4;
                    DealingValue = 4;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Four Of Diamonds", moduleId);
                }
                else if (DealtCard == 5)
                {
                    ExtraCard.material.mainTexture = FiveOfDiamonds;
                    totalSum = totalSum + 5;
                    DealingValue = 5;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Five Of Diamonds", moduleId);
                }
                else
                {
                    ExtraCard.material.mainTexture = FiveOfDiamonds;
                    totalSum = totalSum + 5;
                    DealingValue = 5;
                    Debug.LogFormat("[Blackjack #{0}] Your extra card is the Five Of Diamonds", moduleId);
                }
            } // Diamonds       
            else
            {
                ExtraCard.material.mainTexture = AceOfSpades;
                totalSum = totalSum + 1;
                DealingValue = 1;
                Debug.LogFormat("[Blackjack #{0}] Your extra card is the Ace Of Spades", moduleId);
            } //If the DealtCard is somehow invalid: Deal Ace of Spades
        }
        Debug.LogFormat("[Blackjack #{0}] Dealing Value is {1}", moduleId, DealingValue);
        DealingRules();
    }

    void DealingRules()
    {
        if (DealingValue < 4)
        {
            if (CorrectBet == 1)
            {
                DealingOrder = 1;
                Debug.LogFormat("[Blackjack #{0}] Dealing Order is 1", moduleId);
            }
            else if (CorrectBet == 10)
            {
                DealingOrder = 2;
                Debug.LogFormat("[Blackjack #{0}] Dealing Order is 2", moduleId);
            }
            else if (CorrectBet == 100)
            {
                DealingOrder = 3;
                Debug.LogFormat("[Blackjack #{0}] Dealing Order is 3", moduleId);
            }
            else if (CorrectBet == 250)
            {
                Debug.LogFormat("[Blackjack #{0}] Dealing Order is 4", moduleId);
                DealingOrder = 4;
            }
        }
        else
        {
            if (CorrectBet == 1)
            {
                DealingOrder = 5;
                Debug.LogFormat("[Blackjack #{0}] Dealing Order is 5", moduleId);
            }
            else if (CorrectBet == 10)
            {
                DealingOrder = 6;
                Debug.LogFormat("[Blackjack #{0}] Dealing Order is 6", moduleId);
            }
            else if (CorrectBet == 100)
            {
                DealingOrder = 7;
                Debug.LogFormat("[Blackjack #{0}] Dealing Order is 7", moduleId);
            }
            else if (CorrectBet == 250)
            {
                DealingOrder = 8;
                Debug.LogFormat("[Blackjack #{0}] Dealing Order is 8", moduleId);
            }
        }
        CheckForAce();
    }

    void CheckForAce()
    {
        if (Aces > 0 && totalSum > 21)
        {
            Aces--;
            totalSum = totalSum - 10;
            Debug.LogFormat("[Blackjack #{0}] One of the aces in your hand was used.", moduleId);
        }
        TotalSumLog();
    }     
    void TotalSumLog()
    {
        Debug.LogFormat("[Blackjack #{0}] The total sum of cards without hitting is {1}", moduleId, totalSum);
    }

    IEnumerator HitOrStand()
    {
        for (int i = 0; i < 3; i++)
        {
            timer = i;
            if (timer == 1)
            {
                UpdateSelectable();
                Response.text = "Hit or Stand?";
            }
            yield return new WaitForSecondsRealtime(1);
        }
    }

    IEnumerator CheckForBlackjack()
    {
        for (int i = 0; i < 999999999; i++)
        {
            if (totalSum == 21 && BlackjackMessage == false)
            {
                Debug.LogFormat("[Blackjack #{0}] Blackjack!", moduleId);
                BlackjackMessage = true;
            }
            yield return new WaitForSecondsRealtime(1);
        }
    }

    IEnumerator Restart()
    {
        for (int i = 0; i < 6; i++)
        {
            timer = i;
            if (timer == 0)
            {
                ListButtons.Clear();
                /*foreach (Transform child in EmptyBtns.transform)
                {
                    GameObject Lol = child.gameObject;
                    ListButtons.Add(Lol.activeSelf ? Lol.GetComponent<KMSelectable>() : null);
                } */
                ModuleSelectable.Children = ListButtons.ToArray();
                ModuleSelectable.UpdateChildren();
                MainCard.material.mainTexture = mainCard;
                CardClosedRender.material.mainTexture = mainCard;
                ExtraCard.material.mainTexture = mainCard;
                HitCard1.material.mainTexture = mainCard;
                HitCard2.material.mainTexture = mainCard;
                Response.text = "Player busted!";
            }
            else if (timer == 1)
            {
                Response.text = "Restarting...";
            }
            else if (timer == 2)
            {
                Response.text = "Player busted!";
            }
            else if (timer == 3)
            {
                Response.text = "Restarting...";
            }
            else if (timer == 4)
            {
                HittingAllowed = false;
                StandingAllowed = false;
                Start();
            }
            else if (timer == 6)
            {
                StopAllCoroutines();
            }
            yield return new WaitForSecondsRealtime(1);
        }
    }

    IEnumerator CheckForOver21()
    {
        for (int i = 0; i < 2000000000; i++)
        {
            if (totalSum > 21)
            {
                if (Aces == 1)
                {
                    Aces = 0;
                    totalSum = totalSum - 10;
                }
                else
                {
                    Debug.LogFormat("[Blackjack #{0}] Busted!", moduleId);
                    Over21();
                    StopCoroutine("CheckForOver21");
                }
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    IEnumerator InputDelay()
    {
        for (int i = 0; i < 3; i++)
        {
            timer = i;
            if (timer == 2)
            {
                if (isCard2Dealt == false)
                {
                    Debug.LogFormat("[Blackjack #{0}] Card can now be dealt again!", moduleId);
                }
                DealCard = true;
            }
            yield return new WaitForSecondsRealtime(1);
        }
    }

    protected bool Betting1()
    {
        if (CorrectBet == 1 && BettingComplete == false)
        {
            if (BlackjackAtStart == true)
            {
                BlackjackAtStart = false;
            }
            HingeHit.gameObject.transform.Rotate(0, 0, 90);
            HingeStand.gameObject.transform.Rotate(0, 0, 90);
            BlackjackBtn.OnInteract = Empty;
            Response.text = "Betting Complete";
            ListButtons.Clear();
            ModuleSelectable.Children = ListButtons.ToArray();
            ModuleSelectable.UpdateChildren();
            StartCoroutine("HitOrStand");
            HittingAllowed = true;
            StandingAllowed = true;
            BettingComplete = true;
            HitText.color = Color.black;
            StandText.color = Color.black;
            DealCard1.PlaySoundAtTransform("DealCard1", transform);
            BettingCoin.material.mainTexture = BetCoin1;
            BetWorth.text = "$1";
            DealingCardExceptions();
        }
        else if (BettingComplete == true)
        {
        }
        else
        {
            GetComponent<KMBombModule>().HandleStrike();
        }
        GetComponent<KMSelectable>().AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        return false;
    }

    protected bool Betting10()
    {
        if (CorrectBet == 10 && BettingComplete == false)
        {
            if (BlackjackAtStart == true)
            {
                BlackjackAtStart = false;
            }
            HingeHit.gameObject.transform.Rotate(0, 0, 90);
            HingeStand.gameObject.transform.Rotate(0, 0, 90);
            Response.text = "Betting Complete";
            BlackjackBtn.OnInteract = Empty;
            ListButtons.Clear();
            ModuleSelectable.Children = ListButtons.ToArray();
            ModuleSelectable.UpdateChildren();
            StartCoroutine("HitOrStand");
            HittingAllowed = true;
            StandingAllowed = true;
            BettingComplete = true;
            HitText.color = Color.black;
            StandText.color = Color.black;
            DealCard1.PlaySoundAtTransform("DealCard1", transform);
            BettingCoin.material.mainTexture = BetCoin10;
            BetWorth.text = "$10";
            DealingCardExceptions();
        }
        else if (BettingComplete == true)
        {
        }
        else
        {
            GetComponent<KMBombModule>().HandleStrike();
        }
        GetComponent<KMSelectable>().AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        return false;
    }

    protected bool Betting100()
    {
        if (CorrectBet == 100 && BettingComplete == false)
        {
            if (BlackjackAtStart == true)
            {
                BlackjackAtStart = false;
            }
            HingeHit.gameObject.transform.Rotate(0, 0, 90);
            HingeStand.gameObject.transform.Rotate(0, 0, 90);
            Response.text = "Betting Complete";
            BlackjackBtn.OnInteract = Empty;
            ListButtons.Clear();
            ModuleSelectable.Children = ListButtons.ToArray();
            ModuleSelectable.UpdateChildren();
            StartCoroutine("HitOrStand");
            BettingComplete = true;
            HittingAllowed = true;
            StandingAllowed = true;
            BetWorth.text = "$100";
            HitText.color = Color.black;
            StandText.color = Color.black;
            DealCard1.PlaySoundAtTransform("DealCard1", transform);
            BettingCoin.material.mainTexture = BetCoin100;
            DealingCardExceptions();
        }
        else if (BettingComplete == true)
        {
        }
        else
        {
            GetComponent<KMBombModule>().HandleStrike();
        }
        GetComponent<KMSelectable>().AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        return false;
    }

    protected bool Betting250()
    {
        if (CorrectBet == 250 && BettingComplete == false)
        {
            if (BlackjackAtStart == true)
            {
                BlackjackAtStart = false;
            }
            HingeHit.gameObject.transform.Rotate(0, 0, 90);
            HingeStand.gameObject.transform.Rotate(0, 0, 90);
            Response.text = "Betting Complete";
            BlackjackBtn.OnInteract = Empty;
            ListButtons.Clear();
            ModuleSelectable.Children = ListButtons.ToArray();
            ModuleSelectable.UpdateChildren();
            StartCoroutine("HitOrStand");
            BettingComplete = true;
            HittingAllowed = true;
            StandingAllowed = true;
            BetWorth.text = "$250";
            HitText.color = Color.black;
            StandText.color = Color.black;
            DealCard1.PlaySoundAtTransform("DealCard1", transform);
            BettingCoin.material.mainTexture = BetCoin250;
            DealingCardExceptions();
        }
        else if (BettingComplete == true)
        {
        }
        else
        {
            GetComponent<KMBombModule>().HandleStrike();
        }
        GetComponent<KMSelectable>().AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        return false;
    }

    protected bool BlackjackCheck()
    {
        GetComponent<KMSelectable>().AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("[Blackjack #{0}] You checked if you have Blackjack", moduleId);
        Debug.LogFormat("[Blackjack #{0}] The total sum of cards is {1}", moduleId, totalSum);
        if (totalSum == 21)
        {
            GetComponent<KMBombModule>().HandlePass();
            Debug.LogFormat("[Blackjack #{0}] You have blackjack! Module passed.", moduleId);
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
            ResultWin();
            Solved = true;
        }
        else
        {
            Bet1.OnInteract = Empty;
            Bet10.OnInteract = Empty;
            Bet100.OnInteract = Empty;
            Bet250.OnInteract = Empty;
            HitBtn.OnInteract = Empty;
            StandBtn.OnInteract = Empty;
            BlackjackBtn.OnInteract = Empty;
            GetComponent<KMBombModule>().HandleStrike();
            ListButtons.Clear();
            foreach (Transform child in Buttons.transform)
            {
                GameObject Lol = child.gameObject;
                ListButtons.Add(Lol.activeSelf ? Lol.GetComponent<KMSelectable>() : null);
            }
            ModuleSelectable.Children = ListButtons.ToArray();
            ModuleSelectable.UpdateChildren();
            Debug.LogFormat("[Blackjack #{0}] You don't have blackjack! Strike handed.", moduleId);
            StartCoroutine("ResultNoBj");
            HittingAllowed = false;
            StandingAllowed = false;
        }
        return false;
    }


    protected bool HitCard()
    {
        if (Solved == true)
        {
            return false;
        }
        if (HittingAllowed == true && DealCard == true)
        {
            if (DealingOrder == 1)
            {
                if (isCard1Dealt == false)
                {
                    HitCard1.material.mainTexture = TwoOfSpades;
                    totalSum = totalSum + 2;
                    isCard1Dealt = true;
                    Hits = 1;
                    DealCard2.PlaySoundAtTransform("DealCard2", transform);
                    DealCard = false;
                    return false;
                }
                else if (isCard1Dealt == true && isCard2Dealt == false)
                {
                    Shuffle.PlaySoundAtTransform("DealCard3", transform);
                    HitCard2.material.mainTexture = EightOfHearts;
                    totalSum = totalSum + 8;
                    isCard2Dealt = true;
                    Hits = 2;
                    DealCard = false;
                    return false;
                }
                else
                {
                }
            }
            else if (DealingOrder == 2)
            {
                if (isCard1Dealt == false)
                {
                    HitCard1.material.mainTexture = SixOfDiamonds;
                    totalSum = totalSum + 6;
                    isCard1Dealt = true;
                    Hits = 1;
                    DealCard2.PlaySoundAtTransform("DealCard2", transform);
                    DealCard = false;
                    return false;
                }
                else if (isCard1Dealt == true && isCard2Dealt == false)
                {
                    DealCard3.PlaySoundAtTransform("DealCard3", transform);
                    HitCard2.material.mainTexture = SevenOfClubs;
                    totalSum = totalSum + 7;
                    isCard2Dealt = true;
                    Hits = 2;
                    DealCard = false;
                    return false;
                }
                else
                {
                }
            }
            else if (DealingOrder == 3)
            {
                if (isCard1Dealt == false)
                {
                    HitCard1.material.mainTexture = ThreeOfClubs;
                    totalSum = totalSum + 3;
                    isCard1Dealt = true;
                    Hits = 1;
                    DealCard2.PlaySoundAtTransform("DealCard2", transform);
                    DealCard = false;
                    StartCoroutine("InputDelay");
                    return false;
                }
                else if (isCard1Dealt == true && isCard2Dealt == false)
                {
                    DealCard3.PlaySoundAtTransform("DealCard3", transform);
                    HitCard2.material.mainTexture = FiveOfHearts;
                    totalSum = totalSum + 5;
                    isCard2Dealt = true;
                    Hits = 2;
                    DealCard = false;
                    StartCoroutine("InputDelay");
                    return false;
                }
                else
                {
                }
            }
            else if (DealingOrder == 4)
            {
                if (isCard1Dealt == false)
                {
                    HitCard1.material.mainTexture = TwoOfDiamonds;
                    totalSum = totalSum + 10;
                    isCard1Dealt = true;
                    Hits = 1;
                    DealCard2.PlaySoundAtTransform("DealCard2", transform);
                    DealCard = false;
                    return false;
                }
                else if (isCard1Dealt == true && isCard2Dealt == false)
                {
                    DealCard3.PlaySoundAtTransform("DealCard3", transform);
                    HitCard2.material.mainTexture = JackOfSpades;
                    totalSum = totalSum + 10;
                    isCard2Dealt = false;
                    Hits = 2;
                    DealCard = false;
                    return false;

                }
                else
                {
                }
            }
            else
            {
                HitCardCont();
            }

        }
        else
        {
        }
        
        GetComponent<KMSelectable>().AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        return false;
    }

    void HitCardCont() //Script somehow won't read the entire thing if it's too long. Oh well.
    {
        if (HittingAllowed == true && DealCard == true)
        {
            if (DealingOrder == 5)
            {
                if (isCard1Dealt == false)
                {
                    HitCard1.material.mainTexture = FiveOfHearts;
                    totalSum = totalSum + 5;
                    isCard1Dealt = true;
                    Hits = 1;
                    DealCard2.PlaySoundAtTransform("DealCard2", transform);
                    DealCard = false;
                    StartCoroutine("InputDelay");

                }
                else if (isCard1Dealt == true && isCard2Dealt == false)
                {
                    DealCard3.PlaySoundAtTransform("DealCard3", transform);
                    HitCard2.material.mainTexture = EightOfSpades;
                    totalSum = totalSum + 8;
                    isCard2Dealt = true;
                    Hits = 2;
                    DealCard = false;
                    StartCoroutine("InputDelay");

                }
                else
                {
                }
            }
            else if (DealingOrder == 6)
            {
                if (isCard1Dealt == false)
                {
                    HitCard1.material.mainTexture = QueenOfHearts;
                    totalSum = totalSum + 10;
                    Hits = 1;
                    Shuffle.PlaySoundAtTransform("DealCard2", transform);
                    isCard1Dealt = true;
                    DealCard = false;
                    StartCoroutine("InputDelay");

                }
                else if (isCard1Dealt == true && isCard2Dealt == false)
                {
                    Shuffle.PlaySoundAtTransform("DealCard3", transform);
                    HitCard2.material.mainTexture = AceOfDiamonds;
                    totalSum = totalSum + 1;
                    Hits = 2;
                    isCard2Dealt = true;
                    DealCard = false;
                    StartCoroutine("InputDelay");

                }
                else
                {
                }
            }
            else if (DealingOrder == 7)
            {
                if (isCard1Dealt == false)
                {
                    HitCard1.material.mainTexture = FiveOfDiamonds;
                    totalSum = totalSum + 5;
                    Hits = 1;
                    Shuffle.PlaySoundAtTransform("DealCard2", transform);
                    isCard1Dealt = true;
                    DealCard = false;
                    StartCoroutine("InputDelay");

                }
                else if (isCard1Dealt == true && isCard2Dealt == false)
                {
                    Shuffle.PlaySoundAtTransform("DealCard3", transform);
                    HitCard2.material.mainTexture = QueenOfClubs;
                    totalSum = totalSum + 10;
                    isCard2Dealt = true;
                    Hits = 2;
                    DealCard = false;
                    StartCoroutine("InputDelay");

                }
                else
                {
                }
            }
            else if (DealingOrder == 8)
            {
                if (isCard1Dealt == false)
                {
                    HitCard1.material.mainTexture = NineOfClubs;
                    totalSum = totalSum + 9;
                    Hits = 1;
                    isCard1Dealt = true;
                    Shuffle.PlaySoundAtTransform("DealCard2", transform);
                    DealCard = false;

                }
                else if (isCard1Dealt == true && isCard2Dealt == false)
                {
                    Shuffle.PlaySoundAtTransform("DealCard3", transform);
                    HitCard2.material.mainTexture = AceOfSpades;
                    totalSum = totalSum + 1;
                    isCard2Dealt = true;
                    Hits = 2;
                    DealCard = false;

                }
                else
                {
                }
            }
        }
        if (totalSum > 21)
        {
            if (Aces == 1)
            {
                Aces = 0;
                totalSum = totalSum - 10;
                LogHitAce();
            }
        }
        LogHit();
        StartCoroutine("InputDelay");
    }

    void LogHit()
    {
        Debug.LogFormat("[Blackjack #{0}] The total sum of cards after hit {1} is {2}", moduleId, Hits, totalSum);
    }

    void LogHitAce()
    {
        Debug.LogFormat("[Blackjack #{0}] The total sum of cards after hit {1} and using 1 ace is {2}", moduleId, Hits, totalSum);
    }

    protected bool Stand()
    {
        GetComponent<KMSelectable>().AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (BlackjackAtStart == true)
        {
            Debug.LogFormat("[Blackjack #{0}] You have blackjack! Module passed.", moduleId, totalSum);
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
            GetComponent<KMBombModule>().HandlePass();
            Solved = true;
            ResultBlackjack();
            Deactivated();

        }
        if (StandingAllowed == false)
        {
            return false;
        }
        if (Solved == true)
        {
            return false;
        }
        Debug.LogFormat("[Blackjack #{0}] Standing...", moduleId);
        if (DealingOrder == 1)
        {
            if (isCard1Dealt == false)
            {
                DealerCard1.material.mainTexture = TwoOfSpades;
                DealerCard2.material.mainTexture = EightOfDiamonds;
                DealerCard3.material.mainTexture = SevenOfHearts;
                totalSumDealer = 17;
            }
            else if (isCard1Dealt == true && isCard2Dealt == false)
            {
                DealerCard1.material.mainTexture = EightOfDiamonds;
                DealerCard2.material.mainTexture = SevenOfHearts;
                DealerCard3.material.mainTexture = NineOfSpades;
                totalSumDealer = 24;
            }
            else
            {
                DealerCard1.material.mainTexture = SevenOfHearts;
                DealerCard2.material.mainTexture = NineOfSpades;
                DealerCard3.material.mainTexture = FourOfDiamonds;
                totalSumDealer = 20;
            }
        }
        else if (DealingOrder == 2)
        {
            if (isCard1Dealt == false)
            {
                DealerCard1.material.mainTexture = SixOfDiamonds;
                DealerCard2.material.mainTexture = SevenOfClubs;
                DealerCard3.material.mainTexture = TwoOfClubs;
                totalSumDealer = 15;
            }
            else if (isCard1Dealt == true && isCard2Dealt == false)
            {
                DealerCard1.material.mainTexture = SevenOfClubs;
                DealerCard2.material.mainTexture = TwoOfClubs;
                DealerCard3.material.mainTexture = EightOfClubs;
                totalSumDealer = 17;
            }
            else
            {
                DealerCard1.material.mainTexture = TwoOfClubs;
                DealerCard2.material.mainTexture = EightOfClubs;
                DealerCard3.material.mainTexture = SixOfSpades;
                totalSumDealer = 16;
            }
        }
        else if (DealingOrder == 3)
        {
            if (isCard1Dealt == false)
            {
                DealerCard1.material.mainTexture = ThreeOfClubs;
                DealerCard2.material.mainTexture = FiveOfHearts;
                DealerCard3.material.mainTexture = QueenOfDiamonds;
                totalSumDealer = 18;
            }
            else if (isCard1Dealt == true && isCard2Dealt == false)
            {
                DealerCard1.material.mainTexture = FiveOfHearts;
                DealerCard2.material.mainTexture = QueenOfDiamonds;
                DealerCard3.material.mainTexture = ThreeOfDiamonds;
                totalSumDealer = 18;
            }
            else
            {
                DealerCard1.material.mainTexture = QueenOfDiamonds;
                DealerCard2.material.mainTexture = ThreeOfDiamonds;
                DealerCard3.material.mainTexture = SixOfSpades;
                totalSumDealer = 19;
            }
        }
        else if (DealingOrder == 4)
        {
            if (isCard1Dealt == false)
            {
                DealerCard1.material.mainTexture = TwoOfDiamonds;
                DealerCard2.material.mainTexture = JackOfSpades;
                DealerCard3.material.mainTexture = NineOfClubs;
                totalSumDealer = 21;
            }
            else if (isCard1Dealt == true && isCard2Dealt == false)
            {
                DealerCard1.material.mainTexture = JackOfSpades;
                DealerCard2.material.mainTexture = NineOfClubs;
                DealerCard3.material.mainTexture = SevenOfHearts;
                totalSumDealer = 26;
            }
            else
            {
                DealerCard1.material.mainTexture = NineOfClubs;
                DealerCard2.material.mainTexture = SevenOfHearts;
                DealerCard3.material.mainTexture = ThreeOfSpades;
                totalSumDealer = 19;
            }
        }
        else if (DealingOrder == 5)
        {
            if (isCard1Dealt == false)
            {
                DealerCard1.material.mainTexture = FiveOfHearts;
                DealerCard2.material.mainTexture = EightOfSpades;
                DealerCard3.material.mainTexture = FiveOfDiamonds;
                totalSumDealer = 18;
            }
            else if (isCard1Dealt == true && isCard2Dealt == false)
            {
                DealerCard1.material.mainTexture = FiveOfSpades;
                DealerCard2.material.mainTexture = FiveOfDiamonds;
                DealerCard3.material.mainTexture = FourOfClubs;
                totalSumDealer = 14;
            }
            else
            {
                DealerCard1.material.mainTexture = FiveOfDiamonds;
                DealerCard2.material.mainTexture = FourOfClubs;
                DealerCard3.material.mainTexture = JackOfHearts;
                totalSumDealer = 19;
            }
        }
        else if (DealingOrder == 6)
        {
            if (isCard1Dealt == false)
            {
                DealerCard1.material.mainTexture = QueenOfHearts;
                DealerCard2.material.mainTexture = AceOfDiamonds;
                DealerCard3.material.mainTexture = FourOfClubs;
                totalSumDealer = 15;
            }
            else if (isCard1Dealt == true && isCard2Dealt == false)
            {
                DealerCard1.material.mainTexture = AceOfDiamonds;
                DealerCard2.material.mainTexture = FourOfClubs;
                DealerCard3.material.mainTexture = KingOfSpades;
                totalSumDealer = 15;
            }
            else
            {
                DealerCard1.material.mainTexture = FourOfClubs;
                DealerCard2.material.mainTexture = KingOfSpades;
                DealerCard3.material.mainTexture = TwoOfDiamonds;
                totalSumDealer = 16;
            }
        }
        else if (DealingOrder == 7)
        {
            if (isCard1Dealt == false)
            {
                DealerCard1.material.mainTexture = FiveOfDiamonds;
                DealerCard2.material.mainTexture = QueenOfClubs;
                DealerCard3.material.mainTexture = SevenOfHearts;
                totalSumDealer = 22;
            }
            else if (isCard1Dealt == true && isCard2Dealt == false)
            {
                DealerCard1.material.mainTexture = QueenOfClubs;
                DealerCard2.material.mainTexture = SevenOfHearts;
                DealerCard3.material.mainTexture = NineOfClubs;
                totalSumDealer = 26;
            }
            else
            {
                DealerCard1.material.mainTexture = SevenOfHearts;
                DealerCard2.material.mainTexture = NineOfClubs;
                DealerCard3.material.mainTexture = FiveOfHearts;
                totalSumDealer = 21;
            }
        }
        else if (DealingOrder == 8)
        {
            if (isCard1Dealt == false)
            {
                DealerCard1.material.mainTexture = NineOfClubs;
                DealerCard2.material.mainTexture = AceOfSpades;
                DealerCard3.material.mainTexture = ThreeOfHearts;
                totalSumDealer = 13;
            }
            else if (isCard1Dealt == true && isCard2Dealt == false)
            {
                DealerCard1.material.mainTexture = AceOfSpades;
                DealerCard2.material.mainTexture = ThreeOfHearts;
                DealerCard3.material.mainTexture = KingOfSpades;
                totalSumDealer = 14;
            }
            else
            {
                DealerCard1.material.mainTexture = ThreeOfHearts;
                DealerCard2.material.mainTexture = KingOfSpades;
                DealerCard3.material.mainTexture = SixOfSpades;
                totalSumDealer = 19;
            }
        }

        Debug.LogFormat("[Blackjack #{0}] Your hand is worth {1}", moduleId, totalSum);
        Debug.LogFormat("[Blackjack #{0}] The dealer's hand is worth {1}", moduleId, totalSumDealer);
        AnswerCheck();
        return false;
    }

    void AnswerCheck()
    {
        if (totalSumDealer > 21)
        {
            GetComponent<KMBombModule>().HandlePass();
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
            Solved = true;
            ResultWin();
            Debug.LogFormat("[Blackjack #{0}] Dealer busted! Module passed.", moduleId, totalSum);
            //All possible Ace cards:
            if (ClosedCard == "KingOfHearts")
            {
                CardClosedRender.material.mainTexture = KingOfHearts;
            }
            if (ClosedCard == "FiveOfDiamonds")
            {
                CardClosedRender.material.mainTexture = FiveOfDiamonds;
            }
            if (ClosedCard == "SevenOfSpades")
            {
                CardClosedRender.material.mainTexture = SevenOfSpades;
            }
            if (ClosedCard == "TwoOfClubs")
            {
                CardClosedRender.material.mainTexture = TwoOfClubs;
            }
            //All possible King cards:
            if (ClosedCard == "QueenOfHearts")
            {
                CardClosedRender.material.mainTexture = QueenOfHearts;
            }
            if (ClosedCard == "NineOfSpades")
            {
                CardClosedRender.material.mainTexture = NineOfSpades;
            }
            if (ClosedCard == "ThreeOfDiamonds")
            {
                CardClosedRender.material.mainTexture = ThreeOfDiamonds;
            }
            if (ClosedCard == "FourOfClubs")
            {
                CardClosedRender.material.mainTexture = FourOfClubs;
            }
            //All possible Two cards:
            if (ClosedCard == "AceOfDiamonds")
            {
                CardClosedRender.material.mainTexture = AceOfDiamonds;
            }
            if (ClosedCard == "ThreeOfHearts")
            {
                CardClosedRender.material.mainTexture = ThreeOfHearts;
            }
            if (ClosedCard == "SevenOfClubs")
            {
                CardClosedRender.material.mainTexture = SevenOfClubs;
            }
            if (ClosedCard == "FourOfSpades")
            {
                CardClosedRender.material.mainTexture = FourOfSpades;
            }
            //All possible Ten cards:
            if (ClosedCard == "FiveOfClubs")
            {
                CardClosedRender.material.mainTexture = FiveOfClubs;
            }
            if (ClosedCard == "ThreeOfHearts")
            {
                CardClosedRender.material.mainTexture = ThreeOfHearts;
            }
            if (ClosedCard == "SixOfDiamonds")
            {
                CardClosedRender.material.mainTexture = SixOfDiamonds;
            }
            if (ClosedCard == "AceOfSpades")
            {
                CardClosedRender.material.mainTexture = AceOfSpades;
            }
        }
        if (totalSum == 21)
        {
            Debug.LogFormat("[Blackjack #{0}] You have Blackjack! Module passed.", moduleId, totalSum);
            GetComponent<KMBombModule>().HandlePass();
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
            Solved = true;
            ResultBlackjack();

        }

        if (Solved == false)
        {
            if (totalSum > totalSumDealer)
            {
                Debug.LogFormat("[Blackjack #{0}] Your hand is worth more! Module passed.", moduleId, totalSum);
                GetComponent<KMBombModule>().HandlePass();
                GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                ResultWin();
                Solved = true;
            }
            else if (totalSum < totalSumDealer)
            {
                Debug.LogFormat("[Blackjack #{0}] Their hand is worth more! Strike handed.", moduleId, totalSum);
                GetComponent<KMBombModule>().HandleStrike();
                StartCoroutine("ResultLost");
                HittingAllowed = false;
                StandingAllowed = false;
            }
            else if (totalSum == totalSumDealer)
            {
                Debug.LogFormat("[Blackjack #{0}] Push, Tie game! Module passed.", moduleId, totalSum);
                GetComponent<KMBombModule>().HandlePass();
                GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                ResultWin();
                Solved = true;
            }
        }
        else if (Solved == true)
        {
        }
    }

    void UpdateSelectable()
    {
        foreach (Transform child in Buttons.transform)
        {
            GameObject Lol = child.gameObject;
            ListButtons.Add(Lol.activeSelf ? Lol.GetComponent<KMSelectable>() : null);
        }
        foreach (Transform child in LockedButtons.transform)
        {
            GameObject Lol = child.gameObject;
            ListButtons.Add(Lol.activeSelf ? Lol.GetComponent<KMSelectable>() : null);
        }
        HitBtn.OnInteract += HitCard;
        StandBtn.OnInteract += Stand;

        ModuleSelectable.Children = ListButtons.ToArray();
        ModuleSelectable.UpdateChildren();
    }

    void RestartSelectable()
    {
        ListButtons.Remove(HitBtn);
        ListButtons.Remove(StandBtn);

        ModuleSelectable.Children = ListButtons.ToArray();
        ModuleSelectable.UpdateChildren();
        return;
    }

    void Over21()
    {
        GetComponent<KMBombModule>().HandleStrike();
        StartCoroutine("Restart");
        BlackjackMessage = false;
        HittingAllowed = false;
        StandingAllowed = false;

        RestartSelectable();

        HitText.color = Color.grey;
        StandText.color = Color.grey;
        HingeHit.gameObject.transform.Rotate(0, 0, 270);
        HingeStand.gameObject.transform.Rotate(0, 0, 270);
    }

    void Deactivated()
    {
        Bet1.OnInteract = Empty;
        Bet10.OnInteract = Empty;
        Bet100.OnInteract = Empty;
        Bet250.OnInteract = Empty;
        HitBtn.OnInteract = Empty;
        StandBtn.OnInteract = Empty;
        BlackjackBtn.OnInteract = Empty;
    }

    protected bool Empty()
    {
        GetComponent<KMSelectable>().AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        return false;
    }

    void ShowClosedCard()
    {
        //All possible Ace cards:
        if (ClosedCard == "KingOfHearts")
        {
            CardClosedRender.material.mainTexture = KingOfHearts;
        }
        if (ClosedCard == "FiveOfDiamonds")
        {
            CardClosedRender.material.mainTexture = FiveOfDiamonds;
        }
        if (ClosedCard == "SevenOfSpades")
        {
            CardClosedRender.material.mainTexture = SevenOfSpades;
        }
        if (ClosedCard == "TwoOfClubs")
        {
            CardClosedRender.material.mainTexture = TwoOfClubs;
        }
        //All possible King cards:
        if (ClosedCard == "QueenOfHearts")
        {
            CardClosedRender.material.mainTexture = QueenOfHearts;
        }
        if (ClosedCard == "NineOfSpades")
        {
            CardClosedRender.material.mainTexture = NineOfSpades;
        }
        if (ClosedCard == "ThreeOfDiamonds")
        {
            CardClosedRender.material.mainTexture = ThreeOfDiamonds;
        }
        if (ClosedCard == "FourOfClubs")
        {
            CardClosedRender.material.mainTexture = FourOfClubs;
        }
        //All possible Two cards:
        if (ClosedCard == "AceOfDiamonds")
        {
            CardClosedRender.material.mainTexture = AceOfDiamonds;
        }
        if (ClosedCard == "ThreeOfHearts")
        {
            CardClosedRender.material.mainTexture = ThreeOfHearts;
        }
        if (ClosedCard == "SevenOfClubs")
        {
            CardClosedRender.material.mainTexture = SevenOfClubs;
        }
        if (ClosedCard == "FourOfSpades")
        {
            CardClosedRender.material.mainTexture = FourOfSpades;
        }
        //All possible Ten cards:
        if (ClosedCard == "FiveOfClubs")
        {
            CardClosedRender.material.mainTexture = FiveOfClubs;
        }
        if (ClosedCard == "ThreeOfHearts")
        {
            CardClosedRender.material.mainTexture = ThreeOfHearts;
        }
        if (ClosedCard == "SixOfDiamonds")
        {
            CardClosedRender.material.mainTexture = SixOfDiamonds;
        }
        if (ClosedCard == "AceOfSpades")
        {
            CardClosedRender.material.mainTexture = AceOfSpades;
        }
    }

    void ResultWin()
    {
        Response.text = "Congratulations!";
        ShowClosedCard();
        Deactivated();
    }

    void ResultBlackjack()
    {
        Response.text = "Blackjack!";
        Deactivated();
    }

    IEnumerator ResultLost()
    {
        for (int i = 0; i < 5; i++)
        {
            timer = i;
            if (timer == 0)
            {
                HitText.color = Color.grey;
                StandText.color = Color.grey;
                ListButtons.Clear();
                ShowClosedCard();
                /*foreach (Transform child in EmptyBtns.transform)
                {
                    GameObject Lol = child.gameObject;
                    ListButtons.Add(Lol.activeSelf ? Lol.GetComponent<KMSelectable>() : null);
                } */
                ModuleSelectable.Children = ListButtons.ToArray();
                ModuleSelectable.UpdateChildren();
                HingeHit.gameObject.transform.Rotate(0, 0, 270);
                HingeStand.gameObject.transform.Rotate(0, 0, 270);
                Response.text = "Dealer won!";
            }
            if (timer == 1)
            {
                Response.text = "Restarting...";
            }
            if (timer == 2)
            {
                Response.text = "Dealer won!";
            }
            if (timer == 3)
            {
                Response.text = "Restarting...";
            }
            if (timer == 4)
            {
                DealingRules();
                DealerCard1.material.mainTexture = mainCard;
                DealerCard2.material.mainTexture = mainCard;
                DealerCard3.material.mainTexture = mainCard;
                MainCard.material.mainTexture = mainCard;
                CardClosedRender.material.mainTexture = mainCard;
                ExtraCard.material.mainTexture = mainCard;
                HitCard1.material.mainTexture = mainCard;
                HitCard2.material.mainTexture = mainCard;
                HittingAllowed = false;
                StandingAllowed = false;
                Response.text = "Betting...";
                Start();
            }
            yield return new WaitForSecondsRealtime(1);
        }
    }

    IEnumerator ResultNoBj()
    {
        for (int i = 0; i < 5; i++)
        {
            if (BettingComplete == true)
            {
                HingeHit.gameObject.transform.Rotate(0, 0, 270);
                HingeStand.gameObject.transform.Rotate(0, 0, 270);
            }
            Response.text = "Uh oh...";
            timer = i;
            if (timer == 0)
            {
                ListButtons.Clear();
                /*foreach (Transform child in EmptyBtns.transform)
                {
                    GameObject Lol = child.gameObject;
                    ListButtons.Add(Lol.activeSelf ? Lol.GetComponent<KMSelectable>() : null);
                } */
                ModuleSelectable.Children = ListButtons.ToArray();
                ModuleSelectable.UpdateChildren();
                ShowClosedCard();
            }
            if (timer == 1)
            {
                Response.text = "No Blackjack!";
            }
            if (timer == 2)
            {
                Response.text = "Restarting...";
            }
            if (timer == 3)
            {
                Response.text = "No Blackjack!";
            }
            if (timer == 4)
            {
                DealingRules();
                DealerCard1.material.mainTexture = mainCard;
                DealerCard2.material.mainTexture = mainCard;
                DealerCard3.material.mainTexture = mainCard;
                MainCard.material.mainTexture = mainCard;
                CardClosedRender.material.mainTexture = mainCard;
                ExtraCard.material.mainTexture = mainCard;
                HitCard1.material.mainTexture = mainCard;
                HitCard2.material.mainTexture = mainCard;
                HittingAllowed = false;
                StandingAllowed = false;
                Response.text = "Betting...";
            }
            if (timer == 4)
            {
                foreach (Transform child in BjBtn.transform)
                {
                    GameObject Lol = child.gameObject;
                    ListButtons.Add(Lol.activeSelf ? Lol.GetComponent<KMSelectable>() : null);
                }
                ModuleSelectable.Children = ListButtons.ToArray();
                ModuleSelectable.UpdateChildren();
                Start();
            }
            yield return new WaitForSecondsRealtime(1);
        }
    }
}