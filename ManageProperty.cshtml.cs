using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RentaMetrix.Bridge;
using RentaMetrix.DataModels;
using RentaMetrix.Global.Services;
using RentaMetrixUI.Helpers;

namespace RentaMetrixUI.Pages.Portfolio
{
    public class ManagePropertyModel : BasePageModel
    {
        private readonly UserRolesAndMarketsClient userRolesAndMarketsClient;
        public List<PropertyType> PropertyTypes { get; set; } = new();
        public List<MarketsViewModel> MarketsViewModels { get; set; }
        public List<UniversityViewModel> Universities { get; set; } = new();
        public Properties Property { get; set; } = new();
        private protected readonly PropertyManagementClient propertyManagementClient;
        public ManagePropertyModel(UserRolesAndMarketsClient userRolesAndMarketsClient, PropertyManagementClient propertyManagementClient, ICookieService _cookieService) : base(_cookieService)
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
            //await GetPropertyOverview();
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

        public async Task<IActionResult> OnGetPropertyOverviewAsync()
        {
            if (PropertyTypes.Count == 0)
                PropertyTypes = await propertyManagementClient.GetPropertyTypes("");
            return new JsonResult(PropertyTypes);
        }

        public async Task<IActionResult> OnGetUniversitiesByMarketAsync(int marketId)
        {
            if (Universities.Count == 0)
                Universities = await propertyManagementClient.GetUniversitiesByMarket(marketId, "");
            return new JsonResult(Universities);
        }

        public async Task<IActionResult> OnGetMarketViewModelAsync()
        {
            if (MarketsViewModels is null || MarketsViewModels.Count == 0)
                MarketsViewModels = await userRolesAndMarketsClient.GetMarketsViewModel(Token);
            return new JsonResult(MarketsViewModels);
        }

        public async Task<IActionResult> OnPostBackButtonClickedAsync()
        {
            return Redirect("/ManagementCompany");
        }



        private async Task GetPropertyTypesAsync()
        {
            PropertyTypes = await propertyManagementClient.GetPropertyTypes("");
        }

        public async Task OnPostPropertyOverviewSubmitAsync(Properties property, List<ContactDetails> contactDetails)
        {
            if (property.PropertyId == 0)
            {
                if (contactDetails.Count > 0)
                {
                    property.ContactDetails = JsonConvert.SerializeObject(contactDetails);
                }
                //property.PropertyType.PropertyTypeName = PropertyTypes.Find(x => x.PropertyTypeId == property.PropertyType.PropertyTypeId).PropertyTypeName;
                property.McId = ManagementCompany.ManagementCompanyId;
                var result = await propertyManagementClient.AddPropertyOverview(property);
            }
            else
            {
                if (contactDetails.Count > 0)
                {
                    property.ContactDetails = JsonConvert.SerializeObject(contactDetails);
                }
                if ((bool)!property.PropertyRenovated)
                {
                    property.RenovationYear = "";
                }
                var result = await propertyManagementClient.UpdatePropertyOverview(property);
            }
            return;
        }

        //public async Task<string> OnPostUpdatePropertyOverviewAsync(Properties property)
        //{
        //    return "";
        //}
        private protected void SetProperties()
        {
            var configuration = CustomConfigurationBuilder.GetConfiguration();
            IsAuthEnabled = Convert.ToBoolean(configuration["IsAuthEnabled"]);
            if (HttpContext is not null)
            {
                if (HttpContext.Request.Cookies.TryGetValue("jwtToken", out string results))
                {
                    if (!string.IsNullOrEmpty(results))
                    {
                        var responseObj = JsonConvert.DeserializeObject<AuthenticateResponse>(results);
                        if (responseObj != null)
                        {
                            if (responseObj.Token != null) Token = responseObj.Token;
                            if (responseObj.User != null) User = responseObj.User;
                            if (responseObj.ManagementCompany != null) ManagementCompany = responseObj.ManagementCompany;
                        }
                    }
                }
            }

        }
    }
}
