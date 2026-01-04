namespace Application.Entities.Derived.PurchaseForecasts.DTOs;

public class PurchaseForecastDto
{
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public int WeekIndex { get; set; } 

    public int ForecastQuantity { get; set; }
    public int QuantityCommitted { get; set; }
    public int QuantityOnOrder { get; set; }      
    public int ProjectedStock { get; set; }       

    public bool NeedsReorder { get; set; }
    public int RecommendedOrderQuantity { get; set; }
    public DateTime? RecommendedOrderDate { get; set; }

    public DateTime CalculatedAt { get; set; }
}
