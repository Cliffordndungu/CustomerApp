using System.Net.Http;
using System.Text;
using Azure.Core;
using CustomerApp.Data;
using CustomerApp.Data.Static;
using CustomerApp.Data.ViewModel;
using CustomerApp.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CustomerApp.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDBContext _context;
        //private readonly Stripe _stripeService;



        string baseurl = "https://eu2-cloud.acronis.com";
        string tenantId = "4d125516-287f-4450-9c3d-1bb8d53e500e";
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AppDBContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register() => View(new RegisterVM());

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid) return View(registerVM);

            var user = await _userManager.FindByEmailAsync(registerVM.EmailAddress);
            if (user != null)
            {
                TempData["Error"] = "This email address is already in use";
                return View(registerVM);
            }

            var newUser = new ApplicationUser()
            {
                Company = registerVM.Company,
                Fullname = registerVM.Fullname,
                Email = registerVM.EmailAddress,
                Phonenumber = registerVM.Phonenumber,
                UserName = registerVM.EmailAddress
            };
            var newUserResponse = await _userManager.CreateAsync(newUser, registerVM.Password);

            if (newUserResponse.Succeeded)
            {

                // Acronis Intergration

                string base_url = "https://eu2-cloud.acronis.com/api/2";
                string accessToken = "eyJhbGciOiJSUzI1NiIsImlyaSI6ImNpNThrdms3ODhmNzdpcnRxZnNnIiwia2lkIjoiMmQyZTc3ZGQtMmQ4Ni00MjA2LTg3YzQtZjgyYThlNDE1ZWMwIn0.eyJhdWQiOiJldTItY2xvdWQuYWNyb25pcy5jb20iLCJleHAiOjE2ODY4MDgyMjIsImp0aSI6Ijk3MDc0YmNlLTQ0YTktNDAyYy1hNjlmLTdiZGJmY2RlZTJhMyIsImlhdCI6MTY4NjgwMTAyMiwiaXNzIjoiaHR0cHM6Ly9ldTItY2xvdWQuYWNyb25pcy5jb20iLCJzdWIiOiJhNWU5MGZmYy0wOTNjLTRlMjItYjExZC1lOGFiYjgxYzBjMGIiLCJzY29wZSI6W3sidGlkIjoiNGQxMjU1MTYtMjg3Zi00NDUwLTljM2QtMWJiOGQ1M2U1MDBlIiwicm9sZSI6InBhcnRuZXJfYWRtaW4ifV0sInZlciI6Miwic3ViX3R5cGUiOiJhcGlfY2xpZW50IiwiY2xpZW50X2lkIjoiYTVlOTBmZmMtMDkzYy00ZTIyLWIxMWQtZThhYmI4MWMwYzBiIn0.eqlfcaonW-upx4D2sNyi2jS1sJd-a_2CeGp0TUG55-0-El36Qd-ZI-BdHs3lLNIc_Dwf_G7bVv5Yyf1gyDjozIq-RRZyldwvss9sHOBkr5WRVeKOubJZ82W0QFxY2LT_Kg2weA355f3OIBJdqXrK_hTuvmDxxcnx9_w11h2UMJRwU74Em88zfjixRACM1K3mdg-lwtlvgGprG2Ek_ujfGOH9jMr87QDin6LwZQUB_IAF-dMYaGGKczgGcHnqbZafgmf5O0VSojqHkzB_Xq8KWcnJo3lxh0W22V63lRaj4xpD4tNwqiVr-kmDf9nS4qkVTP73bLP7gCdOFrIpPSoLFg";

                string tenant_id = "4d125516-287f-4450-9c3d-1bb8d53e500e";
                // ensure these are kept secure 
                // user secrets 
                // azure key vaults


                string tenantname = registerVM.Company;


                var tenant = new
                {
                    name = tenantname,
                    kind = "customer",
                    parent_id = tenant_id,
                    internal_tag = "some.unique.tag.value",
                    language = "en",
                    contact = new
                    {
                        address1 = "366 5th Ave",
                        email = "foo.bar@example.com",
                        phone = "1 123 4567890"
                    }
                };

                // Convert the tenant object to JSON
                string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(tenant);

                using (HttpClient client = new HttpClient())
                {
                    // Set the authorization header
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                    // Set the request content type to application/json
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    // Send the POST request with the JSON body
                    var response = await client.PostAsync($"{base_url}/tenants", content);

                    // Check the response status
                    if (response.IsSuccessStatusCode)
                    {
                        // Tenant creation successful
                        Console.WriteLine("Customer tenant created successfully.");

                        // Convert the response body to JSON and retrieve the created tenant ID
                        string responseBody = await response.Content.ReadAsStringAsync();
                        dynamic responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
                        string createdTenantId = responseObject.id;

                        //save tenant id in a table with customername 
                        var customertenant = new Customertenant()
                        {
                            id = registerVM.Company,
                            tenantid = createdTenantId
                            
                        };
                        await _context.Customertenant.AddAsync(customertenant);
                        await _context.SaveChangesAsync();
                        Console.WriteLine("Created Tenant ID: " + createdTenantId);
                    }
                    else
                    {
                        // Error occurred
                        Console.WriteLine($"An error occurred while creating the customer tenant. Status code: {response.StatusCode}");
                    }

                    // Stripe intergration
                    //create users

                    // This creates the new stripe customer using the Stripe.NET nuget package
                    //tenant.StripeCustomerId = await _stripeService.CreateCustomer(tenant.Email, tenant.Name);

                }

            
               
            }
            return View("RegisterCompleted");

        }


        public IActionResult Login() => View(new LoginVM());

       

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid) return View(loginVM);

            var user = await _userManager.FindByEmailAsync(loginVM.EmailAddress);
            if (user != null)
            {
                var passwordCheck = await _userManager.CheckPasswordAsync(user, loginVM.Password);
                if (passwordCheck)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, false, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                TempData["Error"] = "Wrong credentials. Please, try again!";
                return View(loginVM);
            }

            TempData["Error"] = "Wrong credentials. Please, try again!";
            return View(loginVM);
        }
    }

}
    

