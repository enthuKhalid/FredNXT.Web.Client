#region references
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Script.Serialization;
#endregion

namespace FredNXT.Web.Client
{
    class Program
    {
        #region variables storing values from config
        private static readonly int HttpTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["HttpTimeout"]);     //for setting http timeouts. Useful when querying larege amount of data
        private static readonly string FredApiUrl = ConfigurationManager.AppSettings["FredAPIUrl"];                     //the URL for pointing to the Fred APIs
        private static readonly string FredODataUrl = ConfigurationManager.AppSettings["FredODataUrl"];                 //the URL pointing to the Fred OData service
        private static readonly string DemoUserName = ConfigurationManager.AppSettings["DemoUserName"];                 //the demo username
        private static readonly string DemoPassword = ConfigurationManager.AppSettings["DemoPassword"];                 //the demo account password
        private const string MediaTypeJson = "application/json";                                                        //will use JSON for data transmissions
        #endregion

        #region main function
        static void Main(string[] args)
        {
            //Add a serviec reference to the OData endpoint and,
            //Create the OData service container and associate the credentials
            var container = new FredODataService.Container(new Uri(FredODataUrl))
            {
                Credentials = new NetworkCredential(DemoUserName, DemoPassword)
            };

            //prompt user to select an operation to perform
            int selectedOption = DisplayPrompt();
            
            //perform operation based on user selection
            while (selectedOption != 0)
            {
                switch (selectedOption)
                {
                    case 1:
                        CountAllCustomers(container);
                        break;

                    case 2:
                        GetCustomerById(container);
                        break;

                    case 3:
                        var ctx = new DataServiceContext(new Uri(FredODataUrl))
                        {
                            Credentials = new NetworkCredential(DemoUserName, DemoPassword)
                        };

                        new FredData<FredODataService.TaxOnItem>().Get(ctx, "TaxOnItems");
                        break;

                    case 4:
                        CreatePurchaseOrder(new PODetails());
                        break;

                    case 5:
                        GetUserStores();
                        break;

                    default:
                        Console.WriteLine("Invalid selection. Please try again.");
                        break;
                }

                selectedOption = DisplayPrompt();
            }
        }
        #endregion

        #region Methods calling Fred API
        /// <summary>
        /// Creates a Purchase order via Fred API with the given input details
        /// </summary>
        /// <param name="poDetails">The purchase order details</param>
        private static void CreatePurchaseOrder(PODetails poDetails)
        {
            //generate a disposable httpclient object with the credentials associated with it
            using (var client = GetHttpClient(FredApiUrl, DemoUserName, DemoPassword))
            {
                //serialize the input object into JSON, and set its content type
                var request = new StringContent(new JavaScriptSerializer().Serialize(poDetails))
                {
                    Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
                };

                //call (post to) the specific method, along with the reuired JSON serialized inputs.
                //Note: Can also use PostAsJsonAsync which will internally serialze the object to json.
                var response = client.PostAsync("PO/CreatePurchOrder", request);

                //check the status code to see if its a success
                if (response.Result.IsSuccessStatusCode)
                {
                    //read the return value asynchronously
                    var output = response.Result.Content.ReadAsStringAsync();
                    var orderId = output.Result;
                    Console.WriteLine("    Created order id=" + orderId);
                }
                else
                {
                    //Error details can be got by
                    var errorDetails = response.Result.Content.ReadAsStringAsync();
                    var errorMsg = errorDetails.Result;
                    Console.WriteLine("    Error occured:" + errorMsg);
                }
            }
        }

        /// <summary>
        /// Gets the list of stores the user is associated to via Fred API
        /// </summary>
        private static void GetUserStores()
        {
            //generate a disposable httpclient object with the credentials associated with it
            using (var client = GetHttpClient(FredApiUrl, DemoUserName, DemoPassword))
            {
                //execute a simple get on the GetAlluserStores method
                var response = client.GetAsync("Users/GetAllUserStores");

                //check the status code to see if its a success
                if (response.Result.IsSuccessStatusCode)
                {
                    //read the return value asynchronously
                    var output = response.Result.Content.ReadAsStringAsync();
                    Console.WriteLine("    Raw output: {0}", output.Result);

                    //can also deserialise the returned object and work with it as below
                    //var obj = new JavaScriptSerializer().DeserializeObject(result.Result);
                }
                else
                {
                    //Error details can be got by
                    var errorDetails = response.Result.Content.ReadAsStringAsync();
                    var errorMsg = errorDetails.Result;
                    Console.WriteLine("    Error occured:" + errorMsg);
                }
            }
        }

        #endregion

        #region Methods utilising Fred OData
        /// <summary>
       /// Gets the list of customers via Fred OData and displays them
       /// </summary>
       /// <param name="container">The Odata connection container</param>
        private static void ListAllCustomers(FredODataService.Container container)
        {            
            //access the enumerable objects under any entity directly 
            foreach (var cust in container.CustTables)
            {
                DisplayCustomer(cust);
            }
        }

        /// <summary>
        /// Gets the counts of customers via fred odata
        /// </summary>
        /// <param name="container">The Odata connection container</param>
        private static void CountAllCustomers(FredODataService.Container container)
        {
            //can add additional query parameters on the entities before executing it
            int count = container.CustTables.IncludeTotalCount().Execute().Count();
            Console.WriteLine("    Count of customers:" + count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container">The Odata connection container</param>
        private static void GetCustomerById(FredODataService.Container container)
        {
            Console.WriteLine("    Please enter the customer recId to search (ex:5637146827):");
            long id = Convert.ToInt64(Console.ReadLine());

            //can apply most of the LINQ expressions on the OData entities 
            DisplayCustomer(container.CustTables.Where(p => p.RecId == id).SingleOrDefault());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container">The Odata connection container</param>
        /// <param name="purchId"></param>
        private static void GetPurchById(FredODataService.Container container, string purchId)
        {
            //can add query options like $filter, $top, $skip, $sort etc like this
            DisplayPurchOrder(container.PurchTables.AddQueryOption("$filter","PurchId eq '"+purchId+"'").SingleOrDefault());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container">The Odata connection container</param>
        private static void GetCustomerPurchases(FredODataService.Container container)
        {
            //can use expand expression to get sub-entities within a parent entity
            foreach (var cust in container.VendTables.Expand(p => p.PurchItems))
            {
                DisplayVendor(cust);
                foreach (var purch in cust.PurchItems)
                {
                    DisplayPurchOrder(purch);
                }
                
            }
        }
        #endregion

        #region helper functions
        private static HttpClient GetHttpClient(string url, string username, string password)
        {
            HttpClient client = GetHttpClient(url);

            var credentials = Encoding.ASCII.GetBytes(username + ":" + password);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));
            return client;
        }


        private static HttpClient GetHttpClient(string url)
        {
            HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri(url)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", MediaTypeJson);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeJson));

            client.Timeout = TimeSpan.FromMinutes(HttpTimeout);

            return client;
        }
        #endregion

        #region UI display methods
        /// <summary>
        /// displays a prompt to user for selecting an operation to perform
        /// </summary>
        /// <returns>The selected operation</returns>
        private static int DisplayPrompt()
        {
            Console.WriteLine("");
            Console.WriteLine("***************************************************");
            Console.WriteLine("Please select one of the below, or press 0 to exit.");
            Console.WriteLine("Press 1 to get count of customers via OData");
            Console.WriteLine("Press 2 to search for customer by RecId via OData");
            Console.WriteLine("Press 3 to get first 3 Tax On item entries via OData");
            Console.WriteLine("Press 4 to create Purchase Order via API");
            Console.WriteLine("Press 5 to get current user store via API");
            Console.WriteLine("***************************************************");
            Console.WriteLine("");
            return Convert.ToInt32(Console.ReadLine());
        }

        //displays customer detail
        private static void DisplayCustomer(FredODataService.CustTable cust)
        {
            Console.WriteLine("    {0}  {1}   {2}", cust.AccountNum, cust.RecId, cust.Party);
        }

        //displays vendr details
        private static void DisplayVendor(FredODataService.VendTable vend)
        {
            Console.WriteLine("    {0}  {1}   {2}", vend.AccountNum, vend.RecId, vend.Party);
        }

        //displays order details
        private static void DisplayPurchOrder(FredODataService.PurchTable purch)
        {
            Console.WriteLine("    {0}  {1}   {2}", purch.OrderAccount, purch.PurchId, purch.PurchStatus);
        }

        #endregion
    }

    
}
