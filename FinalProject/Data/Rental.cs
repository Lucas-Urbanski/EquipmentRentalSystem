namespace FinalProject.Data
{
    public class Rental
    {
        public int RentalID { get; set; }
        public int CustomerID { get; set; }
        public int EquipmentID { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public decimal TotalCost { get; set; }

        public Rental() { }

        public async Task<int> StartRentalAsync(Broker broker)
        {
            ArgumentNullException.ThrowIfNull(broker);
            var newId = await broker.InsertRentalAsync(this);
            if (newId > 0) RentalID = newId;
            return newId;
        }


        public async Task<bool> UpdateRentalAsync(Broker broker)
        {
            ArgumentNullException.ThrowIfNull(broker);
            if (RentalID == 0) throw new InvalidOperationException("Cannot update rental with ID = 0");
            return await broker.UpdateRentalAsync(this);
        }

        public async Task<bool> RemoveRentalAsync(Broker broker, bool makeEquipmentAvailable = false)
        {
            ArgumentNullException.ThrowIfNull(broker);
            if (RentalID == 0) throw new InvalidOperationException("Cannot delete rental with ID = 0");
            return await broker.DeleteRentalAsync(RentalID, makeEquipmentAvailable);
        }

        public static async Task<Rental?> SearchRentalByIdAsync(Broker broker, int id)
        {
            ArgumentNullException.ThrowIfNull(broker);
            return await broker.LoadRentalByIdAsync(id);
        }

    }
}