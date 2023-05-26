using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.AccessControl;
using System.Security.Claims;
using WebApplication.Models;

namespace WebApplication.Pages
{
    [AuthorizeAttribute(Policy = "Builder")]
    public partial class BuilderPageModel : PageModel
    {
        protected IDbContextFactory<MispisCourseworkContext> DatabaseFactory { get; set; } = default!;

        protected readonly ILogger<BuilderPageModel> Logger = default!;
        public BuilderPageModel(ILogger<BuilderPageModel> logger, IDbContextFactory<MispisCourseworkContext> factory)
            : base() { this.Logger = logger; this.DatabaseFactory = factory; }

        [BindPropertyAttribute(SupportsGet = true)]
        public string? ErrorMessage { get; set; } = default;

        public List<Buildingorder> BuildingOrderList { get; private set; } = new();
        public List<Resource> ResourcesList { get; private set; } = new();

        [BindPropertyAttribute(SupportsGet = true)]
        public int? SelectedOrderId { get; set; } = default!;
        public Buildingorder? SelectedOrderModel { get; private set; } = default!;

        [BindPropertyAttribute(SupportsGet = true)]
        public int? SelectedResourceId { get; set; } = default!;
        public Resource? SelectedResourceModel { get; private set; } = default!;

        public List<Resourceorder> ResourceOrderList { get; private set; } = new();
        public ResourceRequire? ResourceRequireModel { get; private set; } = default!; 
        public virtual async Task<IActionResult> OnGetAsync()
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                var builderRecord = await dbcontext.Builders.Include(item => item.Employee)
                    .FirstOrDefaultAsync(item => item.Employee.Accountid == profileId);
                if (builderRecord == null) return base.BadRequest("Невозможно загрузить данные о сборщике");

                this.BuildingOrderList = await dbcontext.Buildingorders.Where(item => item.Builderid == builderRecord.Builderid)
                    .Include(item => item.Order).ToListAsync();

                if (this.SelectedOrderId != null)
                {
                    this.SelectedOrderModel = await dbcontext.Buildingorders
                        .Where(item => item.Orderid == this.SelectedOrderId).FirstOrDefaultAsync();

                    this.ResourceRequireModel = await this.GetResourcesTables(this.SelectedOrderId.Value);
                }
                if (this.SelectedResourceId != null) this.SelectedResourceModel = await dbcontext.Resources.Where(item 
                    => item.Resourceid == this.SelectedResourceId)
                    .Include(item => item.Provider).ThenInclude(item => item.Account).FirstOrDefaultAsync();

                this.ResourcesList = await dbcontext.Resources.Include(item => item.Provider).ToListAsync();
                this.ResourceOrderList = await dbcontext.Resourceorders
                    .Where(item => item.Builderid == builderRecord.Builderid).ToListAsync();
            } 
            return this.Page();
        }
        public record class ResourceRequire(string ResourceType)
        {
            public int RequiredResource { get; init; } = default;
            public int RequiredGlass { get; init; } = default;

            public int CountGlass { get; init; } = default;
            public int CountResource { get; init; } = default;

        }
        private async Task<ResourceRequire?> GetResourcesTables(int selectedId)
        {
            ResourceRequire resultModel = default!;
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                var orderRecord = await dbcontext.Buildingorders.Where(item => item.Orderid == selectedId)
                    .Include(item => item.Order).FirstOrDefaultAsync();
                if (orderRecord == null) throw new Exception("Запись о заказе не найдена");

                var requiredResource = (int)((orderRecord.Order.Width + orderRecord.Order.Height) * 2);
                var requiredGlass = orderRecord.Order.Packetcount;

                var resourceType = orderRecord.Order.Windowtype switch
                { "Бюджетный" => "Дерево", "Стандарт" => "Пластик", "Премиум" => "Железо", _ => "Дерево" };

                var countResource = dbcontext.Resources.Where(item => item.Resourcename == resourceType).Sum(item => item.Count);
                var countGlass = dbcontext.Resources.Where(item => item.Resourcename == "Стекло").Sum(item => item.Count);

                resultModel = new ResourceRequire(resourceType)
                {
                    CountGlass = countGlass!.Value, CountResource = countResource!.Value,
                    RequiredGlass = requiredGlass, RequiredResource = requiredResource
                };
            }
            return resultModel;
        }

        private async Task TakeResourceFromStorage(int bufferedRequired, string resourceType)
        {
            using var dbcontext = await this.DatabaseFactory.CreateDbContextAsync();
            var deletedList = new List<int>();
            foreach (var record in dbcontext.Resources.Where(item => item.Resourcename == resourceType).ToList())
            {
                if (record.Count < bufferedRequired)
                {
                    deletedList.Add(record.Resourceid);
                    bufferedRequired -= record.Count.Value;
                }
                else
                {
                    await dbcontext.Resources.Where(item => item.Resourceid == record.Resourceid)
                        .ExecuteUpdateAsync(item => item.SetProperty(p => p.Count, p => record.Count - bufferedRequired));
                    break;
                }
            }
            foreach (var delete in deletedList)
                await dbcontext.Resources.Where(item => item.Resourceid == delete).ExecuteDeleteAsync();
        }

        public virtual async Task<IActionResult> OnPostAsync([FromForm(Name = "selectedId")] int selectedId)
        {
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                var orderRecord = await dbcontext.Buildingorders.Where(item => item.Orderid == selectedId)
                    .Include(item => item.Order).FirstOrDefaultAsync();
                if(orderRecord == null) return base.RedirectToPage("BuilderPage", new { ErrorMessage = "Заказ не найден" });

                var requiredResource = (int)((orderRecord.Order.Width + orderRecord.Order.Height) * 2);
                var requiredGlass = orderRecord.Order.Packetcount;

                var resourceType = orderRecord.Order.Windowtype switch
                { "Бюджетный" => "Дерево", "Стандарт" => "Пластик", "Премиум" => "Железо", _ => "Дерево" };

                var countResource = dbcontext.Resources.Where(item => item.Resourcename == resourceType).Sum(item => item.Count);
                var countGlass = dbcontext.Resources.Where(item => item.Resourcename == "Стекло").Sum(item => item.Count);

                if(countGlass < requiredGlass || countResource < requiredResource)
                { return base.RedirectToPage("BuilderPage", new { ErrorMessage = "Недостаточно ресурсов" }); }

                await this.TakeResourceFromStorage(requiredGlass, "Стекло");
                await this.TakeResourceFromStorage(requiredResource, resourceType);

                await dbcontext.Orders.Where(item => item.Orderid == orderRecord.Orderid).ExecuteUpdateAsync(item => 
                    item.SetProperty(p => p.State, p => "Готов к отправке"));
                await dbcontext.Buildingorders.Where(item => item.Buildingid == orderRecord.Buildingid).ExecuteDeleteAsync();
            }
            return base.RedirectToPage("BuilderPage", new { ErrorMessage = $"Заказ #{selectedId} успешно собран" });
        }
        public virtual async Task<IActionResult> OnPostRequestAsync([FromForm] string requireResource, 
            [FromForm] int requireCount)
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                var builderRecord = await dbcontext.Builders.Include(item => item.Employee)
                    .Where(item => item.Employee.Accountid == profileId).FirstOrDefaultAsync();

                if (builderRecord == null) return base.BadRequest("Запись о работнике не найдена");
                await dbcontext.Resourceorders.AddRangeAsync(new Resourceorder()
                {
                    Resourcename = requireResource, Count = requireCount, Builderid = builderRecord.Builderid
                });
                await dbcontext.SaveChangesAsync();
            }
            return this.RedirectToPage("BuilderPage");
        }
    }
}
