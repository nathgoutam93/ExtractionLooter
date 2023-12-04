using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CountryDropdown : MonoBehaviour
{
    public TMP_Dropdown dropdown;

    private Dictionary<string, string> countries = new Dictionary<string, string> {
        {"+91", "IN" },
        {"+1" , "US" },
        {"+44", "GB" },
        {"+61", "AU" },
        {"+49", "DE" },
        {"+33", "FR" },
        {"+86", "CN" },
        {"+81", "JP" },
        {"+7" , "RU" },
        {"+55", "BR" },
        {"+27", "ZA" },
        {"+52", "MX" },
        {"+34", "ES" },
        {"+39", "IT" },
    };

    private Dictionary<int, string> optionsIndexs = new Dictionary<int, string>();

    public string CurrentValue { get; private set; }

    void Start()
    {
        dropdown.onValueChanged.AddListener(OnValueChanged);

        // Create a list of dropdown options
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        // Add calling code and alpha2code for each country as an option
        foreach (var country in countries.Keys)
        {
            options.Add(new TMP_Dropdown.OptionData($"{country}"));
        }

        // Add options to the dropdown
        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        for (int i = 0; i < dropdown.options.Count; i++)
        {
            optionsIndexs[i] = dropdown.options[i].text;
        }

        // Set the initial value of the CurrentValue property
        CurrentValue = countries[optionsIndexs[dropdown.value]];
    }

    public void OnValueChanged(int index)
    {
        countries.TryGetValue(optionsIndexs[index], out string value);
        CurrentValue = value;
    }
}