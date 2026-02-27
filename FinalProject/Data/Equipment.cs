namespace FinalProject.Data
{
    public class Equipment
    {
        public int EquipmentID { get; set; }
        public int CategoryID { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal DailyRate { get; set; }
        public bool Status { get; set; } = true;

        public Equipment() { }

        public async Task<int> AddEquipmentAsync(Broker broker)
        {
            ArgumentNullException.ThrowIfNull(broker);
            var newId = await broker.InsertEquipmentAsync(this);
            if (newId > 0) EquipmentID = newId;
            return newId;
        }

        public async Task<bool> UpdateEquipmentAsync(Broker broker)
        {
            ArgumentNullException.ThrowIfNull(broker);
            if (EquipmentID == 0) throw new InvalidOperationException("Cannot update equipment with ID = 0");
            return await broker.UpdateEquipmentAsync(this);
        }

        public async Task<bool> RemoveEquipmentAsync(Broker broker)
        {
            ArgumentNullException.ThrowIfNull(broker);
            if (EquipmentID == 0) throw new InvalidOperationException("Cannot delete equipment with ID = 0");
            return await broker.DeleteEquipmentAsync(EquipmentID);
        }

        public static async Task<Equipment?> SearchEquipmentByIdAsync(Broker broker, int id)
        {
            ArgumentNullException.ThrowIfNull(broker);
            return await broker.LoadEquipmentByIdAsync(id);
        }

    }
}