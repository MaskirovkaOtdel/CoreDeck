using Microsoft.AspNetCore.Mvc;

namespace Final_Efstathiadis_Theodors.Components
{
    public class CompareBadgeViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var raw = HttpContext.Session?.GetString("CoreDeck_CompareList");
            int count = 0;
            if (!string.IsNullOrWhiteSpace(raw))
            {
                count = raw.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;
            }
            return View(count);
        }
    }
}
