using CarInsuranceBot.BLL.Enums;
using CarInsuranceBot.BLL.Models;

namespace CarInsuranceBot.BLL
{
    public static class Tracker
    {
        public static Dictionary<long, ProcessStatus> Statuses { get; set; } = [];
        public static Dictionary<long, ExtractedFields> ExtractedFields { get; set; } = [];
    }
}
