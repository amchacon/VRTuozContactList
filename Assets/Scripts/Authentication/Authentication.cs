using System;
using System.Collections.Generic;
using UnityEngine;
using sha1;

public class Authentication : MonoBehaviour {

    public Transform content;
    public GameObject userLinePrefab;
    [SerializeField] List<Users> usersList;
    private SendMail sendMail;
    private int usersCount;
    private const string URL_BASE = "http://auth.playo.com.br/";

    private void Start()
    {
        sendMail = GetComponent<SendMail>();
        usersCount = usersList.Count;
        SetupUserList();
    }

    //Recebe os usuários do celular pelo plugin

    //Gera os botões com a lista de usuários
    public void SetupUserList()
    {
        for (int i = 0; i < usersCount; i++)
        {
            UserLine userBtn = (UserLine)Instantiate(userLinePrefab, content).GetComponent<UserLine>();
            string username = usersList[i].username;
            string email = usersList[i].email;
            userBtn.buttonText.text = email;
            string token = usersList[i].token = GenerateToken();
            string link = URL_BASE + token;
            userBtn.buttonComponent.onClick.AddListener(delegate { sendMail.Send(username, email, link); });
        }
    }
    
    public void _DEBUG_LoadCoreApplication()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("ApplicationCore");
    }

    public void EmailLinkCallback(string urlReturn)
    {
        Debug.Log(urlReturn);
    }

    #region Security
    private string GenerateToken()
    {
        toSha1 tosha = new toSha1();
        return tosha.Sha1Sum(GetTimestamp());
    }

    private string GetTimestamp()
    {
        var dateTime = DateTime.Now;
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var unixDateTime = (dateTime.ToUniversalTime() - epoch).TotalSeconds;
        return unixDateTime.ToString();
    }
    #endregion

}

