using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Proyecto26;
using TMPro;
using UnityEngine.SceneManagement;

public class UIAuth : MonoBehaviour
{
    [SerializeField] private RectTransform m_authPanel;
    [SerializeField] private RectTransform m_verifyPanel;
    [SerializeField] private TextMeshProUGUI m_statusText;

    [SerializeField] private TMP_Dropdown m_counrtyDropdown;
    [SerializeField] private TMP_InputField m_phoneInput;
    [SerializeField] private Button m_loginBtn;

    [SerializeField] private TMP_InputField m_otpInput;
    [SerializeField] private Button m_verifyBtn;

    private string m_phone;
    private string m_countryCode;
    private string m_otp;

// Use this for initialization
    void Start()
    {
        Hide(m_verifyPanel);
        m_statusText.text = string.Empty;
    }

    private void OnEnable()
    {
        m_loginBtn.onClick.AddListener(Login);
        m_verifyBtn.onClick.AddListener(VerifyOTP);
    }

    void Login()
    {
        m_statusText.text = "Logging In...";

        m_countryCode = m_counrtyDropdown.GetComponent<CountryDropdown>().CurrentValue;
        m_phone = m_phoneInput.text;

        //Call thirdparty api to register or signin with this phone number
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("country_code", m_countryCode);
        parameters.Add("phone", m_phone);

        foreach (KeyValuePair<string, string> parameter in parameters)
        {
            Debug.Log($"{parameter.Key}:{parameter.Value}");

        }

        RestClient.Post($"{ThirdPartyAPIs.SERVER_BASE_URL}/register", parameters).Then(response =>
        {
            ServerResponse resp = JsonUtility.FromJson<ServerResponse>(response.Text);
            
            if(resp.status)
            {
                Hide(m_authPanel);
                Show(m_verifyPanel);
                m_statusText.text = string.Empty;
            }
            else
            {
                m_statusText.text = resp.message;
            }

            
        }).Catch(error =>
        {
            m_statusText.text = error.Message;
        });
    }

    private void VerifyOTP()
    {
        m_otp = m_otpInput.text;

        m_statusText.text = "Verifying OTP...";

        //Call thirdparty api to register or signin with this phone number
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("country_code", m_countryCode);
        parameters.Add("phone", m_phone);
        parameters.Add("otp", m_otp);
        parameters.Add("authid", ClientSingleton.Instance.Manager.User.AuthId);

        RestClient.Post($"{ThirdPartyAPIs.SERVER_BASE_URL}/verify-otp", parameters).Then(response =>
        {
            ServerResponse resp = JsonUtility.FromJson<ServerResponse>(response.Text);

            if (resp.status)
            {
                PlayerPrefs.SetString("phone", m_countryCode);
                PlayerPrefs.SetString("country_code", m_countryCode);

                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            }
            else
            {
                m_statusText.text = resp.message;
            }

        }).Catch(error =>
        {
            m_statusText.text = error.Message;
        });
    }

    private void OnDisable()
    {
        m_loginBtn.onClick.RemoveAllListeners();
    }

    void Hide(RectTransform panel)
    {
        panel.gameObject.SetActive(false);
    }
    
    void Show(RectTransform panel)
    {
        panel.gameObject.SetActive(true);
    }

    [Serializable]
    private class ServerResponse
    {
        public bool status;
        public bool isNew;
        public string message;
    }
}
