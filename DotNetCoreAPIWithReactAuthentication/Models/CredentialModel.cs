using System.ComponentModel.DataAnnotations;

namespace DotNetCoreAPIWithReactAuthentication.Models
{
    public class CredentialModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}