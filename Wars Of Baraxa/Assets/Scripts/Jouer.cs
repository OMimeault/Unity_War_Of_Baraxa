﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Specialized;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;
using warsofbaraxa;

public class Jouer : MonoBehaviour
{
    //variable
    static public Joueur joueur1;
    ThreadLire ReceiveMessage;
    Thread t;
    static public bool  gameFini = false;
    static public bool  EstGagnant = false;
    static public bool  EstPerdant = false;
    static public PosZoneCombat[] ZoneCarteJoueur;
    static public PosZoneCombat[] ZoneCombat;
    static public GameObject[] styleCarteAlliercombat;
    static public PosZoneCombat[] ZoneCombatEnnemie;
    static public GameObject[] styleCarteEnnemisCombat;
    static public PosZoneCombat[] ZoneCarteEnnemie;
    public int NbCarteEnMainJoueur;
    public bool placerClick;
    public Texture2D Test;
    public Texture2D ble;
    public Texture2D bois;
    public Texture2D gem;
    public Texture2D Worker;
    public Texture2D PlayerChar;
    public Texture2D EnnemiChar;
    public SpriteRenderer ImageCarte;
    public static int NbBle; //test avec static
    public static int NbBois; // test avec static
    public static int NbGem; // test avec statics

    public AudioClip AttackSound;
    public GUIStyle GUIBox;
    public GUIStyle GUIButton;

    public static int attaqueBonus;
    public static int armureBonus;
    public static int vieBonus;
    ////////////////////////////////////////////// Spellz
    public static bool enTrainCaster = false;   //
    public static bool targetNeeded = false;    //
    public static bool isEnnemi = false;        //
    public static string effet;                 //
    public static string spellTarget;
    public static GameObject spell;             //
    public static string[] texteHabileteSansEspace;
    public static GameObject target;
    public static Carte carteTarget;
    public static int position;
    ////////////////////////////////////////////// Spellz
    public int NbWorkerMax;
    public int NbWorker;
    public int NbBleEnnemis;
    public int NbBoisEnnemis;
    public int NbGemEnnemis;
    public static int HpJoueur;
    public static int HpEnnemi;
    public int nbCarteAllier;
    public int nbCarteEnnemis;
    public Transform PlacementCarte;
    public Transform carteBack;
    public Transform carteBatiment;
    public Transform carteCreature;
    public GameObject card;
    public GameObject cardennemis;
    public JouerCarteBoard ScriptEnnemie;
    public int NoCarte;
    public int noCarteEnnemis;
    static public float pos;
    //deck + carte(objet unity)
    static public Deck tabCarteAllier;
    static public GameObject[] styleCarteAllier;
    static int[] ordrePige;
    static int dernierePige;
    //inutile i guess
    static public Carte[] tabCarteEnnemis;
    static public GameObject[] styleCarteEnnemis;

    static public bool MonTour;

    void Awake()
    {
        tabCarteAllier = ReceiveDeck(connexionServeur.sck);
        string message = recevoirResultat();
        if (message == "Premier Joueur")
            MonTour = true;
        else
            MonTour = false;   
    }

    void OnApplicationQuit()
    {
        envoyerMessage("deconnection");
    }

	//initialization
	void Start () {
        NoCarte = 0;
        ReceiveMessage = new ThreadLire();
        ReceiveMessage.workSocket = connexionServeur.sck;
        InitZoneJoueur();
        InitZoneEnnemie();
        InitZoneCombatEnnemie();
        InitZoneCombatJoueur();
        pos = 0;
        card = null;
        NbCarteEnMainJoueur = 0;
        HpJoueur = 30;
        HpEnnemi = 30;
        nbCarteAllier = 40;
        nbCarteEnnemis = 40;
        NbBle = 0;
        NbBois = 0;
        NbGem = 0;
        NbBleEnnemis = 0;
        NbBoisEnnemis = 0;
        NbGemEnnemis = 0;
        NbWorkerMax = 2;
        NbWorker = NbWorkerMax;
        placerClick = false;
        //stat bonus
        attaqueBonus = 0;
        armureBonus = 0;
        vieBonus = 0;
        //deck
        styleCarteAllier = new GameObject[40];
        //deck ennemis
        tabCarteEnnemis = new Carte[40];
        styleCarteEnnemis = new GameObject[40];
        ordrePige = new int[40];
        //board
        styleCarteAlliercombat = new GameObject[7];
        styleCarteEnnemisCombat = new GameObject[7];
        joueur1 = new Joueur("player1");
        instantiateCardAllies();
        instantiateCardEnnemis();
        initOrdrePige(ordrePige);
        CarteDepart(); 
	}
    private string formaterHabilete(string texte)
    {
        string temp="";
        const int maxcaractere=21;
        string[] tab = texte.Split(new char[] {','});
        for (int i = 0; i < tab.Length; ++i)
        {
            if (tab[i].Length > maxcaractere)
            {
                int longueur=0;
                string[] mot = tab[i].Split(new char[] { ' ' });
                for (int y = 0; y < mot.Length; ++y)
                {
                    if (longueur + mot[y].Length+1 > maxcaractere)
                    {
                        mot[y - 1] += "\n";
                        longueur = 0;
                    }
                    else
                        longueur += mot[y].Length+1;
                         
                }
                for (int y = 0; y < mot.Length; ++y)
                {
                    if (y!=0 && mot[y-1].IndexOf('\n') != -1)
                    {
                        temp += mot[y];
                    }
                    else if (y == 0)
                        temp += mot[y];
                    else
                        temp += " " + mot[y];
                }
            }
            else 
            {
                temp += tab[i] + "\n";
            }
        }
            return temp;
    }
    void OnDestroy()
    {
        if (t != null)
            t.Abort();
    }
    public void instantiateCardAllies()
    {
        Transform cards;
        for (int i = 0; i < styleCarteAllier.Length; ++i)
        {
            cards = null;
            cards = Instantiate(PlacementCarte, new Vector3(0, 0, -5), Quaternion.Euler(new Vector3(0, 0, 0))) as Transform;

            foreach (Transform child in cards)
            {
                child.name = child.name + i;
                child.tag = "textStats";
            }
            styleCarteAllier[i] = cards.gameObject;
            styleCarteAllier[i].name = "card" + i;
            setValue(i, cards, true);
            tabCarteAllier.CarteDeck[i] = setHabilete(tabCarteAllier.CarteDeck[i]);
        }
    }
    public void instantiateCardEnnemis()
    {
        Transform cards;
        for (int i = 0; i < styleCarteEnnemis.Length; ++i)
        {
            cards = null;
            cards = Instantiate(PlacementCarte, new Vector3(0, 0, -10), Quaternion.Euler(new Vector3(0, 0, 0))) as Transform;
            foreach (Transform child in cards)
            {
                child.name = child.name + "Ennemis" + i;
                child.tag = "textStats";
            }
            styleCarteEnnemis[i] = cards.gameObject;
            styleCarteEnnemis[i].name = "cardennemis" + i;
        }
    }
    public void initOrdrePige(int[] tab)
    {
        List<int> tabNombre = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39 };
        for (int i = 0; i < tab.Length; ++i)
        {
            /*besoin du unity engine car il ne sest pas quel prendre entre celle de unityengine et celle de sysytem c#*/
                int temp = UnityEngine.Random.Range(0, tab.Length - (1 + i));
                tab[i] = tabNombre[temp];
                tabNombre.RemoveAt(temp);
        }
    }
    public void InitZoneJoueur()
    {
        ZoneCarteJoueur = new PosZoneCombat[7];
        float pos = 0;
        for (int i = 0; i < ZoneCarteJoueur.Length; ++i)
        {
            ZoneCarteJoueur[i] = new PosZoneCombat();
            ZoneCarteJoueur[i].Pos = new Vector3(-5.0f + pos, -3.9f, 6.0f);
            pos += 1.4f;
        }
    }
    public void InitZoneEnnemie()
    {
        ZoneCarteEnnemie = new PosZoneCombat[7];
        float pos = 0;
        for (int i = 0; i < ZoneCarteEnnemie.Length; ++i)
        {
            ZoneCarteEnnemie[i] = new PosZoneCombat();
            ZoneCarteEnnemie[i].Pos = new Vector3(-4.0f + pos, 3.95f, 6.0f);
            pos += 1.4f;
        }
    }

    public void InitZoneCombatEnnemie()
    {
        ZoneCombatEnnemie = new PosZoneCombat[7];
        float pos = 0;
        for (int i = 0; i < ZoneCombatEnnemie.Length; ++i)
        {
            ZoneCombatEnnemie[i] = new PosZoneCombat();
            ZoneCombatEnnemie[i].Pos = new Vector3(-4.0f + pos, 1.5f, 6.0f);
            pos += 1.4f;
        }
    }

    public void InitZoneCombatJoueur()
    {
        ZoneCombat = new PosZoneCombat[7];
        float pos = 0;
        for (int i = 0; i < ZoneCombat.Length; ++i)
        {
            ZoneCombat[i] = new PosZoneCombat();
            ZoneCombat[i].Pos = new Vector3(-4.0f + pos, -1.5f, 6.0f);
            pos += 1.4f;
        }
    }

    public void CarteDepart()
    {
        int pos = 0;
        float posi = 0;
        while (NoCarte < 4)
        {
            Transform Ennemis = Instantiate(carteBack, ZoneCarteEnnemie[pos].Pos, Quaternion.Euler(new Vector3(0, 0, 0))) as Transform;
            ZoneCarteJoueur[pos].EstOccupee = true;
            ZoneCarteEnnemie[pos].EstOccupee = true;

            cardennemis = Ennemis.gameObject;

            ScriptEnnemie = Ennemis.GetComponent<JouerCarteBoard>();
            ScriptEnnemie.EstEnnemie = true;
            cardennemis.name = "cardbackennemis" + NoCarte.ToString();

            ZoneCarteJoueur[NoCarte].carte = tabCarteAllier.CarteDeck[ordrePige[NoCarte]];
            styleCarteAllier[ordrePige[NoCarte]].gameObject.transform.position = ZoneCarteJoueur[NoCarte].Pos;

             styleCarteEnnemis[NoCarte] = cardennemis;
             ZoneCarteEnnemie[pos].carte = tabCarteEnnemis[pos];
             ++pos;
             --nbCarteAllier;
             --nbCarteEnnemis;
             posi += 1.5f;
             ++NoCarte;
        }
        noCarteEnnemis = NoCarte;
    }
    private void setValueFromCard(int i, Transform t, Carte card, bool allier)
    {
        if (allier)
        {
            t.Find("coutBois" + i).GetComponent<TextMesh>().text = card.CoutBois.ToString();
            t.Find("coutBle" + i).GetComponent<TextMesh>().text = card.CoutBle.ToString();
            t.Find("coutGem" + i).GetComponent<TextMesh>().text = card.CoutGem.ToString();
            t.Find("habilete" + i).GetComponent<TextMesh>().text = formaterHabilete(card.Habilete);
            t.Find("Nom" + i).GetComponent<TextMesh>().text = card.NomCarte;
            t.Find("Image" + i).GetComponent<SpriteRenderer>().sprite = Resources.Load(card.NomCarte, typeof(Sprite)) as Sprite;
            if (card.perm != null)
            {
                t.Find("attaque" + i).GetComponent<TextMesh>().text = card.perm.Attaque.ToString();
                t.Find("armure" + i).GetComponent<TextMesh>().text = card.perm.Armure.ToString();
                t.Find("vie" + i).GetComponent<TextMesh>().text = card.perm.Vie.ToString();
                t.Find("type" + i).GetComponent<TextMesh>().text = card.perm.TypePerm;
            }
            else if (card.TypeCarte == "Sort")
                t.Find("type" + i).GetComponent<TextMesh>().text = card.TypeCarte;
        }
        else
        {
            t.Find("coutBoisEnnemis" + i).GetComponent<TextMesh>().text = card.CoutBois.ToString();
            t.Find("coutBleEnnemis" + i).GetComponent<TextMesh>().text = card.CoutBle.ToString();
            t.Find("coutGemEnnemis" + i).GetComponent<TextMesh>().text = card.CoutGem.ToString();
            t.Find("habileteEnnemis" + i).GetComponent<TextMesh>().text =formaterHabilete(card.Habilete);
            t.Find("NomEnnemis" + i).GetComponent<TextMesh>().text = card.NomCarte;
            t.Find("ImageEnnemis" + i).GetComponent<SpriteRenderer>().sprite = Resources.Load(card.NomCarte, typeof(Sprite)) as Sprite;
            if (card.perm != null)
            {
                t.Find("attaqueEnnemis" + i).GetComponent<TextMesh>().text = card.perm.Attaque.ToString();
                t.Find("armureEnnemis" + i).GetComponent<TextMesh>().text = card.perm.Armure.ToString();
                t.Find("vieEnnemis" + i).GetComponent<TextMesh>().text = card.perm.Vie.ToString();
                t.Find("typeEnnemis" + i).GetComponent<TextMesh>().text = card.perm.TypePerm;
            }
            else if (card.TypeCarte == "Sort")
                t.Find("typeEnnemis" + i).GetComponent<TextMesh>().text = card.TypeCarte;
        }
    }

    private void changestatFromCard(GameObject t, Carte card)
    {
        if (card != null && card.perm != null && t != null)
        {
            TextMesh[] stat = t.GetComponentsInChildren<TextMesh>();
            stat[3].text = card.perm.Armure.ToString();
            stat[4].text = card.perm.Attaque.ToString();
            stat[5].text = card.perm.Vie.ToString();
        }
    }
    private void setValue(int i,Transform t,bool allier)
    {
        if (allier)
        {
            t.Find("coutBois" + i).GetComponent<TextMesh>().text = tabCarteAllier.CarteDeck[i].CoutBois.ToString();
            t.Find("coutBle" + i).GetComponent<TextMesh>().text = tabCarteAllier.CarteDeck[i].CoutBle.ToString();
            t.Find("coutGem" + i).GetComponent<TextMesh>().text = tabCarteAllier.CarteDeck[i].CoutGem.ToString();
            t.Find("habilete" + i).GetComponent<TextMesh>().text = formaterHabilete(tabCarteAllier.CarteDeck[i].Habilete);
            t.Find("Nom" + i).GetComponent<TextMesh>().text = tabCarteAllier.CarteDeck[i].NomCarte;
            t.Find("Image" + i).GetComponent<SpriteRenderer>().sprite = Resources.Load(tabCarteAllier.CarteDeck[i].NomCarte, typeof(Sprite)) as Sprite;

            if (tabCarteAllier.CarteDeck[i].perm != null)
            {
                t.Find("attaque" + i).GetComponent<TextMesh>().text = tabCarteAllier.CarteDeck[i].perm.Attaque.ToString();
                t.Find("armure" + i).GetComponent<TextMesh>().text = tabCarteAllier.CarteDeck[i].perm.Armure.ToString();
                t.Find("vie" + i).GetComponent<TextMesh>().text = tabCarteAllier.CarteDeck[i].perm.Vie.ToString();
                t.Find("type" + i).GetComponent<TextMesh>().text = tabCarteAllier.CarteDeck[i].perm.TypePerm;
            }
            else if (tabCarteAllier.CarteDeck[i].TypeCarte == "Sort")
                t.Find("type" + i).GetComponent<TextMesh>().text = tabCarteAllier.CarteDeck[i].TypeCarte;
        }
        else
        {
            t.Find("coutBoisEnnemis" + i).GetComponent<TextMesh>().text = tabCarteEnnemis[i].CoutBois.ToString();
            t.Find("coutBleEnnemis" + i).GetComponent<TextMesh>().text = tabCarteEnnemis[i].CoutBle.ToString();
            t.Find("coutGemEnnemis" + i).GetComponent<TextMesh>().text = tabCarteEnnemis[i].CoutGem.ToString();
            t.Find("habileteEnnemis" + i).GetComponent<TextMesh>().text = formaterHabilete(tabCarteEnnemis[i].Habilete);
            t.Find("NomEnnemis" + i).GetComponent<TextMesh>().text = tabCarteEnnemis[i].NomCarte;
            t.Find("ImageEnnemis" + i).GetComponent<SpriteRenderer>().sprite = Resources.Load(tabCarteEnnemis[i].NomCarte, typeof(Sprite)) as Sprite;
            if (tabCarteAllier.CarteDeck[i].perm != null)
            {
                t.Find("attaqueEnnemis" + i).GetComponent<TextMesh>().text = tabCarteEnnemis[i].perm.Attaque.ToString();
                t.Find("armureEnnemis" + i).GetComponent<TextMesh>().text = tabCarteEnnemis[i].perm.Armure.ToString();
                t.Find("vieEnnemis" + i).GetComponent<TextMesh>().text = tabCarteEnnemis[i].perm.Vie.ToString();
                t.Find("typeEnnemis" + i).GetComponent<TextMesh>().text = tabCarteEnnemis[i].perm.TypePerm;
            }
            else if (tabCarteEnnemis[i].TypeCarte == "Sort")
                t.Find("typeEnnemis" + i).GetComponent<TextMesh>().text = tabCarteEnnemis[i].TypeCarte;
        }
    }
    // Update is called once per frame
    void Update()
    {
    }
    //affichage (refresh per frame)
    public void OnGUI()
    {
        Event e;
        e = Event.current;

	//Héro Joueur
	GUI.Label(new Rect(Screen.width*0.045f,Screen.height*0.76f,Screen.width*1.0f, Screen.height*1.0f),"Vie: " + HpJoueur.ToString());
        GUI.Label(new Rect(Screen.width * 0.045f, Screen.height * 0.73f, Screen.width * 1.0f, Screen.height * 1.0f), "Nombre de carte: " + nbCarteAllier.ToString());
	//Héro Ennemi
        GUI.Label(new Rect(Screen.width * 0.90f, Screen.height * 0.001f, Screen.width * 1.0f, Screen.height * 1.0f), "Vie: " + HpEnnemi.ToString());
        GUI.Label(new Rect(Screen.width * 0.90f, Screen.height * 0.03f, Screen.width * 1.0f, Screen.height * 1.0f), "Nombre de carte: " + nbCarteEnnemis.ToString());

        if(gameFini)
        {
            GUIBox.fontSize = Screen.width / 25;
            GUIButton.fontSize = Screen.width / 35;
            if (EstGagnant)
            {
                GUI.Box(new Rect(Screen.width * 0.35f, Screen.height * 0.35f, Screen.width * 0.30f, Screen.height * 0.30f), "\n  Vous avez gagné!", GUIBox);
                if (GUI.Button(new Rect((Screen.width * 0.45f), Screen.height * 0.55f, Screen.width * 0.135f, Screen.height * 0.07f), "   Menu", GUIButton))
                {
                    Application.LoadLevel("Menu");
                }
            }
            else if(EstPerdant)
            {
                GUI.Box(new Rect(Screen.width * 0.35f, Screen.height * 0.35f, Screen.width * 0.30f, Screen.height * 0.30f), "\n  Vous avez perdu!", GUIBox);
                if (GUI.Button(new Rect((Screen.width * 0.45f), Screen.height * 0.55f, Screen.width * 0.135f, Screen.height * 0.07f), "   Menu", GUIButton))
                {
                    Application.LoadLevel("Menu");
                }
            }
        }

        //BTN EndTurn
        if (placerClick && MonTour)
        {
            if (GUI.Button(new Rect(Screen.width * 0.067f, Screen.height * 0.47f, Screen.width * 0.07f, Screen.height * 0.05f), "Fini"))
            {
                envoyerMessage("Fin De Tour");
                MonTour = false;
                resetArmor(ZoneCombat,styleCarteAlliercombat,true);
                resetArmor(ZoneCombatEnnemie,styleCarteEnnemisCombat,false);
            }
        }
        else
        {
            GUI.enabled = false;
            GUI.Button(new Rect(Screen.width * 0.067f, Screen.height * 0.47f, Screen.width * 0.07f, Screen.height * 0.05f), "Fini");
            GUI.enabled = true;
        }
        //blé
        GUI.Label(new Rect(Screen.width * 0.005f, Screen.height * 0.005f, Screen.width * 0.05f, Screen.height * 0.07f), ble);
        GUI.Label(new Rect(Screen.width * 0.005f, Screen.height * 0.07f, Screen.width * 0.09f, Screen.height * 0.07f), "Blé: " + NbBleEnnemis.ToString());
        //bois
        GUI.Label(new Rect(Screen.width * 0.06f, Screen.height * 0.005f, Screen.width * 0.05f, Screen.height * 0.07f), bois);
        GUI.Label(new Rect(Screen.width * 0.06f, Screen.height * 0.07f, Screen.width * 0.09f, Screen.height * 0.07f), "Bois: " + NbBoisEnnemis.ToString());
        //gem
        GUI.Label(new Rect(Screen.width * 0.14f, Screen.height * 0.005f, Screen.width * 0.05f, Screen.height * 0.07f), gem);
        GUI.Label(new Rect(Screen.width * 0.14f, Screen.height * 0.07f, Screen.width * 0.09f, Screen.height * 0.07f), "Gem: " + NbGemEnnemis.ToString());
        if (!placerClick && MonTour)
        {
            //BLE
            if (GUI.Button(new Rect(Screen.width / 1.27f, Screen.height / 1.12f, Screen.width * 0.05f, Screen.height * 0.07f), ble))
            {
                NbBle = SetManaAjouter(e, NbBle, "ble");
            }
            GUI.Label(new Rect(Screen.width / 1.27f, Screen.height / 1.04f, Screen.width * 0.09f, Screen.height * 0.07f), "Blé: " + NbBle.ToString());
            //BOIS
            if (GUI.Button(new Rect((Screen.width / 1.17f), Screen.height / 1.12f, Screen.width * 0.05f, Screen.height * 0.07f), bois))
            {
                NbBois = SetManaAjouter(e, NbBois, "bois");
            }
            GUI.Label(new Rect(Screen.width / 1.17f, Screen.height / 1.04f, Screen.width * 0.09f, Screen.height * 0.07f), "Bois: " + NbBois.ToString());
            //GEM
            if (GUI.Button(new Rect((Screen.width / 1.08f), Screen.height / 1.12f, Screen.width * 0.05f, Screen.height * 0.07f), gem))
            {
                NbGem = SetManaAjouter(e, NbGem, "gem");
            }
            GUI.Label(new Rect(Screen.width / 1.08f, Screen.height / 1.04f, Screen.width * 0.09f, Screen.height * 0.07f), "Gem: " + NbGem.ToString());
            //WORKER
            GUI.Label(new Rect(Screen.width / 1.3f, Screen.height / 1.26f, Screen.width * 0.25f, Screen.height * 0.10f), Worker);
            GUI.Label(new Rect(Screen.width / 1.23f, Screen.height / 1.17f, Screen.width * 0.15f, Screen.height * 0.1f), "Worker: " + NbWorker.ToString());

            if (GUI.Button(new Rect(Screen.width / 1.12f, Screen.height / 1.26f, Screen.width * 0.1f, Screen.height * 0.1f), "Placer") && NbWorker == 0)
            {
                placerClick = true;
                joueur1.nbBle = NbBle;
                joueur1.nbBois = NbBois;
                joueur1.nbGem = NbGem;
                envoyerMessage("Ajouter Mana." + NbBle + "." + NbBois + "." + NbGem);
            }
        }
        else
        {
            GUI.enabled = false;
            GUI.Button(new Rect(Screen.width / 1.27f, Screen.height / 1.12f, Screen.width * 0.05f, Screen.height * 0.07f), ble);
            GUI.Button(new Rect((Screen.width / 1.17f), Screen.height / 1.12f, Screen.width * 0.05f, Screen.height * 0.07f), bois);
            GUI.Button(new Rect((Screen.width / 1.08f), Screen.height / 1.12f, Screen.width * 0.05f, Screen.height * 0.07f), gem);

            GUI.enabled = true;
            GUI.Label(new Rect(Screen.width / 1.27f, Screen.height / 1.04f, Screen.width * 0.09f, Screen.height * 0.07f), "Blé: " + NbBle.ToString());
            GUI.Label(new Rect(Screen.width / 1.17f, Screen.height / 1.04f, Screen.width * 0.09f, Screen.height * 0.07f), "Bois: " + NbBois.ToString());
            GUI.Label(new Rect(Screen.width / 1.08f, Screen.height / 1.04f, Screen.width * 0.09f, Screen.height * 0.07f), "Gem: " + NbGem.ToString());
            GUI.Label(new Rect(Screen.width / 1.3f, Screen.height / 1.26f, Screen.width * 0.25f, Screen.height * 0.10f), Worker);
            GUI.Label(new Rect(Screen.width / 1.23f, Screen.height / 1.17f, Screen.width * 0.15f, Screen.height * 0.1f), "Worker: " + NbWorker.ToString());
        }
        if (!MonTour)
        {
            if (ReceiveMessage.message == "vous avez gagné" || ReceiveMessage.message == "vous avez perdu")
                gameFini = true;
            if (!gameFini && ReceiveMessage.message == "")
            {
                if (t == null || !t.IsAlive)
                {
                    t = new Thread(ReceiveMessage.doWork);
                    t.Start();
                }
            }
            attaque s = GetComponent<attaque>();
            s.enabled = false;
            if (ReceiveMessage.message != "")
            traiterMessagePartie(ReceiveMessage.message.Split(new char[] { '.' }));
        }
    }
    private void setSpecialHability(string[] data)
    {
        if (data[0] == "Ajoute")
        {
            setManaBonus(data);
        }
    }
    private void setManaBonus(string[] data)
    {
        int num = getNumBonus(data[1]);
        string sorteMana = data[2];
        if (sorteMana == "bois")
            NbBoisEnnemis += num;
        else if (sorteMana == "gem")
            NbGemEnnemis += num;
        else if (sorteMana == "ble")
            NbBleEnnemis += num;
    }
    private int getNumBonus(string laplace)
    {
        char[] tab = { ' ', '+' };
        string nombre = laplace.Trim(tab);
        return int.Parse(nombre);
    }
    private void traiterMessagePartie(string[] data)
    {
        switch (data[0])
        {
            case "JePart":
                HpEnnemi = 0;
                gameFini = true;
                EstGagnant = true;
                ReceiveMessage.message = "";
            break;
            case "AjouterManaEnnemis":
                setManaEnnemis(int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]));
                ReceiveMessage.message = "";
                break;
            case "Tour Commencer":
                MonTour = true;
                placerClick = false;
                resetArmor(ZoneCombat, styleCarteAlliercombat, true);
                resetArmor(ZoneCombatEnnemie, styleCarteEnnemisCombat, false);
                attaque s = GetComponent<attaque>();
                s.enabled = true;
                //max mana =5
                if (NbWorkerMax < 5)
                    setWorker(true);
                else
                    setWorker(false);
                setpeutAttaquer();
                PigerCarte();

                ReceiveMessage.message = "";
                break;
            case "AjouterCarteEnnemis":
                Carte temp = createCarte(data, 2);
                temp = setHabilete(temp);
                setManaEnnemis(NbBleEnnemis - int.Parse(data[2]), NbBoisEnnemis - int.Parse(data[3]), NbGemEnnemis - int.Parse(data[4]));
                if (temp.perm.specialhability)
                {
                    setSpecialHability(temp.perm.habilityspecial.Split(new char[] { ' ' }));
                }

                /*trouver le back de carte pour prendre son nom e tla detruire pour contruire un prefab de devant de carte avec les stats de la carte*/
                GameObject temps = trouverBackCard();
                int place = TrouverEmplacementCarteJoueur(temps.transform.position, ZoneCarteEnnemie);
                ZoneCarteEnnemie[place].EstOccupee = false;
                Destroy(temps);

                //crée lobjet carte (celle de face)
                string nomtemp = data[1].Insert(data[1].IndexOf("d") + 1, "ennemis");
                GameObject zeCarteEnnemis = GameObject.Find(nomtemp);
                int index = zeCarteEnnemis.name.IndexOf("s");
                string nombre = zeCarteEnnemis.name.Substring(index + 1, zeCarteEnnemis.name.Length - (index + 1));

                int posCombat = placerCarte(zeCarteEnnemis, ZoneCombatEnnemie);
                tabCarteEnnemis[int.Parse(nombre)] = temp;
                setValueFromCard(int.Parse(nombre), zeCarteEnnemis.transform, temp, false);

                ZoneCombatEnnemie[posCombat].carte = temp;
                styleCarteEnnemisCombat[posCombat] = zeCarteEnnemis;
                JouerCarteBoard a = zeCarteEnnemis.GetComponent<JouerCarteBoard>();
                a.EstJouer = true;
                a.EstEnnemie = true;
                ReceiveMessage.message = "";
                break;
            case "Joueur attaquer":
                HpJoueur = int.Parse(data[1]);
                ReceiveMessage.message = "";
                audio.PlayOneShot(AttackSound);
                if (HpJoueur <= 0)
                {
                    gameFini = true;
                    EstPerdant = true;
                    ReceiveMessage.message = "";                   
                }
                break;
            case "Combat Creature":
                Carte attaque = createCarte(data, 5);
                Carte defenseur = createCarte(data, 16);
                int num = data[3].IndexOf("d");
                data[3] = data[3].Insert(num + 1, "ennemis");
                data[4] = data[4].Replace("ennemis", "");
                combat(attaque, defenseur, int.Parse(data[2]), int.Parse(data[1]), data[3], data[4]);
                audio.PlayOneShot(AttackSound);
                ReceiveMessage.message = "";
                break;
            case "Ennemis pige":
                Transform t = Instantiate(carteBack, new Vector3(0, 0, -100), Quaternion.Euler(new Vector3(0, 0, 0))) as Transform;
                GameObject zeCartePiger = t.gameObject;
                zeCartePiger.name = "cardbackennemis" + noCarteEnnemis;
                placerCarte(zeCartePiger, ZoneCarteEnnemie);
                JouerCarteBoard pigerScript = zeCartePiger.GetComponent<JouerCarteBoard>();
                pigerScript.EstEnnemie = true;
                ++noCarteEnnemis;
                --nbCarteEnnemis;
                ReceiveMessage.message = "";
                break;
            case "spellwithtarget":
                Carte target = null;
                Carte spell = createCarte(data, 3);
                if (data[2] != "hero ennemis")
                {
                   target = createCarte(data, 10);
                }
                setManaEnnemis(NbBleEnnemis - spell.CoutBle, NbBoisEnnemis - spell.CoutBois, NbGemEnnemis - spell.CoutGem);
                int num3 = data[1].IndexOf("d");
                data[1] = data[1].Insert(num3 + 1, "ennemis");
                if (spell.Habilete.Split(new char[] { ' ' })[0] != "Donne")
                {
                    data[2] = data[2].Replace("ennemis", "");
                }
                else
                {
                    int num2 = data[1].IndexOf("d");
                    data[2] = data[2].Insert(num2 + 1, "ennemis");
                }
                GameObject gamespell = GameObject.Find(data[1]);
                GameObject gametarget = GameObject.Find(data[2]);
                gamespell.transform.position = new Vector3(-5.4f, 0.0f, 6.0f);
                Destroy(gamespell, 3);
                TextMesh[] stat = gametarget.GetComponentsInChildren<TextMesh>();
                if (spell.Habilete.Split(new char[] { ' ' })[0] == "Detruit")
                {
                    ZoneCombat[TrouverEmplacementCarteJoueur(gametarget.transform.position, ZoneCombat)].EstOccupee = false;
                    Destroy(gametarget, 1);
                }
                else if (spell.Habilete.Split(new char[] { ' ' })[0] == "Inflige")
                {
                    if (target != null && target.perm != null)
                    {
                        if (target != null && target.perm != null && target.perm.Vie <= 0)
                        {
                            ZoneCombat[TrouverEmplacementCarteJoueur(gametarget.transform.position, ZoneCombat)].EstOccupee = false;
                            Destroy(gametarget, 1);
                        }
                        else
                        {

                            stat[3].text = target.perm.Armure.ToString();
                            stat[5].text = target.perm.Vie.ToString();
                        }
                    }
                    else
                    {
                        if (data[2] == "hero")
                            HpEnnemi -= int.Parse(spell.Habilete.Split(new char[] { ' ' })[1]);
                        else
                            HpJoueur -= int.Parse(spell.Habilete.Split(new char[] { ' ' })[1]);
                            
                    }
                    
                }
                else if (spell.Habilete.Split(new char[] { ' ' })[0] == "Endort")
                {
                    //afficher un ZZzzzz en haut de la carte
                }
                else if (spell.Habilete.Split(new char[] { ' ' })[0] == "Transforme")
                {
                    stat[3].text = target.perm.Armure.ToString();
                    stat[4].text = target.perm.Attaque.ToString();
                    stat[5].text = target.perm.Vie.ToString();

                }
                else if (spell.Habilete.Split(new char[] { ' ' })[0] == "Donne")
                {
                    stat[3].text = target.perm.Armure.ToString();
                    stat[4].text = target.perm.Attaque.ToString();
                    stat[5].text = target.perm.Vie.ToString();
                    //afficher un !!!!!!!!  de taunt if there's taunt!
                }

                ReceiveMessage.message = "";
                break;
            case "spellNoTarget":
                Carte spell2 = createCarte(data, 2);
                bool valide = false;
                string typeTarget = trouverTypeTarget(spell2.Habilete.Split(new char[] { ' ' }));
                setManaEnnemis(NbBleEnnemis - spell2.CoutBle, NbBoisEnnemis - spell2.CoutBois, NbGemEnnemis - spell2.CoutGem);
                num = data[1].IndexOf("d");
                data[1] = data[1].Insert(num + 1, "ennemis");
                GameObject gamespell2 = GameObject.Find(data[1]);
                gamespell2.transform.position = new Vector3(-5.4f, 0.0f, 6.0f);
                Destroy(gamespell2, 3);
                if (spell2.Habilete.Split(new char[] { ' ' })[0] == "Inflige")
                {
                    //styleCarteEnnemis[i] == gameObject  || ZoneCombatEnnemie[i].carte == carte
                    int dmg = int.Parse(spell2.Habilete.Split(new char[] { ' ' })[1]);
                    for (int i = 0; i < ZoneCombat.Length; i++)
                    {
                        valide = false;
                        if (typeTarget == "touteslescartes" || typeTarget == "touslesbatiments" || typeTarget == "placedecombat" || typeTarget == "ennemis")
                        {
                            if (styleCarteEnnemisCombat[i] != null)
                            {
                                valide = checkIfValide(typeTarget, i, spell2.Habilete.Split(new char[] { ' ' })[0], ZoneCombatEnnemie);
                                if (valide)
                                    doTheDmg(dmg, i, ZoneCombatEnnemie);
                                
                            }
                        }
                        if (styleCarteAlliercombat[i] != null)
                        {
                            valide = checkIfValide(typeTarget, i, spell2.Habilete.Split(new char[] { ' ' })[0], ZoneCombat);
                            if (valide)
                                doTheDmg(dmg, i, ZoneCombat);
                        }
                    }
                    if (typeTarget == "placedecombat" || typeTarget == "ennemis")
                        if (typeTarget == "placedecombat")
                        {
                            HpEnnemi -= dmg;
                            HpJoueur -= dmg;
                        }
                        else
                            HpEnnemi -= dmg;
                }
                else if (spell2.Habilete.Split(new char[] { ' ' })[0] == "Detruit")
                {
                    for (int i = 0; i < ZoneCombat.Length; i++)
                    {
                        valide = false;
                        if (typeTarget == "touteslescartes" || typeTarget == "touslesbatiments" || typeTarget == "touteslescreatures")
                        {
                            if (styleCarteEnnemisCombat[i] != null)
                            {
                                valide = checkIfValide(typeTarget, i, spell2.Habilete.Split(new char[] { ' ' })[0], ZoneCombatEnnemie);
                                if (valide)
                                    doDestruction(styleCarteEnnemisCombat[i], i, ZoneCombatEnnemie);
                            }
                        }
                        if (styleCarteAlliercombat[i] != null)
                        {
                            valide = checkIfValide(typeTarget, i, spell2.Habilete.Split(new char[] { ' ' })[0], ZoneCombat);
                            if (valide)
                                doDestruction(styleCarteAlliercombat[i], i, ZoneCombat);
                        }
                    }
                }
                else if (spell2.Habilete.Split(new char[] { ' ' })[0] == "Transforme")
                {
                    string[] tabHabileteSpell = spell2.Habilete.Split(new char[] { ' ' });
                    string[] statsTransforme = tabHabileteSpell[tabHabileteSpell.Length - 1].Split(new char[] { '/' });
                    for (int i = 0; i < ZoneCombat.Length; i++)
                    {
                        valide = false;
                        if (typeTarget == "touteslescartes" || typeTarget == "touslesbatiments" || typeTarget == "touteslescreatures")
                        {
                            if (styleCarteEnnemisCombat[i] != null)
                            {
                                valide = checkIfValide(typeTarget, i, spell2.Habilete.Split(new char[] { ' ' })[0], ZoneCombatEnnemie);
                                if (valide)
                                    doTransformation(styleCarteEnnemisCombat[i], i, ZoneCombatEnnemie, statsTransforme);
                            }
                        }
                        if (styleCarteAlliercombat[i] != null)
                        {
                            valide = checkIfValide(typeTarget, i, spell2.Habilete.Split(new char[] { ' ' })[0], ZoneCombat);
                            if (valide)
                                doTransformation(styleCarteAlliercombat[i], i, ZoneCombat, statsTransforme);
                        }
                    }
                }
                else if (spell2.Habilete.Split(new char[] { ' ' })[0] == "Endort")
                {
                    //afficher icon ZZzz
                    int nbTours = int.Parse(spell2.Habilete.Split(new char[] { ' ' })[spell2.Habilete.Split(new char[] { ' ' }).Length - 1]);
                    for (int i = 0; i < ZoneCombat.Length; i++)
                    {
                        valide = false;
                        if (typeTarget == "touteslescreatures")
                        {
                            if (styleCarteEnnemisCombat[i] != null)
                            {
                                valide = checkIfValide(typeTarget, i, spell2.Habilete.Split(new char[] { ' ' })[0], ZoneCombatEnnemie);
                                if (valide)
                                    doSleep(i, ZoneCombatEnnemie, nbTours);
                            }
                        }
                        if (styleCarteAlliercombat[i] != null)
                        {
                            valide = checkIfValide(typeTarget, i, spell2.Habilete.Split(new char[] { ' ' })[0], ZoneCombat);
                            if (valide)
                                doSleep(i, ZoneCombat, nbTours);
                        }
                    }
                }
                else if (spell2.Habilete.Split(new char[] { ' ' })[0] == "Donne")
                {
                    string[] tabHabileteSpell = spell2.Habilete.Split(new char[] { ' ' });
                    for(int i = 0; i < ZoneCombatEnnemie.Length; i++)
                    {
                        valide = false;
                        if(typeTarget == "touteslescreatures")
                        {
                            if (styleCarteAlliercombat[i] != null)
                            {
                                valide = checkIfValide(typeTarget, i, spell2.Habilete.Split(new char[] { ' ' })[0], ZoneCombat);
                                if (valide)
                                    doBuffShitUp(i, ZoneCombat, tabHabileteSpell);
                            }
                        }
                        if (styleCarteEnnemisCombat[i] != null)
                        {
                            valide = checkIfValide(typeTarget, i, spell2.Habilete.Split(new char[] { ' ' })[0], ZoneCombatEnnemie);
                            if (valide)
                                doBuffShitUp(i, ZoneCombatEnnemie, tabHabileteSpell);
                        }
                    }
                }
                else if (spell2.Habilete.Split(new char[] { ' ' })[0] == "Soigne")
                {
                    int nbHeal = int.Parse(spell2.Habilete.Split(new char[] {' '})[1]);
                    for(int i = 0; i < ZoneCombatEnnemie.Length; i++)
                    {
                        valide = false;
                        if(typeTarget == "touteslescreatures" || typeTarget == "touslesbatiments" || typeTarget == "placedecombat" || typeTarget == "touteslescartes")
                        {
                            if (styleCarteAlliercombat[i] != null)
                            {
                                valide = checkIfValide(typeTarget, i, spell2.Habilete.Split(new char[] { ' ' })[0], ZoneCombat);
                                if (valide)
                                    doHealingStuff(i, nbHeal, ZoneCombatEnnemie);
                            }
                        }
                        if (styleCarteEnnemisCombat[i] != null)
                        {
                            valide = checkIfValide(typeTarget, i, spell2.Habilete.Split(new char[] { ' ' })[0], ZoneCombatEnnemie);
                            if (valide)
                                doHealingStuff(i, nbHeal, ZoneCombatEnnemie);
                        }
                    }
                }

                ReceiveMessage.message = "";
                break;

            case "Carte manquante":
                HpEnnemi = 0;
                gameFini = true;
                break;
        }
            if (data[0] == "Carte manquante")
            {
                EstGagnant = true;
            }
    }

    private void checkIfDmgHeroes(string typeTarget, int dmg)
    {
        if (typeTarget == "placedecombat" || typeTarget == "ennemis")
        {
            if (typeTarget == "placedecombat")
            {
                HpEnnemi -= dmg;
                HpJoueur -= dmg;
            }
            else
                HpEnnemi -= dmg;
        }
    }
    private void doTheDmg(int dmg, int i, PosZoneCombat[] zone)
    {
        if ((zone[i].carte.perm.Armure - dmg) >= 0)
            zone[i].carte.perm.Armure -= dmg;
        else
        {
            int difference = dmg - zone[i].carte.perm.Armure;
            zone[i].carte.perm.Armure = 0;
            zone[i].carte.perm.Vie -= difference;
            if (zone[i].carte.perm.Vie <= 0)
            {
                Destroy(styleCarteAllier[i], 1);
                zone[i].EstOccupee = false;
                zone[i].carte = null;
            }
        }

        if (styleCarteEnnemis[i] != null && ZoneCombat[i].carte != null)
        {
            TextMesh[] zeStat = styleCarteAllier[i].GetComponentsInChildren<TextMesh>();
            zeStat[3].text = ZoneCombat[i].carte.perm.Armure.ToString();
            zeStat[5].text = ZoneCombat[i].carte.perm.Vie.ToString();
        }
    }
    private void doSleep(int i,PosZoneCombat[] zone, int nbTours)
    {
        zone[i].carte.perm.estEndormi = nbTours;
    }
    private void doBuffShitUp(int pos, PosZoneCombat[] zone, string[] tabHabileteSpell)
    {
        string buffHabilete = ""; int buffVie = 0; int buffArmure = 0; int buffAttaque = 0;
        for (int i = 1; i < tabHabileteSpell.Length; i++)
        {
            if (tabHabileteSpell[i] == "et")
            {
                if (tabHabileteSpell[i + 2] == "double" || tabHabileteSpell[i + 2] == "puissante")
                    buffHabilete = tabHabileteSpell[i + 1] + ' ' + tabHabileteSpell[i + 2];
                else
                    buffHabilete = tabHabileteSpell[i + 1];
            }
            else if (tabHabileteSpell[i][0] == '+')
            {
                if (tabHabileteSpell[i + 1] == "pV")
                    buffVie = int.Parse(tabHabileteSpell[i].Split(new char[] { '+' })[1]);
                else if (tabHabileteSpell[i + 1] == "pAtt")
                    buffAttaque = int.Parse(tabHabileteSpell[i].Split(new char[] { '+' })[1]);
                else if (tabHabileteSpell[i + 1] == "pArm")
                    buffArmure = int.Parse(tabHabileteSpell[i].Split(new char[] { '+' })[1]);
            }
        }

        zone[pos].carte.perm.Vie += buffVie;
        zone[pos].carte.perm.basicVie += buffVie;
        zone[pos].carte.perm.Attaque += buffAttaque;
        zone[pos].carte.perm.basicAttaque += buffAttaque;
        zone[pos].carte.perm.Armure += buffArmure;
        zone[pos].carte.perm.basicArmor += buffArmure;

        TextMesh[] zeStat = styleCarteEnnemisCombat[pos].GetComponentsInChildren<TextMesh>();
        zeStat[3].text = zone[pos].carte.perm.Armure.ToString();
        zeStat[4].text = zone[pos].carte.perm.Attaque.ToString();
        zeStat[5].text = zone[pos].carte.perm.Vie.ToString();


        if (buffHabilete == "provocateur")
        {
            if (!zone[pos].carte.perm.estInvisible)
                zone[pos].carte.perm.estTaunt = true;
        }
        else if (buffHabilete == "attaque puissante")
        {
            zone[pos].carte.perm.estAttaquePuisante = true;
        }
        else if (buffHabilete == "attaque double")
        {
            zone[pos].carte.perm.estAttaqueDouble = true;
        }
    }
    private void doHealingStuff(int i, int nbHeal, PosZoneCombat[] zone)
    {
        if ((zone[i].carte.perm.Vie + nbHeal) >= zone[i].carte.perm.basicVie)
            zone[i].carte.perm.Vie = zone[i].carte.perm.basicVie;
        else
            zone[i].carte.perm.Vie += nbHeal;

            TextMesh[] stat = styleCarteAlliercombat[i].GetComponentsInChildren<TextMesh>();
            stat[5].text = zone[i].carte.perm.Vie.ToString();
    }
    private void doTransformation(GameObject t, int i, PosZoneCombat[] zone, string[] statsTransforme)
    {
        int vieTransformation = int.Parse(statsTransforme[0]); int attaqueTransformation = int.Parse(statsTransforme[1]); int armureTransformation = int.Parse(statsTransforme[2]);

        zone[i].carte.perm.Vie = vieTransformation;
        zone[i].carte.perm.Attaque = attaqueTransformation;
        zone[i].carte.perm.Armure = armureTransformation;

        TextMesh[] stat = t.GetComponentsInChildren<TextMesh>();
        stat[3].text = zone[i].carte.perm.Armure.ToString();
        stat[4].text = zone[i].carte.perm.Attaque.ToString();
        stat[5].text = zone[i].carte.perm.Vie.ToString();
    }
    private void doDestruction(GameObject t, int i, PosZoneCombat[] zone)
    {
        Destroy(t, 1);
        zone[i].EstOccupee = false;
        zone[i].carte = null;
    }
    private bool checkIfValide(string typeTarget, int i, string typeSpell, PosZoneCombat[] zone)
    {
        bool valide = false;

        if (typeSpell == "Inflige")
        {
            if (typeTarget == "cible" || typeTarget == "ennemis" || typeTarget == "touteslescartesennemis" || typeTarget == "touteslescartes" || typeTarget == "placedecombat")
                valide = true;
            else if (typeTarget == "creature" || typeTarget == "touteslescreatures" || typeTarget == "touteslescreaturesennemis")
                if (zone[i].carte.perm.TypePerm == "Creature")
                    valide = true;
                else if (typeTarget == "batiment" || typeTarget == "touslesbatiments" || typeTarget == "touslesbatimentsennemis")
                    if (zone[i].carte.perm.TypePerm == "Batiment")
                        valide = true;
        }
        else if (typeSpell == "Detruit" || typeSpell == "Transforme" || typeSpell == "Endort")
        {
            if (typeTarget == "cible" || typeTarget == "touteslescartesennemis" || typeTarget == "touteslescartes")
                valide = true;
            else if (typeTarget == "creature" || typeTarget == "touteslescreatures" || typeTarget == "touteslescreaturesennemis")
                if (zone[i].carte.perm.TypePerm == "Creature")
                    valide = true;
                else if (typeTarget == "batiment" || typeTarget == "touslesbatiments" || typeTarget == "touslesbatimentsennemis")
                    if (zone[i].carte.perm.TypePerm == "Batiment")
                        valide = true;
        }
        else if (typeSpell == "Soigne")
        {
            if (typeTarget == "cible" || typeTarget == "ennemis" || typeTarget == "touteslescartesennemis" || typeTarget == "touteslescartes" || typeTarget == "placedecombat")
                valide = true;
            else if (typeTarget == "creature" || typeTarget == "touteslescreatures" || typeTarget == "touteslescreaturesalliees")
            {
                if (zone[i].carte.perm.TypePerm == "Creature")
                    valide = true;
            }
            else
                if (typeTarget == "batiment" || typeTarget == "touslesbatiments" || typeTarget == "touslesbatimentsalliees")
                    valide = true;
        }
        else if (typeSpell == "Donne")
        {
            if (typeTarget == "cible" || typeTarget == "touteslescartesalliees" || typeTarget == "touteslescartes")
                valide = true;
            else if (typeTarget == "creature" || typeTarget == "touteslescreatures" || typeTarget == "touteslescreaturesalliees")
            {
                if (zone[i].carte.perm.TypePerm == "Creature")
                    valide = true;
            }
            else if (typeTarget == "batiment" || typeTarget == "touslesbatiments" || typeTarget == "touslesbatimentsalliees")
                if (zone[i].carte.perm.TypePerm == "Batiment")
                    valide = true;
        }
        return valide;

    }
    private GameObject trouverBackCard()
    {
        GameObject game = null;
        for (int i = 0; game == null && i < tabCarteEnnemis.Length; ++i)
        {
            game = GameObject.Find("cardbackennemis" + i);
        }
        return game;
    }
    private void setpeutAttaquer()
    {
        for (int i = 0; i < ZoneCombat.Length; ++i)
        {
            if (ZoneCombat[i].carte != null && ZoneCombat[i].carte.perm.TypePerm == "Creature")
            {
                ZoneCombat[i].carte.perm.aAttaque = false;
                ZoneCombat[i].carte.perm.aAttaquerDouble = false;
            }
        }
    }
    private string SetCarteString(Carte temp)
    {
        /*0                    1                     2                   3                      4                      5                    6                     7                            8                   9                         10*/
        return temp.CoutBle + "." + temp.CoutBois + "." + temp.CoutGem + "." + temp.Habilete + "." + temp.TypeCarte + "." + temp.NomCarte + "." + temp.NoCarte + "." + temp.perm.Attaque + "." + temp.perm.Vie + "." + temp.perm.Armure + "." + temp.perm.TypePerm;
    }
    private Carte createCarte(string[] data, int posDepart)
    {
        Carte zeCarte = null;
        zeCarte = new Carte(int.Parse(data[posDepart + 6]), data[posDepart + 5], data[posDepart + 4], data[posDepart + 3], int.Parse(data[posDepart]), int.Parse(data[posDepart + 1]), int.Parse(data[posDepart + 2]));
        if (zeCarte.TypeCarte == "Permanents" || zeCarte.TypeCarte == "creature" || zeCarte.TypeCarte == "batiment" || zeCarte.TypeCarte == "Permanent")
            zeCarte.perm = new Permanent(data[posDepart + 10], int.Parse(data[posDepart + 7]), int.Parse(data[posDepart + 8]), int.Parse(data[posDepart + 9]));
        return zeCarte;
    }
    private void combat(Carte attaquant, Carte ennemi, int posAllier, int posDefenseur, string nomAttaquant, string nomDefenseur)
    {
        recevoirDegat(attaquant, posAllier, false, nomAttaquant);
        recevoirDegat(ennemi, posDefenseur, true, nomDefenseur);
        if (attaquant.perm.Vie <= 0)
        {
            GameObject temp = GameObject.Find(nomAttaquant);
            Destroy(temp);
        }
        if (ennemi.perm.Vie <= 0)
        {
            GameObject temp = GameObject.Find(nomDefenseur);
            Destroy(temp);
        }
    }
    void setStat(Permanent perm, int[] stat)
    {
        perm.Attaque = stat[0];
        perm.Vie = stat[1];
        perm.Armure = stat[2];
    }
    public void recevoirDegat(Carte carte, int pos, bool allier, string nom)
    {
        GameObject t = null;
        if (carte != null)
        {
            if (allier)
            {
                int posi = nom.IndexOf("d");
                string position = nom.Substring(posi + 1, nom.Length - (posi + 1));
                t = GameObject.Find("armure" + position);
                t.GetComponent<TextMesh>().text = carte.perm.Armure.ToString();
                t = GameObject.Find("vie" + position);
                t.GetComponent<TextMesh>().text = carte.perm.Vie.ToString();
            }
            else
            {
                int posi = nom.IndexOf("s");
                string position = nom.Substring(posi + 1, nom.Length - (posi + 1));
                t = GameObject.Find("armureEnnemis" + position);
                t.GetComponent<TextMesh>().text = carte.perm.Armure.ToString();
                t = GameObject.Find("vieEnnemis" + position);
                t.GetComponent<TextMesh>().text = carte.perm.Vie.ToString();
            }
        }
    }
    public IEnumerator wait(int i)
    {
        yield return new WaitForSeconds(i);
    }
    //remet le nombre de worker au debut du tour
    // si newWorker = true on rajouter un worker
    private void setWorker(bool newWorker)
    {
        if (newWorker)
            NbWorkerMax++;
        NbWorker = NbWorkerMax;
    }
    private void setManaEnnemis(int ble, int bois, int gem)
    {
        NbBleEnnemis = ble;
        NbBoisEnnemis = bois;
        NbGemEnnemis = gem;
    }
    public int SetManaAjouter(Event events, int ressource, string typeRessource)
    {
        if (events.button == 0 && NbWorker > 0)
        {
            ressource++;
            NbWorker--;
        }
        else if (events.button == 1 && ressource != 0)
        {
            if (typeRessource == "ble" && joueur1.nbBle <= ressource - 1)
            {
                ressource--;
                NbWorker++;
            }
            else if (typeRessource == "bois" && joueur1.nbBois <= ressource - 1)
            {
                ressource--;
                NbWorker++;
            }
            else if (typeRessource == "gem" && joueur1.nbGem <= ressource - 1)
            {
                ressource--;
                NbWorker++;
            }
        }
        return ressource;
    }

    private void resetArmor(PosZoneCombat [] tab,GameObject[] style,bool allier)
    {
        for (int i = 0; i < tab.Length; ++i)
        {
            if (tab[i] != null && tab[i].carte != null && tab[i].carte.perm != null)
            {
                tab[i].carte.perm.Armure = tab[i].carte.perm.getBasicArmor();
                changestatFromCard(style[i], tab[i].carte);
            }
        }
    }

    //Retourne la premiere position qui est vide (disponible) sur la zone passé en parametre
    public static int TrouverOuPlacerCarte(PosZoneCombat[] Zone)
    {
        int OuPlacerCarte = 0;
        for (int i = 0; i < Zone.Length && Zone[i].EstOccupee == true; ++i)
        {
            OuPlacerCarte++;
        }
        return OuPlacerCarte;
    }

    private void PigerCarte()
    {
        NbCarteEnMainJoueur = 0;
	//compte le nombre de carte en main
        for (int i = 0; i < ZoneCarteJoueur.Length; ++i)
        {
            if (ZoneCarteJoueur[i].EstOccupee == true)
            {
                ++NbCarteEnMainJoueur;
            }
        }

        /*il n'y a plus de carte*/
        if(NoCarte >=40)
        {
            envoyerMessage("Carte manquante");
            //afficher vous avez perdu
            EstPerdant = true;
            //Application.LoadLevel("Menu");
        }
        /*main pleine*/
        else if(NbCarteEnMainJoueur >= 7)
        {
            //on montre la carte
            styleCarteAllier[ordrePige[NoCarte]].transform.position =new Vector3(-7,-1.2f,1);
            //on la detruit
            Destroy(styleCarteAllier[ordrePige[NoCarte]],2.0f);
            tabCarteAllier.CarteDeck[ordrePige[NoCarte]] = null;
            ++NoCarte;
        }
            /*on peut piger*/
        else
        {
            //on trouve ou mettre la carte dans la main
            int OuPlacerCarte = TrouverOuPlacerCarte(ZoneCarteJoueur);           
            ZoneCarteJoueur[OuPlacerCarte].EstOccupee = true;
            //on la place
            ZoneCarteJoueur[OuPlacerCarte].carte = tabCarteAllier.CarteDeck[ordrePige[NoCarte]];
            styleCarteAllier[ordrePige[NoCarte]].gameObject.transform.position = ZoneCarteJoueur[OuPlacerCarte].Pos;
            ++NoCarte;
            //on envoye au serveur qu'on a piger
            envoyerMessage("Piger");
        }
        --nbCarteAllier;
    }
    private Carte setHabilete(Carte card)
    {
        if (card.Habilete != "" && card.Habilete != null)
        {
            string[] data = card.Habilete.Split(new char[] { ',' });
            for (int i = 0; i < data.Length; ++i)
            {
                string trimmer = data[i].Trim();
                if (card.esthabileteNormal(data[i].Trim()))
                    card.setHabileteNormal(data[i].Trim());
                else
                {

                    string[] zeSpecialHability = data[i].Split(new char[] {' '});
                    if (getHabilete(zeSpecialHability[0]) && card != null && card.perm != null)
                    {
                        card.perm.specialhability = true;
                        card.perm.habilityspecial = data[i];
                    }
                }
            }
        }
        return card;
    }
    private bool getHabilete(string mot)
    {
        return mot == "Donne" || mot == "Ajoute";
    }
    private int placerCarte(GameObject carte, PosZoneCombat[] zone)
    {
        int PlacementZoneCombat = TrouverOuPlacerCarte(zone);
        if (PlacementZoneCombat != -1)
        {
            carte.transform.position = zone[PlacementZoneCombat].Pos;
            zone[PlacementZoneCombat].EstOccupee = true;
        }
        return PlacementZoneCombat;
    }
    private int TrouverEmplacementCarteJoueur(Vector3 PosCarte, PosZoneCombat[] Zone)
    {
        int Emplacement = 0;
        for (int i = 0; i < Zone.Length; ++i)
        {
            if (PosCarte.Equals(Zone[i].Pos))
            {
                return Emplacement;
            }
            else
            {
                ++Emplacement;
            }
        }
        return -1; // -1 pour savoir qu'il ne trouve aucune position (techniquement il devrais toujours retourner un pos valide)
    }
    private void envoyerMessage(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        connexionServeur.sck.Send(data);
    }
    private string lire()
    {
        string message = null;
        do
        {
            recevoirResultat();
        } while (message == null);
        return message;
    }
    private string recevoirResultat()
    {
        byte[] buff = new byte[connexionServeur.sck.SendBufferSize];
        int bytesRead = connexionServeur.sck.Receive(buff);
        byte[] formatted = new byte[bytesRead];

        for (int i = 0; i < bytesRead; i++)
        {
            formatted[i] = buff[i];
        }
        string strData = Encoding.ASCII.GetString(formatted);
        return strData;
    }
    private Carte ReceiveCarte(Socket client)
    {
        Carte carte = null;
        try
        {
            byte[] buffer = new byte[client.SendBufferSize];
            int bytesRead = client.Receive(buffer);
            byte[] formatted = new byte[bytesRead];
            BinaryFormatter receive = new BinaryFormatter();

            for (int i = 0; i < bytesRead; i++)
            {
                formatted[i] = buffer[i];
            }
            using (var recstream = new MemoryStream(formatted))
            {
                carte = receive.Deserialize(recstream) as Carte;
            }

        }
        catch (TimeoutException ex) { Console.Write("Erreur de telechargement des données"); }
        return carte;
    }
    private void EnvoyerCarte(Socket client, Carte carte)
    {
        byte[] data;
        BinaryFormatter b = new BinaryFormatter();
        using (var stream = new MemoryStream())
        {
            b.Serialize(stream, carte);
            data = stream.ToArray();
        }
        client.Send(data);
    }
    private Deck ReceiveDeck(Socket client)
    {
        Deck zeDeck = null;
        try
        {
            byte[] buffer = new byte[client.SendBufferSize];
            int bytesRead = client.Receive(buffer);
            byte[] formatted = new byte[bytesRead];
            BinaryFormatter receive = new BinaryFormatter();

            for (int i = 0; i < bytesRead; i++)
            {
                formatted[i] = buffer[i];
            }
            using (var recstream = new MemoryStream(formatted))
            {
                zeDeck = receive.Deserialize(recstream) as Deck;
            }

        }
        catch (TimeoutException ex) { Console.Write("Erreur de telechargement des données"); }
        return zeDeck;
    }

    private string trouverTypeTarget(string[] motsHabilete)
    {
        string target = ""; int i = 0;
        bool pasTrouver = true;
        while (pasTrouver || i > motsHabilete.Length)
        {
            if (motsHabilete[i] == "cible" || motsHabilete[i] == "creature" || motsHabilete[i] == "batiment" || motsHabilete[i] == "ennemis")
            {
                target = motsHabilete[i];
                pasTrouver = false;
            }
            else if (motsHabilete[i] == "vos")
            {
                if (motsHabilete[i + 1] == "creatures" || motsHabilete[i + 1] == "batiments" || motsHabilete[i + 1] == " cartes")
                {
                    target = motsHabilete[i] + motsHabilete[i + 1];
                    pasTrouver = false;
                }
            }
            else if (motsHabilete[i] == "toutes")
            {
                if (motsHabilete[i + 1] == "les" && (motsHabilete[i + 2] == "cartes" || (motsHabilete[i + 2] == "creatures")))
                {
                    if (motsHabilete[i + 3] == "ennemis" || motsHabilete[i + 3] == "alliees")
                    {
                        target = motsHabilete[i] + motsHabilete[i + 1] + motsHabilete[i + 2] + motsHabilete[i + 3];
                        pasTrouver = false;
                    }
                    else
                    {
                        target = motsHabilete[i] + motsHabilete[i + 1] + motsHabilete[i + 2];
                        pasTrouver = false;
                    }
                }
            }
            else if (motsHabilete[i] == "tous")
            {
                if (motsHabilete[i + 1] == "les" && motsHabilete[i + 2] == "batiments")
                {
                    target = motsHabilete[i] + motsHabilete[i + 1] + motsHabilete[i + 2];
                    pasTrouver = false;
                }
            }
            else if (motsHabilete[i] == "heros")
            {
                if (motsHabilete[i + 1] == "ennemi")
                {
                    target = motsHabilete[i] + motsHabilete[i + 1];
                    pasTrouver = false;
                }
            }
            else if (motsHabilete[i] == "place")
            {
                if (motsHabilete[i + 1] == "de" && motsHabilete[i + 2] == "combat")
                {
                    target = motsHabilete[i] + motsHabilete[i + 1] + motsHabilete[i + 2];
                    pasTrouver = false;
                }
            }
            ++i;
        }
        return target;
    }
}
