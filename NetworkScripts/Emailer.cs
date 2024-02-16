using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using UnityEngine;
using System;

//THIS SCRIPT IS FOR SENDING FEEDBACK TO THE EMAIL
public class Emailer : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI txtData;
    [SerializeField] UnityEngine.UI.Button btnSubmit;
    [SerializeField] bool sendDirect;
    public GameObject loadingIcon;

    [SerializeField] string kSenderEmailAddress = "gameTest0988@gmail.com";
    [SerializeField] string kSenderPassword = "puzzleTest123";
    [SerializeField] string kReceiverEmailAddress = "gameTest0988@gmail.com";

    // Method 2: Server request
    //const string url = "https://coderboy6000.000webhostapp.com/emailer.php";//website name here
    
    void Start()
    {
        UnityEngine.Assertions.Assert.IsNotNull(txtData);
        UnityEngine.Assertions.Assert.IsNotNull(btnSubmit);
    }
    public void SendEmailTask()
    {
        if (sendDirect)
        {
            //SendAnEmail(txtData.text);
            Task g = new Task(LoadingIconEvent);
            g.Start();
            Task t = new Task(() =>
            {
                SendAnEmail(txtData.text);
            });
            t.Start();
        }
        else
        {
            //method 2
            // SendServerRequestForEmail(txtData.text);
        }
    }
    // Method 1: Direct message
    private void SendAnEmail(string message)
    {
        // Create mail
        MailMessage mail = new MailMessage();
        mail.From = new MailAddress(kSenderEmailAddress);
        mail.To.Add(kReceiverEmailAddress);
        mail.Subject = "Game Feedback";
        mail.Body = message;

        // Setup server 
        SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
        smtpServer.Port = 587;
        smtpServer.Credentials = new NetworkCredential(
            kSenderEmailAddress, kSenderPassword) as ICredentialsByHost;
        smtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors) {
                return true;
            };

        // Send mail to server, print results
        try
        {
            smtpServer.Send(mail);
        }
        catch (System.Exception e)
        {
           // Debug.Log("Email error: " + e.Message);
        }
        finally
        {
            loadingIcon.SetActive(false);
            loadingIcon.GetComponent<Animator>().SetBool("loading", false);
        }
    }

    // Method 2: Server request
    //private void SendServerRequestForEmail(string message)
    //{
    //    StartCoroutine(SendMailRequestToServer(message));
    //}

    //// Method 2: Server request
    //static IEnumerator SendMailRequestToServer(string message)
    //{
    //    // Setup form responses
    //    WWWForm form = new WWWForm();
    //    form.AddField("name", "It's me!");
    //    form.AddField("fromEmail", kSenderEmailAddress);
    //    form.AddField("toEmail", kReceiverEmailAddress);
    //    form.AddField("message", message);

    //    // Submit form to our server, then wait
    //    WWW www = new WWW(url, form);
    //    Debug.Log("Email sent!");

    //    yield return www;

    //    // Print results
    //    if (www.error == null)
    //    {
    //        Debug.Log("WWW Success!: " + www.text);
    //    }
    //    else
    //    {
    //        Debug.Log("WWW Error: " + www.error);
    //    }
    //}
    static Action LoadingIconEvent;
    private void OnEnable()
    {
        LoadingIconEvent += ShowLoadingIcon;
    }
    public void ShowLoadingIcon()
    {
        loadingIcon.SetActive(true);
        loadingIcon.GetComponent<Animator>().SetBool("loading", true);
    }
    public void BackToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene((int)SceneIndexes.MAINMENU);
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
}