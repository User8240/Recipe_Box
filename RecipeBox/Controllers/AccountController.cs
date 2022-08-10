using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using RecipeBox.Models;
using System.Threading.Tasks;
using RecipeBox.ViewModels;

namespace RecipeBox.Controllers
{
    public class AccountController : Controller
    {
        private readonly RecipeBoxContext _db;
        //We have private preferences for _userManager and _signInManager
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController (UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RecipeBoxContext db)
        {
            //Here we're using what's called "Dependency Injection" to configure our -
            //userManager and signInManager services for us!

            //Dependency Injection: Making sure an application doesn't have to locate,
            //load, find, or create a service on it's own (the service would be a 
            //helpful tool for the application to use - there to be used when needed)

            //Basically, we're injecting the Identity's UserManager and SignInManager 
            //services into the AccountController constructor so that our controller 
            //will have access to these services

            //The UserManager service helps manage saving and updating user account information
            _userManager = userManager;
            //The SignInManager provides functionality for users to log into their accounts
            _signInManager = signInManager;
            _db = db;
        }

        public ActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost] //all things below are asynchronous actions since Register is
                   //defined as a async Task<ActionResult>
                   //the Register() action returns a Task containing an ActionResult
                   //Register() also takes a model as an argument (the type of this model 
                   // is RegisterViewModel)
        public async Task<ActionResult> Register (RegisterViewModel model)
        {   //This creates a new ApplicationUser with the email from the form submission
            //as its UserName
            var user = new ApplicationUser { UserName = model.Email };
            //Here's the async method; where the code will pause and wait for this IdentityResult
            //valued variable called result to be returned before executing more code.
            
            //UserManager is a service, and from that class we can use the CreateAsync() method
            //which will create a user WITH the provided password

            //We use await because CreateAsync() is an asynchronous action
            //IdentityResult class simply represents the result of an - 
            //Identity-driven action whether it's successful or not.
            //*Our application needs to wait until CreateAsync() successfully
            //returns an IdentityResult before we actually define result
            IdentityResult result = await _userManager.CreateAsync(user, model.Password); //CreateAsync() takes two arguments:  
            //If CreateAsync() is successful, the controller redirects to Index           //An ApplicationUser with user information;
            if (result.Succeeded)                                                         //A password that will be encrypted when the
            {                                                                             //user is added to the database.
                return RedirectToAction("Index");                                                                                     
            }
            //Otherwise it returns the Register view.
            else 
            {
                return View();
            }
            //We can do the if statement above because our IdentityResult object already 
            //contains information about whether or not Identity was successful in 
            //registering the new user account
        }

        public ActionResult Login()
        {
            return View();
        }
        //Login() POST method uses an asynchronous method
        //Returns a Task<ActionResult>
        //Takes a ViewModel as an argument
        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel model)
        {                                                                   //Identity method (v) ending with Async
                                                                            //SignInManager class includes the PasswordSignInAsync() method,
                                                                            //it's an async method that allows users to sign in with a password
                                                                            //takes four parameters: userName, password, isPersistent and lockoutOnFailure
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: true, lockoutOnFailure: false);
            //Microsoft.AspNetCore.Identity.SignInResult object has a Succeeded boolean property to help with this.
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public async Task<ActionResult> LogOff()
        {   //SignInManager has the asynchronous method SignOutAsync() that signs the user out
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }
    }
}