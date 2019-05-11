using System;
namespace ReservationAssistant.Model
{
    public class TableReservationModel
    {
        public string ReservedBy { get; set; }
        public int TotalOccupants { get; set; }
        public DateTime ReservationDate { get; set; }
    }
}
