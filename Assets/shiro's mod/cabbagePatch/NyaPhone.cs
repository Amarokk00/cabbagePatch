using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NyaPhone : MonoBehaviour
{
    public KMSelectable HomeButton;
    public TextMesh timetext;
    public TextMesh datetext;

    public Material screen;
    public GameObject Apps;
    public GameObject phoneScreen;
    List<GameObject> appAreas = new List<GameObject>();


    void Start()
    {
        phoneScreen = this.transform.Find("phoneScreen").gameObject;
        Apps = this.transform.Find("AppArea").gameObject;

        foreach (Transform t in Apps.transform)
        {
            appAreas.Add(t.gameObject);
        }

        HomeButton.OnInteract += delegate ()
        {
            home();
            return false;
        };

        string theDate = System.DateTime.Now.ToString("dddd MMMM dd");
        datetext.text = theDate;
        string theTime = System.DateTime.Now.ToString("HH:mm");
        timetext.text = theTime;

    }

    void FixedUpdate()
    {
        try
        {
            string theTime = System.DateTime.Now.ToString("HH:mm");
            timetext.text = theTime;
        }
        catch { }
    }

    void home()
    {
        var materials = phoneScreen.GetComponent<MeshRenderer>().materials;
        materials[1] = screen;
        phoneScreen.GetComponent<MeshRenderer>().materials = materials;

        datetext.gameObject.SetActive(true);
        timetext.gameObject.SetActive(true);
        Apps.SetActive(true);
    }
}

