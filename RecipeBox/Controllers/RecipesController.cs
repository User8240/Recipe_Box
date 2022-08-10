using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RecipeBox.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Claims;

namespace RecipeBox.Controllers
{
  //This allows access to the RecipesController only if a user is logged in
  //the entirety of the controller is now shielded from unauthorized users
  [Authorize]
  public class RecipesController : Controller
  {
    private readonly RecipeBoxContext _db;
    //We need an instance of UserManager to work with signed-in users
    private readonly UserManager<ApplicationUser> _userManager;
    //include a constructor to instantiate private readonly instances of the database and the UserManager
    public RecipesController(UserManager<ApplicationUser> userManager, RecipeBoxContext db)
    {
      _userManager = userManager;
      _db = db;
    }
    //Because the action is asynchronous, it also returns a Task containing an action result
    public async Task<ActionResult> Index()
    { //locate the unique identifier for the currently-logged-in user and assign it the variable name userId
      //FOR "var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;" LINE BELOW: 
      // - this refers to the RecipeController itself
      // - FindFirst() is a method that locates the first record that meets the provided criteria
      // - FindFirst() is a method that locates the first record that meets the provided criteria
      // - NameIdentifier is a property that refers to an Entity's unique ID

      // - For the ? operator: This is called an existential operator
      //   It states that we should only call the property to the right
      //   of the ? if the method to the left of the ? doesn't return null
      //   So below: 
      //   if this.User.FindFirst(ClaimTypes.NameIdentifier) returns null, 
      //   don't call the property to the right of the existential operator. 
      //   However, if it doesn't return null, it retrieves Value property.

      //   if we successfully locate the NameIdentifier of the current user, 
      //   we'll call Value to retrieve the actual unique identifier value.
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      // Once we have the userId value, we're ready to call our async method:
      // First we call the UserManager service that we've injected into this controller
      // We provide the userId we just located as an argument to FindByIdAsync()
      // We include the await keyword so the code will wait for Identity to locate the correct user before moving on
      var currentUser = await _userManager.FindByIdAsync(userId);
      // Create a variable to store a collection containing only the Recipes that are
      // associated with the currently-logged-in user's unique Id property:

      // We use the Where() method, which is a LINQ method we can use to query a 
      // collection in a way that echoes the logic of SQL. 

      // We're simply asking Entity to find recipes in the database where the user
      // id associated with the recipe is the same id as the id that belongs to the currentUser
      var userRecipes = _db.Recipes.Where(entry => entry.User.Id == currentUser.Id).OrderByDescending(recipe => recipe.Rate).ToList();
      return View(userRecipes);
    }

    public ActionResult Create()
    {
      return View();
    }

    // We start by finding the value of the current user. Then we associate the current 
    // user with the Recipe's User property. This makes the association so that an Recipe 
    // belongs to a User. Finally, we add the recipe to the database and save it 
    [HttpPost]
    public async Task<ActionResult> Create(Recipe recipe, int CategoryId)
    {
        var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var currentUser = await _userManager.FindByIdAsync(userId);
        recipe.User = currentUser;
        _db.Recipes.Add(recipe);
        _db.SaveChanges();
        if (CategoryId != 0)
        {
            _db.CategoryRecipe.Add(new CategoryRecipe() { CategoryId = CategoryId, RecipeId = recipe.RecipeId });
        }
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    public ActionResult Details(int id)
    {
      var thisRecipe = _db.Recipes
          .Include(recipe => recipe.JoinEntities)
          .ThenInclude(join => join.Category)
          .FirstOrDefault(recipe => recipe.RecipeId == id);
      return View(thisRecipe);
    }

    public ActionResult Edit(int id)
    {
      var thisRecipe = _db.Recipes.FirstOrDefault(recipe => recipe.RecipeId == id);
      ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
      return View(thisRecipe);
    }

    [HttpPost]
    public ActionResult Edit(Recipe recipe, int CategoryId)
    {
      if (CategoryId != 0)
      {
        _db.CategoryRecipe.Add(new CategoryRecipe() { CategoryId = CategoryId, RecipeId = recipe.RecipeId });
      }
      _db.Entry(recipe).State = EntityState.Modified;
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    public ActionResult AddCategory(int id)
    {
      var thisRecipe = _db.Recipes.FirstOrDefault(recipe => recipe.RecipeId == id);
      ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
      ViewBag.CategoryList = _db.Categories.ToList();
      return View(thisRecipe);
    }

    [HttpPost]
    public ActionResult AddCategory(Recipe recipe, int CategoryId)
    {
      if (CategoryId != 0)
      {
        _db.CategoryRecipe.Add(new CategoryRecipe() { CategoryId = CategoryId, RecipeId = recipe.RecipeId });
        _db.SaveChanges();
      }
      return RedirectToAction("Index");
    }

    public ActionResult Delete(int id)
    {
      var thisRecipe = _db.Recipes.FirstOrDefault(recipe => recipe.RecipeId == id);
      return View(thisRecipe);
    }

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id)
    {
      var thisRecipe = _db.Recipes.FirstOrDefault(recipe => recipe.RecipeId == id);
      _db.Recipes.Remove(thisRecipe);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult DeleteCategory(int joinId)
    {
      var joinEntry = _db.CategoryRecipe.FirstOrDefault(entry => entry.CategoryRecipeId == joinId);
      _db.CategoryRecipe.Remove(joinEntry);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    public ActionResult Search()
    {
      return View();
    }

    [HttpPost]
    public async Task<ActionResult> ShowSearchResults(string searchPhrase)
    { 
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);
      //where each recipe in the recipes table where it contains the search phrase for ingredient
      List<Recipe> recipeList = _db.Recipes.Where(entry => entry.User.Id == currentUser.Id).Where(recipe => recipe.Ingredient.Contains(searchPhrase)).OrderByDescending(recipe => recipe.Rate).ToList();
      return View("Index", recipeList); 
    }
  }
}
