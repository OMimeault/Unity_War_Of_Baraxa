﻿using UnityEngine;
using System.Collections;
using System.Net;
using System.Text;
using System.Net.Sockets;

public class Script_logIn : MonoBehaviour {
//Variables
    public string alias  = "";
    public string password = "";
    public string nom = "";
    public string prenom  = "";
    public string messageErreur="";
    bool nouveauCompte = false;
    bool showBox = false;
    public Socket sck;

//GUIStyle
    public GUIStyle textArea;
    public GUIStyle buttons;
    public GUIStyle text;
    public GUIStyle warOfBaraxa;
    public GUIStyle Background;
    public GUIStyle GUIBox;
    public GUIStyle GUIButton;


    public void Awake()
    {
        sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("172.17.104.114"), 1234);
        try
        {
            sck.Connect(localEndPoint);
        }
        catch(SocketException ex)
        {
            
        }
    }
public void OnGUI() {
	GUI.Box(new Rect(0,0,Screen.width,Screen.height),"",Background);
	warOfBaraxa.fontSize = Screen.width / 10;
	text.fontSize = Screen.width/42;
	textArea.fontSize = Screen.width/42;
	buttons.fontSize = Screen.width/42;
    if (!showBox)
    {
        //Titre
        GUI.Label(new Rect((Screen.width / 2) - (Screen.width * 0.6f / 2), Screen.height * 0.1f, Screen.width * 0.6f, Screen.height * 0.1f), "Wars of Baraxa", warOfBaraxa);
        //Alias
        GUI.Label(new Rect((Screen.width / 2) - (Screen.width * 0.40f / 2), Screen.height * 0.40f, Screen.width * 0.10f, Screen.height * 0.05f), "Alias", text);
        alias = GUI.TextField(new Rect((Screen.width / 2) - (Screen.width * 0.25f / 2), Screen.height * 0.40f, Screen.width * 0.25f, Screen.height * 0.05f), alias, 20, textArea);
        //Mot de passe
        GUI.Label(new Rect((Screen.width / 2) - (Screen.width * 0.6f / 2), Screen.height * 0.47f, Screen.width * 0.20f, Screen.height * 0.05f), "Mot de passe", text);
        password = GUI.PasswordField(new Rect((Screen.width / 2) - (Screen.width * 0.25f / 2), Screen.height * 0.47f, Screen.width * 0.25f, Screen.height * 0.05f), password, "*"[0], 20, textArea);

        //Si veut se connecter a un compte
        if (!nouveauCompte)
        {
            //Tente de se connecter avec les informations fournis
            if (GUI.Button(new Rect((Screen.width * 0.435f) - (Screen.width * 0.12f / 2), Screen.height * 0.54f, Screen.width * 0.12f, Screen.height * 0.06f), "Connecter", buttons))
            {
                //Conditions d'erreurs
                if (alias == "" || password == "")
                {
                    showBox = true;
                    messageErreur = "Un des champs est vide. \n Veuillez entrer tous les champs.";
                }
                else if (!estDansBd(alias,password)){
                    showBox = true;
                    messageErreur = "Votre alias ou votre mot de passe \n est invalide.";
                }
                else
                {
                    Application.LoadLevel("Menu");                    
                }

            }
            //Change l'interface pour pouvoir créer un compte
            if (GUI.Button(new Rect((Screen.width * 0.564f) - (Screen.width * 0.12f / 2), Screen.height * 0.54f, Screen.width * 0.12f, Screen.height * 0.06f), "Créer", buttons))
            {
                nouveauCompte = true;
            }
        }
        //Si on veut créer un nouveau compte (plus de champs a remplir et boutons changent de place)
        if (nouveauCompte)
        {
            //Prenom
            GUI.Label(new Rect((Screen.width / 2) - (Screen.width * 0.43f / 2), Screen.height * 0.54f, Screen.width * 0.10f, Screen.height * 0.05f), "Prenom", text);
            prenom = GUI.TextField(new Rect((Screen.width / 2) - (Screen.width * 0.25f / 2), Screen.height * 0.54f, Screen.width * 0.25f, Screen.height * 0.05f), prenom, 25, textArea);
            //Nom
            GUI.Label(new Rect((Screen.width / 2) - (Screen.width * 0.40f / 2), Screen.height * 0.61f, Screen.width * 0.10f, Screen.height * 0.05f), "Nom", text);
            nom = GUI.TextField(new Rect((Screen.width / 2) - (Screen.width * 0.25f / 2), Screen.height * 0.61f, Screen.width * 0.25f, Screen.height * 0.05f), nom, 25, textArea);

            //Si on click sur "RETOUR" on retourne a la connection d'un compte existant
            if (GUI.Button(new Rect((Screen.width * 0.435f) - (Screen.width * 0.12f / 2), Screen.height * 0.68f, Screen.width * 0.12f, Screen.height * 0.05f), "Retour", buttons))
            {
                alias = "";
                password = "";
                nom = "";
                prenom = "";
                nouveauCompte = false;
            }
            //Si on click sur "CRÉER" on tente de créer le compte selon les informations données (vérifications)
            if (GUI.Button(new Rect((Screen.width * 0.564f) - (Screen.width * 0.12f / 2), Screen.height * 0.68f, Screen.width * 0.12f, Screen.height * 0.05f), "Créer", buttons))
            {
                if (alias == "" || password == "" || nom == "" || prenom == "")
                {
                    showBox = true;
                    messageErreur = "Un des champs est vide. \n Veuillez entrer tous les champs.";
                }
                else if (getAliasBd(alias,password,nom,prenom))
                {
                    showBox = true;
                    messageErreur = "\n Votre alias est deja utiliser.";
                }
                else 
                {
                    //manque a se connecter
                    Application.LoadLevel("Menu"); 
                }
            }
        }
    }
    else 
    {
        GUIBox.fontSize = Screen.width / 40;
        GUIButton.fontSize = Screen.width / 50;
        GUI.Box(new Rect(Screen.width * 0.30f, Screen.height * 0.35f, Screen.width * 0.40f, Screen.height * 0.20f),messageErreur, GUIBox);
        if (GUI.Button(new Rect((Screen.width * 0.425f), Screen.height * 0.45f, Screen.width * 0.15f, Screen.height * 0.07f), "ok", GUIButton))
        {
            showBox = false;
        }
    }
}		
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    private bool getAliasBd(string alias,string mdp,string nom,string prenom)
    {
        envoyerMessage(alias+","+mdp+","+nom+","+prenom);
        string reponse = lire();
        if (reponse == "oui")
            return true;

        return false;
    }
    private bool estDansBd(string alias, string mdp)
    {
        envoyerMessage(alias +","+mdp);
        string reponse = lire();
        if (reponse == "oui")
            return true;

        return false;
    }
    private void envoyerMessage(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        sck.Send(data);
    }
    private string lire()
    {
        string message = null;
        do
        {
            message = recevoirResultat();
        } while (message == null);
        return message;
    }
    private string recevoirResultat()
    {
        byte[] buff = new byte[sck.SendBufferSize];
        int bytesRead = sck.Receive(buff);
        byte[] formatted = new byte[bytesRead];
        for (int i = 0; i < bytesRead; i++)
        {
            formatted[i] = buff[i];
        }
        string strData = Encoding.ASCII.GetString(formatted);
        return strData;
    }
}