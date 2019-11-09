using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SiteAwake.Domain.Entities;
using SiteAwake.Domain.Infrastructure;

namespace SiteAwake.WebApplication.Infrastructure
{
    public class AccountValidation
    {
        public class ValidationMessage
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; }
        }

        public ValidationMessage IsUserNameValid(IContext context, long accountId, string userName)
        {
            ValidationMessage validationMessage = new ValidationMessage() { IsValid = true };
            
            if (context.Accounts.Any(x => x.Email.Equals(userName, StringComparison.OrdinalIgnoreCase) && (x.Id != accountId || accountId == 0)))
            {
                validationMessage.ErrorMessage = "Email already in use. Please select another.";
                validationMessage.IsValid = false;
                return validationMessage;
            }

            return validationMessage;
        }

        public ValidationMessage IsPasswordValid(string password)
        {
            ValidationMessage validationMessage = new ValidationMessage() { IsValid = true };

            if (password.Length < 8 || password.Length > 14)
            {
                validationMessage.ErrorMessage = "Invalid password length. Must be from 8 to 14 characters.";
                validationMessage.IsValid = false;
                return validationMessage;
            }

            return validationMessage;
        }

        public ValidationMessage IsWakeUpIntervalValid(int wakeUpInterval)
        {
            ValidationMessage validationMessage = new ValidationMessage() { IsValid = true };

            if (wakeUpInterval < 1 || wakeUpInterval > 60)
            {
                validationMessage.ErrorMessage = "Invalid wake-up interval. Must be from 1 to 60 minutes.";
                validationMessage.IsValid = false;
                return validationMessage;
            }

            return validationMessage;
        }

        public ValidationMessage IsUrlValid(IContext context, long accountId, string url)
        {
            ValidationMessage validationMessage = new ValidationMessage() { IsValid = true };

            url = url.ToLower();

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                validationMessage.ErrorMessage = "The URL is not well-formed. Please select another in the format \"http://domain.com\" or \"https://www.domain.com\", etc";
                validationMessage.IsValid = false;
                return validationMessage;
            }

            var urlParts = url.Split('.');

            if (urlParts.Length < 2)
            {
                validationMessage.ErrorMessage = "The URL is invald. Please select another in the format \"domain.com\"";
                validationMessage.IsValid = false;
                return validationMessage;
            }
            
            var domainZone = (urlParts[urlParts.Length - 2] + "." + urlParts[urlParts.Length - 1]).Replace("https://", "").Replace("http://", "");

            if (context.SiteMetadatas.Any(x => x.Url.Contains(domainZone) && (x.AccountId != accountId || accountId == 0)))
            {
                var siteMetadatas = context.SiteMetadatas.Where(x => x.Url.Contains(domainZone) && (x.AccountId != accountId || accountId == 0)).ToList();

                foreach (var siteMetadata in siteMetadatas)
                {
                    //compare domain and zone
                    var sUrlParts = siteMetadata.Url.Split('.');
                    var sDomainZone = (sUrlParts[sUrlParts.Length - 2] + "." + sUrlParts[sUrlParts.Length - 1]).Replace("https://", "").Replace("http://", "");
                    
                    if (sDomainZone == domainZone)
                    {
                        validationMessage.ErrorMessage = "URL already in use. Please select another.";
                        validationMessage.IsValid = false;
                        return validationMessage;
                    }
                }
            }

            return validationMessage;
        }
    }
}