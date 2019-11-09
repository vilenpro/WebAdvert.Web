using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.AspNetCore.Identity.Cognito;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.Account;
using Microsoft.AspNetCore.Authentication;

namespace WebAdvert.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<CognitoUser> _signinManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;

        public AccountController(SignInManager<CognitoUser> signinManager, UserManager<CognitoUser> userManager, CognitoUserPool pool)
        {
            this._signinManager = signinManager;
            this._userManager = userManager;
            this._pool = pool;
        }

        public async Task<IActionResult> SignUp()
        {
            return View(new SignupModel());
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignupModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _pool.GetUser(model.EmailAddress);
                if (user.Status != null)
                {
                    ModelState.AddModelError("UserExists", "User with this email already exists");
                }
                else
                {
                    user.Attributes.Add(CognitoAttribute.Name.AttributeName, model.EmailAddress);
                    var createdUser = await _userManager.CreateAsync(user, model.Password);
                    if (createdUser.Succeeded)
                    {
                        //await _signinManager.SignInAsync(user, true);
                        return RedirectToAction("Confirm");
                    }
                    else
                    {
                        foreach (var item in createdUser.Errors)
                        {
                            ModelState.AddModelError(item.Code, item.Description);
                        }
                    }
                }

            }
            return View(model);
        }


        public async Task<IActionResult> Confirm()
        {
            return View(new ConfirmModel());
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(ConfirmModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("UserNotFound", "User not found");
                    return View(model);
                }
                //await _signinManager.SignInAsync(user, false).ConfigureAwait(false);
                var result = await ((CognitoUserManager<CognitoUser>)(_userManager)).ConfirmSignUpAsync(user, model.Code, true).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Login()
        {
            return View(new LoginModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signinManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("InvalidCredentials", "Invalid credentials");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    await user.ForgotPasswordAsync().ConfigureAwait(false);
                    return RedirectToAction("ResetPassword");
                }
                else
                {
                    ModelState.AddModelError("UserNotFound", "Unable to find the user for provided email address");
                }
            }
            return View(model);
        }


        public async Task<IActionResult> ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _pool.GetUser();
                if (user != null)
                {
                    await user.ConfirmForgotPasswordAsync(model.Code, model.Password);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(null, "User not found");
                }
            }
            return View(model);
        }

    }
}