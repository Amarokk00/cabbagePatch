using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Rendering;


public class cabbagePatch : MonoBehaviour
{
    public KMBombModule BombModule;
    public KMBombInfo BombInfo;
    public Sprite[] spritesFile;
    public KMSelectable[] selectables;
    public SpriteRenderer[] sprites;

    public KMSelectable shovel;
    public KMSelectable water;

    bool evilcabbage = false;
    bool activated = false;
    bool bshovel = false;
    bool bwater = false;
    bool weedscheck = false;
    List<Crop> cropList = new List<Crop>();

    void ActivateModule()
    {
        activated = true;

    }

    void setShovel()
    {
        bshovel = true;
        bwater = false;
    }

    void SetWater()
    {
        bshovel = false;
        bwater = true;
    }

    void Start()
    {

        BombModule.OnActivate += ActivateModule;

        foreach (SpriteRenderer sprite in sprites)
        {
            sprite.sprite = null;
        }

        for (int i = 0; i < selectables.Length; i++)
        {
            int j = i;
            selectables[i].OnInteract += delegate () { OnPress(j); return false; };
        }

        shovel.OnInteract += delegate () { setShovel(); return false; };
        water.OnInteract += delegate () { SetWater(); return false; };
        generatePatch();


    }

    void OnPress(int index)
    {
        if (activated)
        {
            if (bshovel)
            {
                if (cropList[index].weeds)
                {
                    cropList[index].weeds = false;
                }
                else
                {
                    Debug.Log("[Cabbage Patch] Strike for shoveling healthy crop");
                    BombModule.HandleStrike();
                }
            }
            else if (bwater)
            {
                if (checkEvil(index))
                {
                    string timerText = BombInfo.GetFormattedTime();
                    if (timerText.Contains("" + cropList[index].time) && !cropList[index].watered && !cropList[index].weeds)
                    {
                        cropList[index].watered = true;
                        cropList[index].cropLevel++;
                        if (checkWatering())
                        {
                            clearWatered();
                        }
                        updateCrop(index);
                    }
                    else
                    {
                        Debug.Log("[Cabbage Patch] Strike for wrong watering of " + cropList[index].type);
                        Debug.Log("[Cabbage Patch] Crop already watered : " + cropList[index].watered);
                        Debug.Log("[Cabbage Patch] Crop had weeds : " + cropList[index].weeds);
                        Debug.Log("[Cabbage Patch] Timing is wrong : " + !timerText.Contains("" + cropList[index].time));

                        BombModule.HandleStrike();
                    }
                }
            }
        }
    }
    int incrementCrop(int i, string serial, List<String> indicators)
    {
        if (weedscheck) { weedscheck = false; }
        i++;
        if (!evilcabbage && (i == 4 && ((checkLitIndicator(indicators, "BOB") || serial.Contains("0")))))
        {
            cropList.Insert(i, new Crop("cabbage", false, false, 0, 0));
            evilcabbage = true;
            i++;
        }
        return i;
    }
    bool checkLitIndicator(List<String> indicators, string strIndicator)
    {
        foreach (String indicator in indicators)
        {
            Dictionary<string, string> indicatorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(indicator);
            if (indicatorDict["label"] == strIndicator && indicatorDict["on"] == "True")
            {
                return true;
            }
        }
        return false;
    }
    int countLit(List<String> indicators)
    {
        int count = 0;
        foreach (String indicator in indicators)
        {
            Dictionary<string, string> indicatorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(indicator);
            if (indicatorDict["on"] == "True")
            {
                count++;
            }
        }
        return count;
    }
    bool checkPort(List<String> ports, string strPort)
    {
        foreach (String port in ports)
        {
            Dictionary<string, List<string>> portDict = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(port);
            foreach (string portname in portDict["presentPorts"])
            {
                if (portname == strPort)
                {
                    return true;
                }
            }
        }
        return false;
    }
    void generatePatch()
    {
        int i = 0;

        string serial = JsonConvert.DeserializeObject<Dictionary<string, string>>(BombInfo.QueryWidgets("serial", null)[0])["serial"];
        List<String> indicators = BombInfo.QueryWidgets("indicator", null);
        List<String> batteries = BombInfo.QueryWidgets("batteries", null);
        List<String> ports = BombInfo.QueryWidgets("ports", null);
        int birthdaycakeCount = BombInfo.QueryWidgets("birthdaycake", null).Count();
        int batteriesCount = countBatteries(batteries);
        int batteryGroup = batteries.Count;
        int indicatorCount = indicators.Count;
        int litCount = countLit(indicators);
        bool emptyPlate = checkEmptyPlate(ports);
        bool vgaCheck = checkPort(ports, "VGA");
        bool hdmiCheck = checkPort(ports, "HDMI");
        bool ParallelCheck = checkPort(ports, "Parallel");
        bool needyCheck = checkNeedy();
        int vowelCount = serial.Count(c => "aeiou".Contains(Char.ToLower(c)));

        if (batteriesCount > 2)
        {
            cropList.Insert(i, new Crop("carrot", false, weedscheck, 1, 0));
            i = incrementCrop(i, serial, indicators);
        }
        if (emptyPlate)
        {
            cropList.Insert(i, new Crop("tomato", false, weedscheck, 3, 0));
            i = incrementCrop(i, serial, indicators);
        }
        if (litCount > batteryGroup)
        {
            cropList.Insert(i, new Crop("Ginseng", false, weedscheck, 4, 0));
            i = incrementCrop(i, serial, indicators);
        }
        if (indicatorCount > 0 || ParallelCheck)
        {
            weedscheck = true;
        }
        if (vowelCount > 0)
        {
            cropList.Insert(i, new Crop("pumpkin", false, weedscheck, 5, 0));
            i = incrementCrop(i, serial, indicators);
        }
        if (birthdaycakeCount > 0)
        {
            cropList.Insert(i, new Crop("broccoli", false, weedscheck, 6, 0));
            i = incrementCrop(i, serial, indicators);
        }
        if (vgaCheck || hdmiCheck)
        {
            cropList.Insert(i, new Crop("potato", false, weedscheck, 7, 0));
            i = incrementCrop(i, serial, indicators);
        }
        if (needyCheck)
        {
            cropList.Insert(i, new Crop("parsnip", false, weedscheck, 9, 0));
            i = incrementCrop(i, serial, indicators);
        }

        if (!needyCheck)
        {
            cropList.Insert(i, new Crop("parsnip", false, weedscheck, 9, 0));
            i = incrementCrop(i, serial, indicators);
        }
        if (!(vgaCheck || hdmiCheck))
        {
            cropList.Insert(i, new Crop("potato", false, weedscheck, 7, 0));
            i = incrementCrop(i, serial, indicators);
        }
        if (!(birthdaycakeCount > 0))
        {
            cropList.Insert(i, new Crop("broccoli", false, weedscheck, 6, 0));
            i = incrementCrop(i, serial, indicators);
        }
        if (!(vowelCount > 0))
        {
            cropList.Insert(i, new Crop("pumpkin", false, weedscheck, 5, 0));
            i = incrementCrop(i, serial, indicators);
        }
        if (!(indicatorCount > 0 || ParallelCheck))
        {
            weedscheck = true;
        }
        if (!(litCount > batteryGroup))
        {
            cropList.Insert(i, new Crop("ginseng", false, weedscheck, 4, 0));
            i = incrementCrop(i, serial, indicators);
        }
        if (!(emptyPlate))
        {
            cropList.Insert(i, new Crop("tomato", false, weedscheck, 3, 0));
            i = incrementCrop(i, serial, indicators);
        }
        if (!(batteriesCount > 2))
        {
            cropList.Insert(i, new Crop("carrot", false, weedscheck, 1, 0));
        }

        foreach (Crop crop in cropList)
        {
            Debug.Log("[Cabbage Patch] Crop added : " + crop.type);
        }
    }
    bool checkNeedy()
    {
        if (BombInfo.GetModuleNames().Count - BombInfo.GetSolvableModuleNames().Count > 0)
        {
            return true;
        }
        return false;
    }
    bool checkEmptyPlate(List<String> ports)
    {
        foreach (String port in ports)
        {
            Dictionary<string, List<string>> portDict = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(port);
            if (portDict["presentPorts"].Count == 0)
            {
                return true;
            }
        }
        return false;
    }
    int countBatteries(List<string> batteries)
    {
        int count = 0;
        foreach (String battery in batteries)
        {
            Dictionary<string, string> batteryDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(battery);
            count += int.Parse(batteryDict["numbatteries"]);
        }
        return count;
    }
    void updateCrop(int index)
    {
        Crop crop = cropList[index];
        sprites[index].sprite = findsprite("" + crop.type + "_" + crop.cropLevel);
    }
    Sprite findsprite(string name)
    {
        switch (name)
        {
            case "broccoli_0":
                return spritesFile[0];
                break;
            case "broccoli_1":
                return spritesFile[1];
                break;
            case "broccoli_2":
                return spritesFile[2];
                break;
            case "cabbage_0":
                return spritesFile[3];
                break;
            case "cabbage_1":
                return spritesFile[4];
                break;
            case "cabbage_2":
                return spritesFile[5];
                break;
            case "carrot_0":
                return spritesFile[6];
                break;
            case "carrot_1":
                return spritesFile[7];
                break;
            case "carrot_2":
                return spritesFile[8];
                break;
            case "ginseng_0":
                return spritesFile[9];
                break;
            case "ginseng_1":
                return spritesFile[10];
                break;
            case "ginseng_2":
                return spritesFile[11];
                break;
            case "parsnip_0":
                return spritesFile[12];
                break;
            case "parsnip_1":
                return spritesFile[13];
                break;
            case "parsnip_2":
                return spritesFile[14];
                break;
            case "potato_0":
                return spritesFile[15];
                break;
            case "potato_1":
                return spritesFile[16];
                break;
            case "potato_2":
                return spritesFile[17];
                break;
            case "pumpkin_0":
                return spritesFile[18];
                break;
            case "pumpkin_1":
                return spritesFile[19];
                break;
            case "pumpkin_2":
                return spritesFile[20];
                break;
            case "tomato_0":
                return spritesFile[21];
                break;
            case "tomato_1":
                return spritesFile[22];
                break;
            case "tomato_2":
                return spritesFile[23];
                break;
            default:
                return null;
                
        }
    }
    void clearWatered()
    {
        foreach (Crop crop in cropList)
        {
            crop.watered = false;
        }

        checkPass();
    }
    bool checkWatering()
    {
        int checkWatering = 0;
        foreach (Crop crop in cropList)
        {
            if (crop.watered) { checkWatering++; }
        }
        if (checkWatering >= cropList.Count)
        {

            return true;
        }
        return false;
    }
    void checkPass()
    {
        int checkPass = 0;
        foreach (Crop crop in cropList)
        {
            if (crop.cropLevel >= 2) { checkPass++; }
        }
        if (checkPass >= cropList.Count)
        {
            activated = false;
            BombModule.HandlePass();
        }

    }
    bool checkEvil(int index)
    {
        if (!evilcabbage)
        {
            return true;
        }
        if (index != 4 && !cropList[4].watered)
        {
            Debug.Log("[Cabbage Patch] Strike for not watering cabbage first");
            BombModule.HandleStrike();
            return false;
        }
        return true;
    }

}

public class Crop
{
    public string type { get; set; }
    public bool watered { get; set; }
    public bool weeds { get; set; }
    public int time { get; set; }
    public int cropLevel { get; set; }
    public Crop(string type, bool watered, bool weeds, int time, int cropLevel)
    {
        this.type = type;
        this.watered = watered;
        this.weeds = weeds;
        this.time = time;
        this.cropLevel = cropLevel;
    }
}