namespace robot_project_v3.Mail;

public class SmtpSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string User { get; set; }
    public string Password { get; set; }

    public string DefaultEmail { get; set; }
}