using Newtonsoft.Json;
using RentaMetrix.Bridge;
using RentaMetrix.DataModels;
using RentaMetrix.Global.Services;

namespace RentaMetrixUI.Pages.Portfolio
{
    public class PropertyOverview_PartialModel : BasePageModel
    {

        private readonly UserRolesAndMarketsClient userRolesAndMarketsClient;
        public List<PropertyType> PropertyTypes { get; set; } = new();
        public List<MarketsViewModel> MarketsViewModels { get; set; }
        public List<UniversityViewModel> Universities { get; set; } = new();
        public Properties Property { get; set; } = new();
        private protected readonly PropertyManagementClient propertyManagementClient;
        public PropertyOverview_PartialModel(UserRolesAndMarketsClient userRolesAndMarketsClient, PropertyManagementClient propertyManagementClient, ICookieService cookieService) : base(cookieService)
        {
            this.userRolesAndMarketsClient = userRolesAndMarketsClient;
            this.propertyManagementClient = propertyManagementClient;
            GetPropertyOverview();
        }

        public async void OnGetAsync()
        {
            if (User is null || Token is null)
                SetProperties();
            if (TempData != null && TempData["PropertyOverviewSelected"] is not null)
            {
                var propertyJson = TempData.Peek("PropertyOverviewSelected")?.ToString();
                Property = JsonConvert.DeserializeObject<Properties>(propertyJson);
            }
            else
            {
                Property = new();
            }
            await GetPropertyTypesAsync();
        }

        public async void GetPropertyOverview()
        {
            if (TempData != null && TempData["PropertyOverviewSelected"] is not null)
            {
                var propertyJson = TempData.Peek("PropertyOverviewSelected")?.ToString();
                Property = JsonConvert.DeserializeObject<Properties>(propertyJson);
            }
            else
            {
                Property = new();
            }
            PropertyTypes = await propertyManagementClient.GetPropertyTypes("");
            MarketsViewModels = await userRolesAndMarketsClient.GetMarketsViewModel(Token);
        }

        private async Task GetPropertyTypesAsync()
        {
            PropertyTypes = await propertyManagementClient.GetPropertyTypes("");
        }
    }
}
