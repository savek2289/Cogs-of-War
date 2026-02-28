using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;

public class PlayerCharacteristics : MonoBehaviour
{
    public static PlayerCharacteristics Instance { get; private set; }
    

    [System.Serializable]
    public class Characteristic
    {
        [SerializeField] private string name;

        [Space(10)]

        [SerializeField] Slider whiteSlider; 
        [SerializeField] private Slider colorSlider;
        [SerializeField] private TextMeshProUGUI value;

        public string Name => name;
        public Slider WhiteSlider => whiteSlider;
        public Slider ColorSlider => colorSlider;
        public TextMeshProUGUI Value => value;
    }

    [SerializeField] private List<Characteristic> characteristicsData;

    [Space(10)]

    [SerializeField] private List<UIModule> modules;

    [Space(5)]

    [SerializeField] private GameObject celectFrame;

    private Dictionary<string, Characteristic> characteristicsMap;
    private int? celectedModule = null;
    private int currentHpValue;
    private int currentDamageValue;
    private int maxHpValue;
    private int maxDamageValue;

    private bool lockApplyChanges = false;
    private bool needToReset = false;

    private const int baseHpValue = 100;
    private const int baseDamageValue = 10;

    private void Awake()
    {
        Instance = this;

        if (characteristicsData == null) return;

        characteristicsMap = new Dictionary<string, Characteristic>();
        foreach (var characteristic in characteristicsData)
        {
            if (characteristic == null) continue;

            if (!characteristicsMap.ContainsKey(characteristic.Name))
            {
                characteristicsMap.Add(characteristic.Name, characteristic);
            }
            else
            {
                Debug.LogWarning($"Duplicate characteristic name: {characteristic.Name}");
            }
        }

        maxHpValue = baseHpValue;
        maxDamageValue = baseDamageValue;

        foreach (var module in modules)
        {
            Dictionary<string, object> values = module.GetValues();

            int moduleHpValue = values["Hp"].ConvertTo<int>();
            if ((baseHpValue + moduleHpValue) > maxHpValue)
                maxHpValue = baseHpValue + moduleHpValue;

            int moduleDamageValue = values["Damage"].ConvertTo<int>();
            if ((baseDamageValue + moduleDamageValue) > maxDamageValue) 
                maxDamageValue = baseDamageValue + moduleDamageValue; 
        }

        characteristicsMap["Hp"].Value.text = baseHpValue.ToString();
        characteristicsMap["Hp"].WhiteSlider.maxValue = maxHpValue;
        characteristicsMap["Hp"].WhiteSlider.value = baseHpValue;
        characteristicsMap["Hp"].ColorSlider.maxValue = maxHpValue;
        characteristicsMap["Hp"].ColorSlider.value = baseHpValue;

        characteristicsMap["Damage"].Value.text = baseDamageValue.ToString();
        characteristicsMap["Damage"].WhiteSlider.maxValue = maxDamageValue;
        characteristicsMap["Damage"].WhiteSlider.value = baseDamageValue;
        characteristicsMap["Damage"].ColorSlider.maxValue = maxDamageValue;
        characteristicsMap["Damage"].ColorSlider.value = baseDamageValue;

        currentHpValue = baseHpValue;
        currentDamageValue = baseDamageValue;
    }


    public void SetChanges(string parametrName, int value)
    {
        if (characteristicsMap == null) return;

        if (characteristicsMap.TryGetValue(parametrName, out var characteristic))
        {
            StartCoroutine(SetChangesCoroutine(
                characteristic.WhiteSlider,
                characteristic.ColorSlider,
                characteristic.Value,
                value
            ));
        }
    }

    private IEnumerator SetChangesCoroutine(Slider whiteSlider, Slider colorSlider, TextMeshProUGUI text, int value, float duration = 0.5f)
    {
        lockApplyChanges = true;

        bool? isAffirmative = value > 0 ? true : (value < 0 ? false : null);

        if (isAffirmative == null)
        {
            lockApplyChanges = false;
            yield break;
        }

        if (needToReset)
        {
            characteristicsMap["Hp"].Value.text = baseHpValue.ToString();
            characteristicsMap["Hp"].WhiteSlider.value = baseHpValue;
            characteristicsMap["Hp"].ColorSlider.value = baseHpValue;

            characteristicsMap["Damage"].Value.text = baseDamageValue.ToString();
            characteristicsMap["Damage"].WhiteSlider.value = baseDamageValue;
            characteristicsMap["Damage"].ColorSlider.value = baseDamageValue;

            needToReset = false;
        }        

        colorSlider.gameObject.SetActive(true);
        colorSlider.fillRect.GetComponent<Image>().color = isAffirmative == true ? Color.green : Color.red;
        if (isAffirmative == false)
        {
            whiteSlider.value += value;
        }
        else
        {
            colorSlider.value += value;
        }
        text.text = Convert.ToString(Convert.ToInt32(text.text) + value);

        lockApplyChanges = false;
    }

    public void ApplyChanges()
    {
        StartCoroutine(ApplyAllChanges());
    }

    private IEnumerator ApplyAllChanges()
    {
        if (characteristicsMap["Hp"].ColorSlider.fillRect.GetComponent<Image>().color == Color.red)
        {
            currentDamageValue = Convert.ToInt32(characteristicsMap["Hp"].WhiteSlider.value);
            characteristicsMap["Hp"].ColorSlider.value = currentDamageValue;
        }
        else
        {
            currentHpValue = Convert.ToInt32(characteristicsMap["Hp"].ColorSlider.value);
            characteristicsMap["Hp"].WhiteSlider.value = currentHpValue;
        }
        
        if (characteristicsMap["Damage"].ColorSlider.fillRect.GetComponent<Image>().color == Color.red)
        {
            currentDamageValue = Convert.ToInt32(characteristicsMap["Damage"].WhiteSlider.value);
            characteristicsMap["Damage"].ColorSlider.value = currentDamageValue;
        }
        else
        {
            currentDamageValue = Convert.ToInt32(characteristicsMap["Damage"].ColorSlider.value);
            characteristicsMap["Damage"].WhiteSlider.value = currentDamageValue;
        }

        yield return null;
    }

    public void SetNeedToReset(bool parametr) => needToReset = parametr;

    public void SetCelectedModule(ModuleButton targetModule)
    {
        if (targetModule == null || !targetModule.TryGetComponent<UIModule>(out UIModule targetModuleScript) || celectFrame == null) return;

        if (!celectFrame.activeInHierarchy)
            celectFrame.SetActive(true);

        if (celectedModule != null)
        {
            if (celectedModule.HasValue && celectedModule.Value < modules.Count)
            {
                modules[celectedModule.Value].TryGetComponent<ModuleButton>(out ModuleButton modulebutton);
                if (modulebutton != null)
                    modulebutton.enabled = true;
            }
        }

        targetModule.enabled = false;

        celectedModule = modules.IndexOf(targetModuleScript);

        if (celectedModule == -1)
        {
            Debug.LogWarning("Модуль не найден в списке modules!");
            celectedModule = null;
            return;
        }

        RectTransform targetModuleTransform = targetModule.GetComponent<RectTransform>();
        celectFrame.GetComponent<RectTransform>().anchoredPosition = new Vector3(
            targetModuleTransform.anchoredPosition.x,
            targetModuleTransform.anchoredPosition.y - 1);
    }
}
