using CarInsuranceBot.BLL.Enums;

namespace CarInsuranceBot.BLL
{
    public static class Tracker
    {
        public static Dictionary<long, ProcessStatus> Statuses { get; set; } = [];
    }
}
