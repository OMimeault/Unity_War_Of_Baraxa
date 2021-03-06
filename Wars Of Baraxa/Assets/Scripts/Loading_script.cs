﻿using UnityEngine;
using System;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using warsofbaraxa;

public class Loading_script : MonoBehaviour
{
    //pour ONGUI
    public GUIStyle Background;
    public GUI Label_loading;
    public GUIStyle Loading;
    public Texture gear;
    string reponse = "";
    bool appeler = false;
    Color couleur = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    float alpha = 1;
    bool fadeUp;

    // Use this for initialization
    void Start()
    {
    }
    //sleep en unity
    public IEnumerator wait(float i)
    {
        yield return new WaitForSeconds(i);
    }
    //a la fin du programme on envoie au serveur que l'on se déconnect
    void OnApplicationQuit()
    {
        envoyerMessage("deconnection");
    }
    // Update is called once per frame
    void Update()
    {
        //on recois un message a chaque seconde et on en recois un du serveur
        string message = lire();
        if (message == "Partie Commencer")
            Application.LoadLevel("Board");

        envoyerMessage("oui");
    }
    //ONGUI est appelé a chaque frame et s'occupe de l'interface graphique
    void OnGUI()
    {
        //background
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "", Background);
        GUI.Label(new Rect(Screen.width * 0.41f, Screen.height * 0.82f, Screen.width * 0.3f, Screen.height * 0.2f), "Loading", Loading);
        //permet de rendre invisible et visible le texte. il le rend visible et invisible progressivement
        Loading.normal.textColor = couleur;
        //si il est invisible on le met visible sinon c'est le contraire
        if (couleur.a <= 0)
            fadeUp = true;
        else if (couleur.a == 1)
            fadeUp = false;

        if (fadeUp)
        {
            alpha += 0.1f;
            couleur = new Color(1, 1, 1, alpha);
        }
        else
        {
            alpha -= 0.1f;
            couleur = new Color(1, 1, 1, alpha);
        }
    }
    //-------communication serveur-----------////////////
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
            message = recevoirResultat();
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
    //-------Fin communication serveur-----------////////////
}
