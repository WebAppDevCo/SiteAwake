using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiteAwake.PaymentService.Infrastructure;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers.Bases;
using System.Threading;
using System.Configuration;
using SiteAwake.PaymentService.Models;

namespace SiteAwake.PaymentServiceProvider.Infrastructure
{
    public class PaymentServiceContext : IPaymentServiceContext
    {
        #region Properties

        private readonly string ApiLoginID;
        private readonly string ApiTransactionKey;

        #endregion

        #region Constructor

        public PaymentServiceContext()
        {
            ApiLoginID = ConfigurationManager.AppSettings["AuthNetLoginID"];
            ApiTransactionKey = ConfigurationManager.AppSettings["AuthNetTransKey"];
        }
        
        #endregion

        #region Public Methods

        public async Task<ITransactionResponse> ChargeCreditCard(ICustomerPayment customerPayment)
        {
            var transactionResponse = new TransactionResponse();

            var task = Task.Run(() =>
            {
                if (Convert.ToBoolean(string.IsNullOrEmpty(ConfigurationManager.AppSettings["AuthNetIsProduction"]) ? "false" : ConfigurationManager.AppSettings["AuthNetIsProduction"]))
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
                }
                else
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
                }

                // define the merchant information (authentication / transaction id)
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
                {
                    name = ApiLoginID,
                    ItemElementName = ItemChoiceType.transactionKey,
                    Item = ApiTransactionKey,
                };

                var customerData = new customerDataType
                {
                    id = customerPayment.Id,
                    email = customerPayment.Email,
                    type = customerTypeEnum.individual
                };

                var creditCard = new creditCardType
                {
                    cardNumber = customerPayment.CardNumber,
                    expirationDate = customerPayment.ExpirationDate,
                    cardCode = customerPayment.CardCode
                };

                var billingAddress = new customerAddressType
                {
                    company = customerPayment.Company,
                    firstName = customerPayment.FirstName,
                    lastName = customerPayment.LastName,
                    address = customerPayment.Address,
                    city = customerPayment.City,
                    zip = customerPayment.Zip,
                    email = customerPayment.Email
                };

                //standard api call to retrieve response
                var paymentType = new paymentType { Item = creditCard };
                
                // Add line Items
                var lineItems = new lineItemType[customerPayment.LineItems != null ? customerPayment.LineItems.Count : 0];

                if (customerPayment.LineItems != null)
                {
                    for (int i = 0; i < customerPayment.LineItems.Count; i++)
                    {
                        var lineItem = customerPayment.LineItems[i];
                        lineItems[i] = new lineItemType {
                            itemId = lineItem.ItemId,
                            description = lineItem.Description,
                            name = lineItem.Name,
                            quantity = lineItem.Quantity,
                            unitPrice = lineItem.UnitPrice
                        };
                    }
                }

                var transactionRequest = new transactionRequestType
                {
                    transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),    // charge the card

                    customer = customerData,
                    amount = customerPayment.Amount,
                    payment = paymentType,
                    billTo = billingAddress,
                    lineItems = lineItems,
                    customerIP = customerPayment.IpAddress,
                    //refTransId = ? //used when voiding payments and should be an auth.net transId
                    userFields = new userField[1]
                };

                transactionRequest.userFields[0] = new userField() { name = "Customer Payment Id", value = customerPayment.Id };
                
                var request = new createTransactionRequest { transactionRequest = transactionRequest };
                
                // instantiate the contoller that will call the service
                var controller = new createTransactionController(request);
                controller.Execute();

                // get the response from the service (errors contained if any)
                var response = controller.GetApiResponse();

                if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.transactionResponse != null)
                    {
                        transactionResponse.AuthCode = response.transactionResponse.authCode;
                        transactionResponse.AccountNumber = response.transactionResponse.accountNumber;
                        transactionResponse.AccountType = response.transactionResponse.accountType;
                        transactionResponse.AvsResultCode = response.transactionResponse.avsResultCode;
                        transactionResponse.CavvResultCode = response.transactionResponse.cavvResultCode;
                        transactionResponse.CvvResultCode = response.transactionResponse.cvvResultCode;
                        transactionResponse.Errors = null;
                        transactionResponse.IsSuccess = true;
                        transactionResponse.Messages = null;
                        transactionResponse.RawResponseCode = response.transactionResponse.rawResponseCode;
                        transactionResponse.RefTransID = response.transactionResponse.refTransID;
                        transactionResponse.ResponseCode = response.transactionResponse.responseCode;
                        transactionResponse.TestRequest = response.transactionResponse.testRequest;
                        transactionResponse.TransHash = response.transactionResponse.transHash;
                        transactionResponse.TransId = response.transactionResponse.transId;

                        System.Diagnostics.Debug.WriteLine("Success, Auth Code : " + response.transactionResponse.authCode);
                    }
                }
                else if (response != null && response.messages.message != null)
                {
                    transactionResponse.IsSuccess = false;

                    System.Diagnostics.Debug.WriteLine("Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text);

                    transactionResponse.Messages = new TransactionResponseMessage[] { new TransactionResponseMessage() { Code = response.messages.message[0].code, Description = response.messages.message[0].text } };

                    if (response.transactionResponse != null)
                    {
                        transactionResponse.AuthCode = response.transactionResponse.authCode;
                        transactionResponse.AccountNumber = response.transactionResponse.accountNumber;
                        transactionResponse.AccountType = response.transactionResponse.accountType;
                        transactionResponse.AvsResultCode = response.transactionResponse.avsResultCode;
                        transactionResponse.CavvResultCode = response.transactionResponse.cavvResultCode;
                        transactionResponse.CvvResultCode = response.transactionResponse.cvvResultCode;
                        transactionResponse.Errors = new TransactionResponseError[] { new TransactionResponseError() { ErrorCode = response.transactionResponse.errors[0].errorCode, ErrorText = response.transactionResponse.errors[0].errorText } };
                        transactionResponse.RawResponseCode = response.transactionResponse.rawResponseCode;
                        transactionResponse.RefTransID = response.transactionResponse.refTransID;
                        transactionResponse.ResponseCode = response.transactionResponse.responseCode;
                        transactionResponse.TestRequest = response.transactionResponse.testRequest;
                        transactionResponse.TransHash = response.transactionResponse.transHash;
                        transactionResponse.TransId = response.transactionResponse.transId;

                        System.Diagnostics.Debug.WriteLine("Transaction Error : " + response.transactionResponse.errors[0].errorCode + " " + response.transactionResponse.errors[0].errorText);
                    }
                }

                return response;
            });

            await task;

            return transactionResponse;
        }

        public async Task<ITransactionResponse> ChargeCustomerProfile(ICustomerPayment customerPayment)
        {
            var transactionResponse = new TransactionResponse();

            var task = Task.Run(() =>
            {
                if (Convert.ToBoolean(string.IsNullOrEmpty(ConfigurationManager.AppSettings["AuthNetIsProduction"]) ? "false" : ConfigurationManager.AppSettings["AuthNetIsProduction"]))
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
                }
                else
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
                }

                // define the merchant information (authentication / transaction id)
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
                {
                    name = ApiLoginID,
                    ItemElementName = ItemChoiceType.transactionKey,
                    Item = ApiTransactionKey,
                };

                customerProfilePaymentType profileToCharge = new customerProfilePaymentType();
                profileToCharge.customerProfileId = customerPayment.CustomerProfileId;
                profileToCharge.paymentProfile = new paymentProfile { paymentProfileId = customerPayment.CustomerPaymentProfileId };

                var transactionRequest = new transactionRequestType
                {
                    transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),    // refund type
                    amount = customerPayment.Amount,
                    profile = profileToCharge
                };

                var request = new createTransactionRequest { transactionRequest = transactionRequest };

                // instantiate the collector that will call the service
                var controller = new createTransactionController(request);
                controller.Execute();

                // get the response from the service (errors contained if any)
                var response = controller.GetApiResponse();

                if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.transactionResponse != null)
                    {
                        transactionResponse.AuthCode = response.transactionResponse.authCode;
                        transactionResponse.Errors = null;
                        transactionResponse.IsSuccess = true;
                        transactionResponse.Messages = null;
                        transactionResponse.TransId = response.transactionResponse.transId;

                        System.Diagnostics.Debug.WriteLine("Success, Auth Code : " + response.transactionResponse.authCode);
                    }
                }
                else if (response != null && response.messages.message != null)
                {
                    transactionResponse.IsSuccess = false;

                    System.Diagnostics.Debug.WriteLine("Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text);

                    transactionResponse.Messages = new TransactionResponseMessage[] { new TransactionResponseMessage() { Code = response.messages.message[0].code, Description = response.messages.message[0].text } };

                    if (response.transactionResponse != null)
                    {
                        transactionResponse.AuthCode = response.transactionResponse.authCode;
                        transactionResponse.Errors = new TransactionResponseError[] { new TransactionResponseError() { ErrorCode = response.transactionResponse.errors[0].errorCode, ErrorText = response.transactionResponse.errors[0].errorText } };
                        transactionResponse.RawResponseCode = response.transactionResponse.rawResponseCode;
                        transactionResponse.TransId = response.transactionResponse.transId;

                        System.Diagnostics.Debug.WriteLine("Transaction Error : " + response.transactionResponse.errors[0].errorCode + " " + response.transactionResponse.errors[0].errorText);
                    }
                }

                return response;
            });

            await task;

            return transactionResponse;
        }

        public async Task<ITransactionResponse> CreateSubscription(ICustomerPayment customerPayment)
        {
            var transactionResponse = new TransactionResponse();

            var task = Task.Run(() =>
            {
                if (Convert.ToBoolean(string.IsNullOrEmpty(ConfigurationManager.AppSettings["AuthNetIsProduction"]) ? "false" : ConfigurationManager.AppSettings["AuthNetIsProduction"]))
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
                }
                else
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
                }

                // define the merchant information (authentication / transaction id)
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
                {
                    name = ApiLoginID,
                    ItemElementName = ItemChoiceType.transactionKey,
                    Item = ApiTransactionKey,
                };

                paymentScheduleTypeInterval interval = new paymentScheduleTypeInterval();

                interval.length = 1;                        // months can be indicated between 1 and 12
                interval.unit = ARBSubscriptionUnitEnum.months;

                paymentScheduleType schedule = new paymentScheduleType
                {
                    interval = interval,
                    startDate = customerPayment.StartDate,      // start date should be tomorrow
                    totalOccurrences = customerPayment.TotalOccurrences,                          // 9999 indicates no end date
                    trialOccurrences = customerPayment.TrialOccurrences
                };

                var creditCard = new creditCardType
                {
                    cardNumber = customerPayment.CardNumber,
                    expirationDate = customerPayment.ExpirationDate,
                    cardCode = customerPayment.CardCode
                };

                var billingAddress = new nameAndAddressType
                {
                    company = customerPayment.Company,
                    firstName = customerPayment.FirstName,
                    lastName = customerPayment.LastName,
                    address = customerPayment.Address,
                    city = customerPayment.City,
                    zip = customerPayment.Zip
                };
                
                //standard api call to retrieve response
                var paymentType = new paymentType { Item = creditCard };

                ARBSubscriptionType subscriptionType = new ARBSubscriptionType()
                {
                    amount = customerPayment.Amount,
                    trialAmount = customerPayment.TrialAmount,
                    paymentSchedule = schedule,
                    billTo = billingAddress,
                    payment = paymentType
                };
                   
                var request = new ARBCreateSubscriptionRequest { subscription = subscriptionType };

                // instantiate the contoller that will call the service
                var controller = new ARBCreateSubscriptionController(request);
                controller.Execute();

                // get the response from the service (errors contained if any)
                var response = controller.GetApiResponse();

                if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response != null && response.messages.message != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Success, Subscription ID : " + response.subscriptionId.ToString());

                        transactionResponse.Errors = null;
                        transactionResponse.IsSuccess = true;
                        transactionResponse.Messages = null;
                        transactionResponse.RefTransID = response.refId;
                        transactionResponse.SubscriptionId = response.subscriptionId;
                    }
                    else
                    {
                        transactionResponse.IsSuccess = false;
                    }
                }
                else if (response != null && response.messages.message != null)
                {
                    transactionResponse.IsSuccess = false;

                    System.Diagnostics.Debug.WriteLine("Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text);
                    
                    transactionResponse.Messages = new TransactionResponseMessage[] { new TransactionResponseMessage() { Code = response.messages.message[0].code, Description = response.messages.message[0].text } };
                }

                return response;
            });

            await task;

            return transactionResponse;
        }

        public async Task<ITransactionResponse> CancelSubscription(string subscriptionId)
        {
            var transactionResponse = new TransactionResponse();

            var task = Task.Run(() =>
            {
                if (Convert.ToBoolean(string.IsNullOrEmpty(ConfigurationManager.AppSettings["AuthNetIsProduction"]) ? "false" : ConfigurationManager.AppSettings["AuthNetIsProduction"]))
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
                }
                else
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
                }

                // define the merchant information (authentication / transaction id)
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
                {
                    name = ApiLoginID,
                    ItemElementName = ItemChoiceType.transactionKey,
                    Item = ApiTransactionKey,
                };

                var request = new ARBCancelSubscriptionRequest { subscriptionId = subscriptionId };
                var controller = new ARBCancelSubscriptionController(request);                          // instantiate the contoller that will call the service
                controller.Execute();

                // get the response from the service (errors contained if any)
                var response = controller.GetApiResponse();
               
                //validate
                if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response != null && response.messages.message != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Success, Subscription Cancelled");

                        transactionResponse.Errors = null;
                        transactionResponse.IsSuccess = true;
                        transactionResponse.Messages = null;
                    }
                    else
                    {
                        transactionResponse.IsSuccess = false;
                    }
                }
                else if (response != null && response.messages.message != null)
                {
                    transactionResponse.IsSuccess = false;

                    System.Diagnostics.Debug.WriteLine("Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text);

                    transactionResponse.Messages = new TransactionResponseMessage[] { new TransactionResponseMessage() { Code = response.messages.message[0].code, Description = response.messages.message[0].text } };
                }

                return response;
            });

            await task;

            return transactionResponse;
        }

        public async Task<ITransactionResponse> UpdateSubscription(ICustomerPayment customerPayment)
        {
            var transactionResponse = new TransactionResponse();

            var task = Task.Run(() =>
            {
                if (Convert.ToBoolean(string.IsNullOrEmpty(ConfigurationManager.AppSettings["AuthNetIsProduction"]) ? "false" : ConfigurationManager.AppSettings["AuthNetIsProduction"]))
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
                }
                else
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
                }

                // define the merchant information (authentication / transaction id)
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
                {
                    name = ApiLoginID,
                    ItemElementName = ItemChoiceType.transactionKey,
                    Item = ApiTransactionKey,
                };

                // get subscription
                var subscriptionRequest = new ARBGetSubscriptionRequest { subscriptionId = customerPayment.SubscriptionId };

                var controllerSubscription = new ARBGetSubscriptionController(subscriptionRequest);          // instantiate the contoller that will call the service
                controllerSubscription.Execute();

                ARBGetSubscriptionResponse subscriptionResponse = controllerSubscription.GetApiResponse();   // get the response from the service (errors contained if any)

                //validate
                if (subscriptionResponse != null && subscriptionResponse.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (subscriptionResponse.subscription != null)
                    {
                        //Do nothing
                    }
                }
                else if (subscriptionResponse != null)
                {
                    if (subscriptionResponse.messages.message.Length > 0)
                    {
                        transactionResponse.IsSuccess = false;

                        System.Diagnostics.Debug.WriteLine("Error: " + subscriptionResponse.messages.message[0].code + "  " + subscriptionResponse.messages.message[0].text);

                        transactionResponse.Messages = new TransactionResponseMessage[] { new TransactionResponseMessage() { Code = subscriptionResponse.messages.message[0].code, Description = subscriptionResponse.messages.message[0].text } };
                    }
                }
                else
                {
                    if (controllerSubscription.GetErrorResponse().messages.message.Length > 0)
                    {
                        transactionResponse.IsSuccess = false;

                        System.Diagnostics.Debug.WriteLine("Error: " + subscriptionResponse.messages.message[0].code + "  " + subscriptionResponse.messages.message[0].text);

                        transactionResponse.Messages = new TransactionResponseMessage[] { new TransactionResponseMessage() { Code = subscriptionResponse.messages.message[0].code, Description = subscriptionResponse.messages.message[0].text } };
                    }
                }

                var currentSubscription = subscriptionResponse.subscription;
   
                var creditCard = new creditCardType
                {
                    cardNumber = customerPayment.CardNumber,
                    expirationDate = customerPayment.ExpirationDate,
                    cardCode = customerPayment.CardCode
                };

                var billingAddress = new nameAndAddressType
                {
                    company = customerPayment.Company,
                    firstName = customerPayment.FirstName,
                    lastName = customerPayment.LastName,
                    address = customerPayment.Address,
                    city = customerPayment.City,
                    zip = customerPayment.Zip
                };

                //standard api call to retrieve response
                var paymentType = new paymentType { Item = creditCard };

                ARBSubscriptionType subscriptionType = new ARBSubscriptionType()
                {
                    billTo = billingAddress,
                    payment = paymentType
                };

                var request = new ARBUpdateSubscriptionRequest { subscription = subscriptionType, subscriptionId = customerPayment.SubscriptionId };

                // instantiate the contoller that will call the service
                var controller = new ARBUpdateSubscriptionController(request);
                controller.Execute();

                // get the response from the service (errors contained if any)
                var response = controller.GetApiResponse();

                if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response != null && response.messages.message != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Success");

                        transactionResponse.Errors = null;
                        transactionResponse.IsSuccess = true;
                        transactionResponse.Messages = null;
                    }
                    else
                    {
                        transactionResponse.IsSuccess = false;
                    }
                }
                else if (response != null && response.messages.message != null)
                {
                    transactionResponse.IsSuccess = false;

                    System.Diagnostics.Debug.WriteLine("Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text);

                    transactionResponse.Messages = new TransactionResponseMessage[] { new TransactionResponseMessage() { Code = response.messages.message[0].code, Description = response.messages.message[0].text } };
                }

                return response;
            });

            await task;

            return transactionResponse;
        }

        public async Task<ITransactionResponse> CreateCustomerProfileFromTransaction(string transactionId)
        {
            var transactionResponse = new TransactionResponse();

            var task = Task.Run(() =>
            {
                if (Convert.ToBoolean(string.IsNullOrEmpty(ConfigurationManager.AppSettings["AuthNetIsProduction"]) ? "false" : ConfigurationManager.AppSettings["AuthNetIsProduction"]))
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
                }
                else
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
                }

                // define the merchant information (authentication / transaction id)
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
                {
                    name = ApiLoginID,
                    ItemElementName = ItemChoiceType.transactionKey,
                    Item = ApiTransactionKey,
                };

                var request = new createCustomerProfileFromTransactionRequest { transId = transactionId };

                var controller = new createCustomerProfileFromTransactionController(request);
                controller.Execute();

                createCustomerProfileResponse response = controller.GetApiResponse();

                //validate
                if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response != null && response.messages.message != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Success, Customer Profile ID : " + response.customerProfileId + " Customer Payment Profile ID : " + response.customerPaymentProfileIdList[0]);

                        transactionResponse.TransId = transactionId;
                        transactionResponse.Errors = null;
                        transactionResponse.IsSuccess = true;
                        transactionResponse.Messages = null;
                        transactionResponse.CustomerProfileId = response.customerProfileId;
                        transactionResponse.CustomerPaymentProfileId = response.customerPaymentProfileIdList[0];
                    }
                    else
                    {
                        transactionResponse.IsSuccess = false;
                    }
                }
                else if (response != null && response.messages.message != null)
                {
                    transactionResponse.IsSuccess = false;

                    System.Diagnostics.Debug.WriteLine("Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text);

                    transactionResponse.Messages = new TransactionResponseMessage[] { new TransactionResponseMessage() { Code = response.messages.message[0].code, Description = response.messages.message[0].text } };
                }

                return response;
            });

            await task;

            return transactionResponse;
        }

        public async Task<ITransactionResponse> UpdateCustomerPaymentProfile(ICustomerPayment customerPayment)
        {
            var transactionResponse = new TransactionResponse();

            var task = Task.Run(() =>
            {
                if (Convert.ToBoolean(string.IsNullOrEmpty(ConfigurationManager.AppSettings["AuthNetIsProduction"]) ? "false" : ConfigurationManager.AppSettings["AuthNetIsProduction"]))
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
                }
                else
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
                }

                // define the merchant information (authentication / transaction id)
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
                {
                    name = ApiLoginID,
                    ItemElementName = ItemChoiceType.transactionKey,
                    Item = ApiTransactionKey,
                };                

                var creditCard = new creditCardType
                {
                    cardNumber = customerPayment.CardNumber,
                    expirationDate = customerPayment.ExpirationDate,
                    cardCode = customerPayment.CardCode
                };

                //standard api call to retrieve response
                var paymentType = new paymentType { Item = creditCard };

                var paymentProfile = new customerPaymentProfileExType
                {
                    billTo = new customerAddressType
                    {
                        // change information as required for billing
                        company = customerPayment.Company,
                        firstName = customerPayment.FirstName,
                        lastName = customerPayment.LastName,
                        address = customerPayment.Address,
                        city = customerPayment.City,
                        zip = customerPayment.Zip,
                        email = customerPayment.Email
                    },
                    payment = paymentType,
                    customerPaymentProfileId = customerPayment.CustomerPaymentProfileId
                };

                var request = new updateCustomerPaymentProfileRequest();
                request.customerProfileId = customerPayment.CustomerProfileId;
                request.paymentProfile = paymentProfile;
                request.validationMode = validationModeEnum.liveMode;

                // instantiate the controller that will call the service
                var controller = new updateCustomerPaymentProfileController(request);
                controller.Execute();

                // get the response from the service (errors contained if any)
                var response = controller.GetApiResponse();

                if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response != null && response.messages.message != null)
                    {
                        System.Diagnostics.Debug.WriteLine(response.messages.message[0].text);

                        transactionResponse.IsSuccess = true;
                    }
                    else
                    {
                        transactionResponse.IsSuccess = false;
                    }
                }
                else if (response != null && response.messages.message != null)
                {
                    transactionResponse.IsSuccess = false;

                    System.Diagnostics.Debug.WriteLine("Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text);

                    transactionResponse.Messages = new TransactionResponseMessage[] { new TransactionResponseMessage() { Code = response.messages.message[0].code, Description = response.messages.message[0].text } };
                }

                return response;
            });

            await task;

            return transactionResponse;
        }

        public async Task<ITransactionResponse> DeleteCustomerProfile(string customerProfileId)
        {
            var transactionResponse = new TransactionResponse();

            var task = Task.Run(() =>
            {
                if (Convert.ToBoolean(string.IsNullOrEmpty(ConfigurationManager.AppSettings["AuthNetIsProduction"]) ? "false" : ConfigurationManager.AppSettings["AuthNetIsProduction"]))
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
                }
                else
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
                }

                // define the merchant information (authentication / transaction id)
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
                {
                    name = ApiLoginID,
                    ItemElementName = ItemChoiceType.transactionKey,
                    Item = ApiTransactionKey,
                };

                var request = new deleteCustomerProfileRequest
                {
                    customerProfileId = customerProfileId
                };

                //Prepare Request
                var controller = new deleteCustomerProfileController(request);
                controller.Execute();
                
                // get the response from the service (errors contained if any)
                var response = controller.GetApiResponse();

                //validate
                if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response != null && response.messages.message != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Success, ResultCode : " + response.messages.resultCode.ToString());

                        transactionResponse.Errors = null;
                        transactionResponse.IsSuccess = true;
                        transactionResponse.Messages = null;
                    }
                    else
                    {
                        transactionResponse.IsSuccess = false;
                    }
                }
                else if (response != null && response.messages.message != null)
                {
                    transactionResponse.IsSuccess = false;

                    System.Diagnostics.Debug.WriteLine("Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text);

                    transactionResponse.Messages = new TransactionResponseMessage[] { new TransactionResponseMessage() { Code = response.messages.message[0].code, Description = response.messages.message[0].text } };
                }
                
                return response;
            });

            await task;

            return transactionResponse;
        }

        #endregion
    }
}
