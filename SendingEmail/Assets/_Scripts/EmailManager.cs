/***********************************************************
#Script Name:	EmailManager
#Author:		Deiniol Owen
#GitHub:		https://github.com/Dowen92?tab=repositories
#Blog:			https://dowen92dev.wordpress.com/
***********************************************************/

#region Using directives
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
#endregion

public class EmailManager : MonoBehaviour {

    #region Variables
    #region UI
    [Header("UI")]
    public InputField nameInputField;
    public InputField emailInputField;
    public InputField feedbackInputField;
    public Text errorText;
    #endregion

    #region Email
    [Header("E-mail")]
    public string senderEmail;
    public string senderPassword;
    public string recepientEmail;
    public string attachmentPath;

    public enum MailClient
    {
        GMAIL,
        HOTMAIL
    }
    public MailClient mailClient;
    #endregion
    #endregion

    private void Awake()
    {
        if (!InputIsValid(senderEmail.Trim()) || !InputIsValid(senderPassword.Trim()) || !InputIsValid(recepientEmail.Trim()))
            Debug.LogError("Please make sure to fill in the email fields on the EmailManager GameObject", this);
    }

    #region Send E-mail
    public void SendFeedbackEmail()
    {
        MailMessage mail = new MailMessage();

        #region Build up mail
        mail.From = new MailAddress(senderEmail, "Feedback from app X");
        mail.To.Add(recepientEmail);
        mail.Subject = "Feedback";
        mail.Body = "Name: " + nameInputField.text + "\nEmail: " + emailInputField.text + "\n\nFeedback\n\n" + feedbackInputField.text;

        if (!string.IsNullOrEmpty(attachmentPath)) //Add attachment if path isn't empty in inspector
        {
            Attachment attachment = new Attachment(attachmentPath);
            mail.Attachments.Add(attachment);
        }
        
        //smtpServer.Timeout = 999999999;
        #endregion

        #region smtp & Authoristion
        SmtpClient smtpServer = GetSmtpClient(mailClient);
        smtpServer.Port = 587;
        smtpServer.Credentials = new NetworkCredential(senderEmail, senderPassword) as ICredentialsByHost;
        smtpServer.EnableSsl = true;

        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };
        #endregion

        #region Send E-mail
        //smtpServer.Send(mail);
        smtpServer.SendAsync(mail, null);
        smtpServer.SendCompleted += MailSendComplete;
        #endregion

        ClearAllInputs();
    }
    #endregion

    #region CallBack
    void MailSendComplete(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        if (e.Cancelled)
        {
            Debug.Log("Mail sending was cancelled");
        }
        if (e.Error != null)
        {
            Debug.Log("Error: " + e.Error.ToString());
        }
        else
        {
            Debug.Log("Mail sent!");
        }
    }
    #endregion

    #region Helpers
    //Get smtpClient based on choice in inspector
    private SmtpClient GetSmtpClient(MailClient client)
    {
        SmtpClient smtpClientToReturn = null;

        switch (client)
        {
            case MailClient.GMAIL:
                smtpClientToReturn = new SmtpClient("smtp.gmail.com");
                break;
            case MailClient.HOTMAIL:
                smtpClientToReturn = new SmtpClient("smtp.live.com");
                break;
            default:
                Debug.Log("Client doesn't exist, do you need to add it?");
                break;
        }

        return smtpClientToReturn;
    }

    //Validate user input - Make sure it isn't empty
    private bool InputIsValid(string input)
    {
        if (string.IsNullOrEmpty(input.Trim()))
            return false;

        return true;
    }

    //Notify user of input error
    private void AlertError()
    {
        errorText.text = "*ERROR: Please fill in all required fields";
    }

    //Clear all inputs after sending E-mail
    private void ClearAllInputs()
    {
        nameInputField.text = "";
        emailInputField.text = "";
        feedbackInputField.text = "";
        errorText.text = "";
    }
    #endregion

    #region OnClicks
    public void OnClickSendFeedback()
    {
        if (InputIsValid(nameInputField.text.Trim()) && InputIsValid(emailInputField.text.Trim()) && InputIsValid(feedbackInputField.text.Trim()))
            SendFeedbackEmail();
        else
            AlertError();
    }
    #endregion
}