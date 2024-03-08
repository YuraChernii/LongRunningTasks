using Google.Apis.Sheets.v4.Data;

namespace LongRunningTasks.Application.Services
{
    public interface IGoogleSheetsService
    {
        public UpdateValuesResponse UpdateSheet(IList<IList<object>> newRecords);
    }
}