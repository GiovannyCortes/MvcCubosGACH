using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MvcCubosGACH.Models;
using System.Security.Claims;
using MvcCubosGACH.Services;

namespace MvcCubosGACH.Controllers {
    public class ManagedController : Controller {

        private ServiceCubos service;

        public ManagedController(ServiceCubos service) {
            this.service = service;
        }

        public IActionResult LogIn() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(string email, string password) {
            string? token = await this.service.GetToken(email, password);

            if (token == null) {
                ViewData["MENSAJE"] = "Usuario/Password incorrectos";
                return View();
            } else {
                HttpContext.Session.SetString("TOKEN", token);

                Usuario user = await this.service.GetPerfil();

                ClaimsIdentity identity = new ClaimsIdentity(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    ClaimTypes.Name,
                    ClaimTypes.Role
                );

                Claim claimId = new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString());
                Claim claimName = new Claim(ClaimTypes.Name, user.Nombre);
                Claim claimImage = new Claim("IMAGEN", user.Imagen);
                Claim claimEmail = new Claim("EMAIL", user.Email);
                Claim claimRole = new Claim(ClaimTypes.Role, "USER");

                identity.AddClaim(claimId);
                identity.AddClaim(claimName);
                identity.AddClaim(claimImage);
                identity.AddClaim(claimEmail);
                identity.AddClaim(claimRole);


                ClaimsPrincipal userPrincipal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    userPrincipal, new AuthenticationProperties {
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                    });
                return RedirectToAction("Perfil", "User");
            }

        }

        public async Task<IActionResult> LogOut() {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("TOKEN");
            return RedirectToAction("LogIn", "Managed");
        }

    }
}
