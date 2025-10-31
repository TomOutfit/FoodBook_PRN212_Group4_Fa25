namespace Foodbook.Business.Models
{
    public class IngredientDto
    {
        public string Name { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        
        // Nutritional values per unit
        public double CaloriesPerUnit { get; set; }
        public double ProteinPerUnit { get; set; }
        public double CarbohydratesPerUnit { get; set; }
        public double FatPerUnit { get; set; }
        public double FiberPerUnit { get; set; }
        public double SugarPerUnit { get; set; }
        public double SodiumPerUnit { get; set; }
    }
}
