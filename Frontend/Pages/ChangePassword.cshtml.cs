using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebService.Adapters;

namespace WebService.Pages
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly UsersServiceAdapter _usersService;

        public ChangePasswordModel(UsersServiceAdapter usersService)
        {
            _usersService = usersService;
        }

        [BindProperty]
        public ChangePasswordInput Input { get; set; } = new();

        [BindProperty]
        public bool CurrentPasswordVerified { get; set; }

        public bool PasswordChanged { get; set; }

        public IActionResult OnGet()
        {
            CurrentPasswordVerified = false;
            return Page();
        }

        public async Task<IActionResult> OnPostVerifyCurrentPasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(Input.CurrentPassword))
            {
                return BadRequest(new { message = "La contraseña actual es obligatoria." });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var usuarioLoginId))
            {
                ModelState.AddModelError(string.Empty, "No se pudo identificar el usuario autenticado.");
                return Page();
            }

            var result = await _usersService.VerifyCurrentPasswordAsync(usuarioLoginId, Input.CurrentPassword);
            if (!result.ok)
            {
                return BadRequest(new { message = result.error ?? "No fue posible validar la contraseña actual." });
            }

            CurrentPasswordVerified = true;
            return new JsonResult(new { valid = true });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!CurrentPasswordVerified)
            {
                ModelState.AddModelError(string.Empty, "Primero valida tu contraseña actual antes de ingresar una nueva.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Input.CurrentPassword))
            {
                ModelState.AddModelError("Input.CurrentPassword", "La contraseña actual es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(Input.NewPassword))
            {
                ModelState.AddModelError("Input.NewPassword", "La nueva contraseña es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(Input.ConfirmPassword))
            {
                ModelState.AddModelError("Input.ConfirmPassword", "Debe confirmar la contraseña.");
            }

            if (!string.IsNullOrWhiteSpace(Input.NewPassword) && !string.IsNullOrWhiteSpace(Input.ConfirmPassword) && Input.NewPassword != Input.ConfirmPassword)
            {
                ModelState.AddModelError("Input.ConfirmPassword", "Las contraseñas no coinciden.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var usuarioLoginId))
            {
                ModelState.AddModelError(string.Empty, "No se pudo identificar el usuario autenticado.");
                return Page();
            }

            var result = await _usersService.ChangePasswordAsync(
                usuarioLoginId,
                Input.CurrentPassword,
                Input.NewPassword,
                Input.ConfirmPassword);

            if (!result.ok)
            {
                ModelState.AddModelError(string.Empty, result.error ?? "No fue posible actualizar la contraseña.");
                return Page();
            }

            if (ShouldRedirectToLoginAfterChange())
            {
                TempData["PasswordChangedOnFirstLogin"] = true;
                return RedirectToPage("/Login");
            }

            TempData["PasswordChangedSuccess"] = true;
            return RedirectToPage("/Index");
        }

        private bool ShouldRedirectToLoginAfterChange()
        {
            var requiresPasswordChange = User.FindFirstValue("RequiereCambio");
            return bool.TryParse(requiresPasswordChange, out var result) && result;
        }
    }

    public class ChangePasswordInput
    {
        public string CurrentPassword { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
