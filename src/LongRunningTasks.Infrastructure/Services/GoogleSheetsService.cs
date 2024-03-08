using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using LongRunningTasks.Application.Services;
using LongRunningTasks.Infrastructure.Configs;
using Microsoft.Extensions.Options;
using static Google.Apis.Sheets.v4.SheetsService;

namespace LongRunningTasks.Infrastructure.Services
{
    internal class GoogleSheetsService : IGoogleSheetsService
    {
        private readonly SheetsService _sheetsService;
        private readonly GoogleSheetsConfig _googleSheetsConfig;

        public GoogleSheetsService(
            IOptions<GoogleSheetsConfig> googleSheetsConfig,
            IOptions<GoogleApplicationConfig> googleApplicationConfig)
        {
            _googleSheetsConfig = googleSheetsConfig.Value;
            _sheetsService = GetService(_googleSheetsConfig, googleApplicationConfig.Value);
        }
        
        public UpdateValuesResponse UpdateSheet(IList<IList<object>> newRecords)
        {
            string range = $"{_googleSheetsConfig.Sheet}!A:E";
            ValueRange valueRange = new()
            {
                Values = newRecords
            };
            SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest =
                _sheetsService.Spreadsheets.Values.Update(valueRange, _googleSheetsConfig.SpreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

            return updateRequest.Execute();
        }

        private SheetsService GetService(GoogleSheetsConfig googleSheetsConfig, GoogleApplicationConfig googleApplicationConfig)
        {
            TokenResponse tokenResponse = new()
            {
                AccessToken = googleSheetsConfig.AccessToken,
                RefreshToken = googleSheetsConfig.RefreshToken
            };
            GoogleAuthorizationCodeFlow apiCodeFlow = new(new()
            {
                ClientSecrets = new()
                {
                    ClientId = googleApplicationConfig.ClientId,
                    ClientSecret = googleApplicationConfig.ClientSecret
                },
                Scopes = new[] { Scope.Spreadsheets },
                DataStore = new FileDataStore(googleApplicationConfig.Name)
            });
            UserCredential credential = new(apiCodeFlow, googleSheetsConfig.Username, tokenResponse);
            SheetsService service = new(new()
            {
                HttpClientInitializer = credential,
                ApplicationName = googleApplicationConfig.Name
            });

            return service;
        }
    }
}