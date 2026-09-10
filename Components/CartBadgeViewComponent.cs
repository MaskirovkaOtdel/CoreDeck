using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Final_Efstathiadis_Theodors.Models;
using Final_Efstathiadis_Theodors.Services;

namespace Final_Efstathiadis_Theodors.Components
{
    public class CartBadgeViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartBadgeViewComponent(ICartService cartService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int count = 0;
            if (UserClaimsPrincipal.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(UserClaimsPrincipal);
                if (!string.IsNullOrEmpty(userId))
                {
                    count = await _cartService.GetCartItemCountAsync(userId);
                }
            }

            return View(count);
        }
    }
}
